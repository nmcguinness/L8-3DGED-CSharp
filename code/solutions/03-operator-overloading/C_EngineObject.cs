// Exercise 03 C - worked solution. One defensible route.
//
// THE FOUR PAIRS, and which I had a real choice about:
//   destroyed vs null                 -> TRUE. Forced; this is the whole point.
//   destroyed vs the SAME object      -> TRUE. A real choice. Unity reports this
//                                        as equal-to-null on both sides, so two
//                                        "nulls" compare equal. I follow Unity.
//   destroyed vs a DIFFERENT destroyed-> TRUE. A real choice, and the more
//                                        surprising one: both sides read as null,
//                                        so they are equal. Defensible only
//                                        because it keeps == transitive with null.
//   two distinct live objects         -> FALSE. Forced; reference identity.
//
// GETHASHCODE is derived from Name alone and deliberately NOT from _isDestroyed.
// If the hash changed on destruction, an object already used as a dictionary key
// would move bucket and become permanently unreachable - including by the code
// trying to remove it.
//
// WHY ?. AND ?? IGNORE THIS OPERATOR: the compiler emits a direct reference
// comparison for them - the same check ReferenceEquals performs - rather than a
// call to op_Equality. The overload is never consulted, so a destroyed object
// looks perfectly alive to both.
//
// WHICH I WOULD RATHER REVIEW: SafeUse. UnsafeUse throws, which at least arrives
// with a stack trace pointing at the line. The ?? form produces no exception at
// all and hands back a destroyed object that travels onwards, so the symptom
// appears somewhere unrelated. Silent wrong answers are harder to diagnose than
// loud crashes.

using System;

namespace Solutions.T03.C
{
    /// <summary>
    /// Stands in for UnityEngine.Object. A destroyed instance reports as equal to
    /// null through ==, while remaining a live reference.
    /// </summary>
    public class EngineObject
    {
        private bool _isDestroyed;

        /// <summary>Gets or sets the name used in diagnostics.</summary>
        public string Name { get; set; }

        /// <summary>Marks this object as destroyed.</summary>
        public void Destroy()
        {
            _isDestroyed = true;
        }

        /// <summary>Performs work that requires the underlying object to exist.</summary>
        /// <returns>A short description of the work done.</returns>
        public string DoWork()
        {
            if (_isDestroyed)
            {
                throw new InvalidOperationException(
                    "The object of type EngineObject has been destroyed but you are still trying to access it.");
            }

            return Name + " did some work.";
        }

        /// <summary>Reports whether two references are both absent, or the same object.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>True when both read as null, or both are the same live object.</returns>
        public static bool operator ==(EngineObject a, EngineObject b)
        {
            bool aIsNull = ReferenceEquals(a, null) || a._isDestroyed;
            bool bIsNull = ReferenceEquals(b, null) || b._isDestroyed;

            if (aIsNull && bIsNull)
            {
                return true;
            }

            if (aIsNull || bIsNull)
            {
                return false;
            }

            return ReferenceEquals(a, b);
        }

        /// <summary>The inverse of the equality operator.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>True when the two are not considered equal.</returns>
        public static bool operator !=(EngineObject a, EngineObject b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            return this == other as EngineObject;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // Deliberately independent of _isDestroyed, so an object already used
            // as a dictionary key stays findable after destruction.
            return Name == null ? 0 : Name.GetHashCode();
        }
    }

    /// <summary>Two ways of using a reference that may have been destroyed.</summary>
    public static class NullHandling
    {
        /// <summary>
        /// Uses the null-conditional operator, which bypasses the == overload and
        /// therefore throws for a destroyed object.
        /// </summary>
        /// <param name="o">The object to use.</param>
        /// <returns>The work description, or null when the reference is genuinely null.</returns>
        public static string UnsafeUse(EngineObject o)
        {
            return o?.DoWork();
        }

        /// <summary>
        /// Uses an explicit comparison, which runs the == overload and therefore
        /// treats a destroyed object as absent.
        /// </summary>
        /// <param name="o">The object to use.</param>
        /// <returns>The work description, or a message when the object is absent.</returns>
        public static string SafeUse(EngineObject o)
        {
            if (o != null)
            {
                return o.DoWork();
            }

            return "(skipped - null or destroyed)";
        }
    }
}
