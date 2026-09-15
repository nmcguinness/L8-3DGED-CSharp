// Exercise 05 B - worked solution.
//
// DESIGN CHOICE: the four responsibilities of the original IGameEntity are split
// into four independent contracts, and each type declares only the ones it can
// honour. No implementation contains NotImplementedException.
//
// THE ALTERNATIVE - keeping one large interface and throwing for the members a
// type cannot support - compiles, and moves the failure from compile time to run
// time on a code path nobody tests. ApplySplashDamage would accept a Checkpoint
// and blow up in the field rather than in the editor.
//
// Each system method takes the narrowest contract that does its job, so the
// compiler rather than a comment enforces which types may be passed.

using System.Collections.Generic;

namespace Solutions.T05.B
{
    /// <summary>A target that can receive damage.</summary>
    public interface IDamageable
    {
        /// <summary>Applies damage to this target.</summary>
        /// <param name="amount">Points of damage to apply.</param>
        void TakeDamage(int amount);
    }

    /// <summary>A target whose condition can be restored.</summary>
    public interface IRepairable
    {
        /// <summary>Restores condition to this target.</summary>
        /// <param name="amount">Points of condition to restore.</param>
        void Repair(int amount);
    }

    /// <summary>Something that can change position.</summary>
    public interface IMovable
    {
        /// <summary>Moves this object to the given coordinates.</summary>
        /// <param name="x">The destination x coordinate.</param>
        /// <param name="y">The destination y coordinate.</param>
        /// <param name="z">The destination z coordinate.</param>
        void Move(float x, float y, float z);
    }

    /// <summary>Something whose state can be written out.</summary>
    public interface ISaveable
    {
        /// <summary>Returns this object's state as text.</summary>
        /// <returns>The serialised state.</returns>
        string Save();
    }

    /// <summary>A crate: damageable and repairable, but it neither moves nor saves.</summary>
    public class Crate : IDamageable, IRepairable
    {
        /// <summary>Gets the remaining integrity.</summary>
        public int Integrity { get; private set; } = 100;

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            Integrity -= amount;
            if (Integrity < 0)
            {
                Integrity = 0;
            }
        }

        /// <inheritdoc />
        public void Repair(int amount)
        {
            Integrity += amount;
        }
    }

    /// <summary>A guard: damageable, mobile and saved, but never repaired.</summary>
    public class Guard : IDamageable, IMovable, ISaveable
    {
        /// <summary>Gets the remaining health.</summary>
        public int Health { get; private set; } = 60;

        /// <summary>Gets how many times this guard has moved.</summary>
        public int MoveCount { get; private set; }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0)
            {
                Health = 0;
            }
        }

        /// <inheritdoc />
        public void Move(float x, float y, float z)
        {
            MoveCount++;
        }

        /// <inheritdoc />
        public string Save()
        {
            return "guard:" + Health;
        }
    }

    /// <summary>A checkpoint: saved, and nothing else.</summary>
    public class Checkpoint : ISaveable
    {
        /// <summary>Gets or sets the checkpoint index.</summary>
        public int Index { get; set; }

        /// <inheritdoc />
        public string Save()
        {
            return "checkpoint:" + Index;
        }
    }

    /// <summary>The systems that act on the contracts above.</summary>
    public static class Systems
    {
        /// <summary>Damages every target given. A Checkpoint cannot be passed here.</summary>
        /// <param name="targets">The targets to damage.</param>
        /// <param name="amount">Points of damage to apply to each.</param>
        public static void ApplySplashDamage(List<IDamageable> targets, int amount)
        {
            foreach (IDamageable target in targets)
            {
                target.TakeDamage(amount);
            }
        }

        /// <summary>Saves every item given. A Crate cannot be passed here.</summary>
        /// <param name="items">The items to save.</param>
        /// <returns>One serialised string per item.</returns>
        public static List<string> SaveAll(List<ISaveable> items)
        {
            List<string> lines = new List<string>();

            foreach (ISaveable item in items)
            {
                lines.Add(item.Save());
            }

            return lines;
        }
    }
}
