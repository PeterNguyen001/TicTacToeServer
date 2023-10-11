using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;

public class NetworkServer : MonoBehaviour
{
    private LinkedList<Account> accountsList = new LinkedList<Account>();

    private const int Username = 1;
    private const int Password = 2;

    private string clientUserID;
    private string clientPass;

    private string serverUserID;
    private string serverPass;

    public const int userType = 0;
    public const string registerType = "2";
    public const string loginType = "3";
    public const string loggedInType = "4";

    public NetworkDriver networkDriver;
    private NativeList<NetworkConnection> networkConnections;

    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;

    const ushort NetworkPort = 9001;

    const int MaxNumberOfClientConnections = 1000;
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
        networkDriver = NetworkDriver.Create();
        reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = NetworkPort;

        int error = networkDriver.Bind(endpoint);
        if (error != 0)
            Debug.Log("Failed to bind to port " + NetworkPort);
        else
            networkDriver.Listen();

        networkConnections = new NativeList<NetworkConnection>(MaxNumberOfClientConnections, Allocator.Persistent);
    }

    void OnDestroy()
    {
        networkDriver.Dispose();
        networkConnections.Dispose();
    }

    void Update()
    {
        #region Check Input and Send Msg

        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < networkConnections.Length; i++)
            {
                SendMessageToClient("Hello client's world, sincerely your network server", networkConnections[i]);
            }
        }

        #endregion

        networkDriver.ScheduleUpdate().Complete();

        #region Remove Unused Connections

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
            {
                networkConnections.RemoveAtSwapBack(i);
                i--;
            }
        }

        #endregion

        #region Accept New Connections

        while (AcceptIncomingConnection())
        {
            Debug.Log("Accepted a client connection");
        }

        #endregion

        #region Manage Network Events

        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;
        NetworkEvent.Type networkEventType;

        for (int i = 0; i < networkConnections.Length; i++)
        {
            if (!networkConnections[i].IsCreated)
                continue;

            while (PopNetworkEventAndCheckForData(networkConnections[i], out networkEventType, out streamReader, out pipelineUsedToSendEvent))
            {
                if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                    Debug.Log("Network event from: reliableAndInOrderPipeline");
                else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                    Debug.Log("Network event from: nonReliableNotInOrderedPipeline");

                switch (networkEventType)
                {
                    case NetworkEvent.Type.Data:
                        int sizeOfDataBuffer = streamReader.ReadInt();
                        NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                        streamReader.ReadBytes(buffer);
                        byte[] byteBuffer = buffer.ToArray();
                        string msg = Encoding.Unicode.GetString(byteBuffer);
                        ProcessReceivedMsg(msg, networkConnections[i]);
                        buffer.Dispose();
                        break;
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Client has disconnected from server");
                        networkConnections[i] = default(NetworkConnection);
                        break;
                }
            }
        }

        #endregion
    }

    private bool AcceptIncomingConnection()
    {
        NetworkConnection connection = networkDriver.Accept();
        if (connection == default(NetworkConnection))
            return false;

        networkConnections.Add(connection);
        return true;
    }

    private bool PopNetworkEventAndCheckForData(NetworkConnection networkConnection, out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);

        if (networkEventType == NetworkEvent.Type.Empty)
            return false;
        return true;
    }

    private void ProcessReceivedMsg(string msg, NetworkConnection networkConnection)
    {
        Debug.Log("Msg received = " + msg);
        string[] userData = msg.Split(',');
        string type = userData[userType];
        if (type == registerType)
        {
            SendMessageToClient("Registering", networkConnection);
            RegisterUser(userData, networkConnection);
        }
        if (type == loginType)
        {
            SendMessageToClient("Logging in", networkConnection);
            LoginUser(userData, networkConnection);
        }
        if (type == loggedInType)
        {

        }
    }

    public void SendMessageToClient(string msg, NetworkConnection networkConnection)
    {
        byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
        NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Persistent);


        //Driver.BeginSend(m_Connection, out var writer);
        DataStreamWriter streamWriter;
        //networkConnection.
        networkDriver.BeginSend(reliableAndInOrderPipeline, networkConnection, out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
    }
    private void RegisterUser(string[] userData, NetworkConnection networkConnection)
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
                SendMessageToClient("Name Taken", networkConnection);
            }
        }

        if (!sameName)
        {
            Debug.Log("Registered");
            SaveNewProfile(userData, clientUserID);
            accountsList.AddLast(new Account(clientUserID, clientPass));
            SendMessageToClient("Registered", networkConnection);
        }
        else
        {
            Debug.Log("Name Taken");
        }

    }

    private void SaveNewProfile(string[] data, string id)
    {
        using (StreamWriter sw = new StreamWriter("Profiles/" + id + ".txt"))
        {
            sw.Write(data[Password]);
        }
    }
    private void LoginUser(string[] userData, NetworkConnection networkConnection)
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
                SendMessageToClient(loggedInType.ToString() + ',', networkConnection);
                break;
            }

        }

        if (!sameProfile)
        {
            Debug.Log("Invalid User or Password");
        }
    }

}
