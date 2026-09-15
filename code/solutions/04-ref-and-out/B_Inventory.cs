// Exercise 04 B - worked solution.
//
// DESIGN CHOICE: TryGetItem returns bool and supplies the item through an out
// parameter, assigning it on every path including failure.
//
// THE ALTERNATIVE - returning the item and using null for "not found" - is
// shorter, and it works here because Item is a reference type. It stops working
// the moment the stored type is a value type, where there is no null to return,
// and it lets a caller ignore the failure case without the compiler noticing.
// The Try shape puts the outcome in the return value, which is hard to ignore.
//
// ResolveHit takes one ref and one out, which is a shape worth being suspicious
// of - it means the method does two things. It is written this way because the
// exercise asks for it; a Health class with a TakeDamage method and an IsDead
// property would express the same behaviour better.

using System.Collections.Generic;

namespace Solutions.T04.B
{
    /// <summary>An item that can be held in an inventory slot.</summary>
    public class Item
    {
        /// <summary>Creates an item.</summary>
        /// <param name="name">The item's display name.</param>
        public Item(string name)
        {
            Name = name;
        }

        /// <summary>Gets the item's display name.</summary>
        public string Name { get; }
    }

    /// <summary>Holds items against named slots.</summary>
    public class Inventory
    {
        private readonly Dictionary<string, Item> _slots = new Dictionary<string, Item>();

        /// <summary>Places an item in a slot, replacing anything already there.</summary>
        /// <param name="slot">The slot name.</param>
        /// <param name="item">The item to place.</param>
        public void Place(string slot, Item item)
        {
            _slots[slot] = item;
        }

        /// <summary>
        /// Attempts to read the item in a slot. Never throws: an empty slot is a
        /// normal outcome rather than an error.
        /// </summary>
        /// <param name="slot">The slot name to read.</param>
        /// <param name="item">The item found, or null when the slot is empty.</param>
        /// <returns>True when the slot held an item.</returns>
        public bool TryGetItem(string slot, out Item item)
        {
            // TryGetValue already assigns item on both paths, which is exactly the
            // guarantee out requires. Forwarding it keeps that guarantee intact.
            return _slots.TryGetValue(slot, out item);
        }
    }

    /// <summary>Resolving a hit against a health value.</summary>
    public static class Combat
    {
        /// <summary>Applies damage and reports whether the target died.</summary>
        /// <param name="health">The health to reduce. Read and written.</param>
        /// <param name="damage">Points of damage to apply.</param>
        /// <param name="isDead">True when health reached zero. Written only.</param>
        public static void ResolveHit(ref int health, int damage, out bool isDead)
        {
            health -= damage;

            if (health < 0)
            {
                health = 0;
            }

            isDead = health == 0;
        }
    }
}
