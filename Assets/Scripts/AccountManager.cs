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
    public void DisconnectPlayer(int playerID)
    {
        Debug.Log("Removing");
        if (acivePlayers.ContainsKey(playerID))
        {
            acivePlayers.Remove(playerID);
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, playerID, TransportPipeline.ReliableAndInOrder);
        }
    }  
    public Dictionary<int, Account> ActivePlayers { get {  return acivePlayers; } }
}
