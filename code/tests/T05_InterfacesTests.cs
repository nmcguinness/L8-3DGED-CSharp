// Tests for the note 05 B and C solutions.
//
// The C tests exercise every rule WITHOUT constructing a Turret, which is the
// constraint exercise 05 C imposed. Only the last test needs one.

using System.Collections.Generic;
using Xunit;
using B = Solutions.T05.B;
using C = Solutions.T05.C;

namespace Week01.Tests
{
    public class T05_SegregatedContracts_B_Tests
    {
        [Fact]
        public void ApplySplashDamage_DamagesEveryTarget()
        {
            B.Crate crate = new B.Crate();
            B.Guard guard = new B.Guard();

            B.Systems.ApplySplashDamage(new List<B.IDamageable> { crate, guard }, 10);

            Assert.Equal(90, crate.Integrity);
            Assert.Equal(50, guard.Health);
        }

        [Fact]
        public void TakeDamage_FloorsIntegrityAtZero()
        {
            B.Crate crate = new B.Crate();

            crate.TakeDamage(500);

            Assert.Equal(0, crate.Integrity);
        }

        [Fact]
        public void SaveAll_SavesEveryItemGiven()
        {
            B.Guard guard = new B.Guard();
            B.Checkpoint checkpoint = new B.Checkpoint { Index = 3 };

            List<string> saved = B.Systems.SaveAll(new List<B.ISaveable> { guard, checkpoint });

            Assert.Equal(2, saved.Count);
            Assert.Contains("checkpoint:3", saved);
        }

        [Fact]
        public void ACrate_IsDamageableAndRepairableButNeitherMovableNorSaveable()
        {
            B.Crate crate = new B.Crate();

            Assert.IsAssignableFrom<B.IDamageable>(crate);
            Assert.IsAssignableFrom<B.IRepairable>(crate);
            Assert.False(crate is B.IMovable);
            Assert.False(crate is B.ISaveable);
        }

        [Fact]
        public void ACheckpoint_IsSaveableAndNothingElse()
        {
            B.Checkpoint checkpoint = new B.Checkpoint();

            Assert.IsAssignableFrom<B.ISaveable>(checkpoint);
            Assert.False(checkpoint is B.IDamageable);
        }

        [Fact]
        public void Repair_RestoresIntegrity()
        {
            B.Crate crate = new B.Crate();
            crate.TakeDamage(50);

            crate.Repair(20);

            Assert.Equal(70, crate.Integrity);
        }
    }

    public class T05_TurretTargeting_C_Tests
    {
        private readonly List<C.Enemy> _candidates;

        public T05_TurretTargeting_C_Tests()
        {
            _candidates = new List<C.Enemy>
            {
                new C.Enemy { Name = "far-healthy", Health = 100, DistanceFromTurret = 50f, SecondsSinceItAttackedUs = 30f },
                new C.Enemy { Name = "near-wounded", Health = 10, DistanceFromTurret = 5f, SecondsSinceItAttackedUs = 20f },
                new C.Enemy { Name = "recent-attacker", Health = 60, DistanceFromTurret = 25f, SecondsSinceItAttackedUs = 1f }
            };
        }

        [Fact]
        public void NearestRule_ChoosesTheClosestEnemy()
        {
            C.Enemy chosen = new C.NearestRule().Choose(_candidates);

            Assert.Equal("near-wounded", chosen.Name);
        }

        [Fact]
        public void WeakestRule_ChoosesTheEnemyWithTheLeastHealth()
        {
            C.Enemy chosen = new C.WeakestRule().Choose(_candidates);

            Assert.Equal("near-wounded", chosen.Name);
        }

        [Fact]
        public void MostRecentAttackerRule_ChoosesTheEnemyThatAttackedMostRecently()
        {
            C.Enemy chosen = new C.MostRecentAttackerRule().Choose(_candidates);

            Assert.Equal("recent-attacker", chosen.Name);
        }

        [Theory]
        [InlineData(typeof(C.NearestRule))]
        [InlineData(typeof(C.WeakestRule))]
        [InlineData(typeof(C.MostRecentAttackerRule))]
        public void EveryRule_ReturnsNullForAnEmptyCandidateList(System.Type ruleType)
        {
            C.ITargetingRule rule = (C.ITargetingRule)System.Activator.CreateInstance(ruleType);

            Assert.Null(rule.Choose(new List<C.Enemy>()));
        }

        [Theory]
        [InlineData(typeof(C.NearestRule))]
        [InlineData(typeof(C.WeakestRule))]
        [InlineData(typeof(C.MostRecentAttackerRule))]
        public void EveryRule_ReturnsTheOnlyCandidateWhenThereIsOne(System.Type ruleType)
        {
            C.ITargetingRule rule = (C.ITargetingRule)System.Activator.CreateInstance(ruleType);
            C.Enemy only = _candidates[0];

            Assert.Same(only, rule.Choose(new List<C.Enemy> { only }));
        }

        [Fact]
        public void SwappingATurretsRuleAtRunTime_ChangesWhichEnemyItSelects()
        {
            C.Turret turret = new C.Turret(new C.NearestRule());

            C.Enemy first = turret.SelectTarget(_candidates);
            turret.Rule = new C.MostRecentAttackerRule();
            C.Enemy second = turret.SelectTarget(_candidates);

            Assert.Equal("near-wounded", first.Name);
            Assert.Equal("recent-attacker", second.Name);
        }
    }
}
