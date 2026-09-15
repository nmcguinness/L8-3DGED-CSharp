---
title: "Unit testing with xUnit"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t14_unit_testing
description: "Writing tests that state what your code should do, using xUnit facts, theories and assertions, and knowing which tests are worth the keystrokes."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, testing, xunit, fact, theory, assertions, test-isolation, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3, MLO4]
previous_topic: t13_null_handling
prerequisites:
  - All of notes 01 to 13, since the exercises test code from them
  - Lambdas, for assertions that take a delegate
  - Generics, for the shape of Assert.Throws
---

# Unit testing with xUnit

## What this unblocks

Every exercise file in this module ends with a **done when** list. Those lists are specifications written in English, and English is not checkable. This note turns them into something a machine runs in half a second.

The immediate payoff is the coding exam. In a timed exam, the difference between finishing and not finishing is usually how long you spend wondering whether the thing you just changed broke the thing you wrote twenty minutes ago. A test suite answers that in one keystroke.

The longer payoff is that you can change code without fear. Refactoring is only safe when something independent is watching, and later modules in the programme will assume you have met that idea.

## The idea

A test is a small program that runs your code and asserts something about the result.

That is genuinely all. There is no magic in a testing framework - it finds methods you have marked, runs them, and reports which ones threw. The framework's contribution is the finding, the running and the reporting; the thinking is entirely yours.

Every test has three parts, and keeping them visually separate is most of what makes a test readable:

- **Arrange** - build the objects the test needs.
- **Act** - do the one thing being tested.
- **Assert** - state what should now be true.

The value is not in catching a bug today. You already know the code works today; you just ran it. The value is that in three weeks, when you change something apparently unrelated, the test tells you within seconds that you broke this.

## In code

### Setting up

A test project is separate from the code it tests:

```text
dotnet new xunit -o Week01.Tests
cd Week01.Tests
dotnet test
```

`dotnet test` builds and runs everything. For these exercises it is simplest to put the classes under test in the same project as the tests - a real project would reference a separate library, but that is setup you do not need yet.

### A first test

```csharp
using System;
using Xunit;

public class HealthTests
{
    [Fact]
    public void TakeDamage_ReducesHealthByTheAmountGiven()
    {
        // Arrange
        Enemy enemy = new Enemy(100);

        // Act
        enemy.TakeDamage(30);

        // Assert
        Assert.Equal(70, enemy.Health);
    }
}
```

`[Fact]` marks a method as a test that takes no input. The method must be `public` and return `void`. The class must be `public` too, or the framework will not see it.

### Naming

A test name is read in a failure report, out of context, by somebody who did not write it - quite possibly you, three weeks later. Use three parts:

```text
MethodUnderTest_Scenario_ExpectedResult
```

```csharp
TakeDamage_ReducesHealthByTheAmountGiven
TakeDamage_WhenAlreadyDead_DoesNotReduceHealthFurther
Get_WhenPoolIsEmpty_CreatesANewItem
TryGetItem_WithMissingSlot_ReturnsFalseAndNull
```

Those read as sentences and say what the code should do. `Test1` and `HealthTest` do not.

### The assertions you will actually use

```csharp
Assert.Equal(70, enemy.Health);             // expected FIRST, actual second
Assert.NotEqual(0, enemy.Health);

Assert.True(enemy.IsAlive);
Assert.False(enemy.IsDead);

Assert.Null(pool.Find("missing"));
Assert.NotNull(found);

Assert.Empty(results);
Assert.Single(results);                     // exactly one
Assert.Contains(goblin, results);

Assert.Same(original, returned);            // the SAME object
Assert.NotSame(original, returned);         // an equal but distinct object
```

**Argument order matters.** `Assert.Equal(expected, actual)` - expected first. Get it backwards and the code still passes or fails correctly, but the failure message says "Expected: 70, Actual: 100" when the truth is the reverse, and you will chase the wrong thing.

`Assert.Same` against `Assert.Equal` is the distinction from [note 03](03-operator-overloading.md): `Same` asks whether it is the same object, `Equal` asks whether your equality says they match. Testing a method that must return a *copy* needs `NotSame` as well as `Equal`.

### Floating point needs a tolerance

```csharp
double actual = 0.1 + 0.2;

Assert.Equal(0.3, actual);          // FAILS
Assert.Equal(0.3, actual, 10);      // passes - 10 decimal places
```

`0.1 + 0.2` is not `0.3` in binary floating point and never will be. The third argument is the number of decimal places to compare. Any test touching a `float` or `double` needs it - which, in a module full of vectors and colours, is most of them.

### Theories: one test, many rows

When the same logic should hold for several inputs, do not copy the test:

```csharp
[Theory]
[InlineData(0, 0)]
[InlineData(50, 50)]
[InlineData(100, 100)]
[InlineData(250, 100)]      // clamped
[InlineData(-30, 0)]        // clamped
public void Health_IsClampedToTheValidRange(int assigned, int expected)
{
    Enemy enemy = new Enemy(100);

    enemy.Health = assigned;

    Assert.Equal(expected, enemy.Health);
}
```

`[Theory]` takes parameters; each `[InlineData]` supplies one row. **Each row is reported as a separate test**, so a failure names the row that broke rather than the method. Five rows here, five results.

This is where boundary values belong: zero, the maximum, one past the maximum, and one below zero. Those four catch most clamping bugs.

### Isolation: a new instance for every test

This is the xUnit detail that surprises people coming from other frameworks.

**xUnit constructs a new instance of your test class for every single test method.** There is no `[SetUp]` attribute, because the constructor *is* the setup:

```csharp
public class PoolTests
{
    private readonly Pool<Bullet> _pool;

    // Runs once per test method, not once per class.
    public PoolTests()
    {
        _pool = new Pool<Bullet>(5);
    }

    [Fact]
    public void Get_ReducesTheAvailableCount()
    {
        _pool.Get();
        Assert.Equal(4, _pool.AvailableCount);
    }

    [Fact]
    public void NewPool_HasTheRequestedCapacity()
    {
        Assert.Equal(5, _pool.AvailableCount);   // 5, not 4 - a fresh pool
    }
}
```

The second test sees a pool of five even though the first took one out, because it got a different pool. That is the framework making it impossible for tests to contaminate each other through the fields of the test class.

If you need to clean up - close a file, dispose something - implement `IDisposable` on the test class and put it in `Dispose`. There is no `[TearDown]` either.

**The one thing this does not protect you from is `static` state.** A static counter, as in [note 01](01-properties-static-tostring.md), is shared by the whole test run, and tests that touch it will interfere with each other in an order that changes between runs.

### Testing that something throws

```csharp
[Fact]
public void Take_FromAnEmptyBox_Throws()
{
    Box<int> box = new Box<int>();

    InvalidOperationException ex =
        Assert.Throws<InvalidOperationException>(() => box.Take());

    Assert.Equal("The box is empty.", ex.Message);
}
```

`Assert.Throws<T>` takes a lambda, runs it, and fails if the expected exception does not arrive. It **returns** the exception, so you can go on to assert about its message or its `ParamName`.

Note `Assert.Throws<T>` requires that exact type. If a subclass would also be acceptable, use `Assert.ThrowsAny<T>`.

### Testing that an event fired

Events have no return value, so the test has to record what happened:

```csharp
[Fact]
public void TakeDamage_RaisesHealthChangedWithTheNewTotal()
{
    Health health = new Health();
    int seen = -1;

    Action<int> handler = value => seen = value;
    health.HealthChanged += handler;

    health.TakeDamage(30);

    health.HealthChanged -= handler;
    Assert.Equal(70, seen);
}
```

The lambda is the test's way of listening. Note the `-=` before the assertion - the discipline from [note 07](07-delegates-events-action-func.md) applies to tests too, and forgetting it is how one test starts hearing another test's events.

Counting calls is often more useful than capturing values, because it catches the duplicate-subscription bug:

```csharp
int calls = 0;
Action<int> handler = value => calls++;
// ...
Assert.Equal(1, calls);         // exactly once, not twice
```

### Which tests are worth writing

You cannot test everything and should not try. A test earns its place when it checks something that could plausibly be wrong.

Aim at:

- **Boundaries.** Zero, the maximum, one past the maximum, negative, empty.
- **The failure path.** What happens when the key is missing, the list is empty, the target is destroyed.
- **Anything you got wrong once.** A bug you have fixed is a bug that can come back.
- **The contract you stated.** Every "done when" line is a candidate.

Skip:

- Getters and setters that do nothing but store.
- Anything whose test would simply restate the implementation.

Three rules for a test that will still be useful next year:

1. **One behaviour per test.** A test asserting five things fails on the first and hides the rest.
2. **No `if`, no loops in a test.** Logic in a test is untested logic. Use a `[Theory]` instead.
3. **A test should fail for exactly one reason.** If you cannot name that reason, the test is testing too much.

## Common mistakes

### A test that asserts nothing

```csharp
[Fact]
public void TakeDamageWorks()
{
    Enemy enemy = new Enemy(100);
    enemy.TakeDamage(30);           // and nothing else
}
```

**Symptom:** The test passes. It will pass forever, including after you delete the body of `TakeDamage`, because the only way it can fail is by throwing.

**Fix:** Assert something. A test with no assertion tests only that the code does not crash, which is worth roughly nothing and is actively harmful because it makes the suite look healthier than it is.

### Exact equality on a float

```csharp
Assert.Equal(0.3f, colour.Brightness);
```

**Symptom:** A failure reading `Expected: 0.3, Actual: 0.3`. The two differ far below the printed precision, so the message appears to show two identical values and the test looks broken rather than the code.

**Fix:** Use the precision overload - `Assert.Equal(0.3f, colour.Brightness, 4)` - on every floating-point assertion. Not only the ones you expect to be awkward.

### Tests that depend on each other

```csharp
public class PoolTests
{
    private static Pool<Bullet> _shared = new Pool<Bullet>(5);   // static

    [Fact]
    public void A_TakeOne() { _shared.Get(); Assert.Equal(4, _shared.AvailableCount); }

    [Fact]
    public void B_StillHasFive() { Assert.Equal(5, _shared.AvailableCount); }
}
```

**Symptom:** Both pass when run individually. One fails when the suite runs. Which one fails may change between runs, and the failure has nothing to do with the code under test.

**Fix:** Remove the `static`. xUnit already gives each test a fresh instance of the class, so an instance field created in the constructor is isolated for free. Static state defeats that deliberately.

### Testing the implementation instead of the behaviour

```csharp
[Fact]
public void Detonate_CallsTakeDamageOnEachTarget()
{
    // exposes a counter on Explosion purely so the test can read it
    Assert.Equal(3, explosion.InternalCallCount);
}
```

**Symptom:** The test passes, and then breaks the moment you refactor `Explosion` without changing anything a user of it could observe. Worse, the class has grown a public member that exists only for the test, so the production design is now worse because of the test.

**Fix:** Assert on what a caller can see - that the three targets lost health. If the behaviour is genuinely unobservable from outside, ask whether it needs to exist.

## Check yourself

1. What are the three parts of a test, and why keep them visually separate?
2. Why does `Assert.Equal(0.3, 0.1 + 0.2)` fail, and what do you write instead?
3. How many times does an xUnit test class constructor run for a class with four `[Fact]` methods?
4. When should a `[Theory]` replace several `[Fact]` methods?
5. You have a test that passes alone and fails as part of the suite. What is the first thing to look for?

<details>
<summary>Answers</summary>

**1.** Arrange, Act, Assert. Separating them means a reader can find the one line being tested without reading the setup, and it makes a test doing two Acts - which should have been two tests - obvious at a glance.

**2.** Because `0.1 + 0.2` in binary floating point is not exactly `0.3`. Use the precision overload, `Assert.Equal(0.3, actual, 10)`, where the third argument is the number of decimal places compared.

**3.** Four times - once per test method. xUnit builds a new instance for each, which is why the constructor replaces `[SetUp]` and why tests cannot contaminate each other through instance fields.

**4.** When the same assertion should hold across several inputs. A theory keeps the logic in one place and still reports each row separately, so a failure names the input that broke rather than just the method.

**5.** Shared mutable state, and `static` first. xUnit isolates instance fields automatically, so interference almost always comes from something static, a file on disk, or a test that subscribed to an event and never unsubscribed.

</details>

## Further reading

- [Getting started with xUnit.net](https://xunit.net/docs/getting-started/v2/netfx/visual-studio)
- [Unit testing C# with dotnet test and xUnit](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Unit testing best practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t13_null_handling

this_lesson:
  topic_code: t14_unit_testing
  difficulty_tier: Intermediate
mlos: [MLO3, MLO4]
```
