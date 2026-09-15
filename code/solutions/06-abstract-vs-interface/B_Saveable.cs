// Exercise 06 B - worked solution.
//
// DESIGN CHOICE: the dirty flag and the line that clears it live in one place -
// DirtySaveableBase.Save, which is non-virtual. Subclasses supply only the
// type-specific payload through SerialisePayload, so no author can forget to
// clear the flag, whatever they write.
//
// THE ALTERNATIVE - leaving Serialise virtual and asking each subclass to clear
// the flag - is what the starting code did. It worked for two types and would
// have failed for the third, because the requirement is invisible at the point
// where a new author writes their override.
//
// SettingsFile deliberately does NOT derive from the base class. It is always
// dirty and has no flag to manage, so the family would give it nothing and cost
// it its one inheritance slot. It satisfies ISaveable directly, which is the
// whole reason the contract is an interface rather than the base class.

using System.Collections.Generic;

namespace Solutions.T06.B
{
    /// <summary>Something whose state can be written out.</summary>
    public interface ISaveable
    {
        /// <summary>Gets a value indicating whether this needs saving.</summary>
        bool IsDirty { get; }

        /// <summary>Returns this object's state as text.</summary>
        /// <returns>The serialised state.</returns>
        string Serialise();
    }

    /// <summary>
    /// Shared dirty-flag handling for saveable types that track changes.
    /// </summary>
    public abstract class DirtySaveableBase : ISaveable
    {
        private bool _isDirty;

        /// <inheritdoc />
        public bool IsDirty
        {
            get { return _isDirty; }
        }

        /// <summary>Marks this object as needing to be saved.</summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Serialises this object and clears the dirty flag. Not virtual, so the
        /// clearing cannot be lost by a subclass.
        /// </summary>
        /// <returns>The serialised state.</returns>
        public string Serialise()
        {
            _isDirty = false;
            return SerialisePayload();
        }

        /// <summary>Returns the type-specific part of the serialised state.</summary>
        /// <returns>The payload text.</returns>
        protected abstract string SerialisePayload();
    }

    /// <summary>A player profile, saved when changed.</summary>
    public class PlayerProfile : DirtySaveableBase
    {
        /// <summary>Gets or sets the player's name.</summary>
        public string Name { get; set; }

        /// <inheritdoc />
        protected override string SerialisePayload()
        {
            return "profile:" + Name;
        }
    }

    /// <summary>Level progress, saved when changed.</summary>
    public class LevelProgress : DirtySaveableBase
    {
        /// <summary>Gets or sets the furthest level reached.</summary>
        public int Index { get; set; }

        /// <inheritdoc />
        protected override string SerialisePayload()
        {
            return "level:" + Index;
        }
    }

    /// <summary>
    /// Settings, rewritten on every save. Implements the contract directly because
    /// it has no dirty flag to share.
    /// </summary>
    public class SettingsFile : ISaveable
    {
        /// <summary>Gets or sets the master volume.</summary>
        public float Volume { get; set; }

        /// <inheritdoc />
        public bool IsDirty
        {
            get { return true; }
        }

        /// <inheritdoc />
        public string Serialise()
        {
            return "settings:" + Volume;
        }
    }

    /// <summary>The save system.</summary>
    public static class SaveSystem
    {
        /// <summary>Saves every dirty item, leaving clean ones untouched.</summary>
        /// <param name="items">The items to consider.</param>
        /// <returns>One serialised string per item actually saved.</returns>
        public static List<string> SaveAll(List<ISaveable> items)
        {
            List<string> saved = new List<string>();

            foreach (ISaveable item in items)
            {
                if (item.IsDirty)
                {
                    saved.Add(item.Serialise());
                }
            }

            return saved;
        }
    }
}
