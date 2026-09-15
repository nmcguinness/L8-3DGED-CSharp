# Exercises: Delegates, events, Action and Func

Read [note 07](../notes/07-delegates-events-action-func.md) first. Exercises A and B are plain C#. Exercise C is a design task on paper and in code, still without Unity.

Do not use lambdas in exercise A. They are [note 08](../notes/08-lambdas-and-closures.md) and the point of A is to get the method-group syntax into your fingers first.

---

## A - Mechanical

### Goal

Wire up one complete event from end to end: declare it, attach two listeners, raise it without crashing, and detach one again.

### Constraints

- The event is declared using `Action<int>`, not a custom delegate type.
- It is declared with the `event` keyword.
- It is raised with null-conditional invocation.
- Subscribe using method groups, not lambdas.

### Starter code

```csharp
public class Score
{
    private int _points;

    // TODO: declare an event called PointsChanged carrying the new total

    /// <summary>
    /// Adds points to the total and notifies subscribers.
    /// </summary>
    /// <param name="amount">Points to add.</param>
    public void Add(int amount)
    {
        _points += amount;

        // TODO: raise the event safely
    }
}

public class Program
{
    public static void Main()
    {
        Score score = new Score();

        // TODO: subscribe PrintTotal and PrintMilestone
        // TODO: score.Add(10); score.Add(40);
        // TODO: unsubscribe PrintMilestone
        // TODO: score.Add(50);
    }

    private static void PrintTotal(int total)
    {
        Console.WriteLine("Total: " + total);
    }

    private static void PrintMilestone(int total)
    {
        if (total >= 50)
        {
            Console.WriteLine("Milestone reached at " + total);
        }
    }
}
```

### Done when

- The program runs and prints three totals and exactly one milestone line.
- Adding the line `score.PointsChanged = PrintTotal;` in `Main` produces a compile error. Try it, read the error, then delete the line.
- Commenting out both subscriptions and running again produces no exception.

---

## B - Applied

### Goal

Build something with three listeners that know nothing about each other, attached to a publisher that knows nothing about any of them. This is the shape the event channels take.

### The scenario

A `Furnace` has a temperature. Three things care about it:

- A `Display`, which prints the temperature every time it changes.
- An `Alarm`, which prints a warning the first time the temperature goes above 800 and stays quiet until it drops below 800 again.
- A `Logger`, which records every reading into a list and can report the highest seen.

### Constraints

- `Furnace` exposes an `event` and nothing else that the subscribers use. It must contain no reference to `Display`, `Alarm` or `Logger`.
- Each subscriber exposes an `Attach(Furnace)` and a `Detach(Furnace)` method, and detaching must genuinely stop it receiving readings.
- Use `Action<T>`. Do not declare a custom delegate type.
- The furnace must not throw when its temperature changes with nothing subscribed.
- Add a second event, raised only when the temperature crosses above 800, and have `Alarm` use that instead of filtering in its own handler. Keep both events on the furnace.

### Done when

- Attaching all three, running a sequence of temperature changes, then detaching the alarm and repeating the sequence produces the alarm output the first time and not the second.
- Detaching a subscriber that was never attached does not throw.
- Attaching the same subscriber twice causes its handler to run twice. Observe this, then decide whether `Attach` should guard against it, and implement your decision.
- `Furnace.cs` contains no type name from any of the three subscribers.
- The furnace raises both events with `?.Invoke`.

---

## C - Design

### Goal

You are designing the notification surface for a player character - deciding what it announces, and in how much detail. The character can be damaged, healed, killed, revived, can pick up an item, and can enter and leave a trigger volume.

The observers are a HUD, an achievement tracker, an audio director, and an analytics recorder. Each cares about a different subset, and more observers will be added later by people who cannot modify the character class.

### Constraints

- Produce compiling C# with trivial method bodies.
- No observer may poll. Every observer reacts to a notification.
- The character class must not name any observer type.
- Implement at least two observers fully enough to demonstrate that they receive different subsets.
- Every subscription must have a matching unsubscription, and you must demonstrate detaching an observer and showing it stops receiving.

### The choice you must justify

The design question is granularity, and it has at least three defensible answers:

- One event per thing that can happen. Six events.
- One event carrying a payload that says what happened, with observers filtering.
- Something between the two, grouping related occurrences.

There is a second decision inside the first: what each notification carries. `HealthChanged` could carry the new total, or the delta, or both, or an object describing the change. Each choice makes some observers simple and others awkward.

Write a comment at the top of your character file, eight to twelve lines, that:

- names the granularity you chose and the one you rejected;
- names one observer that your choice makes awkward, and says how awkward;
- states what happens to every existing observer when a seventh occurrence is added later, under your design and under the rejected one.

The last point is the one that separates the answers. A design that requires no observer to change when an event is added is not automatically the winner - say why you accepted the trade you accepted.

### Done when

- Six occurrences can be raised and at least two observers react to different subsets.
- Detaching an observer stops it receiving, demonstrably.
- No observer type name appears in the character class.
- All events are raised with null-conditional invocation.
- The justification comment addresses all three bullets above by name.
