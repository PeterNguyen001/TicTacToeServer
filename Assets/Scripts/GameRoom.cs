using System.Collections.Generic;
using UnityEngine;

public class GameRoom
{
    public string name { get; private set; }


    private Dictionary<string, Account> players = new Dictionary<string, Account>();

public GameRoom(Account player, string roomName)
    {
        player.PutPlayerInGameroom(this);
        players.Add(player.username, player);
        name = roomName;
    }

    public void AddPlayer2(Account player)
    {
        player.PutPlayerInGameroom(this);
        players.Add(player.username, player);
    }

    public void RemovePlayer(string username) 
    {
        if (players.ContainsKey(username))
        {
            players.Remove(username);
            Debug.Log("Remove player " + username);
        }
        else
        {
            Debug.Log("Player not found in the room.");
        }
    }

    public Dictionary<string, Account> GetActivePlayers() { return players; }

    public bool IsEmpty() { return players.Count == 0; }

    public bool IsFull() { return players.Count == 2; }

    public bool IsHalfFull() { return players.Count == 1;}
}
