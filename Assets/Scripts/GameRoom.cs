using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoom : MonoBehaviour
{
    private string name { get; set; }
    private string user1Name { get; set; }
    private string user2Name { get; set; }

    private LinkedList<Account> aciveUsers;

    public GameRoom(string userI,string roomName )
    {
        user1Name = userI;
        name = roomName;
    }
    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetUser1Name(string newUserName)
    {
        // You can set the property like this.
        user1Name = newUserName;
    }

    public string GetUser1Name()
    {
        // You can get the property like this.
        return user1Name;
    }

    public void SetUser2Name(string newUserName)
    {
        // You can set the property like this.
        user2Name = newUserName;
    }

    public string GetUser2Name()
    {
        // You can get the property like this.
        return user2Name;
    }
    public void SetroomName(string newUserName)
    {
        // You can set the property like this.
        name = newUserName;
    }

    public string GetRoomName()
    {
        // You can get the property like this.
        return name;
    }

    public LinkedList<Account> GetAciveUsers()
    { return aciveUsers; }
}
