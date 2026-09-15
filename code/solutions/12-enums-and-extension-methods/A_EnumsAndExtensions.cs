// Exercise 12 A - worked solution.
//
// DESIGN CHOICE: Describe uses a switch with no default case, listing every
// member explicitly. THE ALTERNATIVE - a default catching the rest - is shorter
// and hides the moment a fifth state is added, because the new state silently
// takes the default branch instead of failing visibly. With every case listed,
// adding a member at least leaves an obvious gap for a reader.
//
// DamageType members are powers of two written as shifts, so an out-of-sequence
// value is visible at a glance.
//
// Truncate returns the original string when it is short enough rather than always
// building a new one, because the common case is a string that fits.

using System;

namespace Solutions.T12.A
{
    /// <summary>The phase of play the game is currently in.</summary>
    public enum GameState
    {
        /// <summary>The title screen.</summary>
        MainMenu,

        /// <summary>Active play.</summary>
        Playing,

        /// <summary>Play suspended.</summary>
        Paused,

        /// <summary>Play finished.</summary>
        GameOver
    }

    /// <summary>Kinds of damage, combinable as a set.</summary>
    [Flags]
    public enum DamageType
    {
        /// <summary>No damage type.</summary>
        None = 0,

        /// <summary>Blunt or piercing damage.</summary>
        Physical = 1 << 0,

        /// <summary>Burning damage.</summary>
        Fire = 1 << 1,

        /// <summary>Freezing damage.</summary>
        Ice = 1 << 2,

        /// <summary>Damage over time.</summary>
        Poison = 1 << 3
    }

    /// <summary>Describing game states.</summary>
    public static class GameStates
    {
        /// <summary>Returns a short message for a state.</summary>
        /// <param name="state">The state to describe.</param>
        /// <returns>The message.</returns>
        public static string Describe(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    return "Press Start";

                case GameState.Playing:
                    return "Go";

                case GameState.Paused:
                    return "Paused";

                case GameState.GameOver:
                    return "Game Over";
            }

            return "Unknown";
        }
    }

    /// <summary>Convenience operations on strings.</summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the string unchanged when it fits, otherwise shortens it and
        /// appends an ellipsis.
        /// </summary>
        /// <param name="value">The string to shorten.</param>
        /// <param name="maxLength">The greatest length to keep.</param>
        /// <returns>The original or shortened string.</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength) + "...";
        }
    }
}
