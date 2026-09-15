// Exercise 01 A - worked solution.
//
// DESIGN CHOICE: the four components are private backing fields exposed through
// properties that clamp in the setter, rather than public fields plus a separate
// Validate() method the caller must remember to run.
//
// THE ALTERNATIVE would have been public fields. That costs nothing today and
// everything later: there is no place to put the clamp, so every caller becomes
// responsible for it, and the first one to forget stores an invalid colour that
// is only noticed somewhere else entirely.

using System;

namespace Solutions.T01.A
{
    /// <summary>
    /// A colour with red, green, blue and alpha components, each held in the
    /// range 0 to 1.
    /// </summary>
    public class ColourRGBA
    {
        private float _r;
        private float _g;
        private float _b;
        private float _a;

        /// <summary>Creates opaque white.</summary>
        public ColourRGBA() : this(1f, 1f, 1f, 1f)
        {
        }

        /// <summary>Creates a colour from four components, each clamped to 0 to 1.</summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public ColourRGBA(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>Gets or sets the red component, clamped to 0 to 1.</summary>
        public float R
        {
            get { return _r; }
            set { _r = Clamp01(value); }
        }

        /// <summary>Gets or sets the green component, clamped to 0 to 1.</summary>
        public float G
        {
            get { return _g; }
            set { _g = Clamp01(value); }
        }

        /// <summary>Gets or sets the blue component, clamped to 0 to 1.</summary>
        public float B
        {
            get { return _b; }
            set { _b = Clamp01(value); }
        }

        /// <summary>Gets or sets the alpha component, clamped to 0 to 1.</summary>
        public float A
        {
            get { return _a; }
            set { _a = Clamp01(value); }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "RGBA(" + R + ", " + G + ", " + B + ", " + A + ")";
        }

        // Assigning through the properties in the constructor means the clamp is
        // written once and cannot be bypassed by any route into the object.
        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
