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
//
// Enemy keeps its settable properties so an object initialiser still works, and
// gains a constructor that builds one in a single line. The rules hold no state,
// so they need no constructor; each still overrides ToString, because a log line
// saying which rule a turret is running is the first thing you want when a
// turret shoots the wrong thing.

using System.Collections.Generic;

namespace Solutions.T05.C
{
    /// <summary>An enemy a turret may consider shooting.</summary>
    public class Enemy
    {
        /// <summary>Creates an unnamed enemy at full health, on top of the turret.</summary>
        public Enemy() : this("enemy", 100, 0f, 0f)
        {
        }

        /// <summary>Creates an enemy with every value set.</summary>
        /// <param name="name">The enemy's display name.</param>
        /// <param name="health">Remaining health.</param>
        /// <param name="distanceFromTurret">The distance from the turret.</param>
        /// <param name="secondsSinceItAttackedUs">How long since this enemy attacked the turret.</param>
        public Enemy(string name, int health, float distanceFromTurret, float secondsSinceItAttackedUs)
        {
            Name = name;
            Health = health;
            DistanceFromTurret = distanceFromTurret;
            SecondsSinceItAttackedUs = secondsSinceItAttackedUs;
        }

        /// <summary>Gets or sets the enemy's display name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets remaining health.</summary>
        public int Health { get; set; }

        /// <summary>Gets or sets the distance from the turret.</summary>
        public float DistanceFromTurret { get; set; }

        /// <summary>Gets or sets how long since this enemy attacked the turret.</summary>
        public float SecondsSinceItAttackedUs { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Enemy(" + Name + ", health=" + Health
                + ", distance=" + DistanceFromTurret
                + ", lastAttack=" + SecondsSinceItAttackedUs + "s)";
        }
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

        /// <inheritdoc />
        public override string ToString()
        {
            return "NearestRule";
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

        /// <inheritdoc />
        public override string ToString()
        {
            return "WeakestRule";
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

        /// <inheritdoc />
        public override string ToString()
        {
            return "MostRecentAttackerRule";
        }
    }

    /// <summary>
    /// A turret that engages a target chosen by a rule it knows nothing about.
    /// </summary>
    public class Turret
    {
        /// <summary>Creates an unnamed turret using the supplied rule.</summary>
        /// <param name="rule">The rule deciding which enemy to engage.</param>
        public Turret(ITargetingRule rule) : this("turret", rule)
        {
        }

        /// <summary>Creates a named turret using the supplied rule.</summary>
        /// <param name="name">The name used when this turret is printed.</param>
        /// <param name="rule">The rule deciding which enemy to engage.</param>
        public Turret(string name, ITargetingRule rule)
        {
            Name = name;
            Rule = rule;
        }

        /// <summary>Gets or sets the turret's display name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the rule deciding which enemy to engage.</summary>
        public ITargetingRule Rule { get; set; }

        /// <summary>Chooses a target from the candidates, or null to hold fire.</summary>
        /// <param name="candidates">The enemies currently in range.</param>
        /// <returns>The chosen enemy, or null.</returns>
        public Enemy SelectTarget(List<Enemy> candidates)
        {
            return Rule.Choose(candidates);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Turret(" + Name + ", rule=" + Rule + ")";
        }
    }
}
