// Exercise 11 C - worked solution. One defensible route.
//
// REPRESENTATION CHOSEN: a Predicate<Player> passed in at each call. Rejected: a
// small IQuery interface with an IsMatch method.
//
// WHY: a predicate is already a first-class value, so a designer adding a
// question writes one lambda rather than a class. The interface would buy the
// ability to name and store a query, which matters when queries are configured in
// an editor rather than written in code - a real consideration once
// ScriptableObjects arrive, but not for this scoreboard.
//
// COMBINING TWO CONDITIONS: an And helper taking two predicates and returning a
// third. That scales to three or four by nesting - And(a, And(b, c)) - which
// reads worse the deeper it goes. At four conditions I would switch to a params
// array overload; at that point the interface version starts to look better,
// because a query object can hold a list of children.
//
// SORT-IN-PLACE DECISION: the scoreboard never sorts its own list. Every ordering
// question returns a sorted copy. THE INTERFERENCE BUG THIS AVOIDS: two callers
// asking different questions in the same frame would otherwise leave the list in
// whichever order the second one wanted, so the first caller's indices - or a UI
// holding a row number - would silently point at the wrong player.
//
// LINQ AGAINST HAND-WRITTEN: both versions are provided and a test asserts they
// agree. The LINQ version is shorter and allocates an iterator per operator plus
// the lambda; the hand-written version allocates one list. I would refuse the
// LINQ version anywhere reached from Update, which is the rule from note 10.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Solutions.T11.C
{
    /// <summary>Answers questions about a set of players without knowing the questions.</summary>
    public class Scoreboard
    {
        private readonly List<T11.A.Player> _players;

        /// <summary>Creates a scoreboard over the supplied players.</summary>
        /// <param name="players">The players to report on.</param>
        public Scoreboard(List<T11.A.Player> players)
        {
            _players = players;
        }

        /// <summary>Gets how many players the scoreboard holds.</summary>
        public int Count
        {
            get { return _players.Count; }
        }

        /// <summary>Combines two conditions into one that requires both.</summary>
        /// <param name="first">The first condition.</param>
        /// <param name="second">The second condition.</param>
        /// <returns>A condition true only when both are true.</returns>
        public static Predicate<T11.A.Player> And(
            Predicate<T11.A.Player> first, Predicate<T11.A.Player> second)
        {
            return player => first(player) && second(player);
        }

        /// <summary>Combines two conditions into one that requires either.</summary>
        /// <param name="first">The first condition.</param>
        /// <param name="second">The second condition.</param>
        /// <returns>A condition true when either is true.</returns>
        public static Predicate<T11.A.Player> Or(
            Predicate<T11.A.Player> first, Predicate<T11.A.Player> second)
        {
            return player => first(player) || second(player);
        }

        /// <summary>Returns every player matching a condition.</summary>
        /// <param name="condition">The condition to apply.</param>
        /// <returns>A new list of matching players.</returns>
        public List<T11.A.Player> Where(Predicate<T11.A.Player> condition)
        {
            return _players.FindAll(condition);
        }

        /// <summary>
        /// Returns the players ordered by a rule, as a new list. The scoreboard's
        /// own list is never reordered.
        /// </summary>
        /// <param name="rule">The ordering to apply.</param>
        /// <returns>A new sorted list.</returns>
        public List<T11.A.Player> Ordered(Comparison<T11.A.Player> rule)
        {
            List<T11.A.Player> copy = new List<T11.A.Player>(_players);
            copy.Sort(rule);
            return copy;
        }

        /// <summary>Returns the highest ranked players by a rule, hand-written.</summary>
        /// <param name="rule">The ordering to apply, best first.</param>
        /// <param name="count">How many to return.</param>
        /// <returns>Up to <paramref name="count"/> players.</returns>
        public List<T11.A.Player> Top(Comparison<T11.A.Player> rule, int count)
        {
            List<T11.A.Player> ordered = Ordered(rule);
            List<T11.A.Player> top = new List<T11.A.Player>();

            for (int i = 0; i < ordered.Count && i < count; i++)
            {
                top.Add(ordered[i]);
            }

            return top;
        }

        /// <summary>
        /// The same question written with LINQ. Equivalent, shorter, and not for
        /// use in a per-frame path.
        /// </summary>
        /// <param name="rule">The key to order by, descending.</param>
        /// <param name="count">How many to return.</param>
        /// <returns>Up to <paramref name="count"/> players.</returns>
        public List<T11.A.Player> TopWithLinq(Func<T11.A.Player, int> rule, int count)
        {
            return _players.OrderByDescending(rule).Take(count).ToList();
        }

        /// <summary>Finds the single best player by a rule, in one pass.</summary>
        /// <param name="isBetter">Returns true when the first player beats the second.</param>
        /// <returns>The best player, or null when there are none.</returns>
        public T11.A.Player Best(Func<T11.A.Player, T11.A.Player, bool> isBetter)
        {
            T11.A.Player best = null;

            for (int i = 0; i < _players.Count; i++)
            {
                if (best == null || isBetter(_players[i], best))
                {
                    best = _players[i];
                }
            }

            return best;
        }
    }
}
