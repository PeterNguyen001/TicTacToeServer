using System.Collections.Generic;
using UnityEngine;

public class GameRoom : MonoBehaviour
{
    public string Name { get; private set; }
    public string User1Name { get; private set; }
    public string User2Name { get; private set; }

    public Account activeUser1 { get; private set; }
    public Account activeUser2 { get; private set; }

    public GameRoom(Account user1, string roomName)
    {
        activeUser1 = user1;
        User1Name = user1.name;
        Name = roomName;
    }

    //public void SetUser1(Account user)
    //{
    //    activeUser1 = user;
    //    User1Name = user.name; // Assuming Account has a Username property
    //}

    public void SetUser2(Account user)
    {
        activeUser2 = user;
        User2Name = user.name;
    }

    public Account[] GetActiveUsers()
    {
        return new Account[] { activeUser1, activeUser2 };
    }
}
