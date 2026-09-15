// Exercise 10 B - worked solution.
//
// REPLAY AND UNDO PULL IN OPPOSITE DIRECTIONS. DESIGN CHOICE: one List<Command>
// holding the history in issue order, with replay walking it forwards and undo
// taking from the end. A List supports both cheaply; a Queue would make undo
// awkward and a Stack would make replay awkward, and keeping two collections in
// step would be a bug waiting to happen.
//
// THE ALTERNATIVE - a Stack for undo plus a Queue for replay - reads more
// directly for each operation and doubles the state that has to stay consistent.
// Prune would have to rebuild both.
//
// THE FACT STORE stores object and checks the type on the way out, so TryGet
// returns false rather than throwing when the stored type does not match what the
// caller asked for. A Dictionary<string, object> is the only way to hold values of
// different types behind one key space; the cast is contained in one method.

using System.Collections.Generic;

namespace Solutions.T10.B
{
    /// <summary>A recorded command.</summary>
    public class Command
    {
        /// <summary>Creates a command.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="value">The command's numeric argument.</param>
        public Command(string name, int value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>Gets the command name.</summary>
        public string Name { get; }

        /// <summary>Gets the command's numeric argument.</summary>
        public int Value { get; }
    }

    /// <summary>A history of commands supporting replay in order and undo from the end.</summary>
    public class CommandHistory
    {
        private readonly List<Command> _commands = new List<Command>();

        /// <summary>Gets how many commands are recorded.</summary>
        public int Count
        {
            get { return _commands.Count; }
        }

        /// <summary>Records a command.</summary>
        /// <param name="command">The command to record.</param>
        public void Record(Command command)
        {
            _commands.Add(command);
        }

        /// <summary>Returns every command in the order it was issued.</summary>
        /// <returns>The command names, oldest first.</returns>
        public List<string> ReplayAll()
        {
            List<string> names = new List<string>();

            foreach (Command command in _commands)
            {
                names.Add(command.Name);
            }

            return names;
        }

        /// <summary>Removes and returns the most recently recorded command.</summary>
        /// <returns>The command removed, or null when the history is empty.</returns>
        public Command UndoLast()
        {
            if (_commands.Count == 0)
            {
                return null;
            }

            int last = _commands.Count - 1;
            Command command = _commands[last];
            _commands.RemoveAt(last);
            return command;
        }

        /// <summary>Keeps only the most recent commands, discarding older ones.</summary>
        /// <param name="maxCount">How many commands to keep.</param>
        public void Prune(int maxCount)
        {
            if (maxCount < 0)
            {
                maxCount = 0;
            }

            while (_commands.Count > maxCount)
            {
                _commands.RemoveAt(0);          // drop the oldest
            }
        }
    }

    /// <summary>A store of named facts of differing types.</summary>
    public class FactStore
    {
        private readonly Dictionary<string, object> _facts = new Dictionary<string, object>();

        /// <summary>Stores a fact, replacing any existing value for the key.</summary>
        /// <param name="key">The fact name.</param>
        /// <param name="value">The value to store.</param>
        public void Set(string key, object value)
        {
            _facts[key] = value;
        }

        /// <summary>
        /// Attempts to read a fact as a given type. Returns false when the key is
        /// missing and when it holds a different type. Never throws.
        /// </summary>
        /// <typeparam name="T">The type expected.</typeparam>
        /// <param name="key">The fact name.</param>
        /// <param name="value">The value found, or the type default.</param>
        /// <returns>True when the key existed and held a value of type T.</returns>
        public bool TryGet<T>(string key, out T value)
        {
            object stored;

            if (!_facts.TryGetValue(key, out stored) || !(stored is T))
            {
                value = default(T);
                return false;
            }

            value = (T)stored;
            return true;
        }

        /// <summary>Reports whether a fact exists under the given name.</summary>
        /// <param name="key">The fact name.</param>
        /// <returns>True when the key is present.</returns>
        public bool Has(string key)
        {
            return _facts.ContainsKey(key);
        }

        /// <summary>Removes a fact.</summary>
        /// <param name="key">The fact name.</param>
        /// <returns>True when a fact was removed.</returns>
        public bool Remove(string key)
        {
            return _facts.Remove(key);
        }
    }
}
