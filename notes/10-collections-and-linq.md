---
title: "Collections and LINQ"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t10_collections_linq
description: "Choosing List, Dictionary, Queue or Stack by how the data will be read, and using LINQ for readability while keeping it out of the per-frame path."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, collections, list, dictionary, queue, stack, linq, allocation, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t09_generics
prerequisites:
  - Generics
  - Lambdas, for LINQ predicates
  - out parameters, for TryGetValue
---

# Collections and LINQ

## What this unblocks

The command topic records a history of commands so that actions can be replayed or undone, which needs an ordered collection with a defined end to take from. The blackboard is a shared store that AI components write facts into and read facts out of by name, which is a dictionary. The object pool reuses objects, which needs a store you can take from and return to cheaply.

Three different jobs, three different collections, and choosing wrongly is usually the reason a system ends up slow or awkward. The second half of this note is about LINQ, and about the one rule that keeps it from becoming the reason your game drops frames once the scene gets busy.

## The idea

Collections differ in what they make cheap. A `List<T>` is fast to index and slow to search. A `Dictionary<TKey, TValue>` is fast to look up by key and has no order at all. A `Queue<T>` gives you first-in-first-out and nothing else.

So the question is never "which collection is best". It is "how will this be read?". Answer that and the choice makes itself.

| Access pattern | Collection |
|---|---|
| By position, or iterate all of it | `List<T>` |
| By a key you already hold | `Dictionary<TKey, TValue>` |
| Oldest first | `Queue<T>` |
| Newest first | `Stack<T>` |

## In code

### List

Ordered, indexed, resizable. The default choice when there is no reason for another.

```csharp
List<Enemy> enemies = new List<Enemy>();

enemies.Add(goblin);
enemies.Insert(0, boss);

Enemy first = enemies[0];
int count = enemies.Count;

enemies.Remove(goblin);         // by value, searches the list
enemies.RemoveAt(2);            // by position, immediate
```

`Contains` and `Remove` walk the list from the start, so both get slower as the list grows. Indexing does not. If you find yourself calling `Contains` inside a loop over the same list, you have written something that slows down quadratically, and a `Dictionary` or a `HashSet` is the fix.

### Dictionary

Stores values against keys and finds them in roughly constant time regardless of size. This is the blackboard.

```csharp
Dictionary<string, int> ammunition = new Dictionary<string, int>();

ammunition["rifle"] = 30;           // adds, or overwrites if present
ammunition.Add("pistol", 12);       // adds, or throws if already present

int rifleRounds = ammunition["rifle"];
```

Reading a key that is not there throws. Use `TryGetValue` when absence is a normal outcome rather than a bug:

```csharp
int rounds;

if (ammunition.TryGetValue("shotgun", out rounds))
{
    Reload(rounds);
}
else
{
    ReportEmpty();
}
```

`TryGetValue` is one lookup. `ContainsKey` followed by an indexer is two, doing the same work twice, so prefer `TryGetValue` wherever both would work.

A dictionary has no meaningful order. Iterating one gives you the entries in an order you must not rely on, and it can change between runs.

```csharp
foreach (KeyValuePair<string, int> entry in ammunition)
{
    Console.WriteLine(entry.Key + ": " + entry.Value);
}
```

### Queue and Stack

`Queue<T>` is first-in-first-out. Put things in one end, take them out of the other, in the order they arrived.

```csharp
Queue<string> commandHistory = new Queue<string>();

commandHistory.Enqueue("move");
commandHistory.Enqueue("fire");

string oldest = commandHistory.Dequeue();       // "move"
string next = commandHistory.Peek();            // "fire", left in place
```

`Peek` reads without removing. Both `Dequeue` and `Peek` throw on an empty queue, so check `Count` first.

`Stack<T>` is the same idea reversed: last-in-first-out, with `Push`, `Pop` and `Peek`. Which one you want follows from the job. Replaying commands in the order they were issued is a queue. Undoing the most recent command first is a stack. The command topic will need you to have decided which of those you are building.

For pooling, either works, because the pool does not care which item it hands back. A `Stack<T>` is marginally the better choice, since returning the most recently used item is friendlier to the CPU cache.

### LINQ

LINQ is a set of methods over collections that replace hand-written loops with a description of what you want. It is available after `using System.Linq;`.

```csharp
List<Enemy> enemies = GetEnemies();

// Hand-written
List<Enemy> wounded = new List<Enemy>();
foreach (Enemy enemy in enemies)
{
    if (enemy.Health < 50)
    {
        wounded.Add(enemy);
    }
}

// LINQ
IEnumerable<Enemy> woundedQuery = enemies.Where(enemy => enemy.Health < 50);
```

The operators you will actually use:

```csharp
enemies.Where(e => e.Health < 50);          // filter
enemies.Select(e => e.Name);                // transform to something else
enemies.FirstOrDefault(e => e.IsBoss);      // first match, or null if none
enemies.Any(e => e.IsAlerted);              // is there at least one
enemies.Count(e => e.IsAlerted);            // how many
enemies.OrderBy(e => e.Health);             // sorted copy
```

Those take lambdas, which is [note 08](08-lambdas-and-closures.md), and they are generic methods, which is [note 09](09-generics.md). Nothing here is new machinery; it is the previous two notes applied to collections.

Note the return type in the example above. `Where` returns `IEnumerable<Enemy>`, not a `List<Enemy>`, and it has not done any work yet. The query runs when you iterate it, and it runs *again* every time you iterate it. `ToList()` forces it once and gives you a real list:

```csharp
List<Enemy> wounded = enemies.Where(e => e.Health < 50).ToList();
```

### The allocation warning

Every LINQ call allocates. `Where` allocates an iterator object, `Select` allocates another, a capturing lambda allocates the hidden object from note 04, and `ToList` allocates the list and its backing array. A three-operator chain can easily produce half a dozen objects.

For code that runs once, when a level loads or a menu opens, that is irrelevant and the readability is worth having.

For code that runs every frame it is not irrelevant at all. At sixty frames per second, a chain allocating six objects produces three hundred and sixty objects per second from one line. Those objects are collected eventually, and in Unity that collection is a pause you can see.

**The rule for this module: no LINQ in `Update`, `FixedUpdate`, `LateUpdate`, or anything they call.** Filter once when the set changes and cache the result, or write the loop by hand. The optimisation topic returns to this with a profiler open, and the frame spikes you will be looking at there come from exactly this.

## Common mistakes

### Modifying a collection whilst iterating it

```csharp
foreach (Enemy enemy in enemies)
{
    if (enemy.IsDead)
    {
        enemies.Remove(enemy);          // wrong
    }
}
```

**Symptom:** `InvalidOperationException: Collection was modified; enumeration operation may not execute.` It is thrown on the `foreach` line rather than the `Remove` line, which sends people looking in the wrong place.

**Fix:** Iterate backwards by index, so removal cannot disturb the positions you have not reached yet:

```csharp
for (int i = enemies.Count - 1; i >= 0; i--)
{
    if (enemies[i].IsDead)
    {
        enemies.RemoveAt(i);
    }
}
```

`enemies.RemoveAll(e => e.IsDead)` is the concise alternative and is fine outside `Update`.

### Indexing a dictionary key that is not present

```csharp
int rounds = ammunition["shotgun"];     // wrong, if the player has no shotgun
```

**Symptom:** `KeyNotFoundException: The given key 'shotgun' was not present in the dictionary.` Reliably, this works throughout development because your test save file has every weapon in it.

**Fix:** `TryGetValue` when the key may legitimately be missing. Use the indexer only where absence would be a genuine bug, in which case the exception is telling you the truth.

### Assuming a LINQ query holds a result

```csharp
IEnumerable<Enemy> wounded = enemies.Where(e => e.Health < 50);

HealAll();                              // everyone is now above 50

foreach (Enemy enemy in wounded)        // iterates nothing
{
    Report(enemy);
}
```

**Symptom:** The loop processes the wrong set, or nothing at all, and the query line above it looks obviously correct. Worse, iterating `wounded` twice does the filtering work twice, so a query built from an expensive predicate silently costs double.

**Fix:** Understand that the query is a description, evaluated on iteration. Call `ToList()` at the point where you want the answer fixed, and treat anything typed `IEnumerable<T>` as not yet run.

### LINQ in `Update`

```csharp
private void Update()
{
    Enemy nearest = _enemies
        .Where(e => !e.IsDead)
        .OrderBy(e => Distance(e))
        .FirstOrDefault();              // wrong place
}
```

**Symptom:** Nothing for a long time. Then the profiler shows a steady garbage allocation of several kilobytes per second attributed to this method, and periodic frame drops as the collector runs. `OrderBy` is the worst of it, since sorting allocates a buffer for the whole sequence every single frame.

**Fix:** Recalculate only when the answer could have changed - when an enemy dies, or on a timer every few frames - and store the result in a field. If it genuinely must be every frame, write a single manual loop that tracks the best candidate as it goes, which allocates nothing and does one pass rather than three.

## Check yourself

1. You need to look up a value by a string name. Which collection, and why not a `List`?
2. What is the difference between `ammunition["rifle"] = 30` and `ammunition.Add("rifle", 30)` when the key already exists?
3. What does `enemies.Where(e => e.Health < 50)` return, and when does the filtering actually happen?
4. Why is removing from a list inside a `foreach` over that list an error, and what is the standard fix?
5. Why is `OrderBy` a particularly bad thing to put in `Update`?

<details>
<summary>Answers</summary>

**1.** A `Dictionary<string, T>`. Looking up by name in a `List` means walking it and comparing each element, which gets slower as the list grows; a dictionary finds the entry in roughly constant time no matter how large it is.

**2.** The indexer overwrites the existing value. `Add` throws an `ArgumentException`. Use `Add` when a duplicate key would indicate a bug, and the indexer when overwriting is intended.

**3.** An `IEnumerable<Enemy>`, which is a description of the query rather than a result. The filtering happens when you iterate it, and happens again on every subsequent iteration. `ToList()` runs it once and captures the outcome.

**4.** The enumerator detects that the collection changed underneath it and throws `InvalidOperationException`. Loop backwards by index and use `RemoveAt`, or use `RemoveAll` outside performance-critical code.

**5.** Because sorting cannot be done lazily - it has to allocate a buffer holding the whole sequence and sort that - so it produces a large allocation every frame rather than a small one.

</details>

## Further reading

- [Commonly used collection types](https://learn.microsoft.com/en-us/dotnet/standard/collections/commonly-used-collection-types)
- [Language Integrated Query (LINQ)](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)
- [Memory management best practices in Unity](https://docs.unity3d.com/Manual/performance-managed-memory.html)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t09_generics

this_lesson:
  topic_code: t10_collections_linq
  difficulty_tier: Intermediate
mlos: [MLO3]
```
