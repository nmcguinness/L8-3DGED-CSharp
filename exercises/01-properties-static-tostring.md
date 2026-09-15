# Exercises: Properties, static members and ToString

Read [note 01](../notes/01-properties-static-tostring.md) first. Plain C# console project throughout; none of these need Unity.

All three build one type, `ColourRGBA`, holding red, green, blue and alpha as floats in the range 0 to 1.

---

## A - Mechanical

### Goal

Turn four public fields into four properties that cannot hold an invalid value, and give the type a readable string form.

### Constraints

- Store the four components in private backing fields, underscore-prefixed.
- Expose `R`, `G`, `B` and `A` as properties. Each setter clamps to the range 0 to 1.
- Provide two constructors: one taking all four components, and a parameterless one producing opaque white `(1, 1, 1, 1)`.
- Override `ToString()` to return `RGBA(1, 0.5, 0, 1)` - the four values, comma separated, inside `RGBA(...)`.
- No public fields anywhere in the finished type.

### Starter code

```csharp
public class ColourRGBA
{
    // TODO: four private backing fields

    // TODO: two constructors

    // TODO: four properties, each clamping in the setter

    // TODO: override ToString
}

public class Program
{
    public static void Main()
    {
        ColourRGBA orange = new ColourRGBA(1f, 0.5f, 0f, 1f);
        Console.WriteLine(orange);

        ColourRGBA white = new ColourRGBA();
        Console.WriteLine(white);

        orange.R = 5f;                  // out of range
        Console.WriteLine(orange);      // R must read back as 1
    }
}
```

### Done when

- `Console.WriteLine(orange)` prints the `RGBA(...)` form without you calling `ToString()` yourself.
- Setting any component to `5f` reads back as `1`, and to `-3f` reads back as `0`.
- The parameterless constructor gives opaque white.
- Removing `override` from `ToString` is a compile error. Try it, read the message, restore it.

---

## B - Applied

### Goal

Add the members that belong to the type rather than to any one colour, and the members that are computed rather than stored.

### Constraints

Add to `ColourRGBA`:

- **Static named colours:** `Red`, `Green`, `Blue`, `Black` and `White`, each readable as `ColourRGBA.Red`. Decide whether these are properties or fields and be ready to say why.
- **A computed property `Brightness`**, returning the weighted grey value `0.299 * R + 0.587 * G + 0.114 * B`. It must not be stored.
- **An instance method `ToGreyscale()`** returning a *new* `ColourRGBA` whose R, G and B are all `Brightness`, keeping the original alpha. It must not modify the colour it was called on.
- **A static method `Lerp(ColourRGBA a, ColourRGBA b, float t)`** blending two colours, with `t` clamped to 0 to 1.
- **A static property `Count`** reporting how many `ColourRGBA` instances have been constructed.

### Constraints on how

- `Brightness` must be a property, not a method, and you must be able to justify that choice against the guidance in the note.
- `Lerp` is static because it belongs to no single colour. `ToGreyscale` is an instance method because it does. Do not swap them.
- `Count` must be readable from outside and writable only from inside the class.

### Done when

- `ColourRGBA.Red.ToString()` prints `RGBA(1, 0, 0, 1)`.
- `ColourRGBA.Lerp(black, white, 0.5f)` gives a mid grey, and `t = 0` and `t = 1` give the endpoints exactly.
- Calling `ToGreyscale()` on a colour leaves that colour unchanged, proven by printing it before and after.
- `ColourRGBA.Count` rises as you construct colours, and `someColour.Count` is a compile error.
- Assigning to `Count` from `Main` is a compile error.
- No field stores brightness.

---

## C - Design

### Goal

You now have a working colour type. Decide what it should actually be, and rebuild it to match that decision.

A colour is used in two very different ways in an engine. It is **data**, set once in an editor and read thereafter; and it is a **working value**, produced by the thousand every frame as things fade, flash and blend. Those two uses want opposite designs.

### Constraints

- Produce a second version of the type, `ColourRGBA2`, alongside the first.
- It must support at least: construction from four components, the named colours, blending, conversion to greyscale, and a readable string form.
- Every design decision below must be made deliberately and defended, not left at whatever compiled first.
- Write a short test that exercises both versions through the same sequence of operations and prints the results side by side.

### The choice you must justify

At least four decisions, and they interact:

- **Mutable or immutable?** Get-only properties fixed at construction, so `ToGreyscale` must return a new colour - or settable properties, so a colour can be adjusted in place. Immutability removes a class of bug and creates an object every time anything changes.
- **Class or struct?** [Note 02](../notes/02-value-semantics.md) is next and answers half of this. Say what you would choose and why, even before reading it.
- **Where does clamping live?** In every setter, in the constructor only, or nowhere - with the caller responsible. If the type is immutable, one of these options disappears. Which, and why?
- **Named colours: property or `static readonly` field?** A property returns a fresh instance on every call. A field returns the same one every time. One of those is a serious problem if the type is mutable. Work out which.

Write a comment at the top of `ColourRGBA2.cs`, ten to fifteen lines, covering:

- which of the two uses - editor data, or per-frame working value - you designed for, and what you gave up for the other;
- your mutability decision, and the specific bug it prevents or admits;
- the named-colours trap above, stated as a concrete sequence of three lines of code that goes wrong under the design you rejected;
- what would have to change if this type were later saved to disk and loaded back.

A defensible answer may keep both types and say when each is used, provided you say who chooses and how they know.

### Done when

- Both versions exist, compile, and pass the same test sequence.
- Every property in `ColourRGBA2` is get-only, or every one is settable - not a mixture you cannot account for.
- You can demonstrate the named-colours trap in code against the rejected design, in three lines or fewer.
- The justification comment covers all four bullets by name.
- No component in either version can hold a value outside 0 to 1 by the time it is read back.
