# Exercises: Lambdas and closures

Read [note 08](../notes/08-lambdas-and-closures.md) first. Plain C# throughout.

---

## A - Mechanical

### Goal

Rewrite some ordinary methods as lambdas of each shape, then watch a captured variable change underneath you. The syntax is the easy half of this.

### Starter code

```csharp
public class Program
{
    private static int _threshold = 50;

    public static void Main()
    {
        // TODO: assign a lambda equivalent to Reset
        Action reset = null;

        // TODO: assign a lambda equivalent to Log
        Action<string> log = null;

        // TODO: assign a lambda equivalent to Double
        Func<int, int> twice = null;

        // TODO: assign a lambda that returns true when the value
        //       is below the _threshold field
        Func<int, bool> isLow = null;

        Console.WriteLine(isLow(40));       // expect True

        _threshold = 10;

        Console.WriteLine(isLow(40));       // what does this print?
    }

    private static void Reset() { Console.WriteLine("reset"); }

    private static void Log(string message) { Console.WriteLine(message); }

    private static int Double(int value) { return value * 2; }
}
```

### Constraints

- Every lambda uses the shortest form that compiles: no parameter types where they can be inferred, no braces around a single expression, no `return` where it is not needed.
- Do not change `_threshold` to a local, and do not change the two `Console.WriteLine(isLow(40))` lines.

### Done when

- The program compiles and runs with no `null` delegates remaining.
- You can state, before running it, what the second `isLow(40)` prints and why.
- Run it and confirm you were right.

---

## B - Applied

### Goal

The program below compiles, runs, and throws nothing. It is also wrong in three separate ways, and every one of them is a closure bug. Find them and fix them.

### Starter code

This program is supposed to print `Slot 0`, `Slot 1`, `Slot 2` when the three buttons are pressed in order, and then print nothing at all after the menu is closed. It does neither.

```csharp
public class Button
{
    public event Action Pressed;

    public void Press()
    {
        Pressed?.Invoke();
    }
}

public class Menu
{
    private readonly List<Button> _buttons;

    public Menu(List<Button> buttons)
    {
        _buttons = buttons;
    }

    public void Open()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].Pressed += () => SelectSlot(i);
        }
    }

    public void Close()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].Pressed -= () => SelectSlot(i);
        }
    }

    private void SelectSlot(int slot)
    {
        Console.WriteLine("Slot " + slot);
    }
}

public class Program
{
    public static void Main()
    {
        List<Button> buttons = new List<Button> { new Button(), new Button(), new Button() };
        Menu menu = new Menu(buttons);

        menu.Open();
        foreach (Button button in buttons) { button.Press(); }

        menu.Close();
        Console.WriteLine("--- menu closed ---");
        foreach (Button button in buttons) { button.Press(); }

        menu.Open();
        menu.Open();
        Console.WriteLine("--- opened twice ---");
        buttons[0].Press();
    }
}
```

### Constraints

- Do not change `Button`, and do not change `Main`.
- Fix the wrong slot numbers.
- Fix the failure to unsubscribe.
- Decide what should happen when `Open` is called twice without an intervening `Close`, and make that happen. State your decision in a one-line comment.
- The output after `--- menu closed ---` must be nothing at all.

### Done when

- Pressing the three buttons prints `Slot 0`, `Slot 1`, `Slot 2` in order.
- Pressing them after `Close` prints nothing.
- Pressing button zero after two `Open` calls prints `Slot 0` exactly once, or exactly twice if that was your stated decision - but it does what your comment says it does.
- Running the program twice produces identical output.

---

## C - Design

### Goal

Build something that lets one part of a program record an action now and another part run it later, with neither end knowing much about the other: the recorder has no idea what the action does, and the performer has no idea where it came from.

Demonstrate it with a text adventure: the player types several instructions, and nothing happens until they type `go`, at which point everything runs in order.

### Constraints

- The queue of pending work stores something callable. It must not store strings that are later parsed, and it must not store an enum that is later switched on.
- Support at least four different kinds of instruction, at least two of which take a parameter supplied when the instruction is recorded, not when it is run.
- Provide a way to discard everything pending without running it.
- Provide a way to run everything and then clear the queue, so that a second `go` does nothing.
- The performer must contain no knowledge of any specific instruction.

### The choice you must justify

Each pending item can be stored as a bare `Action`, or as an object that holds the action along with a description of itself.

The bare `Action` is smaller and needs no extra types. The object version costs you a class but lets you print the pending list back to the player, log what ran, and - relevantly to later topics - do things with an item beyond calling it.

There is a related question: if a recorded instruction captures a variable that changes between recording and running, should it act on the value at record time or at run time? Note 04 tells you which one you get by default. Decide which one you *want*, and make your implementation do that deliberately rather than accidentally.

Write a comment at the top of your main file, eight to twelve lines, covering:

- which storage form you chose and what the other would have cost;
- which capture behaviour you chose, how you enforced it, and how you would demonstrate to a sceptic that it does what you say;
- one thing your design would make difficult if the requirement changed to "undo the last instruction that ran".

### Done when

- Four or more instruction kinds can be queued, two of them parameterised at record time.
- Typing `go` runs everything in the order it was recorded.
- A second `go` immediately afterwards does nothing.
- Discarding works and leaves the queue empty.
- A test exists that records an instruction, changes the captured variable, runs the queue, and asserts the behaviour your comment claims.
- The performer contains no instruction-specific code.
