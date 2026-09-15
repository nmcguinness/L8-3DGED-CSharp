# Exercises: Collections and LINQ

Read [note 10](../notes/10-collections-and-linq.md) first. Plain C# throughout.

---

## A - Mechanical

### Goal

Three small problems, each suited to a different collection. Working out which is which is most of the exercise.

### Constraints

Write one method for each of the following. Choose the collection type yourself; part of the exercise is that the right choice is different each time.

1. `CountWords(string text)` - returns how many times each word appears. Given `"the cat the hat"` it reports `the` twice and `cat` and `hat` once each.
2. `ProcessInOrder(string[] commands)` - accepts commands and processes them oldest first, printing each as it goes, until none remain.
3. `TopThree(List<int> scores)` - returns the three highest scores, highest first, without modifying the list passed in.

### Done when

- Each method uses a different collection type and you can say in one sentence why.
- `CountWords` does not use a `List` and does not call `Contains` in a loop.
- `ProcessInOrder` leaves its collection empty afterwards.
- `TopThree` leaves the caller's list in its original order.
- Reading a missing word from your `CountWords` result does not throw.

---

## B - Applied

### Goal

Two small systems that later topics will want: a command history you can replay and undo, and a shared store of facts looked up by name. Choose the right collection for each.

### Part one: the history

```csharp
public class Command
{
    public string Name { get; set; }
    public int Value { get; set; }
}
```

Requirements:

- `Record(Command)` adds to the history.
- `ReplayAll()` prints every command in the order it was originally issued.
- `UndoLast()` removes and returns the most recently recorded command.
- `Prune(int maxCount)` keeps only the most recent `maxCount` commands.

Replaying oldest-first and undoing newest-first pull in opposite directions. Solve it, and write a one-line comment explaining what you did about it.

### Part two: the fact store

A shared store that different systems write facts into and read facts out of by name. Facts have different types: the player's health is an `int`, whether the alarm is raised is a `bool`, the last known position is a `string`.

Requirements:

- `Set(string key, object value)` and `bool TryGet<T>(string key, out T value)`.
- `TryGet` returns `false` when the key is missing, and also when the key exists but holds a different type. It must not throw in either case.
- `Remove(string key)` and `bool Has(string key)`.

### Done when

- Recording three commands, undoing one and replaying prints the first two in order.
- `Prune(2)` after five commands leaves the two most recent, and replay still prints them oldest-first.
- `TryGet<int>("health", out h)` succeeds after `Set("health", 100)`.
- `TryGet<string>("health", out s)` returns `false` and does not throw.
- `TryGet<int>("nothing", out h)` returns `false` and does not throw.
- No method in either class calls `ContainsKey` immediately followed by an indexer lookup of the same key.

---

## C - Design

### Goal

A wave spawner has to answer several different questions about the same set of enemies, every frame, without allocating. Design the data structure behind it, then prove with real numbers that your version beats the obvious one.

The questions are:

- How many enemies are currently alive?
- Which alive enemy is nearest to a given point?
- How many alive enemies are of a given type?
- Which enemies were spawned in the current wave?
- Give me the enemy with a particular identifier.

### Constraints

- Use this shape:

```csharp
public class SpawnedEnemy
{
    public int Id { get; set; }
    public string TypeName { get; set; }
    public int WaveNumber { get; set; }
    public bool IsAlive { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}
```

- Assume up to two thousand enemies exist at once and that all five questions are asked every frame.
- Write a `Tick()` method that answers all five and is called in a loop. Treat it as though it were `Update`.
- LINQ is permitted anywhere except inside `Tick` or anything `Tick` calls. Use it freely in setup and in tests, where it is the right tool.
- Include a second, deliberately naive implementation that answers all five questions with LINQ over a single `List`, and a loop that times both over ten thousand ticks. You are expected to produce a number.

### The choice you must justify

The naive version is one list and five LINQ queries, and it is perfectly readable. The fast version keeps redundant structures - indexes by id, counts by type, a list per wave - and every one of those has to be kept in step whenever an enemy spawns or dies. You are trading clarity and a class of bugs for speed.

Decide how far along that trade to go. Answering all five questions in constant time is possible and is probably too much machinery for a problem this size.

Write a comment at the top of your fast implementation, ten to fifteen lines, covering:

- which of the five questions you optimised and which you left as a scan, and why those ones;
- the redundant state you introduced, and for each piece, the single place it is updated;
- the specific bug that appears if one of those updates is ever missed, and what you did to make that hard;
- your measured figures for both implementations, and whether the difference would have justified the work if the answer had been ten per cent rather than what you found.

The last point is asked seriously. A defensible answer to this exercise is "I optimised two of the five and the other three did not warrant it", provided you have the numbers to support it.

### Done when

- Both implementations exist and produce identical answers to all five questions for the same input.
- A test asserts that identity across a sequence of spawns and deaths.
- `Tick` and everything it calls contain no LINQ and allocate nothing per call.
- The timing loop runs and prints figures for both.
- Every piece of redundant state is written in exactly one place.
- The justification comment contains real measured numbers, not estimates.
