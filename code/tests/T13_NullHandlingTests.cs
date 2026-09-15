// Tests for the note 13 A, B and C solutions.
//
// Exercise A is a reading task, so its test asserts the audit identified exactly
// the four unsafe lines.

using System;
using System.Collections.Generic;
using Xunit;
using A = Solutions.T13.A;
using B = Solutions.T13.B;
using C = Solutions.T13.C;

namespace Week01.Tests
{
    public class T13_NullCheckAudit_A_Tests
    {
        [Fact]
        public void TheAudit_CoversAllTenLines()
        {
            Assert.Equal(10, A.NullCheckAudit.Audit().Count);
        }

        [Fact]
        public void TheAudit_IdentifiesExactlyFourUnsafeLines()
        {
            Assert.Equal(4, A.NullCheckAudit.UnsafeLineNumbers().Count);
        }

        [Fact]
        public void TheUnsafeLines_AreTwoFourEightAndNine()
        {
            Assert.Equal(new List<int> { 2, 4, 8, 9 }, A.NullCheckAudit.UnsafeLineNumbers());
        }

        [Fact]
        public void TheDelegateLine_IsSafeBecauseADelegateIsNotAnEngineObject()
        {
            A.AuditedLine line = A.NullCheckAudit.Audit()[9];

            Assert.Equal(10, line.Number);
            Assert.True(line.IsSafe);
        }
    }

    public class T13_EngineObject_B_Tests
    {
        [Fact]
        public void ALiveObject_IsReportedPresentByEveryOperator()
        {
            B.NullCheckResults results = B.NullDivergence.Inspect(new B.EngineObject { Name = "Goblin" });

            Assert.False(results.EqualsNull);
            Assert.True(results.NotEqualsNull);
            Assert.False(results.IsNullPattern);
            Assert.False(results.ReferenceEqualsNull);
            Assert.Equal("Goblin", results.NameViaNullConditional);
        }

        [Fact]
        public void ADestroyedObject_ReadsAsNullThroughEqualsButNotThroughIsNull()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            B.NullCheckResults results = B.NullDivergence.Inspect(destroyed);

            Assert.True(results.EqualsNull);
            Assert.False(results.IsNullPattern);
            Assert.False(results.ReferenceEqualsNull);
        }

        [Fact]
        public void ADestroyedObject_StillSuppliesItsNameThroughNullConditional()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            B.NullCheckResults results = B.NullDivergence.Inspect(destroyed);

            Assert.Equal("Goblin", results.NameViaNullConditional);
        }

        [Fact]
        public void NullCoalescing_ReturnsTheDestroyedObjectRatherThanTheFallback()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Goblin" };
            destroyed.Destroy();
            B.EngineObject replacement = new B.EngineObject { Name = "Replacement" };

            B.EngineObject chosen = B.NullDivergence.ChooseWithNullCoalescing(destroyed, replacement);

            Assert.Equal("Goblin", chosen.Name);
        }

        [Fact]
        public void TheExplicitComparison_CorrectlyRejectsTheDestroyedObject()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Goblin" };
            destroyed.Destroy();
            B.EngineObject replacement = new B.EngineObject { Name = "Replacement" };

            B.EngineObject chosen = B.NullDivergence.ChooseSafely(destroyed, replacement);

            Assert.Equal("Replacement", chosen.Name);
        }

        [Fact]
        public void UnsafeUse_ThrowsForADestroyedObject()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            InvalidOperationException ex =
                Assert.Throws<InvalidOperationException>(() => B.NullDivergence.UnsafeUse(destroyed));

            Assert.Contains("has been destroyed", ex.Message);
        }

        [Fact]
        public void UnsafeUse_DoesNotThrowForALiveObject()
        {
            Assert.Equal("Goblin did some work.",
                B.NullDivergence.UnsafeUse(new B.EngineObject { Name = "Goblin" }));
        }

        [Fact]
        public void UnsafeUse_DoesNotThrowForAGenuinelyNullReference()
        {
            Assert.Null(B.NullDivergence.UnsafeUse(null));
        }

        [Fact]
        public void SafeUse_ThrowsForNoneOfTheThreeCases()
        {
            B.EngineObject destroyed = new B.EngineObject { Name = "Dead" };
            destroyed.Destroy();

            Assert.Equal("Alive did some work.",
                B.NullDivergence.SafeUse(new B.EngineObject { Name = "Alive" }));
            Assert.Equal("(skipped - null or destroyed)", B.NullDivergence.SafeUse(destroyed));
            Assert.Equal("(skipped - null or destroyed)", B.NullDivergence.SafeUse(null));
        }
    }

    public class T13_Agent_C_Tests
    {
        private readonly C.Agent _agent = new C.Agent();

        [Fact]
        public void WithNothingRemembered_TheAgentReturnsHome()
        {
            Assert.Equal(C.AgentAction.ReturnedHome, _agent.Tick());
        }

        [Fact]
        public void WithOneThreat_TheAgentChoosesItThenActsOnIt()
        {
            _agent.Remember(new C.Target { Name = "goblin" });

            Assert.Equal(C.AgentAction.ChoseNewTarget, _agent.Tick());
            Assert.Equal(C.AgentAction.ActedOnTarget, _agent.Tick());
        }

        [Fact]
        public void WhenATargetAnnouncesItsDeath_TheAgentForgetsItImmediately()
        {
            C.Target threat = new C.Target { Name = "goblin" };
            _agent.Remember(threat);

            threat.Kill();

            Assert.Equal(0, _agent.ThreatCount);
        }

        [Fact]
        public void WhenTheCurrentTargetIsDestroyedAfterSelection_TheAgentDoesNotThrow()
        {
            C.Target threat = new C.Target { Name = "goblin" };
            _agent.Remember(threat);
            _agent.Tick();                          // selects it

            threat.Kill();                          // destroyed before being acted on

            Assert.Equal(C.AgentAction.ReturnedHome, _agent.Tick());
        }

        [Fact]
        public void WhenATargetIsDestroyedSilently_TheAgentStillDoesNotThrow()
        {
            C.Target threat = new C.Target { Name = "goblin" };
            _agent.Remember(threat);

            threat.KillSilently();                  // no event raised

            Assert.Equal(C.AgentAction.ReturnedHome, _agent.Tick());
        }

        [Fact]
        public void WhenEveryThreatIsDestroyedInTheSameTick_TheAgentReturnsHome()
        {
            List<C.Target> threats = new List<C.Target>();

            for (int i = 0; i < 5; i++)
            {
                C.Target threat = new C.Target { Name = "t" + i };
                threats.Add(threat);
                _agent.Remember(threat);
            }

            foreach (C.Target threat in threats)
            {
                threat.Kill();
            }

            Assert.Equal(C.AgentAction.ReturnedHome, _agent.Tick());
            Assert.Equal(0, _agent.ThreatCount);
        }

        [Fact]
        public void TheThreatCollection_StaysBoundedAcrossManySpawnsAndDeaths()
        {
            for (int i = 0; i < 500; i++)
            {
                C.Target threat = new C.Target { Name = "t" + i };
                _agent.Remember(threat);
                threat.Kill();
                _agent.Tick();
            }

            Assert.Equal(0, _agent.ThreatCount);
        }

        [Fact]
        public void TheThreatCollection_StaysBoundedEvenWhenDeathsAreSilent()
        {
            for (int i = 0; i < 200; i++)
            {
                C.Target threat = new C.Target { Name = "t" + i };
                _agent.Remember(threat);
                threat.KillSilently();
                _agent.Tick();
            }

            Assert.Equal(0, _agent.ThreatCount);
        }

        [Fact]
        public void RememberingTheSameThreatTwice_StoresItOnce()
        {
            C.Target threat = new C.Target { Name = "goblin" };

            _agent.Remember(threat);
            _agent.Remember(threat);

            Assert.Equal(1, _agent.ThreatCount);
        }

        [Fact]
        public void TheAgentActsOnALiveTargetRepeatedly()
        {
            _agent.Remember(new C.Target { Name = "goblin" });
            _agent.Tick();

            _agent.Tick();
            _agent.Tick();

            Assert.Equal(2, _agent.ActionsTaken);
        }

        [Fact]
        public void AnySequenceOfDestructions_LeavesTheAgentInADefinedState()
        {
            C.Target a = new C.Target { Name = "a" };
            C.Target b = new C.Target { Name = "b" };
            _agent.Remember(a);
            _agent.Remember(b);

            _agent.Tick();
            a.Kill();
            _agent.Tick();
            b.KillSilently();
            C.AgentAction final = _agent.Tick();

            Assert.Equal(C.AgentAction.ReturnedHome, final);
            Assert.Null(_agent.CurrentTarget);
        }

        [Fact]
        public void DetachAll_ClearsEverythingAndStopsListening()
        {
            C.Target threat = new C.Target { Name = "goblin" };
            _agent.Remember(threat);

            _agent.DetachAll();
            threat.Kill();

            Assert.Equal(0, _agent.ThreatCount);
        }
    }
}
