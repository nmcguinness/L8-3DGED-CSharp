# Exercises: Operator overloading and equality

Read [note 03](../notes/03-operator-overloading.md) first. Plain C# console project throughout.

Exercise C is the one that matters. It builds the mechanism behind the bug in [note 13](../notes/13-null-handling-in-unity.md), and building the trap yourself is a better way to understand it than reading about it.

---

## A - Mechanical

### Goal

Give a small value type arithmetic, and find out for yourself that operator overloads are not symmetric.

### Starter code

```csharp
public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y) { X = x; Y = y; }

    public override string ToString() { return "(" + X + ", " + Y + ")"; }

    // TODO: operator + adding two vectors component-wise
    // TODO: operator - subtracting one from another
    // TODO: operator * scaling a vector by a float
}
```

### Constraints

- Every operator is `public static` and returns a **new** `Vector2`. None may modify an operand.
- After `operator *` works for `v * 2f`, try `2f * v`. It will not compile. Read the error, then add whatever makes it work.
- Do not add any method whose job could be done by an operator you were asked for.

### Done when

- `new Vector2(1, 2) + new Vector2(3, 4)` prints `(4, 6)`.
- `v * 2f` and `2f * v` both compile and agree.
- After `Vector2 c = a + b;`, printing `a` shows it unchanged.
- You can state what `public static` on an operator has to do with [note 01](../notes/01-properties-static-tostring.md).

---

## B - Applied

### Goal

Make two distinct objects compare as equal, and discover what breaks when you do half the job.

### Starter code

```csharp
public class ItemId
{
    public int Value { get; }

    public ItemId(int value) { Value = value; }

    public override string ToString() { return "Item#" + Value; }
}
```

### Part one: the default

```csharp
ItemId a = new ItemId(7);
ItemId b = new ItemId(7);
Console.WriteLine(a == b);              // predict this before running
```

### Part two: overload == and != only

Add `operator ==` and `operator !=` so that two `ItemId` objects with the same `Value` are equal. Handle a null on either side, and both sides.

Then run this and explain every line:

```csharp
Console.WriteLine(a == b);                          // ?

List<ItemId> list = new List<ItemId> { a };
Console.WriteLine(list.Contains(b));                // ?

Dictionary<ItemId, string> map = new Dictionary<ItemId, string>();
map[a] = "sword";
Console.WriteLine(map.ContainsKey(b));              // ?
```

### Part three: finish the job

Override `Equals` and `GetHashCode` so that all three lines agree. Re-run and confirm.

### Constraints

- Inside `operator ==`, you must not compare an operand to `null` with `==`. Work out why before you write it; if you get it wrong the program will tell you loudly.
- `GetHashCode` must be derived from exactly the fields `==` compares.
- Do not make `Value` settable. Be ready to say what would go wrong in the dictionary if you did.

### Done when

- Part one printed `False` and you predicted it.
- After part two, `a == b` is `True` but at least one of the collection lines is `False`.
- After part three, all three lines are `True`.
- Writing `if (a == null)` inside your own `operator ==` produces a `StackOverflowException`. Try it deliberately, then fix it.
- You can say in one sentence why a dictionary can fail to find a key that is present without throwing anything.

---

## C - Design

### Goal

Reproduce Unity's null behaviour from scratch. You are building the overload that makes a destroyed object report as null, then finding out exactly which operators respect it and which ignore it.

### Constraints

Write `EngineObject`:

- A private `_isDestroyed` flag and a public `Destroy()` that sets it.
- A `Name` property.
- A `DoWork()` method that throws if the object is destroyed, with a message resembling Unity's.
- `operator ==` reporting a destroyed object as equal to `null`, plus `operator !=`, `Equals` and `GetHashCode` consistent with it.

Then write a program that creates one, destroys it, and prints the result of each of these. **Predict every answer before running.**

```csharp
obj == null
obj != null
obj is null
ReferenceEquals(obj, null)
obj?.Name ?? "no name"

EngineObject fallback = obj ?? new EngineObject { Name = "Replacement" };
Console.WriteLine(fallback.Name);
```

Finally write `UnsafeUse(EngineObject o)` using `o?.DoWork()` and `SafeUse(EngineObject o)` using `if (o != null)`, and call both with a live object, a destroyed object, and a genuinely null reference.

### The choice you must justify

Your `==` has to decide what several awkward pairs mean, and the answers are not all obvious:

- destroyed against `null`;
- destroyed against the **same** destroyed object;
- destroyed against a **different** destroyed object;
- two distinct live objects.

There is more than one defensible answer to the middle two, and Unity's own choice is not the only one available. Decide deliberately.

There is a second decision: `Equals` and `GetHashCode` must stay consistent with `==`, but a destroyed object's hash must not change when it is destroyed - or anything already filed in a dictionary is instantly unreachable. Work out what that constrains.

Write a comment at the top of `EngineObject.cs`, ten to fifteen lines, covering:

- your answer for each of the four pairs above, and which two you had a real choice about;
- what `GetHashCode` is derived from, and why it must not involve `_isDestroyed`;
- why `?.` and `??` ignore your operator entirely, in terms of what the compiler emits;
- which of `UnsafeUse` and `SafeUse` you would rather find in a colleague's pull request, and which failure is harder to diagnose.

### Done when

- `obj == null` prints `True` and `obj is null` prints `False`, and you predicted both.
- `fallback.Name` prints the original name rather than the replacement, and you can explain that in one sentence.
- `UnsafeUse` throws for the destroyed object; `SafeUse` throws for none of the three inputs.
- `operator ==` handles a null on either side and both sides without recursing.
- A destroyed object put into a `Dictionary` before destruction is still findable afterwards.
- The justification comment covers all four bullets by name.
