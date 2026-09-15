// Exercise 03 B - worked solution.
//
// DESIGN CHOICE: all four members are supplied together - ==, !=, Equals and
// GetHashCode - and GetHashCode is derived from exactly the field == compares.
//
// THE ALTERNATIVE - overloading only == and != - compiles with two warnings and
// then loses items in hash-based collections. `a == b` reports true while
// `dictionary.ContainsKey(b)` reports false, because the dictionary hashes first
// and never reaches the operator. That failure produces no exception, which is
// what makes it expensive to find.
//
// Value is get-only on purpose. A mutable key changes its hash after insertion,
// so the entry is filed under an address nothing will look at again.
//
// The null checks use ReferenceEquals, not ==. Writing `a == null` inside
// operator == calls the operator being defined and recurses until the stack ends.

using System;

namespace Solutions.T03.B
{
    /// <summary>An item identifier compared by value rather than by reference.</summary>
    public class ItemId
    {
        /// <summary>Creates an identifier.</summary>
        /// <param name="value">The underlying numeric identifier.</param>
        public ItemId(int value)
        {
            Value = value;
        }

        /// <summary>Gets the underlying numeric identifier.</summary>
        public int Value { get; }

        /// <summary>Reports whether two identifiers refer to the same item.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>True when both are null, or both hold the same value.</returns>
        public static bool operator ==(ItemId a, ItemId b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;                    // same object, or both null
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;                   // exactly one is null
            }

            return a.Value == b.Value;
        }

        /// <summary>Reports whether two identifiers refer to different items.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>True when the two are not equal.</returns>
        public static bool operator !=(ItemId a, ItemId b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return this == other as ItemId;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value.GetHashCode();         // the same field == compares
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Item#" + Value;
        }
    }
}
