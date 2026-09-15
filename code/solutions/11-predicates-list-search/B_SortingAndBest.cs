// Exercise 11 B - worked solution.
//
// DESIGN CHOICE: SortByHealthDescending and SortByName are written in terms of
// SortBy, so the comparison rules are the only thing they contribute and no
// ordering logic is duplicated.
//
// SORT REORDERS THE CALLER'S LIST. SortedCopyBy is provided alongside for callers
// who must not have their list disturbed. THE ALTERNATIVE - always copying -
// would be safer and would allocate a list on every call, including the many
// where the caller does not care.
//
// FINDFASTEST CANNOT BE WRITTEN WITH Find. A Predicate<Player> is handed one
// player and must answer yes or no; deciding which of two is faster needs both at
// once, which is what Comparison<T> is for. It is written as a single pass rather
// than a sort, because sorting does more work than the question requires and
// would reorder the list as a side effect.
//
// AN EMPTY LIST returns null from FindFastest. The alternative - throwing - makes
// every caller wrap the call, for a case that is entirely normal when every
// player has left the match.

using System;
using System.Collections.Generic;

namespace Solutions.T11.B
{
    /// <summary>Sorting by a supplied rule, and finding a single best item.</summary>
    public static class SortingAndBest
    {
        /// <summary>Sorts the players in place by whatever rule is given.</summary>
        /// <param name="players">The list to sort, in place.</param>
        /// <param name="rule">The ordering to apply.</param>
        public static void SortBy(List<T11.A.Player> players, Comparison<T11.A.Player> rule)
        {
            players.Sort(rule);
        }

        /// <summary>Returns a sorted copy, leaving the caller's list untouched.</summary>
        /// <param name="players">The list to copy and sort.</param>
        /// <param name="rule">The ordering to apply.</param>
        /// <returns>A new sorted list.</returns>
        public static List<T11.A.Player> SortedCopyBy(List<T11.A.Player> players, Comparison<T11.A.Player> rule)
        {
            List<T11.A.Player> copy = new List<T11.A.Player>(players);
            copy.Sort(rule);
            return copy;
        }

        /// <summary>Sorts the players in place, highest health first.</summary>
        /// <param name="players">The list to sort.</param>
        public static void SortByHealthDescending(List<T11.A.Player> players)
        {
            // b.CompareTo(a) rather than -a.CompareTo(b): swapping the operands is
            // the correct way to reverse an ordering.
            SortBy(players, (a, b) => b.Health.CompareTo(a.Health));
        }

        /// <summary>Sorts the players in place, alphabetically by name.</summary>
        /// <param name="players">The list to sort.</param>
        public static void SortByName(List<T11.A.Player> players)
        {
            SortBy(players, (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Finds the player with the highest movement speed, in a single pass and
        /// without reordering the list.
        /// </summary>
        /// <param name="players">The players to search.</param>
        /// <returns>The fastest player, or null when the list is empty.</returns>
        public static T11.A.Player FindFastest(List<T11.A.Player> players)
        {
            T11.A.Player best = null;

            for (int i = 0; i < players.Count; i++)
            {
                if (best == null || players[i].MoveSpeed > best.MoveSpeed)
                {
                    best = players[i];
                }
            }

            return best;
        }
    }
}
