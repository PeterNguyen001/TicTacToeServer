using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameRoom
{
    public string Name { get; private set; }
    private Dictionary<int, Account> players = new Dictionary<int, Account>();
    private List<string> ticTacToeGrid = Enumerable.Repeat(string.Empty, 9).ToList();

    public GameRoom(Account player, string roomName)
    {
        Name = roomName;
        AddPlayer(player, "X");
    }

    public void AddPlayer(Account player, string symbol)
    {
        player.PutPlayerInGameRoom(this);
        players.Add(player.Id, player);
        NotifyPlayerOfSymbol(symbol, player.Id);
    }

    public void AddSecondPlayer(Account player)
    {
        AddPlayer(player, "O");
    }

    public void AddSpectator(Account player)
    {
        AddPlayer(player, "Y");

    }

    public void RemovePlayer(int id)
    {
        if (players.ContainsKey(id))
        {
            players[id].RemoveRoom();
            players.Remove(id);          
            NetworkServerProcessing.ChangeClientUI(ScreenID.GameRoomBrowserScreen, id, TransportPipeline.ReliableAndInOrder);
            Debug.Log("Remove player " + id);
        }
        else
        {
            Debug.Log("Player not found in the room.");
        }
    }

    public void UpdatePlayers(string[] csv, int id)
    {
        string message = string.Join(",", csv);
        foreach (Account account in players.Values.Where(acc => acc.Id != id))
        {
            NetworkServerProcessing.SendMessageToClient(message, account.Id, TransportPipeline.ReliableAndInOrder);
        }
    }

    private void NotifyPlayerOfSymbol(string symbol, int id)
    {
        NetworkServerProcessing.SendMessageToClient($"{ServerToClientSignifiers.StartGame},{symbol}", id, TransportPipeline.ReliableAndInOrder);
    }

    public Dictionary<int, Account> GetActivePlayers() => players;

    public bool IsEmpty() => players.Count == 0;

    public bool IsFull() => players.Count == 2;

    public bool IsHalfFull() => players.Count == 1;
}
