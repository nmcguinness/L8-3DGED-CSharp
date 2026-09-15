// Tests for the note 07 B and C solutions.
//
// Events return nothing, so every test here records what it saw. Counts are used
// in preference to flags wherever a count is more informative - a flag cannot
// catch a handler running twice.

using System;
using Xunit;
using B = Solutions.T07.B;
using C = Solutions.T07.C;

namespace Week01.Tests
{
    public class T07_Furnace_B_Tests
    {
        private readonly B.Furnace _furnace = new B.Furnace();

        [Fact]
        public void ChangingTheTemperature_NotifiesAnAttachedDisplay()
        {
            B.Display display = new B.Display();
            display.Attach(_furnace);

            _furnace.SetTemperature(500);

            Assert.Single(display.Seen);
            Assert.Equal(500, display.Seen[0]);
        }

        [Fact]
        public void ADetachedDisplay_ReceivesNothingFurther()
        {
            B.Display display = new B.Display();
            display.Attach(_furnace);
            _furnace.SetTemperature(500);

            display.Detach(_furnace);
            _furnace.SetTemperature(600);

            Assert.Single(display.Seen);
        }

        [Fact]
        public void AttachingTheSameDisplayTwice_StillResultsInOneCallPerChange()
        {
            B.Display display = new B.Display();
            display.Attach(_furnace);
            display.Attach(_furnace);

            _furnace.SetTemperature(500);

            Assert.Single(display.Seen);
        }

        [Fact]
        public void DetachingSomethingNeverAttached_ThrowsNothing()
        {
            B.Display display = new B.Display();

            display.Detach(_furnace);

            Assert.Empty(display.Seen);
        }

        [Fact]
        public void ChangingTheTemperatureWithNoSubscribers_ThrowsNothing()
        {
            _furnace.SetTemperature(500);

            Assert.Equal(500, _furnace.Temperature);
        }

        [Fact]
        public void TheAlarm_SoundsWhenTheTemperatureFirstCrossesTheThreshold()
        {
            B.Alarm alarm = new B.Alarm();
            alarm.Attach(_furnace);

            _furnace.SetTemperature(900);

            Assert.Equal(1, alarm.TimesSounded);
        }

        [Fact]
        public void TheAlarm_DoesNotSoundAgainWhileStillAboveTheThreshold()
        {
            B.Alarm alarm = new B.Alarm();
            alarm.Attach(_furnace);

            _furnace.SetTemperature(900);
            _furnace.SetTemperature(950);
            _furnace.SetTemperature(1000);

            Assert.Equal(1, alarm.TimesSounded);
        }

        [Fact]
        public void TheAlarm_SoundsAgainAfterDroppingBelowAndCrossingOnceMore()
        {
            B.Alarm alarm = new B.Alarm();
            alarm.Attach(_furnace);

            _furnace.SetTemperature(900);
            _furnace.SetTemperature(500);
            _furnace.SetTemperature(900);

            Assert.Equal(2, alarm.TimesSounded);
        }

        [Fact]
        public void TheAlarm_DoesNotSoundBelowTheThreshold()
        {
            B.Alarm alarm = new B.Alarm();
            alarm.Attach(_furnace);

            _furnace.SetTemperature(700);

            Assert.Equal(0, alarm.TimesSounded);
        }

        [Fact]
        public void ADetachedAlarm_DoesNotSound()
        {
            B.Alarm alarm = new B.Alarm();
            alarm.Attach(_furnace);
            alarm.Detach(_furnace);

            _furnace.SetTemperature(900);

            Assert.Equal(0, alarm.TimesSounded);
        }

        [Fact]
        public void TheLogger_RecordsEveryReadingAndReportsTheHighest()
        {
            B.Logger logger = new B.Logger();
            logger.Attach(_furnace);

            _furnace.SetTemperature(100);
            _furnace.SetTemperature(900);
            _furnace.SetTemperature(400);

            Assert.Equal(3, logger.Count);
            Assert.Equal(900, logger.Highest());
        }

        [Fact]
        public void ThreeSubscribersAttachedTogether_EachReceiveWhatTheyAskedFor()
        {
            B.Display display = new B.Display();
            B.Alarm alarm = new B.Alarm();
            B.Logger logger = new B.Logger();
            display.Attach(_furnace);
            alarm.Attach(_furnace);
            logger.Attach(_furnace);

            _furnace.SetTemperature(900);

            Assert.Single(display.Seen);
            Assert.Equal(1, alarm.TimesSounded);
            Assert.Equal(1, logger.Count);
        }
    }

    public class T07_Character_C_Tests
    {
        private readonly C.Character _character = new C.Character();

        [Fact]
        public void TakingDamage_RaisesDamagedWithTheAmount()
        {
            int seen = -1;
            Action<int> handler = amount => seen = amount;
            _character.Damaged += handler;

            _character.TakeDamage(30);

            _character.Damaged -= handler;
            Assert.Equal(30, seen);
        }

        [Fact]
        public void TakingFatalDamage_RaisesKilledExactlyOnce()
        {
            int kills = 0;
            Action handler = () => kills++;
            _character.Killed += handler;

            _character.TakeDamage(500);

            _character.Killed -= handler;
            Assert.Equal(1, kills);
        }

        [Fact]
        public void TakingDamageWhenAlreadyDead_RaisesNothingFurther()
        {
            _character.TakeDamage(500);
            int kills = 0;
            Action handler = () => kills++;
            _character.Killed += handler;

            _character.TakeDamage(500);

            _character.Killed -= handler;
            Assert.Equal(0, kills);
        }

        [Fact]
        public void Reviving_RaisesRevivedAndRestoresHealth()
        {
            _character.TakeDamage(500);
            int revivals = 0;
            Action handler = () => revivals++;
            _character.Revived += handler;

            _character.Revive(50);

            _character.Revived -= handler;
            Assert.Equal(1, revivals);
            Assert.Equal(50, _character.Health);
            Assert.False(_character.IsDead);
        }

        [Fact]
        public void TheHud_CountsBothDamageAndHealing()
        {
            C.Hud hud = new C.Hud();
            hud.Attach(_character);

            _character.TakeDamage(10);
            _character.Heal(5);

            hud.Detach(_character);
            Assert.Equal(2, hud.Updates);
        }

        [Fact]
        public void TheAchievementTracker_ReceivesDeathsAndPickupsButNotDamage()
        {
            C.AchievementTracker tracker = new C.AchievementTracker();
            tracker.Attach(_character);

            _character.TakeDamage(10);
            _character.PickUp("sword");
            _character.TakeDamage(500);

            tracker.Detach(_character);
            Assert.Equal(1, tracker.Deaths);
            Assert.Single(tracker.Items);
            Assert.Equal("sword", tracker.Items[0]);
        }

        [Fact]
        public void TwoObserversAttachedTogether_ReceiveDifferentSubsets()
        {
            C.Hud hud = new C.Hud();
            C.AchievementTracker tracker = new C.AchievementTracker();
            hud.Attach(_character);
            tracker.Attach(_character);

            _character.TakeDamage(10);
            _character.PickUp("shield");

            hud.Detach(_character);
            tracker.Detach(_character);

            Assert.Equal(1, hud.Updates);           // damage only
            Assert.Single(tracker.Items);           // pickup only
        }

        [Fact]
        public void ADetachedObserver_StopsReceiving()
        {
            C.Hud hud = new C.Hud();
            hud.Attach(_character);
            _character.TakeDamage(10);

            hud.Detach(_character);
            _character.TakeDamage(10);

            Assert.Equal(1, hud.Updates);
        }

        [Fact]
        public void EveryOccurrence_CanBeRaisedWithoutSubscribers()
        {
            _character.TakeDamage(10);
            _character.Heal(5);
            _character.PickUp("rope");
            _character.EnterTrigger("cave");
            _character.TakeDamage(500);
            _character.Revive(20);

            Assert.Equal(20, _character.Health);
        }
    }
}
