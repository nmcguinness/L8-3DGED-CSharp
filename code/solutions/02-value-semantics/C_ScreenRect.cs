// Exercise 02 C - worked solution. One defensible route.
//
// WHICH I WOULD SHIP: the struct, for a screen rectangle. It is sixteen bytes,
// has no identity, and is created constantly during layout. That is the full set
// of conditions for a value type.
//
// MUTATE OR RETURN: return a new region. Inflate and Translate produce a new
// ScreenRect rather than changing this one.
//
// THE PAIRING I REJECTED AS INCOHERENT: mutable struct. A mutable struct is the
// worst of both - it copies silently, so `rect.Inflate(5)` through a property or
// a List indexer modifies a temporary and the caller sees nothing, with no
// warning. Of the four pairings, mutable-struct is the only one that is actively
// a trap; the other three are merely trade-offs.
//
// WHERE THE TWO VERSIONS DISAGREE, as numbered steps:
//   1. Create a region of (0, 0, 100, 100).
//   2. Hand the same region to two pieces of code, A and B.
//   3. A calls MutateInflate(10) on its copy.
//   4. B reads its Width.
//   With the CLASS, B sees 120 - A's change reached it, because both hold one
//   object. With the STRUCT, B sees 100 - B got a copy at step 2.
//   The immutable design makes step 3 impossible, which is why it is preferred.
//
// IF STORED IN A LIST AND EDITED IN PLACE: `rects[0].Left = 5;` on the struct
// version does not compile - the List indexer is a property returning a copy, so
// the compiler refuses to modify a value that is not a variable. The class
// version compiles and works. That single difference is the strongest practical
// argument for making a struct immutable.

namespace Solutions.T02.C
{
    /// <summary>
    /// A rectangular region of the screen. Immutable: every operation returns a
    /// new region rather than modifying this one.
    /// </summary>
    public readonly struct ScreenRectStruct
    {
        /// <summary>Creates a region from its four edges.</summary>
        /// <param name="left">The left edge.</param>
        /// <param name="top">The top edge.</param>
        /// <param name="right">The right edge.</param>
        /// <param name="bottom">The bottom edge.</param>
        public ScreenRectStruct(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>Gets the left edge.</summary>
        public int Left { get; }

        /// <summary>Gets the top edge.</summary>
        public int Top { get; }

        /// <summary>Gets the right edge.</summary>
        public int Right { get; }

        /// <summary>Gets the bottom edge.</summary>
        public int Bottom { get; }

        /// <summary>Gets the width, computed from the edges.</summary>
        public int Width { get { return Right - Left; } }

        /// <summary>Gets the height, computed from the edges.</summary>
        public int Height { get { return Bottom - Top; } }

        /// <summary>Reports whether a point lies inside this region.</summary>
        /// <param name="x">The point's x coordinate.</param>
        /// <param name="y">The point's y coordinate.</param>
        /// <returns>True when the point is inside or on the boundary.</returns>
        public bool Contains(int x, int y)
        {
            return x >= Left && x <= Right && y >= Top && y <= Bottom;
        }

        /// <summary>Returns a region grown by the given amount on every edge.</summary>
        /// <param name="by">How far to grow each edge.</param>
        /// <returns>A new region.</returns>
        public ScreenRectStruct Inflate(int by)
        {
            return new ScreenRectStruct(Left - by, Top - by, Right + by, Bottom + by);
        }

        /// <summary>Returns a region moved by the given offset.</summary>
        /// <param name="dx">The horizontal offset.</param>
        /// <param name="dy">The vertical offset.</param>
        /// <returns>A new region.</returns>
        public ScreenRectStruct Translate(int dx, int dy)
        {
            return new ScreenRectStruct(Left + dx, Top + dy, Right + dx, Bottom + dy);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Rect(Left=" + Left + ", Top=" + Top + ", Right=" + Right + ", Bottom=" + Bottom + ")";
        }
    }

    /// <summary>
    /// The same region as a reference type, with a deliberately mutating operation
    /// so that the difference in aliasing behaviour can be demonstrated.
    /// </summary>
    public class ScreenRectClass
    {
        /// <summary>Creates a region from its four edges.</summary>
        /// <param name="left">The left edge.</param>
        /// <param name="top">The top edge.</param>
        /// <param name="right">The right edge.</param>
        /// <param name="bottom">The bottom edge.</param>
        public ScreenRectClass(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>Gets or sets the left edge.</summary>
        public int Left { get; set; }

        /// <summary>Gets or sets the top edge.</summary>
        public int Top { get; set; }

        /// <summary>Gets or sets the right edge.</summary>
        public int Right { get; set; }

        /// <summary>Gets or sets the bottom edge.</summary>
        public int Bottom { get; set; }

        /// <summary>Gets the width, computed from the edges.</summary>
        public int Width { get { return Right - Left; } }

        /// <summary>Gets the height, computed from the edges.</summary>
        public int Height { get { return Bottom - Top; } }

        /// <summary>Reports whether a point lies inside this region.</summary>
        /// <param name="x">The point's x coordinate.</param>
        /// <param name="y">The point's y coordinate.</param>
        /// <returns>True when the point is inside or on the boundary.</returns>
        public bool Contains(int x, int y)
        {
            return x >= Left && x <= Right && y >= Top && y <= Bottom;
        }

        /// <summary>Grows this region in place. Every holder of it sees the change.</summary>
        /// <param name="by">How far to grow each edge.</param>
        public void MutateInflate(int by)
        {
            Left -= by;
            Top -= by;
            Right += by;
            Bottom += by;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Rect(Left=" + Left + ", Top=" + Top + ", Right=" + Right + ", Bottom=" + Bottom + ")";
        }
    }
}
