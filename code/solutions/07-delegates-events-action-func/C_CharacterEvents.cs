// Exercise 07 C - worked solution. One defensible route.
//
// GRANULARITY CHOSEN: one event per occurrence - six events. Rejected: a single
// event carrying a payload describing what happened.
//
// THE OBSERVER MY CHOICE MAKES AWKWARD: the analytics recorder, which wants every
// occurrence. It must subscribe six times and unsubscribe six times, and gains a
// line of bookkeeping for each. Under the single-event design it would subscribe
// once. That is a real cost and it falls entirely on the observer that cares
// least about the distinctions.
//
// WHEN A SEVENTH OCCURRENCE IS ADDED LATER:
//   Under my design: the character gains an event; existing observers need no
//   change; any observer that wants the new occurrence adds one subscription.
//   Under the rejected design: the character gains an enum member; NO observer
//   needs to change, but every observer that switches on the payload now has an
//   unhandled case that the compiler will not warn about, because a switch over
//   an enum is not required to be exhaustive.
//
// WHY I ACCEPTED THE TRADE: the rejected design is cheaper to extend and more
// expensive to get right. Six separate events mean each observer's subscriptions
// state exactly what it depends on, and an observer cannot silently receive
// something it was never written to handle.

using System;
using System.Collections.Generic;

namespace Solutions.T07.C
{
    /// <summary>A player character that announces what happens to it.</summary>
    public class Character
    {
        private int _health = 100;

        /// <summary>Raised when the character takes damage, carrying the amount.</summary>
        public event Action<int> Damaged;

        /// <summary>Raised when the character is healed, carrying the amount.</summary>
        public event Action<int> Healed;

        /// <summary>Raised when health reaches zero.</summary>
        public event Action Killed;

        /// <summary>Raised when the character is brought back.</summary>
        public event Action Revived;

        /// <summary>Raised when an item is picked up, carrying the item name.</summary>
        public event Action<string> ItemPickedUp;

        /// <summary>Raised when a trigger volume is entered, carrying its name.</summary>
        public event Action<string> TriggerEntered;

        /// <summary>Gets the current health.</summary>
        public int Health
        {
            get { return _health; }
        }

        /// <summary>Gets a value indicating whether health has run out.</summary>
        public bool IsDead
        {
            get { return _health <= 0; }
        }

        /// <summary>Applies damage, announcing it and any resulting death.</summary>
        /// <param name="amount">Points of damage to apply.</param>
        public void TakeDamage(int amount)
        {
            if (IsDead)
            {
                return;
            }

            _health -= amount;
            Damaged?.Invoke(amount);

            if (IsDead)
            {
                Killed?.Invoke();
            }
        }

        /// <summary>Restores health, announcing it.</summary>
        /// <param name="amount">Points of health to restore.</param>
        public void Heal(int amount)
        {
            if (IsDead)
            {
                return;
            }

            _health += amount;
            Healed?.Invoke(amount);
        }

        /// <summary>Brings a dead character back to the given health.</summary>
        /// <param name="health">The health to revive with.</param>
        public void Revive(int health)
        {
            if (!IsDead)
            {
                return;
            }

            _health = health;
            Revived?.Invoke();
        }

        /// <summary>Picks up an item, announcing it.</summary>
        /// <param name="itemName">The item's name.</param>
        public void PickUp(string itemName)
        {
            ItemPickedUp?.Invoke(itemName);
        }

        /// <summary>Enters a trigger volume, announcing it.</summary>
        /// <param name="volumeName">The volume's name.</param>
        public void EnterTrigger(string volumeName)
        {
            TriggerEntered?.Invoke(volumeName);
        }
    }

    /// <summary>A head-up display. Cares only about health changing.</summary>
    public class Hud
    {
        private Action<int> _onDamaged;
        private Action<int> _onHealed;

        /// <summary>Gets how many health updates have been shown.</summary>
        public int Updates { get; private set; }

        /// <summary>Starts observing a character.</summary>
        /// <param name="character">The character to observe.</param>
        public void Attach(Character character)
        {
            _onDamaged = amount => Updates++;
            _onHealed = amount => Updates++;

            character.Damaged += _onDamaged;
            character.Healed += _onHealed;
        }

        /// <summary>Stops observing a character.</summary>
        /// <param name="character">The character to stop observing.</param>
        public void Detach(Character character)
        {
            character.Damaged -= _onDamaged;
            character.Healed -= _onHealed;
        }
    }

    /// <summary>An achievement tracker. Cares about deaths and pickups only.</summary>
    public class AchievementTracker
    {
        private Action _onKilled;
        private Action<string> _onPickedUp;

        /// <summary>Gets how many times the character has died.</summary>
        public int Deaths { get; private set; }

        /// <summary>Gets the items collected, in order.</summary>
        public List<string> Items { get; } = new List<string>();

        /// <summary>Starts observing a character.</summary>
        /// <param name="character">The character to observe.</param>
        public void Attach(Character character)
        {
            _onKilled = () => Deaths++;
            _onPickedUp = name => Items.Add(name);

            character.Killed += _onKilled;
            character.ItemPickedUp += _onPickedUp;
        }

        /// <summary>Stops observing a character.</summary>
        /// <param name="character">The character to stop observing.</param>
        public void Detach(Character character)
        {
            character.Killed -= _onKilled;
            character.ItemPickedUp -= _onPickedUp;
        }
    }
}
