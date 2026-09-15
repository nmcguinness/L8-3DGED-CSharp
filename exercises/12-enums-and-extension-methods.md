# Exercises: Enums, Flags and extension methods

Read [note 12](../notes/12-enums-and-extension-methods.md) first. Plain C# console project throughout.

---

## A - Mechanical

### Goal

Declare an enum that behaves, a `[Flags]` enum that combines, and an extension method on a type you do not own.

### Part one: a plain enum

```csharp
// TODO: declare GameState { MainMenu, Playing, Paused, GameOver }
// TODO: write Describe(GameState s) printing a different line for each, using switch
```

Every member must be handled. Decide whether you want a `default` case and say why in a one-line comment.

### Part two: flags

```csharp
// TODO: declare a [Flags] enum DamageType with
//       None, Physical, Fire, Ice, Poison - each its own bit
```

Then, in `Main`:

```csharp
DamageType incoming = DamageType.Fire | DamageType.Poison;

// TODO: print whether incoming contains Fire
// TODO: print whether incoming contains Ice
// TODO: remove Poison and print the result
// TODO: toggle Physical twice and print after each
// TODO: print the numeric value of incoming
```

### Part three: an extension method

```csharp
public static class StringExtensions
{
    // TODO: Truncate(this string value, int maxLength) returns the string unchanged
    //       if short enough, otherwise the first maxLength characters plus "..."
}
```

Call it as `"a long piece of text".Truncate(6)`.

### Done when

- `Describe` handles all four states.
- Printing `incoming` shows member names, not a bare number.
- Removing `Poison` leaves `Fire` only; toggling `Physical` twice returns to where you started.
- `Truncate` works for strings both shorter and longer than the limit.
- Deleting `static` from the extension class is a compile error. Try it, read the message, restore it.

---

## B - Applied

### Goal

Build a layer filter, and make an enum safe to load from outside your program.

### Starter code

```csharp
[Flags]
public enum Layers
{
    None = 0,
    Player = 1 << 0,
    Enemy = 1 << 1,
    Scenery = 1 << 2,
    Trigger = 1 << 3,
    Projectile = 1 << 4
}

public class SceneObject
{
    public string Name { get; set; }
    public Layers Layer { get; set; }
}
```

### Part one: filtering

Write:

- `List<SceneObject> Filter(List<SceneObject> objects, Layers mask)` returning every object whose layer is in the mask. An object sits on exactly one layer; a mask may name several.
- `Layers Everything()` returning every real layer.
- `Layers Except(Layers all, Layers unwanted)`.

Then write `Filter` again as an **extension method** on `List<SceneObject>`, so it reads `objects.OnLayers(mask)`. Add a one-line comment saying whether that was worth doing here, and why.

### Part two: loading an enum safely

A save file stores the layer as a number and the difficulty as text. Write:

- `bool TryParseLayer(int code, out Layers layer)` - false when the number matches no member.
- `bool TryParseDifficulty(string text, out Difficulty difficulty)` - case-insensitive, false when unrecognised.

Declare `Difficulty { Easy, Normal, Hard }` yourself.

### Constraints

- Neither `Try` method throws for bad input. A corrupt save file is a normal outcome here.
- Both use an `out` parameter and assign it on every path - [note 04](../notes/04-ref-and-out.md).
- Test `TryParseLayer` with a number that is a valid *combination* but not a single member. Decide whether that should succeed, and say so in a comment.

### Done when

- `Filter` with `Player | Enemy` returns players and enemies and nothing else.
- `Filter` with `Layers.None` returns an empty list.
- `Except(Everything(), Layers.Scenery)` used as a mask returns everything except scenery.
- `TryParseDifficulty("HARD", out d)` succeeds; `TryParseDifficulty("Brutal", out d)` returns false and throws nothing.
- `TryParseLayer` behaves on a combination value the way your comment says it does.
- The extension version of `Filter` gives identical results to the plain one.

---

## C - Design

### Goal

Design a small query API for finding scene objects, of the kind an engine would hand you. The unusual part is the order of work: decide what the calling code should read like **first**, then make it work.

The calls it must support, in whatever form you design:

- everything on a given set of layers;
- everything on a given set of layers within a radius of a point;
- the nearest thing on a given set of layers;
- everything on a given set of layers, excluding a specific object - almost always the one asking.

### Constraints

- Build on `SceneObject` and `Layers` from exercise B, plus a simple `Vector2` or `Vector3` of your own.
- Write down the four calls you want to be able to make **before** you write the API, and paste them into your comment block.
- At least one extension method must appear, and you must be able to say why it is an extension method rather than an ordinary one.
- Provide one method that is safe to call every frame, and document what it gives up to be so.

### The choice you must justify

The shape of the API is the decision, and the candidates feel very different:

- one method with several optional parameters;
- several narrowly named methods;
- a chained builder, where `Query.On(Layers.Enemy).Within(10f).Nearest()` accumulates criteria and executes at the end.

The chained form reads best and allocates most, which puts it firmly among the things that must not go in `Update` - see [note 10](../notes/10-collections-and-linq.md). The many-methods form does not combine: a fifth criterion means several more overloads.

There is a second decision: whether an empty result is an empty list, a `null`, or something that says why it was empty.

Write a comment at the top of your query file, ten to fifteen lines, covering:

- the four call sites you wanted, written out;
- which shape you chose and which you rejected;
- what the rejected shape would cost, specifically, when a fifth criterion arrives;
- your empty-result decision and what it forces every caller to do;
- which part of your API is not safe to call every frame, and what the frame-safe alternative gives up.

### Done when

- All four queries work and are demonstrated.
- The call sites in your finished code match the ones you wrote down first, or you have noted what changed and why.
- At least one extension method exists with a stated reason.
- One method is documented as frame-safe and allocates nothing when called.
- A test covers the empty-result case and asserts the behaviour your comment claims.
- The justification comment covers all five bullets by name.
