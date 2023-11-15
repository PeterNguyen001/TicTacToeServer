using System.Collections.Generic;
using UnityEngine;

public class GameRoom
{
    public string name { get; private set; }

    public Account activePlayer1 { get; private set; }
    public Account activePlayer2 { get; private set; }

    public GameRoom(Account user1, string roomName)
    {
        activePlayer1 = user1;
        name = roomName;
    }

    public void AddPlayer2(Account user)
    {
        activePlayer2 = user;
    }

    //public LinkedList<Account> GetActivePlayers()
    //{
    //    return new LinkedList<Account> { activePlayer1, activePlayer2 };
    //}
}
