# Worked solutions and tests

Reference implementations for every exercise, and one test project covering them.

## Running the tests

```bash
cd code/tests
dotnet test
```

That builds the solutions and runs the whole suite. There is **one test project for everything**, not one per exercise.

## How this is laid out

```text
code/
  solutions/
    01-properties-static-tostring/
      A_ColourRGBA.cs        one file per exercise: A, B and C
      B_ColourRGBA.cs
      C_ColourRGBA2.cs
    02-value-semantics/
      ...
  tests/
    Week01.Tests.csproj      the only project in this folder
    T01_ColourRGBATests.cs   one test file per topic
    ...
```

The solutions are plain `.cs` files with no project of their own, so you can read any one of them without opening anything. The test project compiles them in directly:

```xml
<Compile Include="../solutions/**/*.cs" />
```

Each topic sits in its own namespace - `Solutions.T05.C` for note 5, exercise C - so the same class name can appear in several exercises without colliding. That is why the test files start with aliases like `using C = Solutions.T05.C;`.

Where an exercise builds on the one before it, as notes 01 A and B do, each file carries the complete type rather than a diff. You can read any single file and see a whole working answer.

## How to use these

**Attempt the exercise first.** Then compare approaches rather than checking for a match. Reading a solution feels like understanding; only writing the code builds it.

Every file opens with a comment naming the **design choice** made and what the **alternative** would have cost. That comment is the part worth reading closely - the code is one defensible answer, and the reasoning is what tells you when a different answer would have been better.

For exercise C the solutions are explicit that they are one route among several. Exercise 09 C is the clearest case: the pool's behaviour when exhausted is genuinely open, and the solution chose recycling while naming where throwing would have been correct instead.

## What is tested, and what is not

Tests cover the **B and C** solutions. The A solutions are mechanical and are left for you to check by running them.

The test suite is also the answer to [exercise 14](../exercises/14-unit-testing.md), so read it as worked examples of the techniques in [note 14](../notes/14-unit-testing.md):

| Technique | Where to look |
|:--|:--|
| `[Theory]` with boundary values | `T01_ColourRGBATests` - clamping |
| Floating-point precision overload | Every assertion in `T01` and `T02` |
| `Assert.NotSame` to prove a copy was returned | `T01` - `ToGreyscale` |
| The equality contract, including the dictionary case | `T03_OperatorOverloadingTests` |
| Testing `out` parameters and the `Try` pattern | `T04_RefAndOutTests` |
| Testing events by counting rather than flagging | `T07_DelegatesAndEventsTests` |
| One shared suite run against two implementations | `T09_GenericsTests` - the abstract contract class |
| `Assert.Throws` and asserting on the message | `T13_NullHandlingTests` |

`T09_IPoolContractTests` is worth particular attention. It is an abstract class holding only what `IPool<T>` guarantees, with two subclasses supplying the implementation to test. Behaviour that differs between the two pools - what happens when one is exhausted - lives in a separate suite, because it is not part of the contract.

## A note on the Unity types

None of this needs Unity. Where a solution needs a Unity type it declares a minimal stand-in in the same file:

- `Solutions.T02.B` - `Vector3` as a struct and `Transform` with `Position` as a property, which is the pairing that makes `transform.position.x = 5f` fail.
- `Solutions.T13.B` - `EngineObject`, reproducing Unity's overloaded `==` in about thirty lines.
