---
title: "Operator overloading and equality"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t03_operator_overloading
description: "Giving your own types arithmetic with operator + and *, and the obligations that come with overloading == on a reference type."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, operator-overloading, equality, equals, gethashcode, structs, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t02_value_semantics
prerequisites:
  - Static members
  - Struct and class value semantics
---

# Operator overloading and equality

## What this unblocks

Directly: [note 13](13-null-handling-in-unity.md). Unity overloads `==` on `UnityEngine.Object` so that a destroyed object reports as null, and that single decision is behind the worst bug in the AI capstone. You cannot understand why `?.` and `??` get it wrong until you know what overloading `==` actually does.

Less dramatically, it is why `transform.position + offset` reads the way it does. Every vector, colour and quaternion in the engine is a type with overloaded operators, and you will write your own the first time you build a value type of your own.

## The idea

An operator is a static method with an unusual name.

`a + b` compiles to a call. For `int` the compiler emits an instruction; for your own type it looks for a method called `operator +` that takes two of your type and returns something. If it finds one, `a + b` works. If not, you get a compile error and a `.Add(b)` method instead.

Equality is the same mechanism with a much longer list of obligations attached, because the language, the collections and every reader already believe things about what `==` means.

## In code

### Arithmetic operators

Declare them `public static`, inside the type they operate on:

```csharp
public struct Vector2
{
    /// <summary>Gets or sets the horizontal component.</summary>
    public float X { get; set; }

    /// <summary>Gets or sets the vertical component.</summary>
    public float Y { get; set; }

    /// <summary>Creates a vector from its components.</summary>
    /// <param name="x">The horizontal component.</param>
    /// <param name="y">The vertical component.</param>
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Adds two vectors component-wise.</summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>The component-wise sum.</returns>
    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    /// <summary>Scales a vector by a number.</summary>
    /// <param name="v">The vector to scale.</param>
    /// <param name="scale">The factor to scale by.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 operator *(Vector2 v, float scale)
    {
        return new Vector2(v.X * scale, v.Y * scale);
    }
}
```

```csharp
Vector2 a = new Vector2(1f, 2f);
Vector2 b = new Vector2(3f, 4f);

Vector2 sum = a + b;            // (4, 6)
Vector2 doubled = a * 2f;       // (2, 4)
```

Two rules that catch people:

**Return a new value; never modify an operand.** `a + b` must leave both `a` and `b` untouched. Everyone assumes this, and [note 02](02-value-semantics.md) explains why a value type makes the assumption natural.

**Operators are not symmetric for free.** `a * 2f` compiles; `2f * a` does not, because you declared `(Vector2, float)` and not `(float, Vector2)`. Write the second overload if you want both.

### Equality on a reference type

Overloading `==` looks like more of the same and is not. Consider a class where two instances with the same id should be treated as the same thing:

```csharp
public class ItemId
{
    /// <summary>Gets the underlying identifier.</summary>
    public int Value { get; }

    public ItemId(int value) { Value = value; }
}
```

By default `==` on a class compares **references**: two separate objects are never equal, however identical their contents.

```csharp
ItemId a = new ItemId(7);
ItemId b = new ItemId(7);

Console.WriteLine(a == b);      // False, by default
```

To change that, you must supply four members, not one. The compiler insists on `!=` alongside `==`, and every collection in the framework uses `Equals` and `GetHashCode` rather than your operator:

```csharp
public class ItemId
{
    public int Value { get; }

    public ItemId(int value) { Value = value; }

    /// <summary>Reports whether two identifiers refer to the same item.</summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>True when both are null, or both hold the same value.</returns>
    public static bool operator ==(ItemId a, ItemId b)
    {
        if (ReferenceEquals(a, b)) return true;                 // same object, or both null
        if (ReferenceEquals(a, null)) return false;             // exactly one is null
        if (ReferenceEquals(b, null)) return false;
        return a.Value == b.Value;
    }

    /// <summary>The inverse of the equality operator.</summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>True when the two are not equal.</returns>
    public static bool operator !=(ItemId a, ItemId b)
    {
        return !(a == b);
    }

    /// <inheritdoc />
    public override bool Equals(object other)
    {
        return this == other as ItemId;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
```

**The `ReferenceEquals` lines are not optional and not stylistic.** Writing `if (a == null)` inside your own `operator ==` calls the operator you are defining, which calls itself, until the stack runs out. `ReferenceEquals` performs a genuine reference comparison and cannot recurse.

### Why GetHashCode comes along

A `Dictionary` and a `HashSet` find things by hash first and only then compare. If two objects are equal but hash differently, the dictionary looks in the wrong bucket, fails to find an entry that is demonstrably present, and you get a bug with no exception and no stack trace.

The contract is one-directional, and only one half is enforced by reality:

- Equal objects **must** return the same hash code.
- Different objects **may** share a hash code. That is a collision, and it is merely slower.

So derive the hash from exactly the fields your `==` compares, and no others. If `==` ignores `Name`, `GetHashCode` must ignore it too.

A mutable key is the related trap: change a field after the object is used as a dictionary key and its hash changes, so it is now filed under an address nothing will look at. Prefer get-only properties on any type you intend to compare.

### What Unity does with this

Unity overloads `==` on `UnityEngine.Object` so that comparing a destroyed object to `null` returns `true`, even though the C# reference is still perfectly alive. It is a deliberate, useful lie.

The catch is that `?.`, `??`, `??=` and `is null` **do not call `operator ==`**. They compile to a direct reference comparison, so they never see the lie and treat a destroyed object as a live one. That is the whole of [note 13](13-null-handling-in-unity.md), and it now has a mechanism behind it rather than an assertion.

### When not to overload

Overload an operator only when the meaning is obvious to somebody who has never seen your type. `+` on two vectors is obvious. `+` on two `Player` objects is not - and a reader who has to open your source to find out what `a + b` does would have been better served by `a.Merge(b)`.

The test: if you need a comment to explain what the operator does, write a named method instead.

## Common mistakes

### Infinite recursion in operator ==

```csharp
public static bool operator ==(ItemId a, ItemId b)
{
    if (a == null) return b == null;        // wrong
    return a.Value == b.Value;
}
```

**Symptom:** `StackOverflowException`, usually with no useful stack trace, and often only on the path where something is null. The method calls itself on its first line.

**Fix:** `ReferenceEquals(a, null)`. It is the only null check that cannot re-enter your operator.

### Overloading == and forgetting Equals and GetHashCode

```csharp
public static bool operator ==(ItemId a, ItemId b) { ... }
public static bool operator !=(ItemId a, ItemId b) { ... }
// and nothing else
```

**Symptom:** Two compiler warnings you can ignore, and then a bug you cannot. `a == b` says true, but `list.Contains(b)` says false and `dictionary[b]` throws `KeyNotFoundException` - because collections use `Equals` and `GetHashCode`, which you left comparing references.

**Fix:** Override both, and derive the hash from the same fields `==` compares.

### Equal objects with different hash codes

```csharp
public override bool Equals(object other)
{
    ItemId o = other as ItemId;
    return o != null && Value == o.Value;       // compares Value
}

public override int GetHashCode()
{
    return base.GetHashCode();                  // does NOT use Value
}
```

**Symptom:** A `Dictionary` that loses items. You add an entry, then look it up with an equal key and it is not found - with no exception, because the dictionary checked one bucket, found nothing, and correctly reported absence.

**Fix:** Hash the same fields you compare. Here, `return Value.GetHashCode();`.

### An operator that modifies its operands

```csharp
public static Vector2 operator +(Vector2 a, Vector2 b)
{
    a.X += b.X;                 // wrong
    a.Y += b.Y;
    return a;
}
```

**Symptom:** With a struct, nothing - `a` was a copy, so the damage is contained and the code merely misleads. With a class it is far worse: `c = a + b` silently mutates `a`, and a line that reads like pure arithmetic has a side effect nobody will look for.

**Fix:** Construct and return a new value. An operator reads as arithmetic, so it must behave like arithmetic.

## Check yourself

1. What kind of member is `operator +`, in terms of the modifiers it must carry?
2. `a * 2f` compiles but `2f * a` does not. Why, and what fixes it?
3. Why must `ReferenceEquals` be used for the null checks inside `operator ==`?
4. You overload `==` but leave `GetHashCode` alone. What breaks, and does it throw?
5. When should you write `a.Merge(b)` instead of `a + b`?

<details>
<summary>Answers</summary>

**1.** `public static`. Both are required - an operator belongs to the type rather than to an instance, which is why [note 01](01-properties-static-tostring.md) had to come first.

**2.** Because you declared an overload taking `(Vector2, float)` and the compiler matches the operand types in order. Overloads are not symmetric automatically. Add a second `operator *` taking `(float, Vector2)`.

**3.** Because `a == null` inside `operator ==` calls the operator being defined, which calls itself until the stack runs out. `ReferenceEquals` does a genuine reference comparison and cannot recurse.

**4.** Hash-based collections break - `Dictionary`, `HashSet`, and anything built on them. An equal key hashes to a different bucket, so a lookup misses an entry that is present. Nothing throws; the collection simply reports the item is not there.

**5.** Whenever the meaning of `+` for your type is not obvious to someone who has never seen it. If explaining the operator needs a comment, it should have been a named method.

</details>

## Further reading

- [Operator overloading (C# reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading)
- [How to define value equality for a type](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [Object.GetHashCode method](https://learn.microsoft.com/en-us/dotnet/api/system.object.gethashcode)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t02_value_semantics

this_lesson:
  topic_code: t03_operator_overloading
  difficulty_tier: Intermediate
mlos: [MLO3]
```
