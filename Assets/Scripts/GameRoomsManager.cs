using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomsManager : MonoBehaviour
{
    [SerializeField]
    LinkedList<GameRoom> gameRooms;
    [SerializeField]
    GameObject roomList;

    // Start is called before the first frame update
    void Start()
    {
        gameRooms = new LinkedList<GameRoom>(); // Initialize the LinkedList.
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateRoom(Account user, string gameRoomName)
    {
        //GameObject newGameRoom = new GameObject(gameRoomName);
        //GameRoom gameRoomScript = newGameRoom.AddComponent<GameRoom>();
        //gameRoomScript.SetUser1(user);

        //gameRooms.AddLast(gameRoomScript); // Add to the end of the linked list.

        gameRooms.AddLast(new GameRoom(user, gameRoomName));

        if (gameRooms.Count > 0 && gameRooms.First.Value != null)
        {
            Debug.Log("Room created successfully.");
        }
        else
        {
            Debug.Log("Failed to create a room.");
        }

        //newGameRoom.transform.SetParent(roomList.transform);
    }
}
