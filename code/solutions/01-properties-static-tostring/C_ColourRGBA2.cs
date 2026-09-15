// Exercise 01 C - worked solution. One defensible route; it is not the only one.
//
// THE USE I DESIGNED FOR: the per-frame working value, not editor data. Colours
// in an engine are produced by the thousand as things fade and blend, and almost
// never edited in place after creation.
//
// MUTABILITY: immutable. Every property is get-only and every operation returns a
// new colour. This prevents the aliasing bug where two systems hold the same
// colour and one adjusts it. What it admits is allocation - every blend makes an
// object - which is the cost I accepted for the use above.
//
// THE NAMED-COLOURS TRAP, in three lines, against the MUTABLE design:
//     ColourRGBA c = ColourRGBA.Red;   // if Red were a shared static field
//     c.G = 1f;                        // legal on a mutable type
//     Console.WriteLine(ColourRGBA.Red);  // now yellow, for the whole program
// Because this version is immutable, line 2 does not compile, so the named
// colours are safely `static readonly` fields sharing one instance each.
//
// CLAMPING lives in the constructor only. With no setters there is nowhere else
// it could go, and nowhere else it needs to go - every instance passes through
// the constructor exactly once.
//
// CLASS OR STRUCT: a struct would suit this better - it is small, has no identity
// and is now immutable, which is the full set of conditions from note 02. It is
// left as a class here so that exercise C of note 02 has something to convert.
//
// IF IT WERE SAVED TO DISK: get-only auto-properties need a constructor the
// serialiser can find, or private setters it can reach. That is the main cost of
// immutability for data that round-trips.

using System;

namespace Solutions.T01.C
{
    /// <summary>
    /// An immutable colour with red, green, blue and alpha components, each held
    /// in the range 0 to 1.
    /// </summary>
    public class ColourRGBA2
    {
        private const float RedWeight = 0.299f;
        private const float GreenWeight = 0.587f;
        private const float BlueWeight = 0.114f;

        /// <summary>Opaque red.</summary>
        public static readonly ColourRGBA2 Red = new ColourRGBA2(1f, 0f, 0f, 1f);

        /// <summary>Opaque green.</summary>
        public static readonly ColourRGBA2 Green = new ColourRGBA2(0f, 1f, 0f, 1f);

        /// <summary>Opaque blue.</summary>
        public static readonly ColourRGBA2 Blue = new ColourRGBA2(0f, 0f, 1f, 1f);

        /// <summary>Opaque black.</summary>
        public static readonly ColourRGBA2 Black = new ColourRGBA2(0f, 0f, 0f, 1f);

        /// <summary>Opaque white.</summary>
        public static readonly ColourRGBA2 White = new ColourRGBA2(1f, 1f, 1f, 1f);

        /// <summary>Creates a colour from four components, each clamped to 0 to 1.</summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        public ColourRGBA2(float r, float g, float b, float a)
        {
            R = Clamp01(r);
            G = Clamp01(g);
            B = Clamp01(b);
            A = Clamp01(a);
        }

        /// <summary>Gets the red component.</summary>
        public float R { get; }

        /// <summary>Gets the green component.</summary>
        public float G { get; }

        /// <summary>Gets the blue component.</summary>
        public float B { get; }

        /// <summary>Gets the alpha component.</summary>
        public float A { get; }

        /// <summary>Gets the perceived brightness of this colour.</summary>
        public float Brightness
        {
            get { return (RedWeight * R) + (GreenWeight * G) + (BlueWeight * B); }
        }

        /// <summary>Blends between two colours.</summary>
        /// <param name="a">The colour returned when <paramref name="t"/> is 0.</param>
        /// <param name="b">The colour returned when <paramref name="t"/> is 1.</param>
        /// <param name="t">The blend position, clamped to 0 to 1.</param>
        /// <returns>A new blended colour.</returns>
        public static ColourRGBA2 Lerp(ColourRGBA2 a, ColourRGBA2 b, float t)
        {
            float amount = Clamp01(t);

            return new ColourRGBA2(
                a.R + ((b.R - a.R) * amount),
                a.G + ((b.G - a.G) * amount),
                a.B + ((b.B - a.B) * amount),
                a.A + ((b.A - a.A) * amount));
        }

        /// <summary>Returns a new greyscale colour, keeping this colour's alpha.</summary>
        /// <returns>A new greyscale colour.</returns>
        public ColourRGBA2 ToGreyscale()
        {
            float grey = Brightness;
            return new ColourRGBA2(grey, grey, grey, A);
        }

        /// <summary>Returns a copy of this colour with a different alpha.</summary>
        /// <param name="alpha">The new alpha component.</param>
        /// <returns>A new colour.</returns>
        public ColourRGBA2 WithAlpha(float alpha)
        {
            return new ColourRGBA2(R, G, B, alpha);
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
