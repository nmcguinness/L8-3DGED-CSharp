// Exercise 13 B - worked solution.
//
// DESIGN CHOICE: the divergence is exposed as a record of results rather than
// printed, so a test can assert that == and is null genuinely disagree.
//
// THE NULL CHECKS INSIDE operator == use ReferenceEquals. Writing `a == null`
// there would call the operator being defined and recurse until the stack ended.
//
// WHY fallback.Name IS THE ORIGINAL, NOT THE REPLACEMENT: ?? compiles to a direct
// reference comparison, which sees a live reference, so the right-hand side is
// never evaluated and the destroyed object is returned.

using System;

namespace Solutions.T13.B
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

        /// <summary>Gets a value indicating whether this object has been destroyed.</summary>
        public bool IsDestroyed
        {
            get { return _isDestroyed; }
        }

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
        /// <returns>True when both read as absent, or both are the same live object.</returns>
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
            return Name == null ? 0 : Name.GetHashCode();
        }
    }

    /// <summary>What each null operator reports for one reference.</summary>
    public class NullCheckResults
    {
        /// <summary>Gets or sets the result of the == comparison.</summary>
        public bool EqualsNull { get; set; }

        /// <summary>Gets or sets the result of the != comparison.</summary>
        public bool NotEqualsNull { get; set; }

        /// <summary>Gets or sets the result of the is null pattern.</summary>
        public bool IsNullPattern { get; set; }

        /// <summary>Gets or sets the result of ReferenceEquals against null.</summary>
        public bool ReferenceEqualsNull { get; set; }

        /// <summary>Gets or sets the result of the null-conditional name read.</summary>
        public string NameViaNullConditional { get; set; }
    }

    /// <summary>Demonstrating which operators respect the == overload.</summary>
    public static class NullDivergence
    {
        /// <summary>Records what every null operator reports for one reference.</summary>
        /// <param name="candidate">The reference to inspect.</param>
        /// <returns>The results of each operator.</returns>
        public static NullCheckResults Inspect(EngineObject candidate)
        {
            return new NullCheckResults
            {
                EqualsNull = candidate == null,
                NotEqualsNull = candidate != null,
                IsNullPattern = candidate is null,
                ReferenceEqualsNull = ReferenceEquals(candidate, null),
                NameViaNullConditional = candidate?.Name ?? "no name"
            };
        }

        /// <summary>
        /// Chooses between a candidate and a fallback using ??, which bypasses the
        /// overload and so returns a destroyed candidate.
        /// </summary>
        /// <param name="candidate">The preferred object.</param>
        /// <param name="fallback">The object to use when the candidate is absent.</param>
        /// <returns>Whichever object ?? selects.</returns>
        public static EngineObject ChooseWithNullCoalescing(EngineObject candidate, EngineObject fallback)
        {
            return candidate ?? fallback;
        }

        /// <summary>
        /// Chooses between a candidate and a fallback using ==, which runs the
        /// overload and so correctly rejects a destroyed candidate.
        /// </summary>
        /// <param name="candidate">The preferred object.</param>
        /// <param name="fallback">The object to use when the candidate is absent.</param>
        /// <returns>The candidate when it is usable, otherwise the fallback.</returns>
        public static EngineObject ChooseSafely(EngineObject candidate, EngineObject fallback)
        {
            return candidate != null ? candidate : fallback;
        }

        /// <summary>Uses a reference through ?., which throws for a destroyed object.</summary>
        /// <param name="candidate">The object to use.</param>
        /// <returns>The work description, or null for a genuinely null reference.</returns>
        public static string UnsafeUse(EngineObject candidate)
        {
            return candidate?.DoWork();
        }

        /// <summary>Uses a reference through !=, which is safe for all three cases.</summary>
        /// <param name="candidate">The object to use.</param>
        /// <returns>The work description, or a message when the object is absent.</returns>
        public static string SafeUse(EngineObject candidate)
        {
            if (candidate != null)
            {
                return candidate.DoWork();
            }

            return "(skipped - null or destroyed)";
        }
    }
}
