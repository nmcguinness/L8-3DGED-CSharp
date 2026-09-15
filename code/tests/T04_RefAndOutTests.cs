// Tests for the note 04 B and C solutions.

using System.Collections.Generic;
using Xunit;
using B = Solutions.T04.B;
using C = Solutions.T04.C;

namespace Week01.Tests
{
    public class T04_Inventory_B_Tests
    {
        private readonly B.Inventory _inventory;

        public T04_Inventory_B_Tests()
        {
            _inventory = new B.Inventory();
            _inventory.Place("primary", new B.Item("rifle"));
        }

        [Fact]
        public void TryGetItem_WithAFilledSlot_ReturnsTrueAndSuppliesTheItem()
        {
            B.Item found;

            bool ok = _inventory.TryGetItem("primary", out found);

            Assert.True(ok);
            Assert.NotNull(found);
            Assert.Equal("rifle", found.Name);
        }

        [Fact]
        public void TryGetItem_WithAMissingSlot_ReturnsFalseAndAssignsNull()
        {
            B.Item found;

            bool ok = _inventory.TryGetItem("secondary", out found);

            Assert.False(ok);
            Assert.Null(found);
        }

        [Fact]
        public void TryGetItem_WithAMissingSlot_ThrowsNothing()
        {
            B.Item found;

            // Calling it inside the assertion proves no exception escapes; if one
            // did, the test would fail with that exception rather than an assert.
            bool ok = _inventory.TryGetItem("nothing-here", out found);

            Assert.False(ok);
        }

        [Fact]
        public void ResolveHit_ReducesHealthAndReportsDeathWhenItReachesZero()
        {
            int health = 30;
            bool isDead;

            B.Combat.ResolveHit(ref health, 50, out isDead);

            Assert.Equal(0, health);
            Assert.True(isDead);
        }

        [Fact]
        public void ResolveHit_CalledTwiceOnADeadTarget_LeavesHealthAtZeroAndStillReportsDead()
        {
            int health = 30;
            bool isDead;

            B.Combat.ResolveHit(ref health, 50, out isDead);
            B.Combat.ResolveHit(ref health, 50, out isDead);

            Assert.Equal(0, health);
            Assert.True(isDead);
        }

        [Fact]
        public void ResolveHit_OnASurvivableHit_ReportsNotDead()
        {
            int health = 100;
            bool isDead;

            B.Combat.ResolveHit(ref health, 30, out isDead);

            Assert.Equal(70, health);
            Assert.False(isDead);
        }
    }

    public class T04_EntityStats_C_Tests
    {
        [Fact]
        public void ByValueAndByRef_ProduceIdenticalResultsForTheSameInput()
        {
            C.EntityStats[] byValue = new C.EntityStats[50];
            C.EntityStats[] byRef = new C.EntityStats[50];

            C.StatsUpdater.UpdateAllByValue(byValue);
            C.StatsUpdater.UpdateAllByRef(byRef);

            for (int i = 0; i < byValue.Length; i++)
            {
                Assert.Equal(byValue[i].Total, byRef[i].Total);
            }
        }

        [Fact]
        public void UpdateAllByValue_IncrementsEveryScoreExactlyOnce()
        {
            C.EntityStats[] all = new C.EntityStats[3];

            C.StatsUpdater.UpdateAllByValue(all);

            Assert.Equal(8, all[0].Total);      // eight fields, each incremented once
        }

        [Fact]
        public void UpdateAllByRef_IncrementsEveryScoreExactlyOnce()
        {
            C.EntityStats[] all = new C.EntityStats[3];

            C.StatsUpdater.UpdateAllByRef(all);

            Assert.Equal(8, all[0].Total);
        }

        [Fact]
        public void LevelUpByValue_LeavesTheCallersCopyUnchanged_BecauseTheStructWasCopied()
        {
            C.EntityStats original = new C.EntityStats();

            C.EntityStats result = C.StatsUpdater.LevelUpByValue(original);

            Assert.Equal(0, original.Total);
            Assert.Equal(8, result.Total);
        }

        [Fact]
        public void LevelUpByRef_ChangesTheCallersVariableInPlace()
        {
            C.EntityStats stats = new C.EntityStats();

            C.StatsUpdater.LevelUpByRef(ref stats);

            Assert.Equal(8, stats.Total);
        }

        [Fact]
        public void TimeBothApproaches_ReportsAMeasurementForEach()
        {
            double byValueMs;
            double byRefMs;

            C.StatsUpdater.TimeBothApproaches(10000, out byValueMs, out byRefMs);

            Assert.True(byValueMs >= 0);
            Assert.True(byRefMs >= 0);
        }
    }
}
