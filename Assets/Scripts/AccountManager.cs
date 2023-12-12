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
    private Dictionary<int, Account> acivePlayers = new Dictionary<int, Account>();

    [SerializeField]
    private GameRoomsManager roomsManager;

    private const int UsernameSign = 1;
    private const int PasswordSign = 2;

    string clientUsername;
    string clientPass;

    string serverPass;

    string gameRoomName;

    void Awake()
    {
        LoadAllAccounts();
    }

    void Start()
    {
        NetworkServerProcessing.SetAccountManager(this);
        roomsManager = FindObjectOfType<GameRoomsManager>();
    }

    private void LoadAllAccounts()
    {
        DirectoryInfo dir = new DirectoryInfo("Profiles");
        foreach (FileInfo file in dir.GetFiles())
        {
            string id = Path.GetFileNameWithoutExtension(file.Name);
            string pass = File.ReadAllText(file.FullName);
            Debug.Log($"{id} {pass}");
            accountsList.AddLast(new Account(id, pass));
        }
    }

    //public void CheckForUserType(string[] userData, string type, int clientConnectionID, TransportPipeline pipeline)
    //{
    //    switch (type)
    //    {
    //        case ClientToServerSignifiers.RegisterUser:
    //            RegisterUser(userData, clientConnectionID, pipeline);
    //            break;
    //        case ClientToServerSignifiers.LogInUser:
    //            LoginUser(userData, clientConnectionID, pipeline);
    //            break;
    //        case ClientToServerSignifiers.FindGameRoom:
    //            JoinOrCreateGame(userData, clientConnectionID, pipeline);
    //            break;
    //        case ClientToServerSignifiers.GoBack:
    //            RemovePlayerFromRoom(clientConnectionID);
    //            break;
    //        case ClientToServerSignifiers.Playing:
    //            UpdatePlayers(userData, clientConnectionID);
    //            break;
    //        case "0":
    //            NetworkServerProcessing.DisconnectionEvent(clientConnectionID);
    //            break;
    //        default:
    //            Debug.Log(userData);
    //            break;
    //    }
    //}
    public void RegisterUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Registering");
        bool sameName = false;
        clientUsername = userData[UsernameSign];
        clientPass = userData[PasswordSign];
        Account newAccount = new Account(clientUsername, clientPass);


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
            SaveNewProfile(userData, clientUsername);
            accountsList.AddLast(newAccount);
            acivePlayers.Add(clientConnectionID, newAccount);
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, clientConnectionID, pipeline);
            NetworkServerProcessing.SendMessageToClient("Registered", clientConnectionID, pipeline);
        }
    }

    private void SaveNewProfile(string[] data, string id)
    {
        using (StreamWriter sw = new StreamWriter("Profiles/" + id + ".txt"))
        { sw.Write(data[PasswordSign]); }
    }
    public void LoginUser(string[] userData, int clientConnectionID, TransportPipeline pipeline)
    {
        bool bFoundSameProfile = false;

        clientUsername = userData[UsernameSign];
        clientPass = userData[PasswordSign];

        Account newAccount = new Account(clientUsername, clientPass);
        foreach (Account acc in accountsList)
        {
            serverPass = acc.pass;

            if (newAccount == acc && serverPass == clientPass)
            {
                Debug.Log("Logged In");
                bFoundSameProfile = true;
                NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, clientConnectionID, pipeline);
                acc.Id = clientConnectionID;
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



    public void RemovePlayerFromRoom(int playerID)
    {
        Debug.Log("Removing");
        if(acivePlayers.ContainsKey(playerID) && acivePlayers[playerID].roomPlayerIn != null) 
        {
                string roomPlayerIn = acivePlayers[playerID].roomPlayerIn.Name;
                roomsManager.RemovePlayerFromRoom(playerID, roomPlayerIn);
                Debug.Log("Remove Player from Game Room");
                NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, playerID, TransportPipeline.ReliableAndInOrder);
        }
    }
    public void DisconnectPlayer(int playerID)
    {
        RemovePlayerFromRoom(playerID);
        acivePlayers.Remove(playerID);
    }

    public void UpdatePlayers(string[] NewMove, int clientConnectionID)
    {
        if (acivePlayers.ContainsKey(clientConnectionID))
        {
            acivePlayers[clientConnectionID].roomPlayerIn.UpdatePlayers(NewMove, clientConnectionID);
        }
    }
    
    public Dictionary<int, Account> ActivePlayers { get {  return acivePlayers; } }
}
