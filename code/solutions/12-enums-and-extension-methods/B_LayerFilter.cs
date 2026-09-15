// Exercise 12 B - worked solution.
//
// DESIGN CHOICE: Filter is written once as an ordinary static method and the
// extension method forwards to it, rather than duplicating the loop.
//
// WAS THE EXTENSION METHOD WORTH IT HERE? Marginally. We own SceneObject but not
// List<T>, and the call `objects.OnLayers(mask)` does read better than
// `SceneFilter.Filter(objects, mask)`. It would not be worth it for a type we
// owned, where the method could simply live on the type.
//
// TRYPARSELAYER ACCEPTS A COMBINATION as well as a single member. DECISION: a
// layer mask is meant to combine, so a saved value of Player|Enemy is legitimate
// data rather than corruption. Enum.IsDefined would reject it, because it only
// recognises declared members - so the validation checks that every bit set is
// one we know, rather than that the whole value is a named member.

using System;
using System.Collections.Generic;

namespace Solutions.T12.B
{
    /// <summary>Collision layers, combinable as a set.</summary>
    [Flags]
    public enum Layers
    {
        /// <summary>No layers.</summary>
        None = 0,

        /// <summary>The player.</summary>
        Player = 1 << 0,

        /// <summary>Hostile characters.</summary>
        Enemy = 1 << 1,

        /// <summary>Static level geometry.</summary>
        Scenery = 1 << 2,

        /// <summary>Non-solid trigger volumes.</summary>
        Trigger = 1 << 3,

        /// <summary>Projectiles in flight.</summary>
        Projectile = 1 << 4
    }

    /// <summary>How difficult the game is set to be.</summary>
    public enum Difficulty
    {
        /// <summary>Forgiving.</summary>
        Easy,

        /// <summary>The intended experience.</summary>
        Normal,

        /// <summary>Punishing.</summary>
        Hard
    }

    /// <summary>An object placed in the scene, on exactly one layer.</summary>
    public class SceneObject
    {
        /// <summary>Gets or sets the object's name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the single layer this object sits on.</summary>
        public Layers Layer { get; set; }
    }

    /// <summary>Filtering scene objects by layer.</summary>
    public static class SceneFilter
    {
        private const Layers AllRealLayers =
            Layers.Player | Layers.Enemy | Layers.Scenery | Layers.Trigger | Layers.Projectile;

        /// <summary>Returns every object whose layer is included in the mask.</summary>
        /// <param name="objects">The objects to filter.</param>
        /// <param name="mask">The layers to include.</param>
        /// <returns>A new list of matching objects.</returns>
        public static List<SceneObject> Filter(List<SceneObject> objects, Layers mask)
        {
            List<SceneObject> matches = new List<SceneObject>();

            foreach (SceneObject candidate in objects)
            {
                if ((candidate.Layer & mask) != 0)
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        /// <summary>Returns a mask containing every real layer.</summary>
        /// <returns>All layers except None.</returns>
        public static Layers Everything()
        {
            return AllRealLayers;
        }

        /// <summary>Removes layers from a mask.</summary>
        /// <param name="all">The mask to remove from.</param>
        /// <param name="unwanted">The layers to remove.</param>
        /// <returns>The remaining layers.</returns>
        public static Layers Except(Layers all, Layers unwanted)
        {
            return all & ~unwanted;
        }

        /// <summary>
        /// Attempts to read a layer mask from a stored number. Accepts a
        /// combination of known layers, not only a single declared member.
        /// </summary>
        /// <param name="code">The stored number.</param>
        /// <param name="layer">The mask read, or None when invalid.</param>
        /// <returns>True when every bit set in the number is a known layer.</returns>
        public static bool TryParseLayer(int code, out Layers layer)
        {
            layer = Layers.None;

            if (code == 0)
            {
                return true;                        // None is a legitimate mask
            }

            int known = (int)AllRealLayers;

            if ((code & ~known) != 0)
            {
                return false;                       // a bit we do not recognise
            }

            layer = (Layers)code;
            return true;
        }

        /// <summary>Attempts to read a difficulty from stored text.</summary>
        /// <param name="text">The stored text.</param>
        /// <param name="difficulty">The difficulty read, or Normal when invalid.</param>
        /// <returns>True when the text named a known difficulty.</returns>
        public static bool TryParseDifficulty(string text, out Difficulty difficulty)
        {
            if (Enum.TryParse(text, true, out difficulty) && Enum.IsDefined(typeof(Difficulty), difficulty))
            {
                return true;
            }

            difficulty = Difficulty.Normal;
            return false;
        }
    }

    /// <summary>Reads better at the call site than a static method call.</summary>
    public static class SceneObjectListExtensions
    {
        /// <summary>Returns every object whose layer is included in the mask.</summary>
        /// <param name="objects">The objects to filter.</param>
        /// <param name="mask">The layers to include.</param>
        /// <returns>A new list of matching objects.</returns>
        public static List<SceneObject> OnLayers(this List<SceneObject> objects, Layers mask)
        {
            return SceneFilter.Filter(objects, mask);
        }
    }
}
