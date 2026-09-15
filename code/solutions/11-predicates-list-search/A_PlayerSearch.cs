// Exercise 11 A - worked solution.
//
// DESIGN CHOICE: the conditions are supplied to the List methods rather than
// written as loops, and two are named methods while two are lambdas, so both
// forms appear.
//
// THE ALTERNATIVE - a hand-written foreach per question - is what these methods
// replace. It is more code per question and puts the condition and the walking
// in the same place, so neither can be reused.
//
// FindById returns null rather than throwing, because a missing id is a normal
// outcome. Player is a class, so null is available; for a value type this would
// have to use FindIndex and -1 instead.

using System;
using System.Collections.Generic;

namespace Solutions.T11.A
{
    /// <summary>A player in a match.</summary>
    public class Player
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the display name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets remaining health.</summary>
        public int Health { get; set; }

        /// <summary>Gets or sets movement speed.</summary>
        public float MoveSpeed { get; set; }

        /// <summary>Gets or sets whether the player is in play.</summary>
        public bool IsActive { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + " (" + Id + ", hp=" + Health + ", spd=" + MoveSpeed + ")";
        }
    }

    /// <summary>Searching a list of players by conditions supplied from outside.</summary>
    public static class PlayerSearch
    {
        /// <summary>A named condition: the player is in play.</summary>
        /// <param name="player">The player to test.</param>
        /// <returns>True when the player is active.</returns>
        public static bool IsActive(Player player)
        {
            return player.IsActive;
        }

        /// <summary>A named condition: the player has lost half their health.</summary>
        /// <param name="player">The player to test.</param>
        /// <returns>True when health is below 50.</returns>
        public static bool IsWounded(Player player)
        {
            return player.Health < 50;
        }

        /// <summary>Returns every active player.</summary>
        /// <param name="players">The players to search.</param>
        /// <returns>The active players, in list order.</returns>
        public static List<Player> FindActive(List<Player> players)
        {
            return players.FindAll(IsActive);           // method group
        }

        /// <summary>Finds the player with the given identifier.</summary>
        /// <param name="players">The players to search.</param>
        /// <param name="id">The identifier to find.</param>
        /// <returns>The matching player, or null when there is none.</returns>
        public static Player FindById(List<Player> players, string id)
        {
            return players.Find(p => p.Id == id);       // lambda
        }

        /// <summary>Counts how many players have lost half their health.</summary>
        /// <param name="players">The players to search.</param>
        /// <returns>The number wounded.</returns>
        public static int CountWounded(List<Player> players)
        {
            int count = 0;

            foreach (Player player in players)
            {
                if (IsWounded(player))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Reports whether at least one player has run out of health.</summary>
        /// <param name="players">The players to search.</param>
        /// <returns>True when any player has zero health.</returns>
        public static bool AnyDead(List<Player> players)
        {
            return players.Exists(p => p.Health == 0);  // lambda
        }

        /// <summary>Removes every inactive player from the list, in place.</summary>
        /// <param name="players">The players to prune.</param>
        /// <returns>How many players were removed.</returns>
        public static int RemoveInactive(List<Player> players)
        {
            return players.RemoveAll(p => !IsActive(p));
        }
    }
}
