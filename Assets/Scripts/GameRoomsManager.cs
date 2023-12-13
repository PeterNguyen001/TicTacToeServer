using System.Collections.Generic;
using UnityEngine;

public class GameRoomsManager : MonoBehaviour
{
    private Dictionary<string, GameRoom> gameRooms = new Dictionary<string, GameRoom>();
    private Dictionary<int, Account> playersUsingRoom = new Dictionary<int, Account>();

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

    public void AddPlayerToRoom(Account player, string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            if (room.IsHalfFull())
            {
                room.AddSecondPlayer(player);
            }
            else if( room.IsFull())
            {
                room.AddSpectator(player);
                Debug.Log($"Room {roomName} already has two users.");
            }
            UpdateUIForAllPlayersInRoom(room);
        }
        else
        {
            Debug.Log($"Room {roomName} not found.");
        }
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

    public void RemovePlayerFromRoom(int id)
    {
        if (playersUsingRoom.ContainsKey(id))
        {
            if (playersUsingRoom[id].roomPlayerIn != null)
            {
                GameRoom room = playersUsingRoom[id].roomPlayerIn;
                room.RemovePlayer(id);
                playersUsingRoom.Remove(id);
                if (room.IsEmpty())
                {
                    gameRooms.Remove(room.Name);
                    Debug.Log($"Room removed because it's empty.");
                }
            }
            else
            {
                Debug.Log($"Room not found.");
            }
        }
    }
    private void UpdateUIForAllPlayersInRoom(GameRoom room)
    {
        foreach (KeyValuePair<int, Account> playerEntry in room.GetActivePlayers())
        {
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomScreen, playerEntry.Key, TransportPipeline.ReliableAndInOrder);
        }
    }
    public void JoinOrCreateGame(string[] userData, int clientConnectionID, Account newAccount)
    {
        string gameRoomName = userData[GameRoomNameSign];

        if (CheckForRoomExistence(gameRoomName) == null)
        { CreateNewRoom(newAccount, gameRoomName); }
        else
        {
            AddPlayerToRoom(newAccount, gameRoomName);
        }
        playersUsingRoom.Add(clientConnectionID, newAccount);

        NetworkServerProcessing.SendMessageToClient("Joining " + gameRoomName, clientConnectionID, TransportPipeline.ReliableAndInOrder);

    }
}
