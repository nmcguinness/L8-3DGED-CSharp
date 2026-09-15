// Exercise 08 B - worked solution.
//
// THE THREE BUGS, and the fixes:
//
// 1. The for loop captured `i` itself, so all three lambdas shared one variable
//    and every button selected the value that ended the loop. FIX: copy the loop
//    variable into a fresh local inside the body and capture the copy.
//
// 2. Close subscribed a NEW lambda to -=, which removed nothing, because -=
//    matches by reference identity and two identical-looking lambdas are two
//    objects. FIX: store the handler created in Open so the exact same delegate
//    can be removed.
//
// 3. Open could run twice, subscribing every handler a second time. DECISION:
//    Open is idempotent - calling it while already open does nothing. The
//    alternative, subscribing again, is never what a menu wants and produces
//    duplicate selections that are hard to trace.
//
// DESIGN CHOICE: the handlers are kept in a list parallel to the buttons. The
// alternative - a Dictionary<Button, Action> - reads slightly better and costs a
// hash per button for a collection that is always walked in full anyway.

using System;
using System.Collections.Generic;

namespace Solutions.T08.B
{
    /// <summary>A button that can be pressed.</summary>
    public class Button
    {
        /// <summary>Raised when this button is pressed.</summary>
        public event Action Pressed;

        /// <summary>Gets the number of handlers currently subscribed.</summary>
        public int SubscriberCount
        {
            get { return Pressed == null ? 0 : Pressed.GetInvocationList().Length; }
        }

        /// <summary>Presses the button, notifying subscribers.</summary>
        public void Press()
        {
            Pressed?.Invoke();
        }
    }

    /// <summary>A menu whose buttons each select their own slot.</summary>
    public class Menu
    {
        private readonly List<Button> _buttons;
        private readonly List<Action> _handlers = new List<Action>();

        /// <summary>Creates a menu over the supplied buttons.</summary>
        /// <param name="buttons">The buttons, in slot order.</param>
        public Menu(List<Button> buttons)
        {
            _buttons = buttons;
        }

        /// <summary>Gets the slots selected so far, in order.</summary>
        public List<int> Selections { get; } = new List<int>();

        /// <summary>Gets a value indicating whether the menu is currently open.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Opens the menu, wiring each button to its own slot. Calling this while
        /// already open does nothing, so handlers cannot be subscribed twice.
        /// </summary>
        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                int slot = i;                       // fix 1: a fresh variable per iteration
                Action handler = () => SelectSlot(slot);

                _handlers.Add(handler);             // fix 2: keep the exact delegate
                _buttons[i].Pressed += handler;
            }

            IsOpen = true;
        }

        /// <summary>Closes the menu, removing every handler it added.</summary>
        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Pressed -= _handlers[i];
            }

            _handlers.Clear();
            IsOpen = false;
        }

        private void SelectSlot(int slot)
        {
            Selections.Add(slot);
        }
    }
}
