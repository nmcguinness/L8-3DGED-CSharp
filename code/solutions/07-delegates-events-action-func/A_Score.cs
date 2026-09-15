// Exercise 07 A - worked solution.
//
// DESIGN CHOICE: PointsChanged is declared with the `event` keyword and typed as
// Action<int>, and it is raised with null-conditional invocation.
//
// THE ALTERNATIVE - a plain public delegate field - would let any code assign
// with = and wipe every other subscriber, or raise a fake event from outside.
// Both compile. The keyword removes them for everyone except this class, at a
// cost of six characters.

using System;

namespace Solutions.T07.A
{
    /// <summary>A running score that notifies subscribers when it changes.</summary>
    public class Score
    {
        private int _points;

        /// <summary>Raised after the total changes, carrying the new total.</summary>
        public event Action<int> PointsChanged;

        /// <summary>Gets the current total.</summary>
        public int Points
        {
            get { return _points; }
        }

        /// <summary>Adds points to the total and notifies subscribers.</summary>
        /// <param name="amount">Points to add.</param>
        public void Add(int amount)
        {
            _points += amount;
            PointsChanged?.Invoke(_points);
        }
    }
}
