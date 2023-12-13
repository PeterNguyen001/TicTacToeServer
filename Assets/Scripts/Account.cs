using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Account
{
    public string username { get; set; }
    public string pass { get; set; }

    public int Id { get; set; }

    public GameRoom roomPlayerIn { get; private set; }

    public Account(string username, string pass)
    {
        this.username = username;
        this.pass = pass;
    }

    public void PutPlayerInGameRoom( GameRoom gameRoom) { roomPlayerIn = gameRoom; }
    public void RemoveRoom() { roomPlayerIn = null; }

    // Overriding the Equals method
    public override bool Equals(object obj)
    {
        var account = obj as Account;
        return account != null && username == account.username;
    }

    // Overriding the GetHashCode method
    public override int GetHashCode()
    {
        return (username != null ? username.GetHashCode() : 0);
    }

    // Overloading the == operator
    public static bool operator ==(Account left, Account right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.username == right.username;
    }

    // Overloading the != operator
    public static bool operator !=(Account left, Account right)
    {
        return !(left == right);
    }

    // Existing code...
}
