// Tests for the note 06 B and C solutions.

using System.Collections.Generic;
using Xunit;
using B = Solutions.T06.B;
using C = Solutions.T06.C;

namespace Week01.Tests
{
    public class T06_Saveable_B_Tests
    {
        [Fact]
        public void ANewProfile_IsNotDirty()
        {
            Assert.False(new B.PlayerProfile().IsDirty);
        }

        [Fact]
        public void MarkDirty_MakesTheProfileDirty()
        {
            B.PlayerProfile profile = new B.PlayerProfile();

            profile.MarkDirty();

            Assert.True(profile.IsDirty);
        }

        [Fact]
        public void Serialise_ClearsTheDirtyFlag()
        {
            B.PlayerProfile profile = new B.PlayerProfile { Name = "Ana" };
            profile.MarkDirty();

            profile.Serialise();

            Assert.False(profile.IsDirty);
        }

        [Fact]
        public void Serialise_ReturnsTheTypeSpecificPayload()
        {
            B.PlayerProfile profile = new B.PlayerProfile { Name = "Ana" };

            Assert.Equal("profile:Ana", profile.Serialise());
        }

        [Fact]
        public void EverySubclassClearsItsFlag_BecauseTheBaseClassDoesItAndIsNotVirtual()
        {
            B.PlayerProfile profile = new B.PlayerProfile { Name = "Ana" };
            B.LevelProgress progress = new B.LevelProgress { Index = 4 };
            profile.MarkDirty();
            progress.MarkDirty();

            profile.Serialise();
            progress.Serialise();

            Assert.False(profile.IsDirty);
            Assert.False(progress.IsDirty);
        }

        [Fact]
        public void SettingsFile_IsAlwaysDirtyAndDoesNotDeriveFromTheBaseClass()
        {
            B.SettingsFile settings = new B.SettingsFile { Volume = 0.5f };

            Assert.True(settings.IsDirty);
            settings.Serialise();
            Assert.True(settings.IsDirty);
            // A type-relationship assertion rather than an `is` check: the compiler can
            // prove the `is` is always false and warns, which it should.
            Assert.False(typeof(B.DirtySaveableBase).IsAssignableFrom(typeof(B.SettingsFile)));
        }

        [Fact]
        public void SaveAll_SavesOnlyTheDirtyItems()
        {
            B.PlayerProfile dirty = new B.PlayerProfile { Name = "Ana" };
            B.LevelProgress clean = new B.LevelProgress { Index = 4 };
            dirty.MarkDirty();

            List<string> saved = B.SaveSystem.SaveAll(new List<B.ISaveable> { dirty, clean });

            Assert.Single(saved);
            Assert.Equal("profile:Ana", saved[0]);
        }

        [Fact]
        public void SaveAll_AcceptsBothTheBaseClassFamilyAndTheDirectImplementer()
        {
            List<B.ISaveable> items = new List<B.ISaveable>
            {
                new B.PlayerProfile { Name = "Ana" },
                new B.LevelProgress { Index = 1 },
                new B.SettingsFile { Volume = 1f }
            };

            List<string> saved = B.SaveSystem.SaveAll(items);

            Assert.Single(saved);               // only the always-dirty settings
            Assert.Equal("settings:1", saved[0]);
        }
    }

    public class T06_Weapons_C_Tests
    {
        [Fact]
        public void APistol_FiresOneShotPerTriggerPull()
        {
            C.Pistol pistol = new C.Pistol();

            pistol.StartFiring();
            pistol.Tick();
            pistol.Tick();

            Assert.Equal(1, pistol.ShotsFired);
        }

        [Fact]
        public void ARifle_KeepsFiringWhileTheTriggerIsHeld()
        {
            C.Rifle rifle = new C.Rifle();

            rifle.StartFiring();
            rifle.Tick();
            rifle.Tick();

            Assert.Equal(3, rifle.ShotsFired);
        }

        [Fact]
        public void ARifle_StopsFiringWhenTheTriggerIsReleased()
        {
            C.Rifle rifle = new C.Rifle();
            rifle.StartFiring();
            rifle.Tick();

            rifle.StopFiring();
            rifle.Tick();

            Assert.Equal(2, rifle.ShotsFired);
        }

        [Fact]
        public void FiringConsumesAmmunition()
        {
            C.Pistol pistol = new C.Pistol();
            int before = pistol.Ammunition;

            pistol.StartFiring();

            Assert.Equal(before - 1, pistol.Ammunition);
        }

        [Fact]
        public void AnEmptyWeapon_FiresNoFurtherShots()
        {
            C.Pistol pistol = new C.Pistol();

            for (int i = 0; i < 20; i++)
            {
                pistol.StartFiring();
                pistol.StopFiring();
            }

            Assert.Equal(0, pistol.Ammunition);
            Assert.Equal(12, pistol.ShotsFired);
        }

        [Fact]
        public void Reload_RefillsToCapacity()
        {
            C.Pistol pistol = new C.Pistol();
            pistol.StartFiring();

            pistol.Reload();

            Assert.Equal(12, pistol.Ammunition);
        }

        [Fact]
        public void ACrowbar_CarriesNoAmmunitionAndCannotBeReloaded()
        {
            C.Crowbar crowbar = new C.Crowbar();

            Assert.False(crowbar is C.IAmmunitionCarrying);
            Assert.False(crowbar is C.IReloadable);
            Assert.True(crowbar is C.IHoldable);
        }

        [Fact]
        public void ATurret_IsNotHeldAndIsNotPartOfTheCarriedWeaponFamily()
        {
            C.Turret turret = new C.Turret();

            Assert.False(turret is C.IHoldable);
            Assert.False(turret is C.IAmmunitionCarrying);
            Assert.False(typeof(C.WeaponBase).IsAssignableFrom(typeof(C.Turret)));
        }

        [Fact]
        public void ATurret_StillFires()
        {
            C.Turret turret = new C.Turret();

            turret.Fire();
            turret.Fire();

            Assert.Equal(2, turret.ShotsFired);
        }

        [Fact]
        public void DisplayAmmunition_AcceptsAnyWeaponCarryingAmmunition()
        {
            Assert.Equal("Ammo: 12", C.WeaponSystems.DisplayAmmunition(new C.Pistol()));
            Assert.Equal("Ammo: 30", C.WeaponSystems.DisplayAmmunition(new C.Rifle()));
        }

        [Fact]
        public void PullTrigger_StartsAHeldWeaponFiring()
        {
            C.Pistol pistol = new C.Pistol();

            C.WeaponSystems.PullTrigger(pistol);

            Assert.True(pistol.IsFiring);
        }

        [Fact]
        public void RecordReloadState_ReturnsTheWeaponsSerialisedState()
        {
            Assert.Equal("pistol:12", C.WeaponSystems.RecordReloadState(new C.Pistol()));
        }
    }
}
