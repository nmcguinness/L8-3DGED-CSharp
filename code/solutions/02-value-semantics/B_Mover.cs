// Exercise 02 B - worked solution.
//
// DESIGN CHOICE: every Mover method copies the position out, modifies the local,
// and assigns it back. The assignment back is the line that actually moves the
// transform, because it calls the property setter.
//
// THE ALTERNATIVE would be to construct a new Vector3 inline:
//     transform.Position = new Vector3(5f, transform.Position.Y, transform.Position.Z);
// That is correct and one line shorter, but it reads the property three times and
// names every component even when only one is changing, so a mistake in an
// unrelated component is easy to miss. The three-line form is used here because
// the exercise is about understanding why the copy is necessary.
//
// `transform.Position.Y = 5f;` does not compile: Position is a property, so the
// getter returns a COPY, and a copy has no permanent home to assign into.
//
// PART TWO: the array version is markedly easier. An array indexer gives direct
// access to the element, so `points[i].Y = y;` compiles. A List<T> indexer is a
// property that returns a copy, so the element has to be taken out, changed and
// put back.

using System;
using System.Collections.Generic;

namespace Solutions.T02.B
{
    /// <summary>Stands in for UnityEngine.Vector3. A struct, so it copies.</summary>
    public struct Vector3
    {
        /// <summary>The x component.</summary>
        public float X;

        /// <summary>The y component.</summary>
        public float Y;

        /// <summary>The z component.</summary>
        public float Z;

        /// <summary>Creates a vector from its components.</summary>
        /// <param name="x">The x component.</param>
        /// <param name="y">The y component.</param>
        /// <param name="z">The z component.</param>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
    }

    /// <summary>Stands in for UnityEngine.Transform. Position is a property.</summary>
    public class Transform
    {
        private Vector3 _position;

        /// <summary>Gets how many times the setter has run.</summary>
        public int MoveCount { get; private set; }

        /// <summary>Gets or sets the world position.</summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                MoveCount++;
            }
        }
    }

    /// <summary>Moves transforms, working around the copy a property getter returns.</summary>
    public static class Mover
    {
        /// <summary>Sets the world y coordinate, leaving x and z unchanged.</summary>
        /// <param name="transform">The transform to move.</param>
        /// <param name="y">The new y coordinate.</param>
        public static void SetHeight(Transform transform, float y)
        {
            Vector3 position = transform.Position;      // copy out
            position.Y = y;                             // modify the copy
            transform.Position = position;              // copy back - this moves it
        }

        /// <summary>Adds an offset to the current position.</summary>
        /// <param name="transform">The transform to move.</param>
        /// <param name="offset">The offset to add.</param>
        public static void Nudge(Transform transform, Vector3 offset)
        {
            Vector3 position = transform.Position;
            position.X += offset.X;
            position.Y += offset.Y;
            position.Z += offset.Z;
            transform.Position = position;
        }

        /// <summary>Sets the y coordinate to zero, leaving x and z unchanged.</summary>
        /// <param name="transform">The transform to flatten.</param>
        public static void Flatten(Transform transform)
        {
            SetHeight(transform, 0f);
        }

        /// <summary>
        /// Sets the y coordinate on every element of a list. The list indexer is a
        /// property, so each element must be taken out, changed and put back.
        /// </summary>
        /// <param name="points">The points to raise.</param>
        /// <param name="y">The new y coordinate.</param>
        public static void RaiseAll(List<Vector3> points, float y)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                point.Y = y;
                points[i] = point;
            }
        }

        /// <summary>
        /// Sets the y coordinate on every element of an array. An array indexer
        /// gives direct access, so the element can be modified in place.
        /// </summary>
        /// <param name="points">The points to raise.</param>
        /// <param name="y">The new y coordinate.</param>
        public static void RaiseAll(Vector3[] points, float y)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].Y = y;
            }
        }
    }

    /// <summary>Extension methods wrapping the copy-out-modify-assign-back dance.</summary>
    public static class TransformExtensions
    {
        /// <summary>Sets the world y coordinate, leaving x and z unchanged.</summary>
        /// <param name="transform">The transform to move.</param>
        /// <param name="y">The new y coordinate.</param>
        public static void SetHeight(this Transform transform, float y)
        {
            Mover.SetHeight(transform, y);
        }

        /// <summary>Adds an offset to the current position.</summary>
        /// <param name="transform">The transform to move.</param>
        /// <param name="offset">The offset to add.</param>
        public static void Nudge(this Transform transform, Vector3 offset)
        {
            Mover.Nudge(transform, offset);
        }

        /// <summary>Sets the y coordinate to zero.</summary>
        /// <param name="transform">The transform to flatten.</param>
        public static void Flatten(this Transform transform)
        {
            Mover.Flatten(transform);
        }
    }
}
