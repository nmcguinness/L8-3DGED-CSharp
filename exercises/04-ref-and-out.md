# Exercises: ref and out

Read [note 04](../notes/04-ref-and-out.md) first. Plain C# console project throughout; none of these need Unity.

---

## A - Mechanical

### Goal

Write one method that changes a variable belonging to the caller, and one that hands back a second value alongside its return value. Get the keyword on both the declaration and the call site into your fingers.

### Constraints

- `ApplyDamage(ref int health, int damage)` subtracts the damage and clamps the result at zero.
- `GetStartPosition(out int x, out int y)` sets `x` to 10 and `y` to 5.
- Neither method returns anything. The values travel through the parameters.
- `GetStartPosition` must assign both parameters before it returns. Comment out one assignment, read the error, then put it back.

### Starter code

```csharp
public class Program
{
    public static void Main()
    {
        int health = 50;
        // TODO: call ApplyDamage so that health becomes 0

        // TODO: declare x and y, call GetStartPosition, print both
    }

    // TODO: ApplyDamage

    // TODO: GetStartPosition
}
```

### Done when

- `health` is `0` after applying 75 damage to 50.
- `GetStartPosition` prints `10, 5`.
- Removing `ref` from the call site but not the declaration is a compile error. Try it, read the message, then restore it.
- You can say in one sentence why `out` does not require the variable to be initialised first and `ref` does.

---

## B - Applied

### Goal

Build the two patterns these keywords actually exist for: a method that resolves an outcome in place, and the `Try` pattern that returns both a success flag and a value without ever throwing.

### Part one: resolve a hit

Write `ResolveHit(ref int health, int damage, out bool isDead)`.

- Health is reduced by the damage and clamped at zero.
- `isDead` is true when health reaches zero.
- Calling it repeatedly on a dead target must leave health at zero and keep reporting dead.

### Part two: a Try method of your own

An `Inventory` holds items against a slot name. Write:

```csharp
public bool TryGetItem(string slot, out Item item)
```

- Returns `true` and sets `item` when the slot is filled.
- Returns `false` and sets `item` to `null` when it is not.
- It must never throw, and it must never leave `item` unassigned on any path.

Then write the caller both ways and keep whichever reads better:

```csharp
Item found;
if (inventory.TryGetItem("primary", out found)) { Equip(found); }
```

### Constraints

- No exceptions for the missing-slot case. Absence is a normal outcome here, not a bug.
- Every code path through a method with an `out` parameter must assign it. The compiler enforces this; do not fight it with a dummy assignment at the top unless you can say why that is the right default.
- Use a `Dictionary<string, Item>` as the backing store.

### Done when

- `ResolveHit` on 30 health with 50 damage leaves health at 0 and `isDead` true.
- A second call with the same variables leaves health at 0 and `isDead` still true.
- `TryGetItem` on a missing slot returns `false`, assigns `null`, and throws nothing.
- `TryGetItem` on a filled slot returns `true` and the item is usable.
- Deleting one `item = null;` line produces a compile error, and you can explain what the compiler is protecting you from.

---

## C - Design

### Goal

A stats system holds a large value type - eight or more numeric fields - for every entity in the world. Thousands of them are updated each frame. Design how updates are applied, and measure whether your choice matters.

### Starter code

```csharp
public struct EntityStats
{
    public int Strength;
    public int Agility;
    public int Endurance;
    public int Intelligence;
    public int Wisdom;
    public int Luck;
    public int Charisma;
    public int Perception;
}
```

### Constraints

- Implement at least two ways of applying a per-frame update to every entity in a `List<EntityStats>`:
  - one that passes the struct by value and assigns the result back;
  - one that passes it with `ref`.
- Both must produce identical results. Assert that, do not assume it.
- Time both over at least one hundred thousand entities with `Stopwatch`, and report real figures.
- Do not change `EntityStats` to a class in either version. Deciding whether it should be one is part of the write-up, not part of the code.

### The choice you must justify

`ref` avoids copying a large struct, and it also lets a method reach into the caller's data, which is exactly the thing that makes code hard to follow. A method taking `ref` can change anything, at any time, and the call site gives only one word of warning.

There is a second decision underneath. A `List<T>` of structs cannot be modified in place at all - the indexer hands you a copy - so a by-value update needs an explicit write-back, and a `ref` update needs an array or a different container entirely. Work out which of those you are actually building.

Write a comment at the top of your main file, ten to fifteen lines, covering:

- your measured figures for both versions, and whether the difference would change your decision if the struct had three fields instead of eight;
- the container you settled on and what it forced;
- one concrete bug that becomes possible in the `ref` version that is impossible in the by-value version;
- whether you would make this type a class instead, and what that would cost per frame.

A defensible answer here is "the difference was too small to justify `ref` and I kept the readable version", provided the numbers back it up.

### Done when

- Two update paths exist and a test asserts they produce identical output for the same input.
- The timing loop runs and prints real figures for both.
- The `ref` version compiles with no per-element copy that you did not intend.
- The justification comment contains measured numbers rather than estimates, and names a specific bug the `ref` version admits.
