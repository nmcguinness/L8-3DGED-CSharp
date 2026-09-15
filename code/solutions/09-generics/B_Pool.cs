// Exercise 09 B - worked solution.
//
// THE CONSTRAINTS, and the error each one answers:
//   where T : IPoolable   - without it, `item.Reset()` is refused: 'T' does not
//                           contain a definition for 'Reset'.
//   where T : new()       - without it, `new T()` is refused: cannot create an
//                           instance of the variable type 'T'.
// Both were added in response to a specific compiler error rather than written
// up front, which is the method the note recommends.
//
// RETURNING THE SAME ITEM TWICE is a silent no-op rather than an exception.
// DESIGN CHOICE: a double return is a caller bug, but it is a recoverable one -
// the pool's state is still correct if the second return is ignored. Throwing
// would turn a harmless mistake into a crash during play. The count returned by
// Return lets a caller detect it if they care.
//
// THE ALTERNATIVE - throwing - would catch the bug at the point it happens rather
// than letting it pass, which is the stronger argument for a library. For a pool
// running inside a game loop, not crashing wins.

using System.Collections.Generic;

namespace Solutions.T09.B
{
    /// <summary>An object that can be returned to a pool and reused.</summary>
    public interface IPoolable
    {
        /// <summary>Returns this object to its unused state.</summary>
        void Reset();
    }

    /// <summary>A projectile that can be pooled.</summary>
    public class Bullet : IPoolable
    {
        /// <summary>Gets or sets the speed.</summary>
        public float Speed { get; set; }

        /// <summary>Gets or sets the damage dealt.</summary>
        public int Damage { get; set; }

        /// <summary>Gets or sets whether this bullet is in flight.</summary>
        public bool IsActive { get; set; }

        /// <inheritdoc />
        public void Reset()
        {
            Speed = 0f;
            Damage = 0;
            IsActive = false;
        }
    }

    /// <summary>Reuses instances instead of allocating new ones.</summary>
    /// <typeparam name="T">The type of item held by this pool.</typeparam>
    public class Pool<T> where T : IPoolable, new()
    {
        private readonly List<T> _available = new List<T>();

        /// <summary>Creates a pool pre-filled with the given number of items.</summary>
        /// <param name="initialCapacity">How many items to create up front.</param>
        public Pool(int initialCapacity)
        {
            for (int i = 0; i < initialCapacity; i++)
            {
                _available.Add(new T());
            }
        }

        /// <summary>Gets how many items are currently waiting to be taken.</summary>
        public int AvailableCount
        {
            get { return _available.Count; }
        }

        /// <summary>Takes an item, creating one when the pool is empty.</summary>
        /// <returns>An item ready for use.</returns>
        public T Get()
        {
            if (_available.Count == 0)
            {
                return new T();
            }

            int last = _available.Count - 1;
            T item = _available[last];
            _available.RemoveAt(last);
            return item;
        }

        /// <summary>
        /// Resets an item and returns it to the pool. Returning an item that is
        /// already in the pool is ignored.
        /// </summary>
        /// <param name="item">The item to return.</param>
        /// <returns>True when the item was added, false when it was already present.</returns>
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
}
