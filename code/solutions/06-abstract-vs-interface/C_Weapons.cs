// Exercise 06 C - worked solution. One defensible route.
//
// WHERE TURRET SITS: outside the carried-weapon family, sharing only interfaces.
//
// THE ROUTE I REJECTED was making Turret derive from WeaponBase alongside Pistol
// and Rifle. It would have shared the firing state machine, which is real
// duplication. What it would have made harder: WeaponBase carries ammunition and
// reloading, so Turret would inherit two members that are meaningless for it, and
// the HUD method taking WeaponBase would then accept a turret it must never
// display. The duplication I accepted is about ten lines; the alternative
// weakened every signature in the system.
//
// IF A THROWN GRENADE WERE ADDED NEXT WEEK: it is carried and has a count, so it
// would take IHoldable and IAmmunitionCarrying, but it fires once and is gone, so
// it would not derive from WeaponBase. It needs no change to any existing type -
// which is the property I was buying with the interfaces.
//
// FAMILY AND CAPABILITY: WeaponBase is the family - carried weapons that share a
// firing state machine. IAmmunitionCarrying, IHoldable and IReloadable are
// capabilities, held by types that are otherwise unrelated.

namespace Solutions.T06.C
{
    /// <summary>Something that reports a remaining ammunition count.</summary>
    public interface IAmmunitionCarrying
    {
        /// <summary>Gets the rounds remaining.</summary>
        int Ammunition { get; }
    }

    /// <summary>Something the player holds and triggers.</summary>
    public interface IHoldable
    {
        /// <summary>Begins firing.</summary>
        void StartFiring();

        /// <summary>Stops firing.</summary>
        void StopFiring();
    }

    /// <summary>Something whose ammunition can be replenished and saved.</summary>
    public interface IReloadable
    {
        /// <summary>Refills this weapon to capacity.</summary>
        void Reload();

        /// <summary>Returns the reload state as text.</summary>
        /// <returns>The serialised state.</returns>
        string SaveState();
    }

    /// <summary>
    /// Shared firing state for weapons the player carries. The family; a turret
    /// is deliberately not part of it.
    /// </summary>
    public abstract class WeaponBase : IHoldable
    {
        /// <summary>Gets a value indicating whether the trigger is held.</summary>
        public bool IsFiring { get; private set; }

        /// <summary>Gets how many shots have been fired.</summary>
        public int ShotsFired { get; protected set; }

        /// <inheritdoc />
        public void StartFiring()
        {
            IsFiring = true;
            OnTriggerPulled();
        }

        /// <inheritdoc />
        public void StopFiring()
        {
            IsFiring = false;
        }

        /// <summary>Advances this weapon by one frame while the trigger is held.</summary>
        public void Tick()
        {
            if (IsFiring)
            {
                OnHeld();
            }
        }

        /// <summary>Called once when the trigger is first pulled.</summary>
        protected abstract void OnTriggerPulled();

        /// <summary>Called each frame while the trigger is held.</summary>
        protected virtual void OnHeld()
        {
            // Most weapons do nothing extra while held.
        }
    }

    /// <summary>Fires one shot per trigger pull.</summary>
    public class Pistol : WeaponBase, IAmmunitionCarrying, IReloadable
    {
        private const int Capacity = 12;

        /// <inheritdoc />
        public int Ammunition { get; private set; } = Capacity;

        /// <inheritdoc />
        public void Reload()
        {
            Ammunition = Capacity;
        }

        /// <inheritdoc />
        public string SaveState()
        {
            return "pistol:" + Ammunition;
        }

        /// <inheritdoc />
        protected override void OnTriggerPulled()
        {
            if (Ammunition > 0)
            {
                Ammunition--;
                ShotsFired++;
            }
        }
    }

    /// <summary>Fires continuously while the trigger is held.</summary>
    public class Rifle : WeaponBase, IAmmunitionCarrying, IReloadable
    {
        private const int Capacity = 30;

        /// <inheritdoc />
        public int Ammunition { get; private set; } = Capacity;

        /// <inheritdoc />
        public void Reload()
        {
            Ammunition = Capacity;
        }

        /// <inheritdoc />
        public string SaveState()
        {
            return "rifle:" + Ammunition;
        }

        /// <inheritdoc />
        protected override void OnTriggerPulled()
        {
            Fire();
        }

        /// <inheritdoc />
        protected override void OnHeld()
        {
            Fire();
        }

        private void Fire()
        {
            if (Ammunition > 0)
            {
                Ammunition--;
                ShotsFired++;
            }
        }
    }

    /// <summary>Swings. No ammunition, and nothing to reload.</summary>
    public class Crowbar : WeaponBase
    {
        /// <inheritdoc />
        protected override void OnTriggerPulled()
        {
            ShotsFired++;
        }
    }

    /// <summary>
    /// Placed in the level, acquires its own targets, and has infinite ammunition.
    /// Shares the IHoldable-free part of the world: it is not carried, not
    /// reloaded, and never shown on the HUD.
    /// </summary>
    public class Turret
    {
        /// <summary>Gets how many shots this turret has fired.</summary>
        public int ShotsFired { get; private set; }

        /// <summary>Fires at the turret's current target.</summary>
        public void Fire()
        {
            ShotsFired++;
        }
    }

    /// <summary>The systems that act on weapons.</summary>
    public static class WeaponSystems
    {
        /// <summary>
        /// Reports remaining ammunition. A Crowbar or a Turret cannot be passed.
        /// </summary>
        /// <param name="weapon">The weapon to display.</param>
        /// <returns>The HUD text.</returns>
        public static string DisplayAmmunition(IAmmunitionCarrying weapon)
        {
            return "Ammo: " + weapon.Ammunition;
        }

        /// <summary>
        /// Tells the held weapon to start firing. A Turret cannot be passed.
        /// </summary>
        /// <param name="weapon">The weapon being held.</param>
        public static void PullTrigger(IHoldable weapon)
        {
            weapon.StartFiring();
        }

        /// <summary>
        /// Records the state of anything that can be reloaded. A Crowbar or a
        /// Turret cannot be passed.
        /// </summary>
        /// <param name="weapon">The weapon to record.</param>
        /// <returns>The serialised state.</returns>
        public static string RecordReloadState(IReloadable weapon)
        {
            return weapon.SaveState();
        }
    }
}
