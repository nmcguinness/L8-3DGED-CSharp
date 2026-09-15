// Tests for the note 03 B and C solutions.
//
// The B tests include the dictionary case, which is the one that catches a
// GetHashCode inconsistent with ==. It fails silently rather than throwing, so
// nothing else would find it.

using System;
using System.Collections.Generic;
using Xunit;
using B = Solutions.T03.B;
using C = Solutions.T03.C;

namespace Week01.Tests
{
    public class T03_ItemId_B_Tests
    {
        [Fact]
        public void TwoDistinctObjectsWithTheSameValue_AreEqualByOperator()
        {
            B.ItemId a = new B.ItemId(7);
            B.ItemId b = new B.ItemId(7);

            Assert.NotSame(a, b);
            Assert.True(a == b);
        }

        [Fact]
        public void TwoDistinctObjectsWithTheSameValue_AreEqualByEquals()
        {
            B.ItemId a = new B.ItemId(7);
            B.ItemId b = new B.ItemId(7);

            Assert.True(a.Equals(b));
        }

        [Fact]
        public void TwoDistinctObjectsWithTheSameValue_ProduceTheSameHashCode()
        {
            B.ItemId a = new B.ItemId(7);
            B.ItemId b = new B.ItemId(7);

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ObjectsWithDifferentValues_AreNotEqual()
        {
            Assert.True(new B.ItemId(7) != new B.ItemId(8));
        }

        [Fact]
        public void Equality_IsSymmetric()
        {
            B.ItemId a = new B.ItemId(7);
            B.ItemId b = new B.ItemId(7);

            Assert.Equal(a == b, b == a);
        }

        [Fact]
        public void ComparingToNull_IsFalseFromEitherSideAndThrowsNothing()
        {
            B.ItemId a = new B.ItemId(7);

            Assert.False(a == null);
            Assert.False(null == a);
        }

        [Fact]
        public void ComparingNullToNull_IsTrue()
        {
            B.ItemId a = null;
            B.ItemId b = null;

            Assert.True(a == b);
        }

        [Fact]
        public void AnEqualButDistinctKey_FindsTheEntryInADictionary()
        {
            Dictionary<B.ItemId, string> map = new Dictionary<B.ItemId, string>();
            map[new B.ItemId(7)] = "sword";

            Assert.True(map.ContainsKey(new B.ItemId(7)));
            Assert.Equal("sword", map[new B.ItemId(7)]);
        }

        [Fact]
        public void AnEqualButDistinctItem_IsFoundByListContains()
        {
            List<B.ItemId> list = new List<B.ItemId> { new B.ItemId(7) };

            Assert.Contains(new B.ItemId(7), list);
        }
    }

    public class T03_EngineObject_C_Tests
    {
        [Fact]
        public void ALiveObject_IsNotEqualToNull()
        {
            C.EngineObject live = new C.EngineObject { Name = "Goblin" };

            Assert.False(live == null);
            Assert.True(live != null);
        }

        [Fact]
        public void ADestroyedObject_ReportsAsEqualToNullThroughTheOperator()
        {
            C.EngineObject destroyed = new C.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            Assert.True(destroyed == null);
        }

        [Fact]
        public void ADestroyedObject_IsStillALiveReferenceAccordingToReferenceEquals()
        {
            C.EngineObject destroyed = new C.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            Assert.False(ReferenceEquals(destroyed, null));
        }

        [Fact]
        public void ADestroyedObject_IsNotSeenAsNullByTheIsNullPattern()
        {
            C.EngineObject destroyed = new C.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            Assert.False(destroyed is null);
        }

        [Fact]
        public void TwoDistinctDestroyedObjects_CompareEqualBecauseBothReadAsAbsent()
        {
            C.EngineObject first = new C.EngineObject { Name = "A" };
            C.EngineObject second = new C.EngineObject { Name = "B" };
            first.Destroy();
            second.Destroy();

            Assert.True(first == second);
        }

        [Fact]
        public void TwoDistinctLiveObjects_AreNotEqual()
        {
            C.EngineObject first = new C.EngineObject { Name = "A" };
            C.EngineObject second = new C.EngineObject { Name = "B" };

            Assert.False(first == second);
        }

        [Fact]
        public void GetHashCode_DoesNotChangeWhenTheObjectIsDestroyed()
        {
            C.EngineObject obj = new C.EngineObject { Name = "Goblin" };
            int before = obj.GetHashCode();

            obj.Destroy();

            Assert.Equal(before, obj.GetHashCode());
        }

        [Fact]
        public void AnObjectUsedAsADictionaryKey_IsStillFoundAfterBeingDestroyed()
        {
            C.EngineObject key = new C.EngineObject { Name = "Goblin" };
            Dictionary<C.EngineObject, string> map = new Dictionary<C.EngineObject, string>();
            map[key] = "entry";

            key.Destroy();

            Assert.True(map.ContainsKey(key));
        }

        [Fact]
        public void UnsafeUse_ThrowsForADestroyedObject_BecauseNullConditionalBypassesTheOperator()
        {
            C.EngineObject destroyed = new C.EngineObject { Name = "Goblin" };
            destroyed.Destroy();

            Assert.Throws<InvalidOperationException>(() => C.NullHandling.UnsafeUse(destroyed));
        }

        [Fact]
        public void SafeUse_ThrowsForNoneOfTheThreeCases()
        {
            C.EngineObject live = new C.EngineObject { Name = "Alive" };
            C.EngineObject destroyed = new C.EngineObject { Name = "Dead" };
            destroyed.Destroy();

            Assert.Equal("Alive did some work.", C.NullHandling.SafeUse(live));
            Assert.Equal("(skipped - null or destroyed)", C.NullHandling.SafeUse(destroyed));
            Assert.Equal("(skipped - null or destroyed)", C.NullHandling.SafeUse(null));
        }
    }
}
