using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Account : MonoBehaviour
{
    // Start is called before the first frame update
    public string userID { get; set; }
    public string pass { get; set; }

    private GameRoom inGameRoom { get; set; }

    public Account(string userID, string pass)
    {
        this.userID = userID;
        this.pass = pass;
    }

    public void PutUserInGameRoom(GameRoom GameRoom)
    { inGameRoom = GameRoom; }
    public GameRoom GetGameRoomUserIn()
    { return inGameRoom; }
    public override bool Equals(object obj) => this.Equals(obj as Account);

    public bool Equals(Account a)
    {
        if (a is null)
        {
            return false;
        }

        // Optimization for a common success case.
        if (Object.ReferenceEquals(this, a))
        {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (this.GetType() != a.GetType())
        {
            return false;
        }

        // Return true if the fields match.
        // Note that the base class is not invoked because it is
        // System.Object, which defines Equals as reference equality.
        return (userID == a.userID) && (pass == a.pass);
    }
    public override int GetHashCode() => (userID, pass).GetHashCode();
    public static bool operator ==(Account lhs, Account rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }
    public static bool operator !=(Account lhs, Account rhs) => !(lhs == rhs);

}