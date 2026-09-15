// Exercise 05 C - worked solution. One defensible route.
//
// THE DESIGN I CHOSE: a rule receives the whole candidate list and returns one
// enemy, rather than scoring a single candidate and letting the turret compare.
//
// THE ALTERNATIVE - Func<Enemy, float> scoring one candidate at a time - is
// tidier for rules that are a simple maximum, and the turret would own the
// comparison loop. It cannot express a rule that depends on the set as a whole,
// such as "the enemy furthest from the others", and adding one later would mean
// changing every existing rule's signature. Whole-list is the more expensive
// contract to implement and the cheaper one to extend, which is the trade I want
// for something a designer will add to.
//
// AN EMPTY CANDIDATE LIST returns null, and the turret treats null as "hold
// fire". The alternative - throwing - would make every rule responsible for a
// case that is entirely normal in play, and would put a try/catch in Update.

using System.Collections.Generic;

namespace Solutions.T05.C
{
    /// <summary>An enemy a turret may consider shooting.</summary>
    public class Enemy
    {
        /// <summary>Gets or sets the enemy's display name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets remaining health.</summary>
        public int Health { get; set; }

        /// <summary>Gets or sets the distance from the turret.</summary>
        public float DistanceFromTurret { get; set; }

        /// <summary>Gets or sets how long since this enemy attacked the turret.</summary>
        public float SecondsSinceItAttackedUs { get; set; }
    }

    /// <summary>A rule for choosing which enemy a turret should engage.</summary>
    public interface ITargetingRule
    {
        /// <summary>Chooses one enemy from the candidates.</summary>
        /// <param name="candidates">The enemies currently in range.</param>
        /// <returns>The chosen enemy, or null when there is nothing to shoot.</returns>
        Enemy Choose(List<Enemy> candidates);
    }

    /// <summary>Chooses the enemy closest to the turret.</summary>
    public class NearestRule : ITargetingRule
    {
        /// <inheritdoc />
        public Enemy Choose(List<Enemy> candidates)
        {
            Enemy best = null;

            foreach (Enemy candidate in candidates)
            {
                if (best == null || candidate.DistanceFromTurret < best.DistanceFromTurret)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }

    /// <summary>Chooses the enemy with the least health remaining.</summary>
    public class WeakestRule : ITargetingRule
    {
        /// <inheritdoc />
        public Enemy Choose(List<Enemy> candidates)
        {
            Enemy best = null;

            foreach (Enemy candidate in candidates)
            {
                if (best == null || candidate.Health < best.Health)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }

    /// <summary>Chooses the enemy that attacked the turret most recently.</summary>
    public class MostRecentAttackerRule : ITargetingRule
    {
        /// <inheritdoc />
        public Enemy Choose(List<Enemy> candidates)
        {
            Enemy best = null;

            foreach (Enemy candidate in candidates)
            {
                if (best == null || candidate.SecondsSinceItAttackedUs < best.SecondsSinceItAttackedUs)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// A turret that engages a target chosen by a rule it knows nothing about.
    /// </summary>
    public class Turret
    {
        /// <summary>Creates a turret using the supplied rule.</summary>
        /// <param name="rule">The rule deciding which enemy to engage.</param>
        public Turret(ITargetingRule rule)
        {
            Rule = rule;
        }

        /// <summary>Gets or sets the rule deciding which enemy to engage.</summary>
        public ITargetingRule Rule { get; set; }

        /// <summary>Chooses a target from the candidates, or null to hold fire.</summary>
        /// <param name="candidates">The enemies currently in range.</param>
        /// <returns>The chosen enemy, or null.</returns>
        public Enemy SelectTarget(List<Enemy> candidates)
        {
            return Rule.Choose(candidates);
        }
    }
}
