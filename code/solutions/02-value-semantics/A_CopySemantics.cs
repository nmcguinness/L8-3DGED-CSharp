// Exercise 02 A - worked solution.
//
// DESIGN CHOICE: the demonstration is exposed as methods returning values rather
// than printing to the console, so the behaviour can be asserted in a test.
//
// THE ALTERNATIVE - Console.WriteLine throughout - reads more like the exercise
// brief but cannot be checked by anything except a human reading the output.

namespace Solutions.T02.A
{
    /// <summary>A value type. Assignment copies the data.</summary>
    public struct Stats
    {
        /// <summary>The strength score.</summary>
        public int Strength;
    }

    /// <summary>A reference type. Assignment copies the reference.</summary>
    public class Gear
    {
        /// <summary>The armour score.</summary>
        public int Armour;
    }

    /// <summary>Demonstrates the difference between value and reference copying.</summary>
    public static class CopySemantics
    {
        /// <summary>
        /// Assigns a struct to a second variable, modifies the second, and returns
        /// the first. The struct was copied, so the original is untouched.
        /// </summary>
        /// <returns>The original strength, which is 10.</returns>
        public static int StructAssignmentCopies()
        {
            Stats a = new Stats();
            a.Strength = 10;

            Stats b = a;
            b.Strength = 99;

            return a.Strength;
        }

        /// <summary>
        /// Assigns a class reference to a second variable and modifies through it.
        /// Both names refer to one object, so the change is visible from either.
        /// </summary>
        /// <returns>The armour seen through the first reference, which is 99.</returns>
        public static int ClassAssignmentShares()
        {
            Gear g1 = new Gear();
            g1.Armour = 10;

            Gear g2 = g1;
            g2.Armour = 99;

            return g1.Armour;
        }

        /// <summary>
        /// Passes a struct to a method that modifies its parameter. The method
        /// receives a copy, so the caller sees nothing change.
        /// </summary>
        /// <returns>The caller's strength, still 10.</returns>
        public static int PassingAStructPassesACopy()
        {
            Stats stats = new Stats();
            stats.Strength = 10;

            Nudge(stats);

            return stats.Strength;
        }

        // Compiles, runs, warns about nothing, and achieves nothing.
        private static void Nudge(Stats stats)
        {
            stats.Strength += 1;
        }
    }
}
