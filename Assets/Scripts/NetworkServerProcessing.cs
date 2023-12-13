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

    }
    private static void HandleMessage(int signifier, string[] csv, int clientConnectionID, TransportPipeline pipeline)
    {
        Dictionary<int, Account> activePlayers = accountManager.ActivePlayers;
        GameRoom roomPlayerIn = null;
        int chatMsgSign = 1;

        if (activePlayers.ContainsKey(clientConnectionID) && activePlayers[clientConnectionID].roomPlayerIn != null)
        {
            roomPlayerIn = activePlayers[clientConnectionID].roomPlayerIn;
        }

        switch (signifier)
        {
            case ClientToServerSignifiers.RegisterUser:
                accountManager.RegisterUser(csv, clientConnectionID, pipeline); break;
            case ClientToServerSignifiers.LogInUser:
                accountManager.LoginUser(csv, clientConnectionID, pipeline); break;
            case ClientToServerSignifiers.FindGameRoom:
                gameRoomsManager.JoinOrCreateGame(csv, clientConnectionID, activePlayers[clientConnectionID]); break;
            case ClientToServerSignifiers.GoBack:
                gameRoomsManager.RemovePlayerFromRoom(clientConnectionID); break;
            case ClientToServerSignifiers.Playing:
                roomPlayerIn.UpdatePlayers(csv, clientConnectionID); break;
            case ClientToServerSignifiers.updateHeartbeat:
                networkServer.UpdateHeartbeatTime(clientConnectionID); break;
            case ClientToServerSignifiers.ChatMessage:
                SendChatMessageToAllClient(csv[chatMsgSign], activePlayers, pipeline); break;
                // Add other cases as needed
        }
    }
    static public void SendMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient(msg, clientConnectionID, pipeline);
    }

    static public void SendChatMessageToAllClient(string msg, Dictionary<int, Account> activePlayers, TransportPipeline pipeline)
    {
        foreach(int clientConnectionID in activePlayers.Keys)
        {
            SendMessageToClient($"{ServerToClientSignifiers.ChatMessage},{msg}", clientConnectionID, TransportPipeline.ReliableAndInOrder);
        }
    }

    static public void SendChatMessageToClient(string msg, int clientConnectionID, TransportPipeline pipeline)
    {
            SendMessageToClient($"{ServerToClientSignifiers.ChatMessage},{msg}", clientConnectionID, TransportPipeline.ReliableAndInOrder);
    }

    static public void ChangeClientUI(int screenID, int id, TransportPipeline pipeline)
    {
        networkServer.SendMessageToClient($"{ServerToClientSignifiers.ChangeUI},{screenID}", id, pipeline);
    }

    #endregion

    #region Connection Events

    static public void ConnectionEvent(int clientConnectionID)
    {
        networkServer.AddPlayerToLastHeartbeat(clientConnectionID);
        Debug.Log("Client connection, ID == " + clientConnectionID);
    }
    static public void DisconnectionEvent(int clientConnectionID)
    {
        gameRoomsManager.RemovePlayerFromRoom(clientConnectionID);
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
    public const int ChatMessage = 11;
}

static public class ServerToClientSignifiers
{
    public const int ChangeUI = 1;
    public const int StartGame = 7;
    public const int Playing = 8;
    public const int Spectate = 9;
    public const int ChatMessage = 11;
}

#endregion

