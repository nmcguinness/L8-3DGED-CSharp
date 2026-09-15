# Exercises: Unit testing with xUnit

Read [note 14](../notes/14-unit-testing.md) first.

Every exercise here writes tests for code **you have already written** in an earlier exercise file. If you have not done that exercise yet, do it first - there is nothing to test otherwise.

Set up one test project and put all seven exercises in it, one test class per exercise:

```text
dotnet new xunit -o Week01.Tests
cd Week01.Tests
dotnet test
```

Copy the classes under test into the project alongside the tests. Seven exercises, rising in difficulty.

---

## A - Mechanical

Three short sets. The point is the mechanics of `[Fact]`, `[Theory]` and the assertions, not cleverness.

### Goal

Get a test project running and cover the boundaries of code whose correct behaviour you already know.

### Exercise 1 - clamping in ColourRGBA

Tests for [exercise 01](01-properties-static-tostring.md), exercise A.

- One `[Theory]` covering the clamping of a component. Include at minimum: `0`, `1`, a value above `1`, a value below `0`, and a value inside the range.
- One `[Fact]` that the parameterless constructor gives opaque white.
- One `[Fact]` that `ToString()` produces the `RGBA(...)` form.

Every assertion on a component must use the floating-point precision overload. Write one of them without it first, watch it fail, then fix it.

### Exercise 2 - static members and computed properties

Tests for [exercise 01](01-properties-static-tostring.md), exercise B.

- `ToGreyscale()` returns a colour whose R, G and B are equal, and whose alpha matches the original.
- `ToGreyscale()` does **not** modify the colour it was called on. This needs two assertions of different kinds - one that the original is unchanged, and one that the result is a different object.
- `Lerp` at `t = 0` and `t = 1` returns the endpoints exactly; at `t = 0.5` it returns the midpoint.
- `Lerp` with `t` outside 0 to 1 clamps.

### Exercise 3 - the Try pattern

Tests for [exercise 04](04-ref-and-out.md), exercise B.

- `TryGetItem` with a filled slot returns `true` and assigns a usable item.
- `TryGetItem` with a missing slot returns `false` and assigns `null`.
- `TryGetItem` with a missing slot **throws nothing**. State how you prove a method does not throw.
- `ResolveHit` called twice on an already-dead target leaves health at zero and still reports dead.

### Done when

- `dotnet test` runs and every test passes.
- Your theory in exercise 1 reports one result per row, not one for the method. Check the output.
- You have seen a float assertion fail without the precision argument and can explain the message it printed.
- Every test name reads as a sentence describing behaviour.
- No test contains an `if` or a loop.

---

## B - Applied

Three sets where the thing being tested resists testing a little.

### Goal

Test behaviour that has no return value, behaviour that depends on identity rather than equality, and behaviour that is supposed to fail.

### Exercise 4 - the equality contract

Tests for [exercise 03](03-operator-overloading.md), exercise B.

An equality implementation has obligations beyond "two equal things are equal". Write a test for each:

- Two distinct objects with the same value are equal by `==`, by `Equals`, and produce the **same hash code**.
- Equality is symmetric: `a == b` and `b == a` agree.
- `a == null` and `null == a` are both false and neither throws.
- `null == null` is true.
- An object used as a dictionary key is found by an equal but distinct key.

That last one is the test that would have caught the bug in the note. Write it even if you are confident.

### Exercise 5 - interchangeable behaviours

Tests for [exercise 05](05-interfaces.md), exercise C.

That exercise required the selection rules to be testable **without constructing a Turret**. This is where you collect on that.

- Each rule, given the same list of enemies, selects the one you expect. One test per rule.
- Every rule handles an empty candidate list in the way you specified. If you chose to return null, assert null; if you chose to throw, use `Assert.Throws`.
- A rule handles a single-candidate list.
- Swapping a turret's rule changes which enemy the **turret** selects. This one does need a `Turret`, and it is the test that proves the mechanism works rather than just the rules.

If any rule cannot be tested without a `Turret`, your exercise 05 design did not meet its constraints. Fix the design, not the test.

### Exercise 6 - events

Tests for [exercise 07](07-delegates-events-action-func.md), exercise B.

Events return nothing, so every test here has to record what it saw.

- Changing the temperature notifies an attached subscriber with the new value.
- A detached subscriber receives nothing. Assert on a call count, not a flag.
- Attaching the same subscriber twice results in **two** calls per change - or one, if you made `Attach` guard against it. Assert whichever your implementation actually promises.
- Changing the temperature with no subscribers attached throws nothing.
- The alarm event fires only when crossing above the threshold, not on every change above it.

Every test that subscribes must unsubscribe before it asserts. Say in a comment why that matters more in a test suite than in a game.

### Done when

- All of B passes.
- Exercise 4 includes the dictionary test and you can say what it would catch.
- Exercise 5 tests at least three rules without a `Turret` anywhere in the test.
- Exercise 6 asserts on counts rather than booleans wherever a count is more informative.
- No test in B shares state with another. Run the suite three times and confirm the results do not change.

---

## C - Design

### Goal

One exercise, and it is a design task rather than a coverage task. You are deciding what a test suite should assert when the thing under test has more than one defensible implementation.

### Exercise 7 - testing an open design

Tests for [exercise 09](09-generics.md), exercise C - the `IPool<T>` interface with two implementations, one growing without limit and one with a fixed maximum.

The difficulty is that exercise 09 deliberately left the exhaustion behaviour open. Yours might throw, return the type default, recycle the oldest item, or expand and log. A test asserting `Assert.Throws` would be wrong for four of those five choices.

Write two test classes:

- **A shared suite** that both implementations must pass, expressing only what `IPool<T>` guarantees regardless of implementation. Run it against both. It must contain no reference to either concrete type except where the instance is created.
- **An implementation-specific suite** for the fixed-size pool, asserting the exhaustion behaviour you actually chose.

### Constraints

- The shared suite must be written once and executed against both implementations. Work out how - xUnit gives you more than one way, and choosing between them is part of the exercise.
- The shared suite must include at least: capacity after construction, that `Get` reduces availability, that `Return` restores it, that a returned item has been reset, and that returning the same item twice behaves as you specified.
- Neither suite may contain an `if` that switches on which implementation is running. If you need one, the split between the two suites is in the wrong place.

### The choice you must justify

Two decisions.

**What belongs in the shared suite.** Anything you put there becomes part of the contract, binding on every future implementation. Anything you leave out is something a future implementation may do differently and still be correct. That boundary is a design decision about `IPool<T>` itself, and writing the tests will expose whether the interface you designed in exercise 09 actually said enough.

**How the shared suite runs twice.** An abstract base test class with an abstract factory method, a `[Theory]` fed by types, or plain construction in each subclass. They differ in how much ceremony they cost and in how obvious the failure output is.

Write a comment at the top of the shared suite, ten to fifteen lines, covering:

- which guarantees you put in the shared suite and which you deliberately left out, with the reason for one of each;
- the mechanism you chose to run it twice, and what the alternative would have cost;
- one thing your tests revealed about the `IPool<T>` interface from exercise 09 - a guarantee it implies but never states, or one it states but cannot enforce;
- whether the exhaustion behaviour you chose in exercise 09 still looks right now that you have had to write its test, and if not, what you would change.

The last bullet is the real exercise. Tests are the first honest reader your design gets.

### Done when

- The shared suite runs against both implementations and passes for both.
- The implementation-specific suite asserts the exhaustion behaviour your exercise 09 comment claimed.
- Deleting one implementation leaves the shared suite still compiling and still meaningful for the other.
- No test branches on which implementation it is running.
- The justification comment covers all four bullets by name.
- `dotnet test` reports every test in the file passing, and you can state the total count without looking.
