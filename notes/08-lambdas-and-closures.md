---
title: "Lambdas and closures"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t08_lambdas_closures
description: "Anonymous methods written where they are used, how a closure captures the variable rather than its value, the captured loop variable bug, and why an inline lambda cannot be unsubscribed."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, lambdas, closures, capture, delegates, allocation, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t07_delegates_events
prerequisites:
  - Delegates, Action and Func
---

# Lambdas and closures

## What this unblocks

Every callback you register from the event channels onwards can be written inline instead of as a named method, and most of the time it will be. The event channels, the command bindings and the strategy selection all read better when the behaviour sits at the point of registration rather than in a method three screens away.

The syntax takes five minutes. The reason this note is long is that a lambda quietly captures the variables around it, and that capture behaves in a way that surprises people. Two of the bugs below are ones you will otherwise meet for the first time under exam conditions.

## The idea

A lambda is a method with no name, written where it is used.

[Note 07](07-delegates-events-action-func.md) established that a delegate holds a method. Everywhere you wrote `bar.Refresh` and had to go and define `Refresh` somewhere, you can instead write the body on the spot.

A **closure** is what happens when that body mentions a variable from the surrounding code. The lambda does not copy the value. It keeps hold of the variable itself, and reads it whenever the lambda eventually runs - which may be long after the surrounding method has returned. That single fact explains everything odd in this note.

## In code

### The syntax

Start from a named method used as a callback:

```csharp
private void LogDamage(int amount)
{
    Debug.Log("Took " + amount + " damage");
}

// elsewhere
_health.HealthChanged += LogDamage;
```

The lambda form puts the body at the call site. `=>` separates the parameters from the body.

```csharp
_health.HealthChanged += (int amount) => { Debug.Log("Took " + amount + " damage"); };
```

Two pieces of that are noise. The parameter type can be inferred from the delegate, and a single-expression body needs no braces:

```csharp
_health.HealthChanged += amount => Debug.Log("Took " + amount + " damage");
```

That is the form you will normally write. The rules:

| Shape | Written as |
|---|---|
| No parameters | `() => DoSomething()` |
| One parameter | `amount => Use(amount)` |
| Several parameters | `(a, b) => Use(a, b)` |
| Several statements | `amount => { Log(amount); Use(amount); }` |
| Returns a value | `amount => amount * 2` |

A lambda with an expression body returns that expression automatically, with no `return` keyword. A lambda with a braced body needs an explicit `return` if it returns anything.

### Lambdas fit Action and Func

Nothing new is being introduced here. A lambda is just another way to produce the delegate value that `Action` and `Func` hold.

```csharp
Action reset = () => _score = 0;

Action<int> log = amount => Debug.Log(amount);

Func<int, int> doubled = value => value * 2;

Func<Enemy, bool> isWounded = enemy => enemy.Health < 50;
```

The compiler checks the lambda against the delegate type on the left. If `isWounded` returned an `int`, that last line would not compile.

### Capture: the lambda holds the variable, not the value

This is the whole topic. Consider:

```csharp
int threshold = 50;

Func<Enemy, bool> isWounded = enemy => enemy.Health < threshold;

Debug.Log(isWounded(goblin));    // compares against 50

threshold = 10;

Debug.Log(isWounded(goblin));    // compares against 10
```

`threshold` was 50 when the lambda was created, but the second call uses 10. The lambda did not take a snapshot. It captured `threshold` itself, and it reads the current value each time it runs.

Mechanically, the compiler moves `threshold` off the stack and into a hidden object shared by the enclosing method and the lambda. Both are looking at the same field. That hidden object is a heap allocation, which is the reason a lambda that captures is more expensive than one that does not, and why this comes back in [note 10](10-collections-and-linq.md) and again when you come to optimise.

A lambda that captures nothing has no such object and is cheap:

```csharp
Func<int, int> doubled = value => value * 2;    // captures nothing
```

### The captured loop variable

Now the bug. This is the one that catches everybody.

```csharp
List<Action> callbacks = new List<Action>();

for (int i = 0; i < 3; i++)
{
    callbacks.Add(() => Debug.Log(i));
}

foreach (Action callback in callbacks)
{
    callback();
}
```

Expected output is `0 1 2`. Actual output is `3 3 3`.

There is one `i` for the entire loop. All three lambdas captured that same variable, and by the time any of them runs the loop has finished and `i` holds the value that ended it. Three lambdas, one shared variable, one value.

The fix is to give each iteration its own variable to capture:

```csharp
for (int i = 0; i < 3; i++)
{
    int index = i;                            // a new variable each iteration
    callbacks.Add(() => Debug.Log(index));
}
```

Now each lambda captures a different `index`, and the output is `0 1 2`.

**`foreach` does not have this problem.** The iteration variable in a `foreach` is a fresh variable on every pass, so this behaves as you would expect:

```csharp
foreach (Enemy enemy in enemies)
{
    callbacks.Add(() => Debug.Log(enemy.name));   // correct: one enemy each
}
```

This distinction is worth remembering precisely, because a great deal of material online predates the language change that fixed `foreach` and will tell you that both loops are broken. Only `for` is.

### A lambda cannot be unsubscribed unless you stored it

[Note 07](07-delegates-events-action-func.md) insisted that every `+=` gets a matching `-=`. Lambdas make that harder than it looks.

```csharp
private void OnEnable()
{
    _health.HealthChanged += amount => Refresh(amount);
}

private void OnDisable()
{
    _health.HealthChanged -= amount => Refresh(amount);   // does nothing
}
```

This compiles, runs, and does not unsubscribe anything. The two lambdas are written identically but they are two separate objects, and `-=` removes by identity, not by appearance. It searches the list for the exact delegate it was given, fails to find it, and - by design - reports no error.

You have two ways out. Either use a method group, which is the same reference both times:

```csharp
private void OnEnable()
{
    _health.HealthChanged += Refresh;
}

private void OnDisable()
{
    _health.HealthChanged -= Refresh;
}
```

Or, when you need the lambda, store it in a field and subscribe that field:

```csharp
private Action<int> _onHealthChanged;

private void OnEnable()
{
    _onHealthChanged = amount => Refresh(amount * 2);
    _health.HealthChanged += _onHealthChanged;
}

private void OnDisable()
{
    _health.HealthChanged -= _onHealthChanged;
}
```

The rule to carry into the event channels: **an inline lambda is a permanent subscription.** If the subscription needs to end, it needs a name.

## Common mistakes

### Capturing the `for` loop variable

```csharp
for (int i = 0; i < buttons.Count; i++)
{
    buttons[i].Clicked += () => SelectSlot(i);      // wrong
}
```

**Symptom:** Every button selects the same slot, and it is the one past the end of the list, so the first thing you see is an `ArgumentOutOfRangeException` rather than the wrong slot being selected. Inspecting the lambdas in the debugger shows them all holding the same value.

**Fix:** Copy the loop variable inside the loop body and capture the copy. `int slot = i;` then `SelectSlot(slot)`. Or use `foreach`, which does this for you.

### Unsubscribing an inline lambda

```csharp
_channel.Raised -= () => Handle();               // wrong
```

**Symptom:** No compiler error, no runtime error, and no unsubscription. The handler keeps firing after the object should have stopped listening, and you get the duplicate-call and `MissingReferenceException` problems from note 03 whilst looking at an `OnDisable` that appears to be correct.

**Fix:** Subscribe a method group, or store the lambda in a field and pass that same field to both `+=` and `-=`.

### Assuming the captured value is frozen at creation

```csharp
Enemy target = FindNearest();
Action attack = () => Strike(target);

target = FindNearest();      // the enemy moved on; re-scan
attack();                    // strikes the second enemy, not the first
```

**Symptom:** The callback acts on the wrong object, and reliably acts on whichever one was assigned most recently. This is hardest to spot when the reassignment is many lines away from the lambda.

**Fix:** Capture a variable that nothing else will reassign. Declaring the variable inside the narrowest possible scope, and never reusing it, removes the problem entirely.

### Capturing in a method that runs every frame

```csharp
private void Update()
{
    float radius = _blastRadius;
    _targets.ForEach(t => Damage(t, radius));       // allocates every frame
}
```

**Symptom:** Nothing visible at first. Then a profiler session shows steady garbage collection pressure and periodic frame spikes, with the allocation attributed to a method that appears to allocate nothing.

**Fix:** A capturing lambda creates a hidden object each time the enclosing method runs, so sixty frames per second means sixty allocations per second per call site. Hoist the lambda into a field and build it once, or write an ordinary `foreach` loop. This is the same rule that keeps LINQ out of `Update` in [note 10](10-collections-and-linq.md).

## Check yourself

1. Rewrite `bool IsWounded(Enemy e) { return e.Health < 50; }` as a lambda assigned to a `Func<Enemy, bool>`.
2. Why does the `for` loop version print `3 3 3` whilst the `foreach` version prints the expected values?
3. Why does `-=` on an inline lambda fail silently rather than raising an error?
4. Which of these allocates, and why: `() => _score = 0` or `x => x * 2`?
5. You need a callback that both captures a local value and can be unsubscribed later. What do you do?

<details>
<summary>Answers</summary>

**1.** `Func<Enemy, bool> isWounded = e => e.Health < 50;` - no braces and no `return`, because a single-expression body returns its expression automatically.

**2.** A `for` loop declares one variable for the whole loop, so all the lambdas capture the same variable and see whatever value ended the loop. A `foreach` declares a fresh iteration variable on each pass, so each lambda captures a different one.

**3.** Because `-=` removes by reference identity and simply does not find a match. Removing a delegate that is not subscribed is a legal no-op - which is convenient for defensive code, and exactly what hides this bug.

**4.** `() => _score = 0` captures `this` in order to reach the field, so it allocates a hidden object. `x => x * 2` captures nothing at all and can be created once and reused by the compiler.

**5.** Store the lambda in a field, subscribe with `+=` on that field, and unsubscribe with `-=` on the same field. The identity has to be stable for `-=` to find it, and a field is what gives it a stable identity.

</details>

## Further reading

- [Lambda expressions (C# reference)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Capturing outer variables](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#capture-of-outer-variables-and-variable-scope-in-lambda-expressions)
- [Understanding the managed heap](https://docs.unity3d.com/Manual/performance-managed-memory.html)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t07_delegates_events

this_lesson:
  topic_code: t08_lambdas_closures
  difficulty_tier: Intermediate
mlos: [MLO3]
```
