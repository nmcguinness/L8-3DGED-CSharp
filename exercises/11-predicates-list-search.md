# Exercises: Predicates, comparisons and List search

Read [note 11](../notes/11-predicates-list-search.md) first. Plain C# console project throughout.

Every exercise uses this shape, so that everyone starts from the same data:

```csharp
public class Player
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public float MoveSpeed { get; set; }
    public bool IsActive { get; set; }

    public override string ToString()
    {
        return Name + " (" + Id + ", hp=" + Health + ", spd=" + MoveSpeed + ")";
    }
}
```

---

## A - Mechanical

### Goal

Use the search methods `List<T>` already has, passing the condition in as a value rather than writing a loop.

### Constraints

Populate a `List<Player>` with five players of differing health, speed and active state, then write one method for each of these. Use the `List<T>` method named in brackets; do not write the loop by hand and do not use LINQ.

1. `FindActive` - every active player (`FindAll`).
2. `FindById` - the player with a given id, or `null` if there is none (`Find`).
3. `CountWounded` - how many have health below 50 (`FindAll` then `Count`, or better).
4. `AnyDead` - whether at least one has health of zero (`Exists`).
5. `RemoveInactive` - drop every inactive player from the list (`RemoveAll`).

### Constraints on how

- Each condition is a `Predicate<Player>`. Write at least two of them as named methods and at least two as lambdas, so you have seen both.
- `FindById` must return `null` for a missing id and must not throw.
- `RemoveInactive` must report how many it removed.

### Done when

- All five work against the same five-player list.
- `FindById` on an id that is not present returns `null`.
- After `RemoveInactive`, the list contains only active players and the count returned matches the number gone.
- You can state what type a `Predicate<Player>` actually is, in terms of [note 07](../notes/07-delegates-events-action-func.md).

---

## B - Applied

### Goal

Sort by a rule supplied from outside, and find a single best item in one pass.

### Part one: sorting

Write these, using `List<T>.Sort` with a comparison supplied by the caller:

- `SortByHealthDescending` - highest health first.
- `SortByName` - alphabetical.
- `SortBy(List<Player> players, Comparison<Player> rule)` - sorts by whatever rule it is handed.

Then write the first two **in terms of** the third. If they duplicate any logic, you have not finished.

### Part two: finding the best

Write `Player FindFastest(List<Player> players)`.

There is a trap here worth hitting deliberately. `Find` takes a `Predicate<Player>`, which can only answer yes or no about **one** player - it cannot compare two. Try to write `FindFastest` with `Find` first, see where it fails, then write it properly.

### Constraints

- `SortBy` must leave a caller free to sort by any field without you adding a method.
- `FindFastest` must do a single pass and must not sort the list.
- `FindFastest` must handle an empty list without throwing. Decide what it returns and say why in a comment.
- `Sort` reorders the list in place. Where that is wrong for a caller, say so and provide a way to avoid it.

### Done when

- `SortBy` handles at least three different rules with no change to its own code.
- `SortByHealthDescending` and `SortByName` contain no comparison logic of their own beyond the rule they pass.
- `FindFastest` returns the right player and does not sort.
- `FindFastest` on an empty list behaves as your comment says it does.
- You can explain in one sentence why `Find` cannot express "the fastest".

---

## C - Design

### Goal

A scoreboard needs to answer several different questions about the same list of players, and the questions are not known in advance - a designer will add more next term without opening your code.

Design the query surface for it.

### Constraints

- The scoreboard holds a `List<Player>` and exposes a way to filter, a way to sort, and a way to pick a single best item.
- A new question must cost no edit to the scoreboard class.
- At least one method must accept a rule for ranking and return the top *n* items.
- Provide a way to combine two conditions - "active **and** wounded" - without writing a third named condition for every pair.
- LINQ is permitted here, but you must also provide the equivalent using only `Predicate<T>`, `Comparison<T>` and `List<T>` methods, and say what each version costs.

### The choice you must justify

The central decision is what a "question" is, and the candidates behave very differently as they multiply:

- **A `Predicate<Player>` passed in at each call.** Simple, and a combination of two conditions needs the caller to write a lambda calling both.
- **A small `IQuery` interface** with an `IsMatch` method, which is [note 05](../notes/05-interfaces.md). More ceremony, but a query becomes an object you can name, store, reuse and combine with a `Combine(a, b)` helper.
- **A string or enum naming the question,** switched on inside the scoreboard. It reads well from a designer's perspective and puts every question back inside the class, which is the thing you were asked to avoid.

There is a second decision: whether sorting reorders the scoreboard's own list or returns a new ordering. Reordering is cheaper and means two callers asking different questions interfere with each other.

Write a comment at the top of your scoreboard file, ten to fifteen lines, covering:

- which representation you chose, and which you rejected;
- how you combined two conditions, and what that costs when it is three or four;
- your sort-in-place decision, and the specific interference bug the other choice avoids;
- your LINQ version against your hand-written version - what each costs, and where you would refuse to use the LINQ one, referring to [note 10](../notes/10-collections-and-linq.md).

### Done when

- Filtering, sorting and top-*n* all work and are demonstrated.
- Adding a fourth question requires no edit to the scoreboard class.
- Two conditions can be combined without a new named method per pair.
- Both the LINQ and the non-LINQ version exist and produce identical results, proven by a test.
- The justification comment covers all four bullets by name.
