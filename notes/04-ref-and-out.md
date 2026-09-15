---
title: "ref and out"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t04_ref_and_out
description: "Passing a variable into and out of a method with ref, returning a second value with out, and the Try pattern that both make possible."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, ref, out, parameters, pass-by-value, try-pattern, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t03_operator_overloading
prerequisites:
  - Struct and class value semantics
  - Methods and parameters
---

# ref and out

## What this unblocks

The `Try` pattern, which you will use constantly from here on. `dictionary.TryGetValue(key, out value)` in [note 10](10-collections-and-linq.md) and `Enum.TryParse(text, out result)` in [note 12](12-enums-and-extension-methods.md) both depend on `out`, and neither makes sense until you have seen it.

It also completes [note 02](02-value-semantics.md). Value semantics told you that arguments are copied. `ref` is how you opt out of that when you genuinely need to, and knowing when you genuinely need to is most of this note.

## The idea

By default, **every argument in C# is passed by value**: the method gets a copy of whatever you handed it.

For a struct, that copies the data. For a class, that copies the *reference* - so the method can still change the object's fields, because both copies point at the same object, but it cannot make your variable point somewhere else.

`ref` and `out` change that. Both hand the method the variable itself rather than a copy of its value, so an assignment inside the method reaches the caller. They differ only in what each promises:

| | Initialised before the call? | Assigned before the method returns? | Means |
|---|---|---|---|
| `ref` | **Required** | Not required | in **and** out |
| `out` | Not required | **Required** | out only |

Both keywords must appear at the **declaration and the call site**. That repetition is deliberate: it puts a warning at the point where a reader would otherwise assume nothing can change.

## In code

### The default, so the contrast is clear

```csharp
private static void TryToReset(int health)
{
    health = 100;               // changes the copy
}

int playerHealth = 20;
TryToReset(playerHealth);
Console.WriteLine(playerHealth);    // 20 - unchanged
```

This compiles, runs, warns about nothing, and does nothing. Exactly the failure from note 02.

### ref: in and out

```csharp
/// <summary>Applies damage to a health value, flooring it at zero.</summary>
/// <param name="health">The health to reduce. Modified in place.</param>
/// <param name="damage">Points of damage to apply.</param>
private static void ApplyDamage(ref int health, int damage)
{
    health -= damage;
    if (health < 0)
    {
        health = 0;
    }
}
```

```csharp
int playerHealth = 50;
ApplyDamage(ref playerHealth, 75);
Console.WriteLine(playerHealth);    // 0
```

`playerHealth` had to hold a value before the call, because `ApplyDamage` reads it. That is what "in and out" means, and it is the whole difference from `out`.

### out: out only

`out` is for a method that needs to hand back more than one thing. The classic case is "did it work, and if so, what is the answer":

```csharp
/// <summary>Finds the spawn point for a level.</summary>
/// <param name="level">The level to look up.</param>
/// <param name="x">The spawn x coordinate, when one exists.</param>
/// <param name="y">The spawn y coordinate, when one exists.</param>
/// <returns>True when a spawn point was found.</returns>
private static bool TryGetSpawnPoint(int level, out int x, out int y)
{
    if (level < 0)
    {
        x = 0;                  // MUST assign on every path, including failure
        y = 0;
        return false;
    }

    x = level * 100;
    y = 50;
    return true;
}
```

```csharp
int spawnX;
int spawnY;

if (TryGetSpawnPoint(3, out spawnX, out spawnY))
{
    Console.WriteLine("Spawn at " + spawnX + ", " + spawnY);
}
```

`spawnX` and `spawnY` were never initialised, and that is fine - `out` promises the method will assign them. The compiler enforces that promise on **every** path, which is why the failure branch still sets both to zero. Delete one of those lines and the method will not compile.

### The Try pattern

Those two rules combine into the convention you have already met without knowing why it looks like that:

```csharp
int rounds;
if (ammunition.TryGetValue("shotgun", out rounds))
{
    Reload(rounds);
}
```

The pattern exists because the alternatives are worse. Returning `-1` for "not found" needs a magic value that might be a legitimate answer. Throwing an exception is expensive and wrong, because a missing key is a normal outcome here rather than a bug. `Try` returns the success flag through the return value and the answer through `out`, and it never throws.

Write your own the same way. A method named `TryX` must return `bool`, take the result as its final `out` parameter, and never throw for the case it is testing.

### ref and out together

```csharp
/// <summary>Applies damage and reports whether the target died.</summary>
/// <param name="health">The health to reduce. Modified in place.</param>
/// <param name="damage">Points of damage to apply.</param>
/// <param name="isDead">True when health reached zero.</param>
private static void ResolveHit(ref int health, int damage, out bool isDead)
{
    health -= damage;
    if (health < 0)
    {
        health = 0;
    }

    isDead = health == 0;
}
```

```csharp
int hp = 30;
bool dead;
ResolveHit(ref hp, 50, out dead);       // hp is 0, dead is true
```

That signature is also a warning sign. A method taking one `ref` and one `out` is doing at least two things, and an object with a `TakeDamage` method and an `IsDead` property would say it better. Reach for these keywords when the alternative is genuinely worse, not as a first move.

### ref with a large struct

`ref` avoids copying. With a struct of eight or more fields updated thousands of times per frame, that can matter:

```csharp
public struct EntityStats
{
    public int Strength;
    public int Agility;
    // ...six more
}

/// <summary>Increments every stat in place.</summary>
/// <param name="stats">The stats to modify.</param>
private static void LevelUp(ref EntityStats stats)
{
    stats.Strength++;
    stats.Agility++;
}
```

Measure before you believe it. For a two-field struct the copy costs less than the indirection, and you have made the code harder to read for nothing. The optimisation topic is where this kind of claim gets tested with a profiler rather than asserted.

### Why not just use ref everywhere

Because a `ref` parameter lets a method reach into your variable and change it, and the call site gives you one word of warning. Code that passes everything by `ref` is code where any call might change anything, and that is precisely the property that makes a bug hard to find.

The order to reach for things:

1. Return a value. Always preferred.
2. Return an object holding several values, if you need more than one.
3. `out`, when the method is a `Try`.
4. `ref`, when you have measured a reason or the method genuinely edits a caller's variable in place.

## Common mistakes

### Forgetting the keyword at the call site

```csharp
int health = 50;
ApplyDamage(health, 25);            // wrong
```

**Symptom:** `CS1620: Argument 1 must be passed with the 'ref' keyword.` Straightforward, and the compiler names the fix.

**Fix:** `ApplyDamage(ref health, 25);`. The keyword is required in both places on purpose, so a reader can see at the call site that the variable may change.

### Not assigning an out parameter on every path

```csharp
private static bool TryGetSpawnPoint(int level, out int x)
{
    if (level < 0)
    {
        return false;               // wrong: x was never assigned
    }

    x = level * 100;
    return true;
}
```

**Symptom:** `CS0177: The out parameter 'x' must be assigned to before control leaves the current method.`

**Fix:** Assign it on the failure path too, usually to a sensible default. The compiler is stopping the caller from reading a variable that was never set - which is the guarantee that makes `out` safe to use without initialising first.

### Using ref when the value is never read

```csharp
private static void GetStartPosition(ref int x, ref int y)
{
    x = 10;                         // the incoming value is ignored
    y = 5;
}
```

**Symptom:** It compiles and works, but every caller is now forced to initialise two variables for no reason, and the signature lies about what the method does.

**Fix:** Use `out`. It documents that the parameter is output only, and it frees the caller from inventing a starting value.

### Expecting ref to be needed for a class

```csharp
private static void Heal(ref Enemy enemy)      // usually unnecessary
{
    enemy.Health = 100;
}
```

**Symptom:** None, which is the problem - it works, so the misunderstanding survives. `Enemy` is a class, so the method already receives a reference to the same object and can change its fields without `ref`.

**Fix:** Drop the `ref`. You only need it if the method must make the **caller's variable** point at a different object - `enemy = new Enemy();` - which is rare and worth flagging when it happens.

## Check yourself

1. Which of `ref` and `out` requires the variable to be initialised before the call, and why?
2. Why must both the declaration and the call site carry the keyword?
3. Why does `TryGetSpawnPoint` assign `x = 0` on a path that returns `false`?
4. You have a class, and a method that changes one of its fields. Do you need `ref`?
5. Why is `Try` preferred over returning `-1` for "not found"?

<details>
<summary>Answers</summary>

**1.** `ref`, because the method may read the value before writing it - that is what "in and out" means. `out` promises the method will assign it before returning, so whatever was there is irrelevant and initialising it would be wasted work.

**2.** So that a reader at the call site can see the variable may change. Without it, a call looks like it cannot affect the arguments, and the repetition removes that false assumption.

**3.** Because the compiler requires an `out` parameter to be assigned on every path out of the method. That requirement is what makes it safe for the caller to pass an uninitialised variable.

**4.** No. A class is a reference type, so the method already has a reference to the same object and can change its fields. You would only need `ref` to repoint the caller's variable at a different object.

**5.** Because `-1` is a magic value that may be a legitimate answer, and callers forget to check it. `Try` puts the success flag in the return value where it is hard to ignore, keeps the answer in the `out` parameter, and never throws for a case that is not an error.

</details>

## Further reading

- [ref keyword (C# reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/ref)
- [out parameter modifier (C# reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out-parameter-modifier)
- [Dictionary.TryGetValue method](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.trygetvalue)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t03_operator_overloading

this_lesson:
  topic_code: t04_ref_and_out
  difficulty_tier: Intermediate
mlos: [MLO3]
```
