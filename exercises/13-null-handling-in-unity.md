# Exercises: Null handling in Unity

Read [note 13](../notes/13-null-handling-in-unity.md) first.

These run in a plain console project. Exercise B has you build a stand-in that overloads `==` the way `UnityEngine.Object` does, which is the most direct way to see for yourself why `?.` and `??` behave as they do. Building the trap is a better way to understand it than reading about it.

---

## A - Mechanical

### Goal

No code for this one. Read the ten lines below and work out which null checks are safe and which will quietly let you down, before you go anywhere near a compiler.

### The listing

Assume `Enemy`, `Transform` and `HealthBar` derive from `UnityEngine.Object`, and that `Inventory` and `SaveData` are plain C# classes.

```csharp
 1  if (_currentTarget != null) { Chase(_currentTarget); }
 2  _healthBar?.Refresh(_health);
 3  _inventory ??= new Inventory();
 4  Enemy target = _lastSeen ?? FindNearest();
 5  if (_saveData is null) { _saveData = new SaveData(); }
 6  if (_muzzleTransform == null) { Debug.LogError("Muzzle not assigned"); }
 7  string weaponName = _inventory?.Equipped?.Name ?? "unarmed";
 8  if (_currentTarget is not null) { Chase(_currentTarget); }
 9  transform.parent?.SendMessage("Hit");
10  _onDeath?.Invoke();
```

### Task

For each of the ten lines, write one line stating:

- safe or unsafe;
- if unsafe, what will actually happen and when;
- if unsafe, the corrected line.

Line 10 is a delegate rather than an engine object. Say what makes it different.

### Done when

- All ten lines are classified.
- You have identified exactly which of them are unsafe. There are four.
- Each correction you propose compiles if you type it out.
- You can state the single rule that decides every case, in one sentence.

---

## B - Applied

### Goal

Build the trap yourself. You will recreate Unity's null behaviour in plain C#, then use it to catch yourself out in each of the ways it catches everybody else out.

### Part one: build the stand-in

```csharp
/// <summary>
/// Stands in for UnityEngine.Object. A destroyed instance reports as null
/// through == but is still a live reference.
/// </summary>
public class EngineObject
{
    private bool _isDestroyed;

    public string Name { get; set; }

    /// <summary>
    /// Marks this object as destroyed.
    /// </summary>
    public void Destroy()
    {
        _isDestroyed = true;
    }

    // TODO: override operator == so that comparing a destroyed object
    //       to null returns true. Handle the case where either side is
    //       genuinely null. You will need operator != as well, and the
    //       compiler will insist on Equals and GetHashCode; override
    //       them to be consistent with your ==.

    /// <summary>
    /// Performs work that requires the underlying object to exist.
    /// </summary>
    public void DoWork()
    {
        if (_isDestroyed)
        {
            throw new InvalidOperationException(
                "The object of type EngineObject has been destroyed but you are still trying to access it.");
        }

        Console.WriteLine(Name + " did some work.");
    }
}
```

### Part two: demonstrate the divergence

Write a program that creates an `EngineObject`, destroys it, and then prints the result of each of the following. Predict every answer before running it.

```csharp
EngineObject obj = new EngineObject { Name = "Goblin" };
obj.Destroy();

Console.WriteLine(obj == null);
Console.WriteLine(obj != null);
Console.WriteLine(obj is null);
Console.WriteLine(ReferenceEquals(obj, null));
Console.WriteLine(obj?.Name ?? "no name");

EngineObject fallback = obj ?? new EngineObject { Name = "Replacement" };
Console.WriteLine(fallback.Name);
```

Then write two methods and demonstrate the difference:

- `UnsafeUse(EngineObject o)` using `o?.DoWork()`.
- `SafeUse(EngineObject o)` using `if (o != null)`.

Call both with a live object, a destroyed object, and a genuinely null reference.

### Constraints

- Your `==` must handle a genuinely null left side, a genuinely null right side, and both being null, without recursing infinitely. Comparing the parameter to `null` with `==` inside your own `==` will call itself; use `ReferenceEquals`.
- `SafeUse` must not throw for any of the three inputs.
- Do not use a try/catch to make `UnsafeUse` pass. The point is that it fails.

### Done when

- All six printed lines match your predictions.
- `obj == null` prints `True` and `obj is null` prints `False`.
- `fallback.Name` prints `Goblin`, not `Replacement`, and you can explain why in one sentence.
- `UnsafeUse` throws for the destroyed object and not for the live one.
- `SafeUse` throws for none of the three inputs.
- Passing a genuinely null reference to `SafeUse` does not throw.

---

## C - Design

### Goal

An agent chases targets that can be destroyed at any moment, including halfway through the frame in which it is acting on one. Decide how your code will cope with that, then build enough of it to show the decision holding up.

Use the `EngineObject` stand-in from exercise B as the base for anything that can be destroyed.

### The scenario

```csharp
public class Agent : EngineObject
{
    // Holds a current target, a list of remembered threats,
    // and a home position it returns to.
}
```

Per tick, the agent must:

- act on its current target if it still exists;
- choose a new target from its remembered threats if the current one is gone;
- return home if nothing remains;
- never throw, under any sequence of destructions.

Targets may be destroyed at any point, including between the agent choosing one and acting on it.

### Constraints

- Write a test that destroys targets at several different points in the sequence and asserts that the agent never throws and always ends in a defined state.
- Include the case where every remembered threat is destroyed in the same tick.
- Include the case where the agent's current target is destroyed after being selected but before being acted on.
- The remembered-threats collection must not grow without bound as targets are destroyed.

### The choice you must justify

There are at least three strategies, and they are not variations on one idea:

- **Check at the point of use.** Every access is guarded by `!= null`. Simple, local, and scattered across every method that touches a reference.
- **Check on the way in.** Validate and clean the whole set of references once at the top of the tick, then trust them for the rest of it. Fewer checks, and one assumption that is only true if nothing is destroyed mid-tick.
- **Remove on destruction.** Have the target announce its own death, using the events from [note 07](../notes/07-delegates-events-action-func.md), and have the agent forget it immediately. No checks at all in the common path, at the cost of subscription bookkeeping and a guarantee that nothing is ever destroyed without announcing it.

The third is the one the module builds towards, and it is not automatically correct here. It fails badly when something is destroyed by a route that does not raise the event, and it makes the unsubscription discipline from note 03 load bearing.

Write a comment at the top of `Agent.cs`, ten to fifteen lines, covering:

- which strategy you chose, and which you rejected;
- the specific sequence of destructions that would break the rejected strategy, written as a numbered sequence of events;
- what your chosen strategy assumes, and what happens the day that assumption is violated;
- whether you used more than one strategy together, and if so why that is not just indecision.

### Done when

- The agent handles every case listed under Constraints without throwing.
- Tests exist for each of those cases and each test name states the behaviour it asserts.
- The remembered-threats collection is demonstrably bounded across a long run of spawns and destructions.
- No `?.` or `??` appears on any `EngineObject`-derived reference anywhere in your solution.
- The justification comment includes a numbered failure sequence, not a general description of one.
