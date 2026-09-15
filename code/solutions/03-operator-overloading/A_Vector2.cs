// Exercise 03 A - worked solution.
//
// DESIGN CHOICE: every operator returns a new Vector2 and modifies neither operand.
// `a + b` reads as arithmetic, so it must behave like arithmetic.
//
// THE ALTERNATIVE - mutating the left operand and returning it - is shorter by a
// line and silently changes `a` at every call site. On a struct the damage is
// contained because the operand was a copy; on a class it would be a side effect
// nobody reading `c = a + b` would look for.
//
// Both (Vector2, float) and (float, Vector2) overloads of * exist because
// operator overloads are NOT symmetric. Declaring only the first means `2f * v`
// does not compile.

namespace Solutions.T03.A
{
    /// <summary>A two-component vector supporting arithmetic.</summary>
    public struct Vector2
    {
        /// <summary>Creates a vector from its components.</summary>
        /// <param name="x">The horizontal component.</param>
        /// <param name="y">The vertical component.</param>
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Gets the horizontal component.</summary>
        public float X { get; }

        /// <summary>Gets the vertical component.</summary>
        public float Y { get; }

        /// <summary>Adds two vectors component-wise.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>A new vector holding the sum.</returns>
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        /// <summary>Subtracts one vector from another component-wise.</summary>
        /// <param name="a">The left operand.</param>
        /// <param name="b">The right operand.</param>
        /// <returns>A new vector holding the difference.</returns>
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        /// <summary>Scales a vector by a number.</summary>
        /// <param name="v">The vector to scale.</param>
        /// <param name="scale">The factor to scale by.</param>
        /// <returns>A new scaled vector.</returns>
        public static Vector2 operator *(Vector2 v, float scale)
        {
            return new Vector2(v.X * scale, v.Y * scale);
        }

        /// <summary>Scales a vector by a number, with the number on the left.</summary>
        /// <param name="scale">The factor to scale by.</param>
        /// <param name="v">The vector to scale.</param>
        /// <returns>A new scaled vector.</returns>
        public static Vector2 operator *(float scale, Vector2 v)
        {
            return v * scale;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}
