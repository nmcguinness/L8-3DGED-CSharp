---
title: "Predicates, comparisons and List search"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t11_predicates_list_search
description: "Passing a condition as a value with Predicate, sorting by a rule with Comparison, and the List search methods that do the work of LINQ without the allocation."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, predicate, comparison, list, find, sort, removeall, allocation, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t10_collections_linq
prerequisites:
  - Delegates, Action and Func
  - Lambdas and capture
  - Generics
  - List and the LINQ operators
---

# Predicates, comparisons and List search

## What this unblocks

The command history needs to find and drop entries by a rule. The blackboard is asked questions its author did not anticipate. The object pool has to pick an item matching a condition. In all three, the condition is decided by the *caller*, not by the collection.

[Note 10](10-collections-and-linq.md) ended with a rule: no LINQ in `Update`. This note is what you use instead. The `List<T>` search methods do the same jobs, in place, without building an iterator chain - which makes them the per-frame tool and LINQ the setup-time tool.

## The idea

A **predicate** is a question you can pass around: given one item, yes or no.

You have already written these. `Predicate<T>` is a delegate taking a `T` and returning a `bool`, which makes it the same shape as `Func<T, bool>` from [note 07](07-delegates-events-action-func.md). Two names for one idea, and which one you meet depends only on the age of the API - `List<T>` predates `Func`, so it takes `Predicate<T>`.

A **comparison** is the two-argument version: given two items, which comes first.

Both let a collection do work it has no knowledge of. `List<T>.FindAll` knows how to walk a list; it does not know what "wounded" means, and it never needs to.

## In code

### Predicate as a value

```csharp
public class Player
{
    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets current health.</summary>
    public int Health { get; set; }

    /// <summary>Gets or sets whether the player is in play.</summary>
    public bool IsActive { get; set; }
}
```

The condition can be a named method, a lambda, or a stored variable:

```csharp
private static bool IsWounded(Player p)
{
    return p.Health < 50;
}

Predicate<Player> wounded = IsWounded;                 // method group
Predicate<Player> active = p => p.IsActive;            // lambda
Predicate<Player> both = p => p.IsActive && p.Health < 50;
```

`Predicate<Player>` and `Func<Player, bool>` describe identical signatures, but they are **different types** and do not implicitly convert. If a method wants one and you hold the other, wrap it:

```csharp
Func<Player, bool> f = p => p.Health < 50;
Predicate<Player> p = new Predicate<Player>(f);        // or: item => f(item)
```

### The List search methods

```csharp
List<Player> players = GetPlayers();

Player first = players.Find(p => p.Health < 50);        // first match, or default
List<Player> all = players.FindAll(p => p.IsActive);    // every match, a new list
int index = players.FindIndex(p => p.Name == "Ana");    // position, or -1
bool any = players.Exists(p => p.Health == 0);          // is there at least one
bool every = players.TrueForAll(p => p.Health > 0);     // do all of them match
int removed = players.RemoveAll(p => !p.IsActive);      // deletes in place, returns count
```

Two of those are worth dwelling on.

`RemoveAll` is the correct answer to the modify-while-iterating problem from [note 10](10-collections-and-linq.md). It does one pass, removes in place, and returns how many went - no backwards loop, no `InvalidOperationException`.

`Find` returns `default(T)` when nothing matches. For a class that is `null`, which is what you expect. For a **struct** it is a zeroed value, not null - a `Find` over a `List<Vector3>` that matches nothing hands you `(0, 0, 0)`, which is a perfectly plausible position. Use `FindIndex` and test for `-1` when the element type is a value type.

### Comparison and Sort

`Comparison<T>` takes two items and returns a number: negative if the first comes first, zero if they are equal, positive if the second comes first.

```csharp
players.Sort((a, b) => b.Health.CompareTo(a.Health));   // health, highest first
players.Sort((a, b) => a.Name.CompareTo(b.Name));       // name, alphabetical
```

`a.CompareTo(b)` gives ascending order. Swapping the operands to `b.CompareTo(a)` gives descending - that reversal is the whole trick, and writing `-a.CompareTo(b)` instead is a common way to get it subtly wrong.

Because the rule is a value, a method can accept one and sort by anything:

```csharp
/// <summary>Sorts the supplied players by whatever rule is given.</summary>
/// <param name="players">The list to sort, in place.</param>
/// <param name="rule">The ordering to apply.</param>
public static void SortBy(List<Player> players, Comparison<Player> rule)
{
    players.Sort(rule);
}
```

`Sort` reorders **the list you gave it**. There is no copy. If a caller is holding that list for another purpose, you have just changed what they see - which is the main practical difference from LINQ's `OrderBy`, and it is covered under common mistakes.

### Choosing between this and LINQ

They overlap almost completely. The differences are what matter:

| | `List<T>` methods | LINQ |
|---|---|---|
| Result | A real list, or in place | A lazy description until enumerated |
| Allocation | `FindAll` allocates one list; the rest allocate nothing | An iterator per operator, plus the lambda |
| Sorting | In place, no copy | A sorted copy, buffering the whole sequence |
| Chaining | No | Yes, and readably |
| Works on | `List<T>` only | Any `IEnumerable<T>` |

The rule follows from the middle two rows. **In `Update` or anything it calls, use the `List<T>` methods.** Outside the per-frame path, use LINQ where it reads better - and it very often does.

One shared caution: a lambda that captures a variable allocates, wherever you write it. `players.Find(p => p.Health < threshold)` captures `threshold` and creates a hidden object on every call, exactly as [note 08](08-lambdas-and-closures.md) described. A predicate that captures nothing is created once and reused, so prefer a method group or a cached predicate in a hot loop.

## Common mistakes

### Assuming Find returns null for a struct

```csharp
List<Vector3> waypoints = GetWaypoints();
Vector3 next = waypoints.Find(w => w.Y > 100f);

if (next == null) { return; }           // will not compile for a struct
MoveTo(next);                           // moves to (0, 0, 0)
```

**Symptom:** For a value type the null check does not even compile, and once you remove it the code silently moves to the origin, because `default(Vector3)` is a valid-looking position.

**Fix:** Use `FindIndex` and compare against `-1`, which is unambiguous for every element type:

```csharp
int i = waypoints.FindIndex(w => w.Y > 100f);
if (i == -1) { return; }
MoveTo(waypoints[i]);
```

### Sorting a list somebody else is holding

```csharp
public static Player GetStrongest(List<Player> players)
{
    players.Sort((a, b) => b.Health.CompareTo(a.Health));   // reorders the caller's list
    return players[0];
}
```

**Symptom:** A method that reads like a query quietly reorders its argument. The scoreboard shifts, the spawn order changes, or an index another system was holding now points at a different player. Nothing throws, and the cause is a method whose name promised only to *get* something.

**Fix:** Either do not sort - a single pass finds the maximum without reordering anything - or sort a copy: `List<Player> byHealth = new List<Player>(players);`. If the method really is meant to reorder, name it so: `SortByHealth`.

### An inconsistent comparison

```csharp
players.Sort((a, b) => a.Health < b.Health ? -1 : 1);       // never returns 0
```

**Symptom:** `InvalidOperationException: IComparer.Compare() method returns inconsistent results`, or - worse - no exception and an order that changes between runs for equal elements.

**Fix:** Return zero for equal items. `a.Health.CompareTo(b.Health)` does this correctly and is shorter than the ternary that got it wrong.

### Capturing a loop variable in a predicate

```csharp
List<Predicate<Player>> checks = new List<Predicate<Player>>();

for (int i = 0; i < thresholds.Count; i++)
{
    checks.Add(p => p.Health < thresholds[i]);      // captures i
}
```

**Symptom:** Every predicate tests against the last threshold, and the first one you run throws `ArgumentOutOfRangeException` because `i` is now past the end. This is the [note 08](08-lambdas-and-closures.md) bug wearing different clothes.

**Fix:** Copy the loop variable inside the body - `int index = i;` - and capture the copy. Or use `foreach`, which gives a fresh variable each pass.

## Check yourself

1. What is the relationship between `Predicate<T>` and `Func<T, bool>`?
2. Why is `Find` unsafe for a `List<Vector3>` and fine for a `List<Player>`?
3. `List.Sort` and LINQ's `OrderBy` both sort. What is the difference a caller will notice?
4. When would you deliberately choose `FindAll` over `Where`?
5. Why must a `Comparison<T>` return zero for equal items?

<details>
<summary>Answers</summary>

**1.** They describe the same signature - one argument in, a `bool` out - but they are distinct types and do not implicitly convert. `Predicate<T>` is the older name and is what `List<T>` takes; `Func<T, bool>` is the generic family from note 07 and is what LINQ takes.

**2.** Because `Find` returns `default(T)` when nothing matches. For `Player`, a class, that is `null` and obvious. For `Vector3`, a struct, it is `(0, 0, 0)` - a legitimate-looking value that cannot be distinguished from a real result. Use `FindIndex` and check for `-1`.

**3.** `Sort` reorders the list in place, so anybody else holding that list sees the new order. `OrderBy` leaves the original untouched and yields a sorted sequence, at the cost of buffering the whole thing.

**4.** In the per-frame path. `FindAll` allocates one list and returns a real result; a `Where` chain allocates an iterator per operator and re-runs on every enumeration. Outside `Update` the readability of LINQ usually wins.

**5.** Because the sort algorithm relies on the comparison being consistent - if `Compare(a, b)` and `Compare(b, a)` both claim a strict ordering, the algorithm can reach a contradiction. It detects this and throws, or produces an unstable order for equal elements.

</details>

## Further reading

- [Predicate delegate](https://learn.microsoft.com/en-us/dotnet/api/system.predicate-1)
- [Comparison delegate](https://learn.microsoft.com/en-us/dotnet/api/system.comparison-1)
- [List.Sort method](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t10_collections_linq

this_lesson:
  topic_code: t11_predicates_list_search
  difficulty_tier: Intermediate
mlos: [MLO3]
```
