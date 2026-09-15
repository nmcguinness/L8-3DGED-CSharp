// Exercise 10 A - worked solution.
//
// DESIGN CHOICE: each method uses the collection suited to how the data is read -
// a Dictionary for lookup by name, a Queue for oldest-first processing, and a
// List plus a sorted copy for the top three.
//
// THE ALTERNATIVE for CountWords would be a List of pairs plus a Contains check
// per word. It is fewer concepts and quadratic: every word walks the whole list
// so far, so the cost grows with the square of the input.
//
// TopThree sorts a COPY. Sorting the caller's list in place would reorder data
// they may be holding for another purpose, which the note flags as the main
// practical difference from LINQ's OrderBy.

using System;
using System.Collections.Generic;

namespace Solutions.T10.A
{
    /// <summary>Choosing a collection by how the data will be read.</summary>
    public static class CollectionChoice
    {
        /// <summary>Counts how many times each word appears.</summary>
        /// <param name="text">The text to count, words separated by spaces.</param>
        /// <returns>A count per distinct word.</returns>
        public static Dictionary<string, int> CountWords(string text)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(text))
            {
                return counts;
            }

            foreach (string word in text.Split(' '))
            {
                if (word.Length == 0)
                {
                    continue;
                }

                int existing;
                counts.TryGetValue(word, out existing);     // one lookup, no throw
                counts[word] = existing + 1;
            }

            return counts;
        }

        /// <summary>Processes commands oldest first until none remain.</summary>
        /// <param name="commands">The commands to process, in the order issued.</param>
        /// <returns>The commands in the order they were processed.</returns>
        public static List<string> ProcessInOrder(string[] commands)
        {
            Queue<string> pending = new Queue<string>();

            foreach (string command in commands)
            {
                pending.Enqueue(command);
            }

            List<string> processed = new List<string>();

            while (pending.Count > 0)
            {
                processed.Add(pending.Dequeue());
            }

            return processed;
        }

        /// <summary>
        /// Returns the three highest scores, highest first, without reordering the
        /// list supplied.
        /// </summary>
        /// <param name="scores">The scores to consider.</param>
        /// <returns>Up to three scores, highest first.</returns>
        public static List<int> TopThree(List<int> scores)
        {
            List<int> copy = new List<int>(scores);         // do not disturb the caller
            copy.Sort((a, b) => b.CompareTo(a));            // descending

            List<int> top = new List<int>();

            for (int i = 0; i < copy.Count && i < 3; i++)
            {
                top.Add(copy[i]);
            }

            return top;
        }
    }
}
