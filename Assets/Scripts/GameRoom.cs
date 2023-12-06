using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameRoom
{
    public string name { get; private set; }

    public const int newGridPositionSign = 1;
    public const int newGridSymbolSign = 2;

    private Dictionary<int, Account> players = new Dictionary<int, Account>();
    private List<string> ticTacToeGrid = Enumerable.Repeat("",9).ToList();
public GameRoom(Account player, string roomName)
    {
        player.PutPlayerInGameroom(this);
        players.Add(player.id, player);
        name = roomName;
        SetPlayerSymbol("X", player.id);
    }

    public void AddPlayer2(int id, Account player)
    {
        player.PutPlayerInGameroom(this);
        players.Add(id, player);
        SetPlayerSymbol("O", player.id);
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

    public void UpdatePlayers(string[] csv, int id) 
    { 
        foreach(Account acc in players.Values) 
        {
            if(acc.id != id) 
            { NetworkServerProcessing.SendMessageToClient(csv[0] + "," + csv[1] + "," + csv[2], acc.id, TransportPipeline.ReliableAndInOrder); }
        }
    }
    public void SetPlayerSymbol(string symbol, int id)
    {
        NetworkServerProcessing.SendMessageToClient(ServerToClientSignifiers.startGame + "," + symbol, id, TransportPipeline.ReliableAndInOrder);
    }
    public Dictionary<int, Account> GetActivePlayers() { return players; }

    public bool IsEmpty() { return players.Count == 0; }

    public bool IsFull() { return players.Count == 2; }

    public bool IsHalfFull() { return players.Count == 1;}
}
