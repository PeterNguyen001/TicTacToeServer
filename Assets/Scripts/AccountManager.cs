using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Burst.CompilerServices;
using Unity.Networking.Transport;
using UnityEngine;
using static UnityEditor.PlayerSettings.Switch;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class AccountManager : MonoBehaviour
{
    private LinkedList<Account> accountsList = new LinkedList<Account>();
    [SerializeField]
    Dictionary<int, Account> acivePlayers = new Dictionary<int, Account>();

    private string clientUserID;
    private string clientPass;
    private string gameRoomName;

    private string serverUserID;
    private string serverPass;

    public const int commandSign = 0;
    private const int usernameSign = 1;
    private const int passwordSign = 2;
    private const int gameRoomNameSign = 1;
    private const int goBackSign = 1;

    public const string changeUI = "1";
    public const string registerType = "2";
    public const string loginType = "3";
    public const string loggedInType = "4";
    public const string waitType = "5";
    public const string inRoom = "6";
    public const string startGame = "7";
    public const string playing = "8";
    public const string goBack = "b";

    [SerializeField]
    GameRoomsManager roomsManager;
    // Start is called before the first frame update
    void Awake()
    {
        GetAllAccount();
    }

    private void GetAllAccount()
    {
        DirectoryInfo dir = new DirectoryInfo("Profiles");
        //int fileCount = Directory.GetFiles("Profiles", "*.*", SearchOption.AllDirectories).Length;
        foreach (FileInfo file in dir.GetFiles())
        {
            string id = Path.GetFileNameWithoutExtension(file.Name);
            string pass;
            using StreamReader sr = new StreamReader("Profiles/" + id + ".txt");
            { pass = sr.ReadLine(); }
            Debug.Log(id + " " + pass);
            accountsList.AddLast(new Account(id, pass));
        }
    }
    void Start()
    {
        NetworkServerProcessing.SetAccountManager(this);
        roomsManager = FindObjectOfType<GameRoomsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CheckForUserType(string[] userData, string type, int clientConnectionID, TransportPipeline pipeline)
    {
        if (type == registerType)
        {
            NetworkServerProcessing.SendMessageToClient("Registering", clientConnectionID, pipeline);
            RegisterUser(userData, clientConnectionID, pipeline);
        }
        else if (type == loginType)
        {
            NetworkServerProcessing.SendMessageToClient("Logging in", clientConnectionID, pipeline);
            LoginUser(userData, clientConnectionID, pipeline);
        }
        else if(type == loggedInType)
        { JoinOrCreateGame(userData, clientConnectionID, pipeline); }
        else if(type == waitType || type == inRoom)
        {
            if (userData[goBackSign] == goBack)
            { RemovePlayer(clientConnectionID); }
        }
        else if(type == playing)
        {
            TalkToGameRoom(userData,clientConnectionID);
        }
        else
        { Debug.Log(userData); }
    }
    private void RegisterUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Registering");
        bool sameName = false;
        clientUserID = userData[usernameSign];
        clientPass = userData[passwordSign];
        Account newAccount = new Account(clientUserID, clientPass);


        foreach (Account acc in accountsList)
        {
            if (newAccount == acc)
            {
                sameName = true;
                NetworkServerProcessing.SendMessageToClient("Name Taken", clientConnectionID, pipeline);
            }
        }

        if (!sameName)
        {
            Debug.Log("Registered");
            SaveNewProfile(userData, clientUserID);
            accountsList.AddLast(newAccount);
            //aciveUsers.Add(clientConnectionID, accountsList);
            NetworkServerProcessing.SendMessageToClient("Registered", clientConnectionID, pipeline);
        }
    }

    private void SaveNewProfile(string[] data, string id)
    {
        using (StreamWriter sw = new StreamWriter("Profiles/" + id + ".txt"))
        { sw.Write(data[passwordSign]); }
    }
    private void LoginUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        bool bFoundSameProfile = false;
        clientUserID = userData[usernameSign];
        clientPass = userData[passwordSign];
        Account newAccount = new Account(clientUserID, clientPass);
        foreach (Account acc in accountsList)
        {
            serverPass = acc.pass;

            if (newAccount == acc && serverPass == clientPass)
            {
                Debug.Log("Logged In");
                bFoundSameProfile = true;
                NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, clientConnectionID, pipeline);
                acc.id = clientConnectionID;
                acivePlayers.Add(clientConnectionID, acc);
                break;
            }
        }
        if (!bFoundSameProfile)
        {

            Debug.Log("Invalid User or Password");
            NetworkServerProcessing.SendMessageToClient("Invalid User or Password", clientConnectionID, pipeline);
            
        }
    }

    private void JoinOrCreateGame(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        Account newAccount = null;
        if ((acivePlayers.ContainsKey(clientConnectionID)))
        {
            newAccount = acivePlayers[clientConnectionID];
        }
        clientUserID = newAccount.username;
        clientPass = "";     
        gameRoomName = userData[gameRoomNameSign];

        if(roomsManager.CheckForRoomExistence(gameRoomName) == null) 
        { roomsManager.CreateNewRoom(newAccount, gameRoomName); }
        else
        { 
            roomsManager.AddSecondPlayerToRoom(newAccount, gameRoomName);          
        }
        
 
        NetworkServerProcessing.SendMessageToClient("Joining " + gameRoomName, clientConnectionID, pipeline);
        Debug.Log("Create Game Room: " + gameRoomName);
    }

    public void RemovePlayer(int playerID)
    {
        Debug.Log("Removing");
        if(acivePlayers.ContainsKey(playerID)) 
        {
            string roomPlayerIn = acivePlayers[playerID].inGameRoom.name;
            roomsManager.RemovePlayerFromRoom(playerID, roomPlayerIn);
            Debug.Log("Remove Player from Game Room");
        }
    }
    public void DisconnectPlayer(int playerID)
    {
        RemovePlayer(playerID);
        acivePlayers.Remove(playerID);
    }

    public void TalkToGameRoom(string[] userData, int clientConnectionID)
    {
        if (acivePlayers.ContainsKey(clientConnectionID))
        {
            acivePlayers[clientConnectionID].inGameRoom.UpdatePlayers(userData, clientConnectionID);
        }
    }
    
}
