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
    Dictionary<int, Account> aciveUsers = new Dictionary<int, Account>();

    private string clientUserID;
    private string clientPass;
    private string gameRoomName;

    private string serverUserID;
    private string serverPass;

    public const int userType = 0;
    private const int Username = 1;
    private const int Password = 2;
    private const int GameRoomName = 1;

    public const string registerType = "2";
    public const string loginType = "3";
    public const string loggedInType = "4";

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
            {
                pass = sr.ReadLine();
            }
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
        if (type == loginType)
        {
            NetworkServerProcessing.SendMessageToClient("0Logging in", clientConnectionID, pipeline);
            LoginUser(userData, clientConnectionID, pipeline);
        }
        if (type == loggedInType)
        {
            JoinOrCreateGame(userData, clientConnectionID, pipeline);
        }
    }
    private void RegisterUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Registering");
        bool sameName = false;
        clientUserID = userData[Username];
        clientPass = userData[Password];
        Account newAccout = new Account(clientUserID, clientPass);


        foreach (Account acc in accountsList)
        {
            if (newAccout == acc)
            {
                sameName = true;
                NetworkServerProcessing.SendMessageToClient("Name Taken", clientConnectionID, pipeline);
            }
        }

        if (!sameName)
        {
            Debug.Log("Registered");
            SaveNewProfile(userData, clientUserID);
            accountsList.AddLast(newAccout);
            //aciveUsers.Add(clientConnectionID, accountsList);
            NetworkServerProcessing.SendMessageToClient("Registered", clientConnectionID, pipeline);
        }
    }

    private void SaveNewProfile(string[] data, string id)
    {
        using (StreamWriter sw = new StreamWriter("Profiles/" + id + ".txt"))
        {
            sw.Write(data[Password]);
        }
    }
    private void LoginUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        bool bFoundSameProfile = false;
        clientUserID = userData[Username];
        clientPass = userData[Password];
        Account newAcc = new Account(clientUserID, clientPass);
        foreach (Account acc in accountsList)
        {
            serverPass = acc.pass;

            if (newAcc == acc && serverPass == clientPass)
            {
                Debug.Log("Logged In");
                bFoundSameProfile = true;
                NetworkServerProcessing.SendMessageToClient(loggedInType.ToString() + ',', clientConnectionID, pipeline);
                aciveUsers.Add(clientConnectionID,acc);
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
        clientUserID = userData[Username];
        gameRoomName = userData[GameRoomName];
        foreach (Account acc in accountsList)
        {
            serverUserID = acc.username;
            if (serverUserID == clientUserID)
            {
                roomsManager.CreateRoom(acc, gameRoomName);
                //aciveUsers.AddLast(acc);
            }
        }
        NetworkServerProcessing.SendMessageToClient("Joining " + gameRoomName, clientConnectionID, pipeline);
        Debug.Log("Create Game Room: " + gameRoomName);
    }
}
