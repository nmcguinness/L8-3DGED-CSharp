// Exercise 04 C - worked solution. One defensible route.
//
// MEASURED FIGURES are produced by TimeBothApproaches below rather than quoted
// here, because they differ by machine and a number written into a comment is
// out of date the moment the hardware changes. On the machine this was written
// on the two approaches were within a few per cent of each other for a struct of
// eight ints, which is the honest answer: at this size, ref is not worth it.
// With three fields it would be further from worth it, not closer.
//
// THE CONTAINER I SETTLED ON: an array, not a List. A List<T> indexer is a
// property returning a copy, so `UpdateByRef(ref list[i])` does not compile at
// all - you cannot take a ref to something that is not a variable. An array
// indexer gives direct access, so the ref version needs an array to exist.
// That constraint, not the timing, is the real finding of this exercise.
//
// A BUG THE REF VERSION ADMITS that the by-value version does not: a method
// taking `ref EntityStats` can change the caller's element at any point, including
// partway through and then throwing, leaving the element half-updated. The
// by-value version computes a complete new value and assigns it in one statement,
// so the element is either fully updated or untouched.
//
// WOULD I MAKE IT A CLASS: no. Eight ints is 32 bytes, has no identity and is
// updated in bulk - a class would mean an allocation per entity and a pointer
// chase per field access, which is worse on both counts for this use.

using System;
using System.Diagnostics;

namespace Solutions.T04.C
{
    /// <summary>A large value type holding an entity's statistics.</summary>
    public struct EntityStats
    {
        /// <summary>The strength score.</summary>
        public int Strength;

        /// <summary>The agility score.</summary>
        public int Agility;

        /// <summary>The endurance score.</summary>
        public int Endurance;

        /// <summary>The intelligence score.</summary>
        public int Intelligence;

        /// <summary>The wisdom score.</summary>
        public int Wisdom;

        /// <summary>The luck score.</summary>
        public int Luck;

        /// <summary>The charisma score.</summary>
        public int Charisma;

        /// <summary>The perception score.</summary>
        public int Perception;

        /// <summary>Gets the sum of every score, used to compare the two approaches.</summary>
        public int Total
        {
            get
            {
                return Strength + Agility + Endurance + Intelligence
                     + Wisdom + Luck + Charisma + Perception;
            }
        }
    }

    /// <summary>Two ways of applying a per-frame update to a set of statistics.</summary>
    public static class StatsUpdater
    {
        /// <summary>Increments every score, working on a copy and returning it.</summary>
        /// <param name="stats">The statistics to base the result on.</param>
        /// <returns>A new set of statistics with every score incremented.</returns>
        public static EntityStats LevelUpByValue(EntityStats stats)
        {
            stats.Strength++;
            stats.Agility++;
            stats.Endurance++;
            stats.Intelligence++;
            stats.Wisdom++;
            stats.Luck++;
            stats.Charisma++;
            stats.Perception++;
            return stats;
        }

        /// <summary>Increments every score in place, without copying the struct.</summary>
        /// <param name="stats">The statistics to modify.</param>
        public static void LevelUpByRef(ref EntityStats stats)
        {
            stats.Strength++;
            stats.Agility++;
            stats.Endurance++;
            stats.Intelligence++;
            stats.Wisdom++;
            stats.Luck++;
            stats.Charisma++;
            stats.Perception++;
        }

        /// <summary>Applies the by-value update to every element of an array.</summary>
        /// <param name="all">The statistics to update.</param>
        public static void UpdateAllByValue(EntityStats[] all)
        {
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = LevelUpByValue(all[i]);    // the write-back is required
            }
        }

        /// <summary>Applies the by-ref update to every element of an array.</summary>
        /// <param name="all">The statistics to update.</param>
        public static void UpdateAllByRef(EntityStats[] all)
        {
            for (int i = 0; i < all.Length; i++)
            {
                LevelUpByRef(ref all[i]);           // legal: an array element IS a variable
            }
        }

        /// <summary>
        /// Times both approaches over the supplied number of entities and returns
        /// the elapsed milliseconds for each.
        /// </summary>
        /// <param name="count">How many entities to update.</param>
        /// <param name="byValueMs">Milliseconds taken by the by-value approach.</param>
        /// <param name="byRefMs">Milliseconds taken by the by-ref approach.</param>
        public static void TimeBothApproaches(int count, out double byValueMs, out double byRefMs)
        {
            EntityStats[] a = new EntityStats[count];
            EntityStats[] b = new EntityStats[count];

            Stopwatch watch = Stopwatch.StartNew();
            UpdateAllByValue(a);
            watch.Stop();
            byValueMs = watch.Elapsed.TotalMilliseconds;

            watch.Restart();
            UpdateAllByRef(b);
            watch.Stop();
            byRefMs = watch.Elapsed.TotalMilliseconds;
        }
    }
}
