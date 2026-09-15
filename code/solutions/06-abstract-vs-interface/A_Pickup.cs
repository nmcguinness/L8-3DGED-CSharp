// Exercise 06 A - worked solution.
//
// DESIGN CHOICE: Collect is non-virtual, so the shared line always runs and
// always runs before the subclass gets control. Only OnCollected is open.
//
// THE ALTERNATIVE - making Collect virtual so a subclass could "customise" it -
// hands every subclass the ability to forget the shared behaviour, which is the
// exact duplication the base class was introduced to remove.

using System.Collections.Generic;

namespace Solutions.T06.A
{
    /// <summary>Something the player can collect.</summary>
    public abstract class PickupBase
    {
        /// <summary>The amount this pickup grants.</summary>
        protected readonly int _amount;

        /// <summary>Creates a pickup granting the given amount.</summary>
        /// <param name="amount">The amount granted on collection.</param>
        protected PickupBase(int amount)
        {
            _amount = amount;
        }

        /// <summary>
        /// Collects this pickup. Not virtual: the shared line is a guarantee, not
        /// a default.
        /// </summary>
        /// <returns>The lines describing what happened, shared line first.</returns>
        public List<string> Collect()
        {
            List<string> lines = new List<string>();
            lines.Add("Collected " + _amount + ".");
            lines.Add(OnCollected());
            return lines;
        }

        /// <summary>Describes what this particular pickup does when collected.</summary>
        /// <returns>A description of the specific effect.</returns>
        protected abstract string OnCollected();
    }

    /// <summary>A pickup restoring health.</summary>
    public class HealthPickup : PickupBase
    {
        /// <summary>Creates a health pickup.</summary>
        /// <param name="amount">The health restored.</param>
        public HealthPickup(int amount) : base(amount)
        {
        }

        /// <inheritdoc />
        protected override string OnCollected()
        {
            return "Health restored.";
        }
    }

    /// <summary>A pickup adding ammunition.</summary>
    public class AmmoPickup : PickupBase
    {
        /// <summary>Creates an ammunition pickup.</summary>
        /// <param name="amount">The rounds added.</param>
        public AmmoPickup(int amount) : base(amount)
        {
        }

        /// <inheritdoc />
        protected override string OnCollected()
        {
            return "Ammunition loaded.";
        }
    }
}
