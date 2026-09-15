// Exercise 09 C - worked solution. One defensible route.
//
// EXHAUSTION BEHAVIOUR CHOSEN for the fixed-size pool: recycle the oldest item
// currently in use, rather than throwing or returning null.
//
// FOR WHOM IT IS RIGHT: bullets, impact effects, damage numbers - anything where
// the oldest one disappearing is less noticeable than the newest one failing to
// appear. A gun that stops firing because the pool is full is a bug the player
// sees; a bullet vanishing at the far end of its flight is not.
//
// WHERE IT WOULD BE WRONG: audio sources holding a music track, or anything
// carrying state that matters after it is handed out. Recycling silently steals
// an object another system believes it owns, and the symptom is a sound cutting
// out for no visible reason. For those, throwing is correct - the caller needs to
// know the budget was exceeded.
//
// THE SIGNATURE OF GET: it returns T. The alternative, `bool TryGet(out T)`, makes
// exhaustion impossible to ignore and would have been the better choice for the
// throwing variant. It is uglier at every call site, and under the recycling
// policy Get can always succeed, so the failure it guards against cannot happen.
//
// CONSTRAINTS ON T: `where T : IPoolable, new()`. What that gives up is any type
// the pool cannot construct itself - which excludes every MonoBehaviour, because
// Unity constructs components rather than user code. A pool of prefabs
// therefore cannot use this interface unchanged; it needs a factory delegate
// instead of the new() constraint.

using System;
using System.Collections.Generic;

namespace Solutions.T09.C
{
    /// <summary>An object that can be returned to a pool and reused.</summary>
    public interface IPoolable
    {
        /// <summary>Returns this object to its unused state.</summary>
        void Reset();
    }

    /// <summary>A store of reusable instances.</summary>
    /// <typeparam name="T">The type of item held.</typeparam>
    public interface IPool<T> where T : IPoolable, new()
    {
        /// <summary>Gets how many items are currently waiting to be taken.</summary>
        int AvailableCount { get; }

        /// <summary>Takes an item from the pool.</summary>
        /// <returns>An item ready for use.</returns>
        T Get();

        /// <summary>Resets an item and returns it to the pool.</summary>
        /// <param name="item">The item to return.</param>
        /// <returns>True when the item was added, false when it was already present.</returns>
        bool Return(T item);
    }

    /// <summary>A projectile that can be pooled.</summary>
    public class Bullet : IPoolable
    {
        /// <summary>Gets or sets the damage dealt.</summary>
        public int Damage { get; set; }

        /// <inheritdoc />
        public void Reset()
        {
            Damage = 0;
        }
    }

    /// <summary>A pool that creates a new item whenever it runs out.</summary>
    /// <typeparam name="T">The type of item held.</typeparam>
    public class GrowingPool<T> : IPool<T> where T : IPoolable, new()
    {
        private readonly List<T> _available = new List<T>();

        /// <summary>Creates a pool pre-filled with the given number of items.</summary>
        /// <param name="initialCapacity">How many items to create up front.</param>
        public GrowingPool(int initialCapacity)
        {
            for (int i = 0; i < initialCapacity; i++)
            {
                _available.Add(new T());
            }
        }

        /// <summary>Gets how many items have ever been created by this pool.</summary>
        public int TotalCreated { get; private set; }

        /// <inheritdoc />
        public int AvailableCount
        {
            get { return _available.Count; }
        }

        /// <inheritdoc />
        public T Get()
        {
            if (_available.Count == 0)
            {
                TotalCreated++;
                return new T();
            }

            int last = _available.Count - 1;
            T item = _available[last];
            _available.RemoveAt(last);
            return item;
        }

        /// <inheritdoc />
        public bool Return(T item)
        {
            if (_available.Contains(item))
            {
                return false;
            }

            item.Reset();
            _available.Add(item);
            return true;
        }
    }

    /// <summary>
    /// A pool with a fixed budget. When exhausted it recycles the item that has
    /// been in use longest rather than creating a new one.
    /// </summary>
    /// <typeparam name="T">The type of item held.</typeparam>
    public class FixedSizePool<T> : IPool<T> where T : IPoolable, new()
    {
        private readonly List<T> _available = new List<T>();
        private readonly Queue<T> _inUse = new Queue<T>();
        private readonly int _capacity;

        /// <summary>Creates a pool holding exactly the given number of items.</summary>
        /// <param name="capacity">The fixed number of items.</param>
        public FixedSizePool(int capacity)
        {
            _capacity = capacity;

            for (int i = 0; i < capacity; i++)
            {
                _available.Add(new T());
            }
        }

        /// <summary>Gets how many times an in-use item has been recycled.</summary>
        public int RecycleCount { get; private set; }

        /// <summary>Gets the fixed number of items this pool holds.</summary>
        public int Capacity
        {
            get { return _capacity; }
        }

        /// <inheritdoc />
        public int AvailableCount
        {
            get { return _available.Count; }
        }

        /// <inheritdoc />
        public T Get()
        {
            if (_available.Count == 0)
            {
                // Exhausted: steal the item that has been out longest.
                T recycled = _inUse.Dequeue();
                recycled.Reset();
                RecycleCount++;
                _inUse.Enqueue(recycled);
                return recycled;
            }

            int last = _available.Count - 1;
            T item = _available[last];
            _available.RemoveAt(last);
            _inUse.Enqueue(item);
            return item;
        }

        /// <inheritdoc />
        public bool Return(T item)
        {
            if (_available.Contains(item))
            {
                return false;
            }

            if (_available.Count >= _capacity)
            {
                return false;           // the budget is full; nothing to do
            }

            item.Reset();
            _available.Add(item);
            return true;
        }
    }

    /// <summary>Fires bullets taken from whatever pool it was given.</summary>
    public class Weapon
    {
        private readonly IPool<Bullet> _pool;

        /// <summary>Creates a weapon drawing from the supplied pool.</summary>
        /// <param name="pool">The pool supplying bullets.</param>
        public Weapon(IPool<Bullet> pool)
        {
            _pool = pool;
        }

        /// <summary>Takes a bullet from the pool and arms it.</summary>
        /// <param name="damage">The damage to arm the bullet with.</param>
        /// <returns>The bullet fired.</returns>
        public Bullet Fire(int damage)
        {
            Bullet bullet = _pool.Get();
            bullet.Damage = damage;
            return bullet;
        }
    }
}
