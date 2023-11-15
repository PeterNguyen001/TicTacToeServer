using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomsManager : MonoBehaviour
{
    private Dictionary<string, GameRoom> gameRooms = new Dictionary<string, GameRoom>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateRoom(Account user, string roomName)
    {


        gameRooms.Add(roomName ,new GameRoom(user, roomName));

        //if (gameRooms.Count > 0)
        //{
        //    Debug.Log("Room created successfully.");
        //}
        //else
        //{
        //    Debug.Log("Failed to create a room.");
        //}


    }

    public bool AddSecondPlayerToRoom(Account user2, string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            if (room.activePlayer2 == null)
            {
                room.AddPlayer2(user2);
                Debug.Log($"User {user2.username} added to room {roomName}.");
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
    public GameRoom GetRoom(string roomName)
    {
        if (gameRooms.TryGetValue(roomName, out GameRoom room))
        {
            return room;
        }

        Debug.Log($"Room {roomName} not found.");
        return null;
    }
}
