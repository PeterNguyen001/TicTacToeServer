using System.Collections.Generic;
using UnityEngine;

public class GameRoom
{
    public string name { get; private set; }


    private Dictionary<int, Account> players = new Dictionary<int, Account>();

public GameRoom(Account player, string roomName)
    {
        player.PutPlayerInGameroom(this);
        players.Add(player.id, player);
        name = roomName;
    }

    public void AddPlayer2(int id, Account player)
    {
        player.PutPlayerInGameroom(this);
        players.Add(id, player);
    }

    public void RemovePlayer(int id) 
    {
        if (players.ContainsKey(id))
        {
            players.Remove(id);
            Debug.Log("Remove player " + id);
        }
        else
        {
            Debug.Log("Player not found in the room.");
        }
    }

    public Dictionary<int, Account> GetActivePlayers() { return players; }

    public bool IsEmpty() { return players.Count == 0; }

    public bool IsFull() { return players.Count == 2; }

    public bool IsHalfFull() { return players.Count == 1;}
}
