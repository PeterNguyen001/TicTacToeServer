using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class NetworkServerProcessing
{
    static int userType = 0;

    #region Send and Receive Data Functions
    static public void ReceivedMessageFromClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        Debug.Log("Network msg received =  " + msg + ", from connection id = " + clientConnectionID + ", from pipeline = " + pipeline);

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        // else if (signifier == ClientToServerSignifiers.asd)
        // {

        // }
        accountManager.CheckForUserType(csv, csv[userType], clientConnectionID,pipeline);
        //gameLogic.DoSomething();
    }
    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public void ChangeClientUI(int screenID, int id, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(ServerToClientSignifiers.ChangeUI + ',' + screenID, id, pipeline);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);
    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
        accountManager.RemovePlayerFromRoom(clientConnectionID);
        accountManager.DisconnectPlayer(clientConnectionID);
        Debug.Log("Client disconnection, ID == " + clientConnectionID);
    }

    #endregion

    #region Setup
    static NetworkServer networkServer;
    static GameLogic gameLogic;
    static AccountManager accountManager;
    static GameRoomsManager gameRoomsManager;

    static public void SetNetworkServer(NetworkServer NetworkServer)
    {
        networkServer = NetworkServer;
    }
    static public NetworkServer GetNetworkServer()
    {
        return networkServer;
    }
    static public void SetGameLogic(GameLogic GameLogic)
    {
        gameLogic = GameLogic;
    }
    static public void SetAccountManager(AccountManager AccountManager)
    {
        accountManager = AccountManager;
    }
    static public void SetGameRoomManager(GameRoomsManager GameRoomsManager) 
    { gameRoomsManager = GameRoomsManager; }
    #endregion
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const string RegisterUser = "2";
    public const string LogInUser = "3";
    public const string FindGameRoom = "4";
    public const string Playing = "8";
    public const string GoBack = "10";
}

static public class ServerToClientSignifiers
{
    public const string ChangeUI = "1";
    public const string StartGame = "7";
    public const string Playing = "8";
}

#endregion

