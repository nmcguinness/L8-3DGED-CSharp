// Exercise 04 A - worked solution.
//
// DESIGN CHOICE: ApplyDamage takes health by ref because it both reads and writes
// it; GetStartPosition uses out because it only writes.
//
// THE ALTERNATIVE for ApplyDamage would be to return the new health and have the
// caller assign it - `health = ApplyDamage(health, 75);` - which is the form this
// module prefers in general. ref is used here because the exercise is about the
// keyword; in production the returning form reads better and cannot surprise a
// caller by changing a variable it passed.

namespace Solutions.T04.A
{
    /// <summary>The mechanics of ref and out.</summary>
    public static class RefAndOutBasics
    {
        /// <summary>Reduces health by the damage given, flooring the result at zero.</summary>
        /// <param name="health">The health to reduce. Read and written.</param>
        /// <param name="damage">Points of damage to apply.</param>
        public static void ApplyDamage(ref int health, int damage)
        {
            health -= damage;

            if (health < 0)
            {
                health = 0;
            }
        }

        /// <summary>Supplies the fixed starting position for a level.</summary>
        /// <param name="x">The starting x coordinate. Written only.</param>
        /// <param name="y">The starting y coordinate. Written only.</param>
        public static void GetStartPosition(out int x, out int y)
        {
            x = 10;
            y = 5;
        }
    }
}
