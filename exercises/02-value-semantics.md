# Exercises: Struct and class, value semantics

Read [note 02](../notes/02-value-semantics.md) first. Plain C# console project throughout. Where a Unity type is needed, a minimal stand-in is supplied so everything runs without the editor.

---

## A - Mechanical

### Goal

See a struct copy itself, and predict the result before the program tells you.

### Starter code

```csharp
public struct Stats
{
    public int Strength;
}

public class Gear
{
    public int Armour;
}

public class Program
{
    public static void Main()
    {
        Stats a = new Stats();
        a.Strength = 10;
        Stats b = a;
        b.Strength = 99;
        // TODO: predict a.Strength, then print it

        Gear g1 = new Gear();
        g1.Armour = 10;
        Gear g2 = g1;
        g2.Armour = 99;
        // TODO: predict g1.Armour, then print it

        // TODO: call Nudge(a) and print a.Strength afterwards
    }

    // TODO: Nudge(Stats s) adds 1 to s.Strength
}
```

### Constraints

- Write your prediction for each `TODO` as a comment on the line above, before you run anything.
- Do not change `struct` to `class` or the reverse until the last step.

### Done when

- All three predictions match what the program prints.
- `Nudge` compiles, runs, and changes nothing - and you can say in one sentence why there is no warning.
- Changing `Stats` from `struct` to `class` changes two of the three outputs, and you can name which two before you try it.

---

## B - Applied

### Goal

Write code that actually moves a transform, which is fiddlier than it looks once a struct sits behind a property.

### Starter code

These stand-ins behave like the Unity types they are named after. `Vector3` is a **struct**; `Position` is a **property** whose getter returns a copy. That pairing is the whole exercise.

```csharp
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }

    public override string ToString() { return "(" + X + ", " + Y + ", " + Z + ")"; }
}

public class Transform
{
    private Vector3 _position;

    public Vector3 Position
    {
        get { return _position; }
        set { _position = value; Console.WriteLine("  [engine] moved to " + value); }
    }
}
```

### Part one: a Mover

Write a `Mover` class with three methods, each of which must genuinely move the transform:

- `SetHeight(Transform transform, float y)` - changes only Y.
- `Nudge(Transform transform, Vector3 offset)` - adds the offset to the current position.
- `Flatten(Transform transform)` - sets Y to zero, leaving X and Z.

Before you work around it, type `transform.Position.Y = 5f;` and read the error. Add a one-line comment saying, in your own words, what the compiler is objecting to.

### Part two: structs inside collections

```csharp
List<Vector3> waypoints = new List<Vector3>();
waypoints.Add(new Vector3(0f, 0f, 0f));

waypoints[0].Y = 5f;            // will not compile
```

Write `RaiseAll(List<Vector3> points, float y)` that sets `Y` on every element of the list. Then write the same method taking a `Vector3[]` instead.

One of those two is markedly easier. Add a one-line comment saying which, and why.

### Done when

- All three `Mover` methods produce `[engine] moved to ...` output, proving the setter ran.
- Your comment correctly describes what `transform.Position.Y = 5f` fails on.
- `RaiseAll` works for both the list and the array.
- Your comment identifies which container was easier and gives the reason.
- Passing a `Vector3` to a method that modifies it leaves the caller's copy unchanged - demonstrate this deliberately, then confirm no method above relies on the opposite.

---

## C - Design

### Goal

Design a type representing a rectangular region of the screen, then decide whether it should be a struct or a class - and prove your answer rather than asserting it.

### Constraints

Build `ScreenRect` with:

- `Left`, `Top`, `Right`, `Bottom` as integers.
- Computed `Width` and `Height`.
- `Contains(int x, int y)`.
- A readable `ToString()` such as `Rect(Left=0, Top=0, Right=1280, Bottom=720)`.
- Two methods that produce a modified region: `Inflate(int by)` and `Translate(int dx, int dy)`.

Then:

- Build **two** versions - one `struct`, one `class` - behind the same public surface.
- Write a test that runs an identical sequence against both and prints the results side by side.
- Include a sequence where the same region is handed to two different pieces of code which both modify it. The two versions must visibly disagree, and that disagreement is the point of the exercise.

### The choice you must justify

The struct-or-class decision here is genuinely close, which is why it is the exercise. A rectangle is small and has no identity, which argues for a struct. But it is also passed around and adjusted, and a mutable struct produces exactly the silent copy bugs the note warns about.

There is a second decision underneath. `Inflate` and `Translate` can **mutate** the region or **return a new one**. Returning a new one makes the struct safe and the class slightly wasteful. Mutating makes the class convenient and the struct treacherous. The two decisions are not independent - work out which pairings are coherent and which are traps.

Write a comment at the top of your main file, ten to fifteen lines, covering:

- which version you would ship, and the use you designed for;
- the mutate-or-return decision, and which pairing of the two decisions you rejected as incoherent;
- the exact sequence from your test where the two versions disagree, written as numbered steps;
- what changes if a rectangle is later stored in a `List` and edited in place - name the specific compiler error you would hit.

### Done when

- Both versions exist and expose the same public surface.
- The test runs both and prints results side by side.
- There is a sequence where the two disagree, it is in your comment as numbered steps, and you can explain the disagreement without rerunning it.
- `Width` and `Height` are computed, not stored.
- The justification comment covers all four bullets by name.
