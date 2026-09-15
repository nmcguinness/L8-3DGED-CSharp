---
title: "Enums, Flags and Extension Methods"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t12_enums_extension_methods
description: "Named constants with enum, combining them as a set with [Flags] and bit arithmetic, and adding call-site vocabulary to types you do not own."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, enum, flags, bitwise, layer-mask, extension-methods, unity, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t11_predicates_list_search
prerequisites:
  - Static classes and static methods
  - Value semantics, for the Transform example
---

# Enums, Flags and Extension Methods

## What this unblocks

`enum` with `[Flags]` is how a layer mask represents a set of layers in a single integer, which matters as soon as you start filtering physics queries and NavMesh areas. Extension methods are how engine code adds vocabulary to types it does not own, and you will see them throughout the engine samples.

Both are conveniences rather than deep rules, which is why they sit late in the sequence. Neither is hard. The `[Flags]` trap in the middle of this note is the part that costs people an afternoon.

## The idea

An `enum` replaces bare numbers and strings with names the compiler can check. An ordinary enum holds **one** of its values at a time. `[Flags]` turns it into something that can hold **a set** of them at once, by giving each member its own bit of the underlying integer.

An extension method is unrelated: it lets you call a static method as though it were an instance method on a type you did not write. You are not modifying the type and you get no access to its private members. It is a call-site convenience, nothing more.

---

## Enums

### Named constants

```csharp
/// <summary>
/// The behaviour an agent is currently exhibiting.
/// </summary>
public enum AgentMode
{
    Idle,
    Patrol,
    Chase,
    Attack
}
```

Values start at zero and increase by one unless you say otherwise. Used as:

```csharp
AgentMode mode = AgentMode.Patrol;

switch (mode)
{
    case AgentMode.Idle:
        Wait();
        break;

    case AgentMode.Patrol:
        FollowRoute();
        break;

    default:
        Engage();
        break;
}
```

Fix the values explicitly when they are written to a save file or sent over a network, because then the number rather than the name is what persists, and reordering the members later would silently change the meaning of old data:

```csharp
public enum DamageType
{
    Physical = 0,
    Fire     = 1,
    Ice      = 2
}
```

### Converting to and from an enum

An enum is an integer underneath, so casting works in both directions - and casting *in* is the dangerous one, because nothing checks that the number corresponds to a member.

```csharp
int code = (int)DamageType.Fire;            // 1, always safe

DamageType type = (DamageType)7;            // compiles, and 7 is not a member
```

`Enum.IsDefined` checks a value before you trust it, and `Enum.TryParse` does the same job for text:

```csharp
if (Enum.IsDefined(typeof(DamageType), code))
{
    DamageType safe = (DamageType)code;
}

DamageType parsed;
if (Enum.TryParse("Fire", ignoreCase: true, out parsed))
{
    Apply(parsed);
}
```

`TryParse` uses an `out` parameter, which is [note 04](04-ref-and-out.md). Validate whenever the value arrives from a file, a server or a player - anywhere outside your own code.

### Why [Flags] is different

A layer mask needs to hold *a set* - "player and enemy but not scenery" - in a single integer. That is what `[Flags]` is for.

The mechanism is bits. Give each member a distinct power of two, and the members then occupy separate bits of the underlying integer, so they can be combined without colliding.

```csharp
/// <summary>
/// Collision layers, combinable as a set.
/// </summary>
[Flags]
public enum CollisionLayers
{
    None      = 0,
    Player    = 1 << 0,     // 1,  binary 0001
    Enemy     = 1 << 1,     // 2,  binary 0010
    Scenery   = 1 << 2,     // 4,  binary 0100
    Projectile = 1 << 3,    // 8,  binary 1000

    Hostile = Enemy | Projectile
}
```

`1 << 0`, `1 << 1` and so on shift the bit left, which is the readable way to write powers of two and makes an out-of-sequence value obvious.

Combine with `|` and test with `HasFlag`:

```csharp
CollisionLayers mask = CollisionLayers.Player | CollisionLayers.Scenery;

if (mask.HasFlag(CollisionLayers.Player))
{
    // true
}

if (mask.HasFlag(CollisionLayers.Enemy))
{
    // false
}
```

The bitwise form of the same test is what you will see in older engine code and in Unity's own samples:

```csharp
if ((mask & CollisionLayers.Player) != 0)
{
    // true
}
```

`HasFlag` is clearer and should be your default. Use the bitwise form when you are reading code that already uses it, or in a hot loop where you have measured a reason to.

The four operators worth knowing:

```csharp
mask = mask | CollisionLayers.Enemy;        // add
mask = mask & ~CollisionLayers.Scenery;     // remove
mask = mask ^ CollisionLayers.Player;       // toggle
bool any = (mask & CollisionLayers.Hostile) != 0;   // test
```

The `[Flags]` attribute itself does not create any of this behaviour. The bit arithmetic works without it. What it does is tell the runtime to print combined values sensibly - `Player, Scenery` rather than `5` - and, more importantly, tell the next reader that this enum is meant to be combined. Always apply it when that is the intent.

Unity's own `LayerMask` is this idea with the layers defined in the editor rather than in code: an `int` in which bit *n* being set means layer *n* is included. It is the same arithmetic.

---

## Extension methods

### Declaring one

Declare a static method in a static class, and mark the first parameter with `this`:

```csharp
/// <summary>
/// Convenience operations on collections.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Returns a random element from the list.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The list to choose from.</param>
    /// <returns>A randomly chosen element.</returns>
    public static T RandomElement<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}
```

The class must be static, the method must be static, and `this` must be on the first parameter. All three are required and the compiler will tell you if any is missing.

At the call site it reads as if `List<T>` had grown a method:

```csharp
List<Enemy> enemies = GetEnemies();

Enemy target = enemies.RandomElement();               // extension method
Enemy same = ListExtensions.RandomElement(enemies);   // exactly the same call
```

Those two lines compile to identical code. The second is what is actually happening.

The common use in engine code is adding readable helpers to Unity's types, which you cannot edit. This one wraps the three-line copy-out-modify-assign-back dance from [note 02](02-value-semantics.md):

```csharp
public static class TransformExtensions
{
    /// <summary>
    /// Sets the world x coordinate, leaving y and z unchanged.
    /// </summary>
    /// <param name="transform">The transform to modify.</param>
    /// <param name="x">The new world x coordinate.</param>
    public static void SetX(this Transform transform, float x)
    {
        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
}
```

Which turns three lines into `transform.SetX(5f)`.

### When not to use one

An extension method is the right tool when you cannot change the type. When you can change the type, put the method on the type. An extension method that exists because the author did not want to open the original file is just a method in the wrong place, and it is harder to find.

## Common mistakes

### A [Flags] enum with sequential values

```csharp
[Flags]
public enum CollisionLayers
{
    Player  = 0,
    Enemy   = 1,
    Scenery = 2,
    Projectile = 3          // wrong
}
```

**Symptom:** No error at all. `Enemy | Scenery` is `1 | 2`, which is `3`, which is `Projectile`. So combining two layers produces a third that was never intended, `HasFlag(Projectile)` returns true for a mask containing neither, and the filtering appears to work until two specific layers are combined. `Player = 0` is a further trap: `HasFlag` returns true for it against every possible mask, because every integer contains the zero bit set.

**Fix:** Every member gets its own power of two: `1 << 0`, `1 << 1`, `1 << 2`. Reserve `0` for a `None` member and never test for it with `HasFlag`.

### Trusting a number cast into an enum

```csharp
DamageType type = (DamageType)LoadFromSaveFile();    // no validation
```

**Symptom:** No exception. The variable holds a value that matches no member, `switch` falls through to `default`, and `ToString()` prints the bare number. The save file was written by an older build with different members, and the bug presents as "one saved game behaves oddly".

**Fix:** `Enum.IsDefined` before the cast, or `Enum.TryParse` for text. Decide what to do with an unrecognised value rather than letting it travel.

### An extension method the compiler cannot see

```csharp
Enemy target = enemies.RandomElement();     // will not compile
```

**Symptom:** The compiler reports that `List<Enemy>` does not contain a definition for `RandomElement`, and that no accessible extension method `RandomElement` accepting a first argument of type `List<Enemy>` could be found. The method exists, and you are looking at it in another file.

**Fix:** Extension methods are found by namespace, not by project. Add a `using` for the namespace the static class lives in. The full error message is unusually helpful here: the phrase "no accessible extension method" is the compiler telling you it looked and a `using` is missing.

### Writing an extension method for a type you own

```csharp
public static class WeaponExtensions
{
    public static bool IsEmpty(this Weapon weapon) { return weapon.Ammunition == 0; }
}
```

**Symptom:** Nothing breaks. It simply means the next person looking for `IsEmpty` opens `Weapon.cs`, does not find it, and concludes it does not exist. The method also cannot touch any private field, so the class has to expose more than it should.

**Fix:** Put it on `Weapon`. Extension methods are for types whose source you cannot edit.

## Check yourself

1. What three things must be true for a method to be usable as an extension method?
2. Why must the members of a `[Flags]` enum be powers of two?
3. What does `mask & ~CollisionLayers.Scenery` do, and what does `^` do instead?
4. Why is `(DamageType)7` worth worrying about when `(int)DamageType.Fire` is not?
5. You can edit the class. Should you still write an extension method for it?

<details>
<summary>Answers</summary>

**1.** The containing class must be static, the method must be static, and the first parameter must be marked `this`. The namespace must also be in scope at the call site, though that is a condition on the caller rather than the method.

**2.** So that each member occupies a distinct bit of the underlying integer. Combining them with `|` then produces a value in which each original member is independently recoverable. With sequential values the combinations collide with other members and `HasFlag` gives wrong answers.

**3.** `& ~` removes that layer from the mask, leaving everything else. `^` toggles it - present becomes absent, absent becomes present - which is what you want for a switch rather than a clear.

**4.** Casting out of an enum is always safe, because every member has a number. Casting in is not, because not every number has a member. `(DamageType)7` produces a value that matches nothing, passes silently, and only surfaces somewhere else.

**5.** No. Put the method on the type. An extension method that exists to avoid opening a file hides the method from the next reader and cannot reach any private state.

</details>

## Further reading

- [Extension methods](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Enumeration types and FlagsAttribute](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)
- [LayerMask in the Unity scripting reference](https://docs.unity3d.com/ScriptReference/LayerMask.html)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t11_predicates_list_search

this_lesson:
  topic_code: t12_enums_extension_methods
  difficulty_tier: Intermediate
mlos: [MLO3]
```
