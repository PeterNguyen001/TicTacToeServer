using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameRoomsManager : MonoBehaviour
{
    private Dictionary<string, GameRoom> gameRooms = new Dictionary<string, GameRoom>();
    public const string registerType = "2";
    public const string loginType = "3";
    public const string loggedInType = "4";
    public const string waitType = "5";
    public const string gamerType = "6";
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateNewRoom(Account user, string roomName)
    {
        gameRooms.Add(roomName ,new GameRoom(user, roomName));

        if (gameRooms.ContainsKey(roomName))
        {
            Debug.Log("Room created successfully.");
        }
        else
        {
            Debug.Log("Failed to create a room.");
        }
    }

    public bool AddSecondPlayerToRoom(Account player2, string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            if (room.IsHalfFull())
            {
                room.AddPlayer2( player2.id, player2);
                Debug.Log($"User {player2.username} added to room {roomName}.");
                foreach (KeyValuePair<int, Account> playerEntry in room.GetActivePlayers())
                {
                    int id = playerEntry.Key; // This is the integer key
                    NetworkServerProcessing.SendMessageToClient(gamerType.ToString() + ',', id, TransportPipeline.ReliableAndInOrder);
                }
                return true;
            }
            else if (room.IsFull())
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

    public void RemoveRoomIfEmpty(string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            if (room.IsEmpty())
            {
                gameRooms.Remove(roomName);
                Debug.Log($"Room {roomName} removed because it's empty.");
            }
        }
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
}
