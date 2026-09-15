// Exercise 12 C - worked solution. One defensible route.
//
// THE FOUR CALL SITES I WANTED, written before the API:
//     scene.Query(Layers.Enemy).All();
//     scene.Query(Layers.Enemy).Within(origin, 10f).All();
//     scene.Query(Layers.Enemy).Within(origin, 10f).Nearest(origin);
//     scene.Query(Layers.Enemy).Excluding(self).All();
//
// SHAPE CHOSEN: the chained builder. REJECTED: several narrowly named methods.
//
// WHAT THE REJECTED SHAPE WOULD COST WHEN A FIFTH CRITERION ARRIVES: the named
// methods do not combine, so every useful subset needs its own overload. Four
// criteria already implies up to fifteen combinations; a fifth roughly doubles
// that. The builder needs one new method and every existing combination keeps
// working.
//
// EMPTY RESULT: an empty list from All, and null from Nearest. This forces every
// caller of Nearest to check for null, which is the honest cost - there genuinely
// may be nothing nearby, and a caller that ignores that is wrong.
//
// NOT SAFE EVERY FRAME: Query and everything chained from it. Each call allocates
// a builder and a result list. NearestOnLayerNonAllocating is the frame-safe
// alternative: one manual pass, no builder, no list, and it gives up composition
// entirely - it does exactly one thing and cannot be extended by chaining.

using System;
using System.Collections.Generic;

namespace Solutions.T12.C
{
    /// <summary>A point in the scene.</summary>
    public struct Point
    {
        /// <summary>Creates a point.</summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Gets the x coordinate.</summary>
        public float X { get; }

        /// <summary>Gets the y coordinate.</summary>
        public float Y { get; }

        /// <summary>Returns the squared distance to another point.</summary>
        /// <param name="other">The point to measure to.</param>
        /// <returns>The squared distance.</returns>
        public float SquaredDistanceTo(Point other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            return (dx * dx) + (dy * dy);
        }
    }

    /// <summary>An object placed in the scene.</summary>
    public class SceneObject
    {
        /// <summary>Gets or sets the object's name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the layer this object sits on.</summary>
        public T12.B.Layers Layer { get; set; }

        /// <summary>Gets or sets the object's position.</summary>
        public Point Position { get; set; }
    }

    /// <summary>The set of objects currently in the scene.</summary>
    public class Scene
    {
        /// <summary>Gets the objects in the scene.</summary>
        public List<SceneObject> Objects { get; } = new List<SceneObject>();

        /// <summary>
        /// Begins a query. Allocates, so it must not be called from a per-frame
        /// path - use NearestOnLayerNonAllocating there instead.
        /// </summary>
        /// <param name="mask">The layers to include.</param>
        /// <returns>A query that can be narrowed further.</returns>
        public SceneQuery Query(T12.B.Layers mask)
        {
            return new SceneQuery(Objects, mask);
        }

        /// <summary>
        /// Finds the nearest object on a layer, allocating nothing. Safe to call
        /// every frame. Gives up composition entirely: it cannot be narrowed by a
        /// radius or an exclusion without a new method.
        /// </summary>
        /// <param name="mask">The layers to include.</param>
        /// <param name="origin">The point to measure from.</param>
        /// <returns>The nearest matching object, or null.</returns>
        public SceneObject NearestOnLayerNonAllocating(T12.B.Layers mask, Point origin)
        {
            SceneObject best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < Objects.Count; i++)
            {
                SceneObject candidate = Objects[i];

                if ((candidate.Layer & mask) == 0)
                {
                    continue;
                }

                float distance = candidate.Position.SquaredDistanceTo(origin);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// A query being built up. Each method narrows the criteria and returns the
    /// query, so calls chain.
    /// </summary>
    public class SceneQuery
    {
        private readonly List<SceneObject> _source;
        private readonly T12.B.Layers _mask;
        private Point _centre;
        private float _radiusSquared = float.MaxValue;
        private SceneObject _excluded;

        internal SceneQuery(List<SceneObject> source, T12.B.Layers mask)
        {
            _source = source;
            _mask = mask;
        }

        /// <summary>Narrows the query to objects within a radius of a point.</summary>
        /// <param name="centre">The point to measure from.</param>
        /// <param name="radius">The greatest distance to include.</param>
        /// <returns>This query, for chaining.</returns>
        public SceneQuery Within(Point centre, float radius)
        {
            _centre = centre;
            _radiusSquared = radius * radius;
            return this;
        }

        /// <summary>Narrows the query to exclude one object, usually the asker.</summary>
        /// <param name="self">The object to leave out.</param>
        /// <returns>This query, for chaining.</returns>
        public SceneQuery Excluding(SceneObject self)
        {
            _excluded = self;
            return this;
        }

        /// <summary>Runs the query and returns every match.</summary>
        /// <returns>A new list of matching objects, empty when none match.</returns>
        public List<SceneObject> All()
        {
            List<SceneObject> matches = new List<SceneObject>();

            foreach (SceneObject candidate in _source)
            {
                if (Matches(candidate))
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        /// <summary>Runs the query and returns the match nearest a point.</summary>
        /// <param name="origin">The point to measure from.</param>
        /// <returns>The nearest match, or null when nothing matches.</returns>
        public SceneObject Nearest(Point origin)
        {
            SceneObject best = null;
            float bestDistance = float.MaxValue;

            foreach (SceneObject candidate in _source)
            {
                if (!Matches(candidate))
                {
                    continue;
                }

                float distance = candidate.Position.SquaredDistanceTo(origin);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private bool Matches(SceneObject candidate)
        {
            if ((candidate.Layer & _mask) == 0)
            {
                return false;
            }

            if (ReferenceEquals(candidate, _excluded))
            {
                return false;
            }

            if (_radiusSquared < float.MaxValue
                && candidate.Position.SquaredDistanceTo(_centre) > _radiusSquared)
            {
                return false;
            }

            return true;
        }
    }
}
