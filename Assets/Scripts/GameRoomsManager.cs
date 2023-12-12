using System.Collections.Generic;
using UnityEngine;

public class GameRoomsManager : MonoBehaviour
{
    private Dictionary<string, GameRoom> gameRooms = new Dictionary<string, GameRoom>();
    private Dictionary<int, Account> activePlayer;

    private const int GameRoomNameSign = 1;
    void Start()
    {

        NetworkServerProcessing.SetGameRoomManager(this);
    }

    public void CreateNewRoom(Account user, string roomName)
    {
        if (!gameRooms.ContainsKey(roomName))
        {
            gameRooms.Add(roomName, new GameRoom(user, roomName));
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameWaitingRoomScreen, user.Id, TransportPipeline.ReliableAndInOrder);
            Debug.Log("Room created successfully.");
        }
        else
        {
            Debug.Log("Room already exists.");
        }
    }

    public bool AddSecondPlayerToRoom(Account player2, string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            if (!room.IsFull())
            {
                room.AddSecondPlayer(player2);
                UpdateUIForAllPlayersInRoom(room);
                return true;
            }
            else
            {
                Debug.Log($"Room {roomName} already has two users.");
            }
        }
        else
        {
            Debug.Log($"Room {roomName} not found.");
        }

        return false;
    }

    public GameRoom CheckForRoomExistence(string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            return room;
        }

        Debug.Log($"Room {roomName} not found.");
        return null;
    }

    public void RemovePlayerFromRoom(int id, string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            room.RemovePlayer(id);
            RemoveRoomIfEmpty(roomName);
        }
        else
        {
            Debug.Log($"Room {roomName} not found.");
        }
    }

    private void RemoveRoomIfEmpty(string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room) && room.IsEmpty())
        {
            gameRooms.Remove(roomName);
            Debug.Log($"Room {roomName} removed because it's empty.");
        }
    }

    private void UpdateUIForAllPlayersInRoom(GameRoom room)
    {
        foreach (KeyValuePair<int, Account> playerEntry in room.GetActivePlayers())
        {
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomScreen, playerEntry.Key, TransportPipeline.ReliableAndInOrder);
        }
    }
    public void JoinOrCreateGame(string[] userData, int clientConnectionID, Account newAccount,TransportPipeline pipeline)
    {




        string gameRoomName = userData[GameRoomNameSign];

        if (CheckForRoomExistence(gameRoomName) == null)
        { CreateNewRoom(newAccount, gameRoomName); }
        else
        {
            AddSecondPlayerToRoom(newAccount, gameRoomName);
        }


        NetworkServerProcessing.SendMessageToClient("Joining " + gameRoomName, clientConnectionID, pipeline);
        Debug.Log("Create Game Room: " + gameRoomName);
    }
}
