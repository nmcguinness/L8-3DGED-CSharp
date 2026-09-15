// Exercise 08 A - worked solution.
//
// DESIGN CHOICE: every lambda uses the shortest form that compiles - no parameter
// types where they can be inferred, no braces around a single expression, no
// return where the expression body supplies it.
//
// THE ALTERNATIVE - writing `(int value) => { return value * 2; }` throughout -
// is legal and equivalent. It is noise once the delegate type is known, and it
// obscures the one case where braces genuinely matter: a body of several
// statements, which does need an explicit return.
//
// IsLow captures the _threshold FIELD, not its value, so changing the field
// changes what the lambda decides. That is the whole point of the exercise.

using System;

namespace Solutions.T08.A
{
    /// <summary>The four lambda shapes, and live capture.</summary>
    public class LambdaShapes
    {
        private int _threshold = 50;

        /// <summary>Gets or sets the threshold the low-value test compares against.</summary>
        public int Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>Gets a lambda taking no arguments and returning nothing.</summary>
        /// <returns>An action that records that it ran.</returns>
        public Action MakeReset()
        {
            return () => LastAction = "reset";
        }

        /// <summary>Gets a lambda taking one argument and returning nothing.</summary>
        /// <returns>An action that records its argument.</returns>
        public Action<string> MakeLog()
        {
            return message => LastAction = message;
        }

        /// <summary>Gets a lambda taking one argument and returning a value.</summary>
        /// <returns>A function doubling its argument.</returns>
        public Func<int, int> MakeDouble()
        {
            return value => value * 2;
        }

        /// <summary>
        /// Gets a lambda that captures the threshold field. It reads the current
        /// value each time it runs, not the value at the moment it was created.
        /// </summary>
        /// <returns>A function reporting whether a value is below the threshold.</returns>
        public Func<int, bool> MakeIsLow()
        {
            return value => value < _threshold;
        }

        /// <summary>Gets the last action recorded by one of the lambdas above.</summary>
        public string LastAction { get; private set; }
    }
}
