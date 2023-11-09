using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Burst.CompilerServices;
using Unity.Networking.Transport;
using UnityEngine;
using static UnityEditor.PlayerSettings.Switch;

public class AccountManager : MonoBehaviour
{
    private LinkedList<Account> accountsList = new LinkedList<Account>();
    [SerializeField]
    LinkedList<Account> aciveUsers = new LinkedList<Account>();

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
        //Debug.Log("Username: " + clientUserID);
        foreach (Account acc in accountsList)
        {
            serverUserID = acc.userID;
            if (serverUserID == clientUserID)
            {
                sameName = true;
                NetworkServerProcessing.SendMessageToClient("Name Taken", clientConnectionID, pipeline);
            }
        }

        if (!sameName)
        {
            Debug.Log("Registered");
            SaveNewProfile(userData, clientUserID);
            accountsList.AddLast(new Account(clientUserID, clientPass));
            NetworkServerProcessing.SendMessageToClient("Registered", clientConnectionID, pipeline);
        }
        else
        {
            Debug.Log("Name Taken");
            NetworkServerProcessing.SendMessageToClient("Name Taken", clientConnectionID, pipeline);
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
        bool sameProfile = false;
        clientUserID = userData[Username];
        clientPass = userData[Password];
        Account newAcc = new Account(clientUserID, clientPass);
        foreach (Account acc in accountsList)
        {
            serverUserID = acc.userID;
            serverPass = acc.pass;
            //Debug.Log("SID:"+serverUserID + "CID:" + clientUserID);
            //Debug.Log("SP:" + serverPass + "CP:" + clientPass);

            if (serverUserID == clientUserID && serverPass == clientPass)
            {
                Debug.Log("Logged In");
                sameProfile = true;
                NetworkServerProcessing.SendMessageToClient(loggedInType.ToString() + ',', clientConnectionID, pipeline);
                aciveUsers.AddLast(acc);
                break;
            }

        }

        if (!sameProfile)
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
            serverUserID = acc.userID;
            if (serverUserID == clientUserID)
            {
                aciveUsers.AddLast(acc);
            }
        }
        NetworkServerProcessing.SendMessageToClient("Joining " + gameRoomName, clientConnectionID, pipeline);
        Debug.Log("Create Game Room: " + gameRoomName);
        //roomsManager.CreateRoom(gameRoomName, clientUserID);
    }
}
