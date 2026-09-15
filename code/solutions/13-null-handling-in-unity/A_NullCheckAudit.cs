// Exercise 13 A - worked solution.
//
// The exercise is a reading task, so the answers are recorded as data rather than
// as prose, which lets a test assert that the four unsafe lines are identified.
//
// THE SINGLE RULE that decides every case: on anything deriving from
// UnityEngine.Object, use == and != only, because every other null operator
// bypasses the overloaded == and cannot see a destroyed object.

using System.Collections.Generic;

namespace Solutions.T13.A
{
    /// <summary>One line of the audited listing.</summary>
    public class AuditedLine
    {
        /// <summary>Creates an audit entry.</summary>
        /// <param name="number">The line number in the listing.</param>
        /// <param name="code">The line of code.</param>
        /// <param name="isSafe">Whether the line is safe as written.</param>
        /// <param name="reason">What happens, and the fix when unsafe.</param>
        public AuditedLine(int number, string code, bool isSafe, string reason)
        {
            Number = number;
            Code = code;
            IsSafe = isSafe;
            Reason = reason;
        }

        /// <summary>Gets the line number.</summary>
        public int Number { get; }

        /// <summary>Gets the line of code.</summary>
        public string Code { get; }

        /// <summary>Gets a value indicating whether the line is safe.</summary>
        public bool IsSafe { get; }

        /// <summary>Gets the explanation.</summary>
        public string Reason { get; }
    }

    /// <summary>The worked audit of the ten-line listing.</summary>
    public static class NullCheckAudit
    {
        /// <summary>The single rule that decides every line.</summary>
        public const string Rule =
            "On a UnityEngine.Object use == or != only; every other null operator "
            + "compiles to a direct reference comparison and cannot see a destroyed object.";

        /// <summary>Returns the audit of all ten lines.</summary>
        /// <returns>One entry per line, in listing order.</returns>
        public static List<AuditedLine> Audit()
        {
            return new List<AuditedLine>
            {
                new AuditedLine(1, "if (_currentTarget != null) { Chase(_currentTarget); }", true,
                    "Engine object compared with !=, so Unity's overload runs and a destroyed target is skipped."),

                new AuditedLine(2, "_healthBar?.Refresh(_health);", false,
                    "?. on an engine object bypasses the overload. A destroyed bar is called and throws "
                    + "MissingReferenceException. Fix: if (_healthBar != null) { _healthBar.Refresh(_health); }"),

                new AuditedLine(3, "_inventory ??= new Inventory();", true,
                    "Inventory is a plain C# class, so ??= behaves normally."),

                new AuditedLine(4, "Enemy target = _lastSeen ?? FindNearest();", false,
                    "?? on an engine object bypasses the overload, so a destroyed _lastSeen is returned and "
                    + "FindNearest is never called. No exception, which makes it worse. "
                    + "Fix: Enemy target = _lastSeen != null ? _lastSeen : FindNearest();"),

                new AuditedLine(5, "if (_saveData is null) { _saveData = new SaveData(); }", true,
                    "SaveData is a plain C# class, so is null is a correct reference test."),

                new AuditedLine(6, "if (_muzzleTransform == null) { Debug.LogError(\"Muzzle not assigned\"); }", true,
                    "Engine object compared with ==, which is exactly the intended use."),

                new AuditedLine(7, "string weaponName = _inventory?.Equipped?.Name ?? \"unarmed\";", true,
                    "Every link in the chain is a plain C# type, so the null-conditional operators are correct."),

                new AuditedLine(8, "if (_currentTarget is not null) { Chase(_currentTarget); }", false,
                    "is not null on an engine object bypasses the overload, so a destroyed target reads as "
                    + "present and Chase throws. Fix: if (_currentTarget != null)."),

                new AuditedLine(9, "transform.parent?.SendMessage(\"Hit\");", false,
                    "parent is a Transform, an engine object, so ?. bypasses the overload. "
                    + "Fix: Transform parent = transform.parent; if (parent != null) { parent.SendMessage(\"Hit\"); }"),

                new AuditedLine(10, "_onDeath?.Invoke();", true,
                    "A delegate, not an engine object. A delegate with no subscribers really is null, so "
                    + "?.Invoke is the correct and required form.")
            };
        }

        /// <summary>Returns the line numbers that are unsafe as written.</summary>
        /// <returns>The unsafe line numbers, in order.</returns>
        public static List<int> UnsafeLineNumbers()
        {
            List<int> unsafeLines = new List<int>();

            foreach (AuditedLine line in Audit())
            {
                if (!line.IsSafe)
                {
                    unsafeLines.Add(line.Number);
                }
            }

            return unsafeLines;
        }
    }
}
