# Exercises: Generics

Read [note 09](../notes/09-generics.md) first. Plain C# throughout. Exercise C produces a design you will recognise again in the object pool topic.

---

## A - Mechanical

### Goal

Take a class that only works with one type and open it up to any type, then write a generic method and let the compiler work the type argument out for you.

### Starter code

```csharp
public class StringBox
{
    private string _item;
    private bool _hasItem;

    public void Put(string item)
    {
        _item = item;
        _hasItem = true;
    }

    public string Take()
    {
        if (!_hasItem)
        {
            throw new InvalidOperationException("The box is empty.");
        }

        _hasItem = false;
        return _item;
    }

    public bool HasItem { get { return _hasItem; } }
}
```

### Constraints

- Convert `StringBox` into `Box<T>` with no change in behaviour.
- The field that held the item must be reset to the default for `T` when taken, so the box does not hold onto it.
- Add a static generic method `Swap<T>(Box<T> first, Box<T> second)` that exchanges the contents of two boxes.
- Call `Swap` without writing the type argument.
- Add XML documentation with `<typeparam>` on the class and `<param>` on the public methods.

### Done when

- `Box<int>` and `Box<string>` both work.
- `box.Take()` on a `Box<int>` returns an `int` with no cast.
- Putting a `string` into a `Box<int>` is a compile error.
- `Swap(a, b)` compiles without `<int>` written anywhere.
- Swapping a `Box<int>` with a `Box<string>` is a compile error.

---

## B - Applied

### Goal

Write a pool that works for anything. The interesting part is the constraints: add each one only at the moment the compiler actually stops you, so you finish knowing exactly what each is there for.

### Constraints

Build `Pool<T>` with:

- `T Get()` - returns a pooled item if one is available, otherwise creates a new one.
- `void Return(T item)` - puts an item back, after resetting it.
- `int AvailableCount` - how many are currently waiting.
- A constructor taking an initial capacity, which pre-creates that many items.

Requirements on the implementation:

- Items must be reset when returned, via a method your own interface declares.
- Returning the same item twice must not put it in the pool twice. Decide whether that is an exception or a silent no-op, and write a one-line comment saying which you chose and why.
- Do not reach for `where T : new()` until the compiler stops you without it, and likewise for the interface constraint. Add each one in response to a specific error, and note the error code in a comment beside the constraint.

### Starter code

```csharp
public class Bullet
{
    public float Speed { get; set; }
    public int Damage { get; set; }
    public bool IsActive { get; set; }
}
```

Write the interface yourself. `Bullet` should end up implementing it.

### Done when

- `Pool<Bullet>` compiles and works.
- `new Pool<Bullet>(10)` starts with an `AvailableCount` of 10.
- Getting an item reduces the count; returning it increases it again.
- An item taken from the pool after being returned has been reset.
- `Pool<int>` is a compile error, and you can say which constraint causes it.
- Each constraint carries a comment naming the compiler error that motivated it.

---

## C - Design

### Goal

This time you are designing the contract rather than the thing behind it. Write `IPool<T>`, then two implementations that genuinely differ, and one consumer that works with either and cannot tell which it was handed.

### Constraints

- Declare `IPool<T>`. Decide its members and its constraints yourself.
- Write two implementations that differ in a way a caller can observe:
  - one that grows without limit;
  - one with a fixed maximum size, which must do something defined when the pool is exhausted.
- Write a `Weapon` class that takes an `IPool<Bullet>` and fires. `Weapon` must compile against the interface alone and must behave correctly with both implementations.
- Write a short test that runs the same sequence against both implementations and shows where their observable behaviour diverges.

### The choice you must justify

The exhausted-pool case is the whole exercise. When a fixed-size pool is asked for an item and has none, the options include: throw; return the type default; return the oldest item currently in use; block the caller; expand anyway and log.

These are not equally good, and which is best depends on what the pool is for. The right answer for bullets is not the right answer for audio sources.

There is a second decision worth making deliberately: whether `Get` returns `T` or returns a `bool` with the item as an `out` parameter. The second form makes failure impossible to ignore and is uglier at every call site.

Write a comment at the top of `IPool.cs`, eight to twelve lines, covering:

- the exhaustion behaviour you chose, for whom it is right, and a concrete case where it would be the wrong choice;
- the signature of `Get` and what the alternative signature would have bought you;
- what constraints you put on `T`, and what you gave up by requiring each one - name a type that can no longer be pooled as a result.

### Done when

- `IPool<T>` exists with constraints you can each justify.
- Two implementations exist and pass the same consumer code.
- `Weapon` names neither implementation.
- A test demonstrates the two implementations behaving differently under exhaustion, and asserts the behaviour your comment claims.
- Swapping which implementation `Weapon` is given requires changing one line.
- The justification comment names a type excluded by your constraints.
