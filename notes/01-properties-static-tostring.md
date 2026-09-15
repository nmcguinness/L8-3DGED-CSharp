---
title: "Properties, static members and ToString"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t01_properties
description: "Controlling access to state through properties, validating in a setter, sharing data with static members, and giving a type a readable string form."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, properties, getters, setters, validation, static, tostring, stage3, comp-i8014]
difficulty_tier: Foundation
mlos: [MLO3]
previous_topic: null
prerequisites:
  - Classes, fields, methods and constructors
  - Basic inheritance
---

# Properties, static members and ToString

## What this unblocks

Almost everything that follows. A property is not a field, and the difference is the entire reason `transform.position.x = 5f` fails to compile in [note 02](02-value-semantics.md) - which you will hit in the first hour of writing movement code. Operators in [note 03](03-operator-overloading.md) are static members, so `static` has to mean something to you first.

Later in the module the same distinction keeps mattering: ScriptableObjects expose their data through properties, and the blackboard is read through them. `[SerializeField]` exists precisely because Unity needs to save a *field* while your code exposes a *property*.

## The idea

A field stores a value. A property is **a pair of methods that look like a field**.

That sentence is the whole note. `enemy.Health` might be a variable, or it might be a method call that runs a clamp, raises an event, and returns a computed number - and the calling code cannot tell which. That is the point: you can change your mind later without editing a single caller.

`static` is unrelated and simpler. A static member belongs to the **type**, not to any instance. There is exactly one of it, shared by everything.

`ToString` is the method every type already has and almost every type should replace.

## In code

### From a field to a property

Start with a public field, which is what you would write first:

```csharp
public class Enemy
{
    public int Health;      // anyone can set this to anything, including -9999
}
```

The problem is not style. It is that `Health` has no rules, and there is nowhere to put any.

A property gives you the place:

```csharp
public class Enemy
{
    private int _health = 100;

    /// <summary>
    /// Gets or sets the current health, clamped to the range 0 to 100.
    /// </summary>
    public int Health
    {
        get { return _health; }
        set { _health = Math.Clamp(value, 0, 100); }
    }
}
```

Three things to notice.

`_health` is the **backing field** - the actual storage, private, underscore-prefixed per the house style. `Health` is the public face.

`value` is a keyword. Inside a setter it holds whatever was assigned, and you never declare it.

The call site does not change:

```csharp
enemy.Health = 250;
Console.WriteLine(enemy.Health);    // 100 - the setter clamped it
```

`enemy.Health = 250` *looks* like an assignment and *is* a method call.

### Auto-properties

When there are no rules yet, writing a backing field by hand is noise. The compiler will generate one:

```csharp
public class Enemy
{
    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; }
}
```

That is an **auto-property**. There is still a hidden backing field; you simply cannot reach it by name. When you later need validation, you replace `{ get; set; }` with a full property and a real field, and **no caller changes**. That is the reason to start with a property rather than a public field even when it does nothing.

### Controlling who can write

The accessors take their own modifiers:

```csharp
public class Enemy
{
    /// <summary>Gets the current health. Only this class may change it.</summary>
    public int Health { get; private set; }

    /// <summary>Gets the maximum health. Fixed once the object is built.</summary>
    public int MaxHealth { get; }

    /// <summary>Creates an enemy at full health.</summary>
    /// <param name="maxHealth">The greatest health this enemy can have.</param>
    public Enemy(int maxHealth)
    {
        MaxHealth = maxHealth;      // a get-only property may be set in the constructor
        Health = maxHealth;
    }

    /// <summary>Applies damage, floored at zero.</summary>
    /// <param name="amount">Points of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
    }
}
```

`private set` means outside code can read but not write. `{ get; }` with no setter at all means the value is fixed after construction. Reach for these constantly - most state should be readable by everyone and writable by almost nobody.

### Computed properties

A property need not store anything:

```csharp
/// <summary>Gets a value indicating whether health has run out.</summary>
public bool IsDead
{
    get { return Health <= 0; }
}

/// <summary>Gets health as a fraction between 0 and 1.</summary>
public float HealthFraction
{
    get { return (float)Health / MaxHealth; }
}
```

There is no `_isDead` field and there must not be, because a stored copy can disagree with `Health` and eventually will. Compute it.

The rule for choosing: a property should be **cheap and side-effect free**. Callers assume reading it is free, because it looks like a field. If it hits the disk or loops over a thousand items, make it a method so the brackets warn them.

### Static members

A static member belongs to the type. There is one, shared:

```csharp
public class Enemy
{
    /// <summary>Gets how many enemies currently exist.</summary>
    public static int Count { get; private set; }

    /// <summary>The damage every enemy takes from a fall, regardless of type.</summary>
    public const int FallDamage = 10;

    public Enemy(int maxHealth)
    {
        Count++;                    // one counter for all enemies
    }
}
```

```csharp
Enemy a = new Enemy(100);
Enemy b = new Enemy(50);

Console.WriteLine(Enemy.Count);     // 2 - on the TYPE, not on a or b
Console.WriteLine(a.Count);         // will not compile
```

Static members are accessed through the type name. An instance member is not visible from a static one, because a static method has no instance to look at.

Static is also how you write a named constant value of your own type:

```csharp
public struct Colour
{
    public float R, G, B;

    /// <summary>Gets opaque red.</summary>
    public static Colour Red { get { return new Colour { R = 1f, G = 0f, B = 0f }; } }
}

Colour c = Colour.Red;
```

That is exactly how `Vector3.zero` and `Color.red` work in Unity.

Use `const` when the value is a literal fixed at compile time, and `static readonly` when it is computed once at startup. Use plain `static` mutable state sparingly - a shared mutable variable is a global, and globals are how two systems end up silently fighting over one value.

### ToString

Every type inherits `ToString()` from `object`. The default is useless - it prints the type name:

```csharp
Enemy e = new Enemy(100);
Console.WriteLine(e);               // Demo.Enemy
```

`Console.WriteLine` and string concatenation call `ToString()` for you, so overriding it improves every log line you will ever write:

```csharp
/// <inheritdoc />
public override string ToString()
{
    return "Enemy(health=" + Health + "/" + MaxHealth + ")";
}
```

```csharp
Console.WriteLine(e);               // Enemy(health=100/100)
```

`override` is required, because you are replacing a virtual method that already exists. Keep the output short, single-line, and aimed at a developer reading a log at 2am - not at a player.

## Common mistakes

### A property that refers to itself

```csharp
public int Health
{
    get { return Health; }          // wrong
    set { Health = value; }         // wrong
}
```

**Symptom:** `StackOverflowException`, and often no stack trace worth reading. The getter calls the getter, forever.

**Fix:** Return the **backing field**, not the property. `get { return _health; }`. This happens when someone converts an auto-property to a full one and forgets to add the field.

### Storing what should be computed

```csharp
public bool IsDead { get; set; }

public void TakeDamage(int amount)
{
    Health -= amount;
    IsDead = Health <= 0;           // must remember this everywhere
}
```

**Symptom:** A corpse walking around. Some other method changes `Health` - a heal, a reset, a respawn - and does not update `IsDead`, so the two disagree. The bug appears far away from the line that caused it.

**Fix:** Compute it: `public bool IsDead { get { return Health <= 0; } }`. It cannot then be wrong. If two pieces of state must always agree, store one and derive the other.

### Expecting a public field to behave like a property later

```csharp
public int Health;                  // shipped like this
```

**Symptom:** Nothing, until you need validation. Changing it to a property is source-compatible for your own code but breaks anything that used it as a `ref`/`out` argument, and it silently changes how Unity serialises it. The refactor lands when you are least able to afford it.

**Fix:** Expose a property from the start, even a bare `{ get; set; }`. It costs one line now and nothing later.

### Reaching for instance state from a static member

```csharp
public static void ResetAll()
{
    Health = 100;                   // wrong
}
```

**Symptom:** `CS0120: An object reference is required for the non-static field, method, or property 'Enemy.Health'.`

**Fix:** A static method has no instance, so there is no `Health` to reset. Either make the method an instance method, or pass in the object it should act on.

## Check yourself

1. What is the difference between `public int Health;` and `public int Health { get; set; }` to the code that calls it, and to you?
2. What does the keyword `value` refer to, and where is it legal?
3. Why should `IsDead` be computed rather than stored?
4. `Enemy.Count` compiles but `a.Count` does not. Why?
5. You convert `{ get; set; }` into a full property and the program immediately throws `StackOverflowException`. What did you forget?

<details>
<summary>Answers</summary>

**1.** To the caller, nothing - both are written `enemy.Health = 5`. To you, everything: the property is a pair of methods, so it has somewhere to put validation, logging or an event, and you can add those later without any caller changing.

**2.** The value being assigned. It is legal only inside a property's `set` accessor, and you never declare it.

**3.** Because stored state can disagree with the thing it describes. Any method that changes `Health` without also updating a stored `IsDead` leaves the two out of step, and the bug surfaces somewhere else entirely. A computed property cannot be wrong.

**4.** `Count` is static, so it belongs to the type rather than to any instance. There is one counter shared by every enemy, and it is reached through the type name.

**5.** The backing field. The getter is returning the property instead of `_health`, so it calls itself until the stack runs out.

</details>

## Further reading

- [Properties (C# programming guide)](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties)
- [Static classes and static class members](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members)
- [Object.ToString method](https://learn.microsoft.com/en-us/dotnet/api/system.object.tostring)

## Lesson Context

```yaml
previous_lesson:
  topic_code: null

this_lesson:
  topic_code: t01_properties
  difficulty_tier: Foundation
mlos: [MLO3]
```
