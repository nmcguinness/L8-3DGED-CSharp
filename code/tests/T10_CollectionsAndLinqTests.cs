// Tests for the note 10 B and C solutions.
//
// The C tests assert the two implementations agree across a sequence of spawns
// and deaths, which is what makes the redundant state in FastSpawner trustworthy.

using System.Collections.Generic;
using Xunit;
using B = Solutions.T10.B;
using C = Solutions.T10.C;

namespace Week01.Tests
{
    public class T10_CommandHistory_B_Tests
    {
        private readonly B.CommandHistory _history = new B.CommandHistory();

        [Fact]
        public void ReplayAll_ReturnsCommandsInTheOrderIssued()
        {
            _history.Record(new B.Command("move", 1));
            _history.Record(new B.Command("turn", 2));
            _history.Record(new B.Command("fire", 3));

            Assert.Equal(new List<string> { "move", "turn", "fire" }, _history.ReplayAll());
        }

        [Fact]
        public void UndoLast_RemovesAndReturnsTheMostRecentCommand()
        {
            _history.Record(new B.Command("move", 1));
            _history.Record(new B.Command("fire", 2));

            B.Command undone = _history.UndoLast();

            Assert.Equal("fire", undone.Name);
            Assert.Equal(1, _history.Count);
        }

        [Fact]
        public void RecordingThreeAndUndoingOne_ReplaysTheFirstTwoInOrder()
        {
            _history.Record(new B.Command("a", 1));
            _history.Record(new B.Command("b", 2));
            _history.Record(new B.Command("c", 3));

            _history.UndoLast();

            Assert.Equal(new List<string> { "a", "b" }, _history.ReplayAll());
        }

        [Fact]
        public void UndoLast_OnAnEmptyHistory_ReturnsNull()
        {
            Assert.Null(_history.UndoLast());
        }

        [Fact]
        public void Prune_KeepsOnlyTheMostRecentCommands()
        {
            for (int i = 1; i <= 5; i++)
            {
                _history.Record(new B.Command("cmd" + i, i));
            }

            _history.Prune(2);

            Assert.Equal(2, _history.Count);
            Assert.Equal(new List<string> { "cmd4", "cmd5" }, _history.ReplayAll());
        }

        [Fact]
        public void Prune_ToMoreThanIsHeld_ChangesNothing()
        {
            _history.Record(new B.Command("only", 1));

            _history.Prune(10);

            Assert.Equal(1, _history.Count);
        }
    }

    public class T10_FactStore_B_Tests
    {
        private readonly B.FactStore _facts = new B.FactStore();

        [Fact]
        public void TryGet_AfterSettingAnInt_ReturnsTrueAndTheValue()
        {
            _facts.Set("health", 100);

            int health;
            bool ok = _facts.TryGet("health", out health);

            Assert.True(ok);
            Assert.Equal(100, health);
        }

        [Fact]
        public void TryGet_WithTheWrongType_ReturnsFalseAndThrowsNothing()
        {
            _facts.Set("health", 100);

            string health;
            bool ok = _facts.TryGet("health", out health);

            Assert.False(ok);
            Assert.Null(health);
        }

        [Fact]
        public void TryGet_WithAMissingKey_ReturnsFalseAndThrowsNothing()
        {
            int value;

            Assert.False(_facts.TryGet("nothing", out value));
            Assert.Equal(0, value);
        }

        [Fact]
        public void FactsOfDifferentTypes_CanBeHeldTogether()
        {
            _facts.Set("health", 100);
            _facts.Set("alarm", true);
            _facts.Set("lastSeen", "corridor");

            int health;
            bool alarm;
            string lastSeen;

            Assert.True(_facts.TryGet("health", out health));
            Assert.True(_facts.TryGet("alarm", out alarm));
            Assert.True(_facts.TryGet("lastSeen", out lastSeen));
            Assert.Equal(100, health);
            Assert.True(alarm);
            Assert.Equal("corridor", lastSeen);
        }

        [Fact]
        public void Remove_DeletesTheFact()
        {
            _facts.Set("health", 100);

            Assert.True(_facts.Remove("health"));
            Assert.False(_facts.Has("health"));
        }

        [Fact]
        public void Has_ReportsWhetherAFactIsPresent()
        {
            _facts.Set("health", 100);

            Assert.True(_facts.Has("health"));
            Assert.False(_facts.Has("mana"));
        }
    }

    public class T10_WaveSpawner_C_Tests
    {
        private static void SpawnInto(C.NaiveSpawner naive, C.FastSpawner fast, int count)
        {
            for (int i = 0; i < count; i++)
            {
                naive.Spawn(MakeEnemy(i));
                fast.Spawn(MakeEnemy(i));
            }
        }

        private static C.SpawnedEnemy MakeEnemy(int id)
        {
            return new C.SpawnedEnemy
            {
                Id = id,
                TypeName = (id % 3) == 0 ? "grunt" : "sniper",
                WaveNumber = id % 4,
                IsAlive = true,
                X = id,
                Y = id
            };
        }

        [Fact]
        public void BothImplementations_AgreeOnAliveCountAfterSpawnsAndDeaths()
        {
            C.NaiveSpawner naive = new C.NaiveSpawner();
            C.FastSpawner fast = new C.FastSpawner();
            SpawnInto(naive, fast, 20);

            naive.Kill(3);
            fast.Kill(3);
            naive.Kill(7);
            fast.Kill(7);

            Assert.Equal(naive.AliveCount(), fast.AliveCount());
            Assert.Equal(18, fast.AliveCount());
        }

        [Fact]
        public void BothImplementations_AgreeOnAliveCountByType()
        {
            C.NaiveSpawner naive = new C.NaiveSpawner();
            C.FastSpawner fast = new C.FastSpawner();
            SpawnInto(naive, fast, 20);
            naive.Kill(3);
            fast.Kill(3);

            Assert.Equal(naive.AliveOfType("grunt"), fast.AliveOfType("grunt"));
            Assert.Equal(naive.AliveOfType("sniper"), fast.AliveOfType("sniper"));
        }

        [Fact]
        public void BothImplementations_AgreeOnLookupById()
        {
            C.NaiveSpawner naive = new C.NaiveSpawner();
            C.FastSpawner fast = new C.FastSpawner();
            SpawnInto(naive, fast, 20);

            Assert.Equal(naive.ById(5).TypeName, fast.ById(5).TypeName);
            Assert.Null(naive.ById(999));
            Assert.Null(fast.ById(999));
        }

        [Fact]
        public void BothImplementations_AgreeOnTheNearestAliveEnemy()
        {
            C.NaiveSpawner naive = new C.NaiveSpawner();
            C.FastSpawner fast = new C.FastSpawner();
            SpawnInto(naive, fast, 20);
            naive.Kill(0);
            fast.Kill(0);

            Assert.Equal(naive.NearestAlive(0f, 0f).Id, fast.NearestAlive(0f, 0f).Id);
        }

        [Fact]
        public void BothImplementations_AgreeOnTheCurrentWaveSpawns()
        {
            C.NaiveSpawner naive = new C.NaiveSpawner();
            C.FastSpawner fast = new C.FastSpawner();
            SpawnInto(naive, fast, 20);
            naive.CurrentWave = 2;
            fast.CurrentWave = 2;

            Assert.Equal(naive.CurrentWaveSpawns().Count, fast.CurrentWaveSpawns().Count);
        }

        [Fact]
        public void KillingTheSameEnemyTwice_DoesNotCorruptTheAliveCount()
        {
            C.FastSpawner fast = new C.FastSpawner();
            for (int i = 0; i < 5; i++)
            {
                fast.Spawn(MakeEnemy(i));
            }

            fast.Kill(2);
            fast.Kill(2);

            Assert.Equal(4, fast.AliveCount());
        }

        [Fact]
        public void KillingAnUnknownId_DoesNotCorruptTheAliveCount()
        {
            C.FastSpawner fast = new C.FastSpawner();
            fast.Spawn(MakeEnemy(1));

            fast.Kill(999);

            Assert.Equal(1, fast.AliveCount());
        }

        [Fact]
        public void NearestAlive_OnAnEmptySpawner_ReturnsNull()
        {
            Assert.Null(new C.FastSpawner().NearestAlive(0f, 0f));
        }

        [Fact]
        public void TimeBoth_ReportsAMeasurementForEachImplementation()
        {
            double naiveMs;
            double fastMs;

            C.SpawnerBenchmark.TimeBoth(200, 200, out naiveMs, out fastMs);

            Assert.True(naiveMs >= 0);
            Assert.True(fastMs >= 0);
        }
    }
}
