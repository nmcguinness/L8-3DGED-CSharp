// Exercise 09 A - worked solution.
//
// DESIGN CHOICE: Take resets the stored item to default(T) before returning, so
// the box does not keep the last item alive after handing it over.
//
// THE ALTERNATIVE - leaving the field set and relying on the HasItem flag - works
// and is one line shorter. It also holds a reference the caller believes it has
// taken sole possession of, which for a reference type means the object cannot be
// collected while the box lives.
//
// Swap is a generic METHOD on a non-generic static class, so the type argument is
// inferred from the arguments and never needs writing at a call site.

using System;

namespace Solutions.T09.A
{
    /// <summary>Holds at most one item of a given type.</summary>
    /// <typeparam name="T">The type of item held.</typeparam>
    public class Box<T>
    {
        private T _item;
        private bool _hasItem;

        /// <summary>Gets a value indicating whether the box currently holds an item.</summary>
        public bool HasItem
        {
            get { return _hasItem; }
        }

        /// <summary>Puts an item into the box, replacing anything already there.</summary>
        /// <param name="item">The item to store.</param>
        public void Put(T item)
        {
            _item = item;
            _hasItem = true;
        }

        /// <summary>Takes the item out of the box.</summary>
        /// <returns>The item that was stored.</returns>
        public T Take()
        {
            if (!_hasItem)
            {
                throw new InvalidOperationException("The box is empty.");
            }

            T item = _item;
            _item = default(T);         // do not keep the item alive after handing it over
            _hasItem = false;
            return item;
        }
    }

    /// <summary>Operations over boxes.</summary>
    public static class Boxes
    {
        /// <summary>Exchanges the contents of two boxes.</summary>
        /// <typeparam name="T">The type of item held by both boxes.</typeparam>
        /// <param name="first">The first box.</param>
        /// <param name="second">The second box.</param>
        public static void Swap<T>(Box<T> first, Box<T> second)
        {
            bool firstHad = first.HasItem;
            bool secondHad = second.HasItem;

            T fromFirst = firstHad ? first.Take() : default(T);
            T fromSecond = secondHad ? second.Take() : default(T);

            if (secondHad)
            {
                first.Put(fromSecond);
            }

            if (firstHad)
            {
                second.Put(fromFirst);
            }
        }
    }
}
