// Tests for the note 09 B and C solutions.
//
// The C tests are split as the exercise requires: a SHARED suite expressing only
// what IPool<T> guarantees, run against both implementations through an abstract
// base class; and an implementation-specific suite for the exhaustion behaviour.

using System;
using Xunit;
using B = Solutions.T09.B;
using C = Solutions.T09.C;

namespace Week01.Tests
{
    public class T09_Pool_B_Tests
    {
        private readonly B.Pool<B.Bullet> _pool = new B.Pool<B.Bullet>(5);

        [Fact]
        public void ANewPool_HoldsTheRequestedCapacity()
        {
            Assert.Equal(5, _pool.AvailableCount);
        }

        [Fact]
        public void EachTestGetsAFreshPool_BecauseXunitBuildsTheClassPerTest()
        {
            _pool.Get();

            Assert.Equal(4, _pool.AvailableCount);
        }

        [Fact]
        public void Get_ReducesTheAvailableCount()
        {
            _pool.Get();

            Assert.Equal(4, _pool.AvailableCount);
        }

        [Fact]
        public void Return_RestoresTheAvailableCount()
        {
            B.Bullet bullet = _pool.Get();

            _pool.Return(bullet);

            Assert.Equal(5, _pool.AvailableCount);
        }

        [Fact]
        public void Return_ResetsTheItemBeforePuttingItBack()
        {
            B.Bullet bullet = _pool.Get();
            bullet.Damage = 25;
            bullet.IsActive = true;

            _pool.Return(bullet);

            Assert.Equal(0, bullet.Damage);
            Assert.False(bullet.IsActive);
        }

        [Fact]
        public void ReturningTheSameItemTwice_IsIgnoredRatherThanCountedTwice()
        {
            B.Bullet bullet = _pool.Get();
            _pool.Return(bullet);

            bool second = _pool.Return(bullet);

            Assert.False(second);
            Assert.Equal(5, _pool.AvailableCount);
        }

        [Fact]
        public void GettingFromAnEmptyPool_CreatesANewItem()
        {
            B.Pool<B.Bullet> empty = new B.Pool<B.Bullet>(0);

            B.Bullet bullet = empty.Get();

            Assert.NotNull(bullet);
            Assert.Equal(0, empty.AvailableCount);
        }
    }

    /// <summary>
    /// The shared suite. Everything asserted here is part of the IPool contract
    /// and must hold for every implementation. It names no concrete type except
    /// in CreatePool, which subclasses supply.
    /// </summary>
    public abstract class T09_IPoolContractTests
    {
        protected abstract C.IPool<C.Bullet> CreatePool(int capacity);

        [Fact]
        public void ANewPool_HoldsTheRequestedCapacity()
        {
            Assert.Equal(4, CreatePool(4).AvailableCount);
        }

        [Fact]
        public void Get_ReturnsAUsableItem()
        {
            C.Bullet bullet = CreatePool(4).Get();

            Assert.NotNull(bullet);
        }

        [Fact]
        public void Get_ReducesTheAvailableCount()
        {
            C.IPool<C.Bullet> pool = CreatePool(4);

            pool.Get();

            Assert.Equal(3, pool.AvailableCount);
        }

        [Fact]
        public void Return_RestoresTheAvailableCount()
        {
            C.IPool<C.Bullet> pool = CreatePool(4);
            C.Bullet bullet = pool.Get();

            pool.Return(bullet);

            Assert.Equal(4, pool.AvailableCount);
        }

        [Fact]
        public void Return_ResetsTheItemBeforePuttingItBack()
        {
            C.IPool<C.Bullet> pool = CreatePool(4);
            C.Bullet bullet = pool.Get();
            bullet.Damage = 25;

            pool.Return(bullet);

            Assert.Equal(0, bullet.Damage);
        }

        [Fact]
        public void ReturningTheSameItemTwice_IsRejectedTheSecondTime()
        {
            C.IPool<C.Bullet> pool = CreatePool(4);
            C.Bullet bullet = pool.Get();
            pool.Return(bullet);

            Assert.False(pool.Return(bullet));
        }

        [Fact]
        public void AWeapon_WorksWithThisImplementationWithoutKnowingWhichItIs()
        {
            C.Weapon weapon = new C.Weapon(CreatePool(4));

            C.Bullet fired = weapon.Fire(15);

            Assert.NotNull(fired);
            Assert.Equal(15, fired.Damage);
        }

        [Fact]
        public void GettingMoreItemsThanTheCapacity_NeverReturnsNull()
        {
            C.IPool<C.Bullet> pool = CreatePool(2);

            for (int i = 0; i < 10; i++)
            {
                Assert.NotNull(pool.Get());
            }
        }
    }

    public class T09_GrowingPool_ContractTests : T09_IPoolContractTests
    {
        protected override C.IPool<C.Bullet> CreatePool(int capacity)
        {
            return new C.GrowingPool<C.Bullet>(capacity);
        }
    }

    public class T09_FixedSizePool_ContractTests : T09_IPoolContractTests
    {
        protected override C.IPool<C.Bullet> CreatePool(int capacity)
        {
            return new C.FixedSizePool<C.Bullet>(capacity);
        }
    }

    /// <summary>
    /// Implementation-specific. These assert the exhaustion behaviour each pool
    /// chose, which is deliberately NOT part of the shared contract.
    /// </summary>
    public class T09_ExhaustionBehaviourTests
    {
        [Fact]
        public void GrowingPool_WhenExhausted_CreatesNewItems()
        {
            C.GrowingPool<C.Bullet> pool = new C.GrowingPool<C.Bullet>(1);

            pool.Get();
            pool.Get();
            pool.Get();

            Assert.Equal(2, pool.TotalCreated);
        }

        [Fact]
        public void FixedSizePool_WhenExhausted_RecyclesRatherThanCreating()
        {
            C.FixedSizePool<C.Bullet> pool = new C.FixedSizePool<C.Bullet>(2);

            pool.Get();
            pool.Get();
            pool.Get();

            Assert.Equal(1, pool.RecycleCount);
        }

        [Fact]
        public void FixedSizePool_WhenExhausted_RecyclesTheOldestItemInUse()
        {
            C.FixedSizePool<C.Bullet> pool = new C.FixedSizePool<C.Bullet>(2);
            C.Bullet first = pool.Get();
            pool.Get();

            C.Bullet recycled = pool.Get();

            Assert.Same(first, recycled);
        }

        [Fact]
        public void FixedSizePool_RecycledItem_HasBeenReset()
        {
            C.FixedSizePool<C.Bullet> pool = new C.FixedSizePool<C.Bullet>(1);
            C.Bullet first = pool.Get();
            first.Damage = 42;

            C.Bullet recycled = pool.Get();

            Assert.Same(first, recycled);
            Assert.Equal(0, recycled.Damage);
        }

        [Fact]
        public void FixedSizePool_NeverExceedsItsCapacity()
        {
            C.FixedSizePool<C.Bullet> pool = new C.FixedSizePool<C.Bullet>(3);

            for (int i = 0; i < 20; i++)
            {
                pool.Get();
            }

            Assert.True(pool.AvailableCount <= pool.Capacity);
        }

        [Fact]
        public void BothImplementations_SatisfyTheSameConsumer()
        {
            C.Weapon fromGrowing = new C.Weapon(new C.GrowingPool<C.Bullet>(1));
            C.Weapon fromFixed = new C.Weapon(new C.FixedSizePool<C.Bullet>(1));

            Assert.Equal(7, fromGrowing.Fire(7).Damage);
            Assert.Equal(7, fromFixed.Fire(7).Damage);
        }
    }
}
