// Exercise 08 C - worked solution. One defensible route.
//
// STORAGE FORM CHOSEN: an object holding the action alongside a description,
// rather than a bare Action.
//
// THE ALTERNATIVE - a plain List<Action> - needs no extra type and is smaller. It
// cannot print the pending list back to the player, cannot log what ran, and
// cannot be extended to support undo without changing the storage type entirely.
// One small class buys all three.
//
// CAPTURE BEHAVIOUR CHOSEN: values are captured at RECORD time, not run time.
// Typing "move north" and then "move south" should queue two different moves, not
// two copies of the last one. This is enforced by taking the parameter as a
// method argument, which gives each recorded instruction its own variable to
// capture - the same fix as the loop-variable bug in exercise B.
//
// HOW I WOULD DEMONSTRATE IT TO A SCEPTIC: record an instruction capturing a
// local, change that local, run the queue, and assert the recorded value was used.
// That test is RecordedInstruction_CapturesTheValueAtRecordTime in the test suite.
//
// WHAT WOULD BE DIFFICULT IF UNDO WERE ADDED: an Action can be run but not
// reversed. Undo would need each instruction to carry a second delegate, or to
// capture enough state to restore what it replaced. The description field already
// gives somewhere to put that, which is part of why the object form was chosen.

using System;
using System.Collections.Generic;

namespace Solutions.T08.C
{
    /// <summary>One recorded instruction, with a description for display and logging.</summary>
    public class PendingInstruction
    {
        /// <summary>Creates a pending instruction.</summary>
        /// <param name="description">Text describing what this will do.</param>
        /// <param name="action">The work to perform when the queue runs.</param>
        public PendingInstruction(string description, Action action)
        {
            Description = description;
            Action = action;
        }

        /// <summary>Gets the text describing this instruction.</summary>
        public string Description { get; }

        /// <summary>Gets the work to perform.</summary>
        public Action Action { get; }
    }

    /// <summary>
    /// Records instructions now and performs them later. The performer knows
    /// nothing about any specific instruction.
    /// </summary>
    public class InstructionQueue
    {
        private readonly List<PendingInstruction> _pending = new List<PendingInstruction>();

        /// <summary>Gets how many instructions are waiting.</summary>
        public int PendingCount
        {
            get { return _pending.Count; }
        }

        /// <summary>Gets a description of each pending instruction, in order.</summary>
        public List<string> Describe()
        {
            List<string> lines = new List<string>();

            foreach (PendingInstruction instruction in _pending)
            {
                lines.Add(instruction.Description);
            }

            return lines;
        }

        /// <summary>Records an instruction to be performed later.</summary>
        /// <param name="description">Text describing the instruction.</param>
        /// <param name="action">The work to perform.</param>
        public void Record(string description, Action action)
        {
            _pending.Add(new PendingInstruction(description, action));
        }

        /// <summary>Discards everything pending without performing any of it.</summary>
        public void Discard()
        {
            _pending.Clear();
        }

        /// <summary>
        /// Performs every pending instruction in the order recorded, then clears
        /// the queue, so a second call does nothing.
        /// </summary>
        /// <returns>How many instructions were performed.</returns>
        public int Run()
        {
            int count = _pending.Count;

            foreach (PendingInstruction instruction in _pending)
            {
                instruction.Action();
            }

            _pending.Clear();
            return count;
        }
    }

    /// <summary>The world the instructions act upon.</summary>
    public class World
    {
        /// <summary>Gets the log of everything that has happened, in order.</summary>
        public List<string> Log { get; } = new List<string>();

        /// <summary>Gets the player's current room.</summary>
        public string Room { get; private set; } = "hall";

        /// <summary>Moves the player to a room.</summary>
        /// <param name="room">The room to move to.</param>
        public void Move(string room)
        {
            Room = room;
            Log.Add("moved to " + room);
        }

        /// <summary>Takes an item.</summary>
        /// <param name="item">The item to take.</param>
        public void Take(string item)
        {
            Log.Add("took " + item);
        }

        /// <summary>Looks around the current room.</summary>
        public void Look()
        {
            Log.Add("looked around " + Room);
        }

        /// <summary>Waits for a moment.</summary>
        public void Wait()
        {
            Log.Add("waited");
        }
    }

    /// <summary>
    /// Builds instructions for a world. Each parameterised method takes its value
    /// as an argument, which gives every recorded lambda its own variable to
    /// capture and so fixes the value at record time.
    /// </summary>
    public static class Instructions
    {
        /// <summary>Records a move to a named room.</summary>
        /// <param name="queue">The queue to record into.</param>
        /// <param name="world">The world to act upon.</param>
        /// <param name="room">The destination room, captured now.</param>
        public static void RecordMove(InstructionQueue queue, World world, string room)
        {
            queue.Record("move to " + room, () => world.Move(room));
        }

        /// <summary>Records taking a named item.</summary>
        /// <param name="queue">The queue to record into.</param>
        /// <param name="world">The world to act upon.</param>
        /// <param name="item">The item to take, captured now.</param>
        public static void RecordTake(InstructionQueue queue, World world, string item)
        {
            queue.Record("take " + item, () => world.Take(item));
        }

        /// <summary>Records looking around.</summary>
        /// <param name="queue">The queue to record into.</param>
        /// <param name="world">The world to act upon.</param>
        public static void RecordLook(InstructionQueue queue, World world)
        {
            queue.Record("look", () => world.Look());
        }

        /// <summary>Records waiting.</summary>
        /// <param name="queue">The queue to record into.</param>
        /// <param name="world">The world to act upon.</param>
        public static void RecordWait(InstructionQueue queue, World world)
        {
            queue.Record("wait", () => world.Wait());
        }
    }
}
