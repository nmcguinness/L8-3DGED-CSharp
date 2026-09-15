// Exercise 01 B - worked solution.
//
// DESIGN CHOICE: the named colours are static PROPERTIES returning a new instance
// each time, not static readonly fields holding one shared instance.
//
// THE ALTERNATIVE - a shared field - would cost correctness here, because this
// type is mutable. `ColourRGBA c = ColourRGBA.Red; c.G = 1f;` would recolour
// Red itself for the whole program, and every later reader of ColourRGBA.Red
// would silently get yellow. A property costs one allocation per read and makes
// that bug impossible. Exercise C revisits this by making the type immutable,
// at which point a shared field becomes the better choice.
//
// Brightness is a computed property with no backing field, so it can never
// disagree with the components it is derived from.

using System;

namespace Solutions.T01.B
{
    /// <summary>
    /// A colour with red, green, blue and alpha components, each held in the
    /// range 0 to 1, plus the standard named colours and blending.
    /// </summary>
    public class ColourRGBA
    {
        private const float RedWeight = 0.299f;
        private const float GreenWeight = 0.587f;
        private const float BlueWeight = 0.114f;

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
            Count++;
        }

        /// <summary>Gets how many colours have been constructed.</summary>
        public static int Count { get; private set; }

        /// <summary>Gets opaque red.</summary>
        public static ColourRGBA Red { get { return new ColourRGBA(1f, 0f, 0f, 1f); } }

        /// <summary>Gets opaque green.</summary>
        public static ColourRGBA Green { get { return new ColourRGBA(0f, 1f, 0f, 1f); } }

        /// <summary>Gets opaque blue.</summary>
        public static ColourRGBA Blue { get { return new ColourRGBA(0f, 0f, 1f, 1f); } }

        /// <summary>Gets opaque black.</summary>
        public static ColourRGBA Black { get { return new ColourRGBA(0f, 0f, 0f, 1f); } }

        /// <summary>Gets opaque white.</summary>
        public static ColourRGBA White { get { return new ColourRGBA(1f, 1f, 1f, 1f); } }

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

        /// <summary>
        /// Gets the perceived brightness of this colour, weighted for human vision.
        /// Computed on every read so it cannot drift from the components.
        /// </summary>
        public float Brightness
        {
            get { return (RedWeight * R) + (GreenWeight * G) + (BlueWeight * B); }
        }

        /// <summary>
        /// Blends between two colours.
        /// </summary>
        /// <param name="a">The colour returned when <paramref name="t"/> is 0.</param>
        /// <param name="b">The colour returned when <paramref name="t"/> is 1.</param>
        /// <param name="t">The blend position, clamped to 0 to 1.</param>
        /// <returns>A new blended colour.</returns>
        public static ColourRGBA Lerp(ColourRGBA a, ColourRGBA b, float t)
        {
            float amount = Clamp01(t);

            return new ColourRGBA(
                a.R + ((b.R - a.R) * amount),
                a.G + ((b.G - a.G) * amount),
                a.B + ((b.B - a.B) * amount),
                a.A + ((b.A - a.A) * amount));
        }

        /// <summary>
        /// Returns a new colour with the components flattened to this colour's
        /// brightness, keeping the original alpha. Leaves this colour unchanged.
        /// </summary>
        /// <returns>A new greyscale colour.</returns>
        public ColourRGBA ToGreyscale()
        {
            float grey = Brightness;
            return new ColourRGBA(grey, grey, grey, A);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "RGBA(" + R + ", " + G + ", " + B + ", " + A + ")";
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
