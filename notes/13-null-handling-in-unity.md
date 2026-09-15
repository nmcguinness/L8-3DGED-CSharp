---
title: "Null handling in Unity"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t13_null_handling
description: "Why a destroyed UnityEngine.Object reports as null through == but not through ?. or ??, and the rule that decides which null check to use where."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, null, null-conditional, operator-overloading, unity, destroyed-objects, stage3, comp-i8014]
difficulty_tier: Advanced
mlos: [MLO3]
previous_topic: t12_enums_extension_methods
prerequisites:
  - Operator overloading, especially operator ==
  - Null-conditional and null-coalescing operators
  - Properties
---

# Null handling in Unity

## What this unblocks

Later in the module you will build an AI capstone in which a state machine drives an agent towards targets that are being destroyed while it is chasing them. Every time a target dies mid-behaviour, some piece of your code is holding a reference to something that no longer exists, and how you check that reference decides whether the agent recovers or the frame throws an exception.

This is the note that will save you the most debugging time, and it is last for a reason: it depends on the null-conditional operator from [note 07](07-delegates-events-action-func.md) and on knowing that `==` can be overloaded, which follows from [note 03](03-operator-overloading.md).

The short version, if you read nothing else: **in Unity, `?.` and `??` are not safe on engine objects.** The rest of this note explains why.

## The idea

In plain C#, `null` means one thing: the reference points at nothing.

In Unity there are two situations that both look like nothing, and only one of them is really null.

1. The reference genuinely is null. Nobody ever assigned it.
2. The reference points at a live C# object whose underlying native engine object has been destroyed.

The second case is the awkward one. When you call `Destroy(enemy)`, Unity tears down the native object, but the C# wrapper is an ordinary managed object and it carries on existing until the garbage collector gets to it. Your reference is not null. It points at a real object. That object is just useless.

Unity's answer was to overload `==` on `UnityEngine.Object` so that comparing a destroyed object to `null` returns `true`. It is a helpful lie, and it works right up until you use an operator that does not go through `==`.

## In code

### Plain C# null handling

None of this is Unity-specific and all of it is correct for your own classes.

```csharp
string name = null;

int length = name?.Length ?? 0;         // 0, no exception
```

`?.` calls the member only if the reference is not null, and evaluates to `null` otherwise. `??` supplies a fallback when the left side is null. `??=` assigns only if the target is currently null:

```csharp
private List<Enemy> _targets;

public void Add(Enemy enemy)
{
    _targets ??= new List<Enemy>();     // create on first use
    _targets.Add(enemy);
}
```

### Nullable reference types

C# can be told to track which references are allowed to be null. Enable it per file or per project:

```csharp
#nullable enable

public class Inventory
{
    private Weapon? _equipped;          // may be null, and the compiler knows

    private readonly List<Item> _items = new List<Item>();  // must never be null

    /// <summary>
    /// Gets the currently equipped weapon, if any.
    /// </summary>
    /// <returns>The equipped weapon, or null if none is equipped.</returns>
    public Weapon? GetEquipped()
    {
        return _equipped;
    }
}
```

`Weapon?` means null is expected. `Weapon` without the question mark means it is not, and the compiler warns when you assign null to it or dereference something it cannot prove is non-null.

Two caveats before you enable this everywhere in a Unity project. These are warnings, not errors, so they inform rather than enforce. And Unity's own API is largely not annotated, so the compiler has no information about whether `GetComponent` can return null - it will not warn you about the cases you most want warned about.

Use it in your own plain C# classes, where it is genuinely useful. Do not expect it to help with engine types.

### The overloaded ==

Here is the behaviour that makes Unity different:

```csharp
Enemy enemy = GetEnemy();

Destroy(enemy);

// Later, after the destruction has been processed
Debug.Log(enemy == null);                       // True
Debug.Log(ReferenceEquals(enemy, null));        // False
```

Both lines are asking about the same reference and they disagree. `==` runs Unity's overload, which checks whether the native object behind the wrapper still exists and reports `true` when it does not. `ReferenceEquals` does a genuine reference comparison and correctly reports that the reference is not null.

The overload is doing what you want. `enemy == null` reading as `true` for a destroyed enemy is the sensible answer to the question you were asking.

### Why ?. and ?? get it wrong

The null-conditional and null-coalescing operators do not call `operator ==`. They compile to a direct reference comparison, the same one `ReferenceEquals` performs. So they do not see the destroyed object as null.

```csharp
Enemy enemy = GetEnemy();
Destroy(enemy);

enemy?.TakeDamage(10);          // calls TakeDamage on a destroyed object

Enemy target = enemy ?? FindNewTarget();
// target is the destroyed enemy, not a new one
```

The first line throws `MissingReferenceException` at the point `TakeDamage` touches the native object. The second silently produces exactly the wrong answer: `??` saw a non-null reference, so it never called `FindNewTarget`, and your agent now spends the rest of the level chasing a corpse.

`is null` has the same problem for the same reason:

```csharp
if (enemy is null)              // False, even for a destroyed enemy
```

This is the reverse of the advice you will find for ordinary C#, where `is null` is recommended *because* it cannot be fooled by a custom `==` overload. In Unity the overload is the thing you want, so bypassing it is the bug.

### The rules

| Checking | Use | Avoid |
|---|---|---|
| A `UnityEngine.Object` (MonoBehaviour, GameObject, Transform, any component) | `== null`, `!= null` | `?.`, `??`, `??=`, `is null` |
| Your own plain C# classes | `?.`, `??`, `is null`, `== null` | nothing |
| An event or delegate | `?.Invoke(...)` | bare invocation |

The middle and bottom rows are why this is confusing: the operators from note 03 and from earlier in this note are correct almost everywhere. It is specifically `UnityEngine.Object` and its descendants that are the exception.

So the safe version of the code above is verbose and correct:

```csharp
if (enemy != null)
{
    enemy.TakeDamage(10);
}

Enemy target = enemy != null ? enemy : FindNewTarget();
```

One further consideration for when you come to optimise: because `== null` on a `UnityEngine.Object` calls into native code rather than comparing a pointer, it is measurably more expensive than an ordinary null check. This does not matter anywhere except in a tight loop running every frame, where the answer is to cache the reference once rather than to skip the check.

### Reading the exception

Three exceptions look similar and mean different things. Knowing which you have tells you where to look.

| Exception | Meaning |
|---|---|
| `NullReferenceException` | The reference really is null. Nothing ever assigned it, or something assigned null. |
| `MissingReferenceException` | The object existed and was destroyed. The reference is stale. |
| `UnassignedReferenceException` | A `[SerializeField]` field was left empty in the inspector. |

`UnassignedReferenceException` is the easiest to fix and the one to check first, because it is not a code problem at all - it means a slot in the inspector is empty. `MissingReferenceException` is the one this note is about, and it almost always means something held a reference across a `Destroy`.

## Common mistakes

### Using ?. on a component reference

```csharp
private void Update()
{
    _target?.MoveTowards(transform.position);        // wrong
}
```

**Symptom:** Works for the whole session until something destroys the target, then `MissingReferenceException: The object of type 'Enemy' has been destroyed but you are still trying to access it.` The `?.` on the line makes it look as though the case was handled, so the line gets skipped over repeatedly during debugging.

**Fix:** `if (_target != null)`. The explicit comparison calls Unity's overload and correctly treats the destroyed object as null.

### Using ?? to supply a fallback target

```csharp
Enemy target = _currentTarget ?? FindNearestEnemy();    // wrong
```

**Symptom:** No exception, which is worse. `??` sees a non-null reference and returns the destroyed enemy, so `FindNearestEnemy` is never called. The agent pathfinds towards a target that no longer exists, and the bug presents as "the AI gets stuck sometimes" rather than as anything pointing at this line.

**Fix:** `Enemy target = _currentTarget != null ? _currentTarget : FindNearestEnemy();`

### Holding a reference across a destroy

```csharp
private readonly List<Enemy> _squad = new List<Enemy>();

private void Update()
{
    foreach (Enemy enemy in _squad)
    {
        enemy.Advance();                // wrong: some of these may be destroyed
    }
}
```

**Symptom:** `MissingReferenceException` from inside `Advance`, on a frame some time after the enemy died. The list still contains the destroyed enemy, because destroying an object does not remove it from your collections. Nothing does that but you.

**Fix:** Remove entries when the object is destroyed rather than checking for destruction on every use. Have the enemy raise an event on death - which is note 03 - and have the squad unsubscribe and remove in response. Checking `!= null` inside the loop is a workaround that leaves the list growing with dead entries forever.

### Assuming Destroy takes effect immediately

```csharp
Destroy(enemy);

if (enemy == null)
{
    Respawn();                          // does not run
}
```

**Symptom:** The block is skipped. `Destroy` schedules destruction for the end of the current frame, so for the remainder of this frame the object is still alive and `== null` is still `false`.

**Fix:** Do not write code that depends on destruction having completed within the same frame. If you need it to be immediate, `DestroyImmediate` exists but is intended for editor tooling and causes problems in play mode. The usual fix is to clear your own reference explicitly - `enemy = null;` - immediately after calling `Destroy`, so that your view of the world is correct straight away.

## Check yourself

1. What are the two different things a Unity reference that "looks null" can actually be?
2. Why does `enemy == null` return `true` for a destroyed object whilst `enemy is null` returns `false`?
3. Which null operators are safe on your own plain C# classes but unsafe on a `MonoBehaviour`?
4. You see `UnassignedReferenceException` in the console. Where do you look first?
5. Why is `?? FindNewTarget()` more dangerous than `?.TakeDamage(10)`?

<details>
<summary>Answers</summary>

**1.** Either the reference genuinely is null and nothing was ever assigned to it, or it points at a live C# wrapper whose underlying native object has been destroyed. Only the first is null in the ordinary C# sense.

**2.** `==` runs Unity's overload on `UnityEngine.Object`, which reports a destroyed object as null. `is null` compiles to a direct reference comparison and never calls the overload, so it sees the wrapper that is still there.

**3.** `?.`, `??`, `??=` and `is null`. All four bypass the `==` overload, so on a `MonoBehaviour` they treat a destroyed object as a perfectly good one.

**4.** The inspector, not the code. It means a `[SerializeField]` field was left empty on the component, so the fix is to drag the missing object into the slot.

**5.** Because `?.TakeDamage(10)` throws, so you find out immediately and get a stack trace pointing at the line. `??` produces no exception at all - it just returns the wrong object and lets everything downstream operate on it, so the symptom appears somewhere else entirely and looks like a logic bug.

</details>

## Further reading

- [Nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)
- [Null-conditional and null-coalescing operators](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-)
- [Object.operator == in the Unity scripting reference](https://docs.unity3d.com/ScriptReference/Object-operator_eq.html)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t12_enums_extension_methods

this_lesson:
  topic_code: t13_null_handling
  difficulty_tier: Advanced
mlos: [MLO3]
```
