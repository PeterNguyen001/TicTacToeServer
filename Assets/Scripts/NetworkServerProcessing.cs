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
        HandleMessage(signifier, csv, clientConnectionID, pipeline);
       


        //if (signifier == ClientToServerSignifiers.RegisterUser)
        //{
        //    accountManager.RegisterUser(csv, clientConnectionID, pipeline);
        //}
        //else if(signifier == ClientToServerSignifiers.LogInUser)
        //{
        //    accountManager.LoginUser(csv, clientConnectionID, pipeline);
        //}
        //else if(signifier == ClientToServerSignifiers.FindGameRoom)
        //{
        //    if(ActivePlayers.ContainsKey(clientConnectionID))
        //    {
        //        gameRoomsManager.JoinOrCreateGame(csv, clientConnectionID, ActivePlayers[clientConnectionID], TransportPipeline.ReliableAndInOrder);
        //    }
        //}
        //else if(signifier == ClientToServerSignifiers.GoBack)
        //{
        //    if (ActivePlayers.ContainsKey(clientConnectionID) && ActivePlayers[clientConnectionID].roomPlayerIn != null)
        //    {
        //        roomPlayerIn.RemovePlayer(clientConnectionID);
        //    }
        //}
        //else if(signifier == ClientToServerSignifiers.Playing)
        //{
        //    if (ActivePlayers.ContainsKey(clientConnectionID) && ActivePlayers[clientConnectionID].roomPlayerIn != null)
        //    {
        //        roomPlayerIn.UpdatePlayers(csv, clientConnectionID);
        //    }
        //}
        //else if (signifier == ClientToServerSignifiers.updateHeartbeat)
        //{
        //    networkServer.UpdateHeartbeatTime(clientConnectionID);
        //}

    }
    private static void HandleMessage(int signifier, string[] csv, int clientConnectionID, TransportPipeline pipeline)
    {
        Dictionary<int, Account> ActivePlayers = accountManager.ActivePlayers;
        GameRoom roomPlayerIn = null;
        if (ActivePlayers.ContainsKey(clientConnectionID) && ActivePlayers[clientConnectionID].roomPlayerIn != null)
        {
            roomPlayerIn = ActivePlayers[clientConnectionID].roomPlayerIn;
        }
        switch (signifier)
        {
            case ClientToServerSignifiers.RegisterUser:
                accountManager.RegisterUser(csv, clientConnectionID, pipeline);
                break;
            case ClientToServerSignifiers.LogInUser:
                accountManager.LoginUser(csv, clientConnectionID, pipeline);
                break;
            case ClientToServerSignifiers.FindGameRoom:
                gameRoomsManager.JoinOrCreateGame(csv, clientConnectionID, ActivePlayers[clientConnectionID]);
                break;
            case ClientToServerSignifiers.GoBack:
                roomPlayerIn.RemovePlayer(clientConnectionID);
                break;
            case ClientToServerSignifiers.Playing:
                roomPlayerIn.UpdatePlayers(csv, clientConnectionID);
                break;
            case ClientToServerSignifiers.updateHeartbeat:
                networkServer.UpdateHeartbeatTime(clientConnectionID);
                break;
                // Add other cases as needed
        }
    }
    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public void ChangeClientUI(int screenID, int id, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient($"{ServerToClientSignifiers.ChangeUI},{screenID}", id, pipeline);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        Debug.Log("Client connection, ID == " + clientConnectionID);
    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
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
    public const int RegisterUser = 2;
    public const int LogInUser = 3;
    public const int FindGameRoom = 4;
    public const int updateHeartbeat = 5;
    public const int Playing = 8;
    public const int GoBack = 10;
}

static public class ServerToClientSignifiers
{
    public const int ChangeUI = 1;
    public const int StartGame = 7;
    public const int Playing = 8;
    public const int Spectate = 9;
}

#endregion

