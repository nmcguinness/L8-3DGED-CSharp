---
title: "Generics"
subtitle: "COMP I8014 — Stage 3 3DGED"
topic_code: t09_generics
description: "Taking a type as a parameter, generic classes and methods with inference, and the constraints new(), class, Component and interface constraints that make T usable."
created: 2026-09-15
last_updated: 2026-09-15
version: 1.0
status: published
authors: ["3DGED Teaching Team"]
tags: [csharp, generics, type-parameters, constraints, object-pool, stage3, comp-i8014]
difficulty_tier: Intermediate
mlos: [MLO3]
previous_topic: t08_lambdas_closures
prerequisites:
  - Interfaces, for interface constraints
  - Collections such as List<T>
---

# Generics

## What this unblocks

Later in the module you will write an object pool. A pool that only handles bullets is useless, because you also want to pool impact effects, shell casings and damage numbers, and writing four nearly identical pool classes is not an acceptable answer. `IPool<T>` lets you write it once.

The typed event channels have the same requirement from the other direction: a channel carrying an `int` and a channel carrying a `Vector3` are the same class with one thing swapped out.

## The idea

A generic type takes a type as a parameter, the same way a method takes a value as a parameter.

You have already used this without writing it. `List<int>` and `List<Enemy>` are the same `List<T>` class with `T` filled in differently. There is one implementation of `List<T>` in the framework, not one per element type, and it is still fully type-checked: `List<int>` will not accept an `Enemy`.

The alternative, before generics existed, was to store everything as `object` and cast on the way out. That compiles, loses all type checking, and moves every mistake from compile time to run time. Generics exist to give you the reuse without giving up the checking.

## In code

### The problem, stated concretely

Here is a pool for bullets:

```csharp
public class BulletPool
{
    private readonly List<Bullet> _available = new List<Bullet>();

    public Bullet Get()
    {
        if (_available.Count == 0)
        {
            return new Bullet();
        }

        Bullet item = _available[_available.Count - 1];
        _available.RemoveAt(_available.Count - 1);
        return item;
    }

    public void Return(Bullet item)
    {
        _available.Add(item);
    }
}
```

Now write `ImpactPool`. It is the same file with `Bullet` replaced by `Impact` nine times. The third one will have a bug the other two do not, because somebody will fix `Get` in one place and forget the others.

### A generic class

Put the varying type in angle brackets after the class name and use it as an ordinary type throughout.

```csharp
/// <summary>
/// Reuses instances of <typeparamref name="T"/> instead of allocating new ones.
/// </summary>
/// <typeparam name="T">The type of item held by this pool.</typeparam>
public class Pool<T> where T : new()
{
    private readonly List<T> _available = new List<T>();

    /// <summary>
    /// Takes an item from the pool, creating one if none are available.
    /// </summary>
    /// <returns>An item ready for use.</returns>
    public T Get()
    {
        if (_available.Count == 0)
        {
            return new T();
        }

        T item = _available[_available.Count - 1];
        _available.RemoveAt(_available.Count - 1);
        return item;
    }

    /// <summary>
    /// Returns an item to the pool for later reuse.
    /// </summary>
    /// <param name="item">The item to return.</param>
    public void Return(T item)
    {
        _available.Add(item);
    }
}
```

Used as:

```csharp
Pool<Bullet> bullets = new Pool<Bullet>();
Pool<Impact> impacts = new Pool<Impact>();

Bullet b = bullets.Get();       // typed as Bullet, no cast
impacts.Return(b);              // will not compile, and should not
```

One implementation, full type checking at every call site. The `<typeparam>` and `<typeparamref>` tags are the XML documentation equivalents for type parameters and are expected on public generic types in this module.

### A generic method

A method can be generic without its class being generic. The type parameter goes after the method name.

```csharp
/// <summary>
/// Returns the first item in the list, or the type default if it is empty.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
/// <param name="items">The list to read from.</param>
/// <returns>The first element, or the default value of <typeparamref name="T"/>.</returns>
public static T FirstOrDefault<T>(List<T> items)
{
    if (items.Count == 0)
    {
        return default(T);
    }

    return items[0];
}
```

`default(T)` is the zero value for whatever `T` turns out to be: `0` for `int`, `false` for `bool`, `null` for any class. It is the only value you can produce for an unconstrained `T` without knowing anything about it.

Calling a generic method rarely requires you to name the type, because the compiler infers it from the arguments:

```csharp
List<Enemy> enemies = new List<Enemy>();

Enemy first = FirstOrDefault(enemies);          // T inferred as Enemy
Enemy same = FirstOrDefault<Enemy>(enemies);    // the same call, spelled out
```

Inference works from arguments only. A method whose type parameter appears solely in the return type must always be given the type explicitly.

### Constraints

By default the compiler knows nothing about `T`, so it will let you do almost nothing with it. This does not compile:

```csharp
public class Pool<T>
{
    public T Get()
    {
        return new T();     // error: T might not have a parameterless constructor
    }
}
```

A constraint is a promise about `T` that the compiler then enforces at every use of the class. The three that matter in this module:

```csharp
public class Pool<T> where T : new() { }            // T has a public parameterless constructor

public class Channel<T> where T : class { }         // T is a reference type

public class Spawner<T> where T : Component { }     // T is or derives from Component
```

`where T : new()` permits `new T()`.

`where T : class` permits comparison with `null` and assignment of `null`. Without it, `T` might be an `int`, and `int` is never null.

`where T : Component` permits everything a `Component` can do, and is how you write a spawner that can only be closed over Unity component types.

You can also constrain to an interface, which is the most useful of the lot because it is the only way to call your own methods on `T`:

```csharp
public interface IPoolable
{
    void Reset();
}

public class Pool<T> where T : IPoolable, new()
{
    public void Return(T item)
    {
        item.Reset();       // legal: the constraint promises Reset exists
        _available.Add(item);
    }
}
```

Constraints combine with commas, and `new()` must always come last. Notice what has happened: `Pool<T>` now works with any type at all, provided that type agrees to the `IPoolable` contract from [note 05](05-interfaces.md). That combination - generics for the container, an interface for the capability - is the shape of the object pool exercise.

## Common mistakes

### Calling `new T()` without the `new()` constraint

```csharp
public class Pool<T>
{
    public T Get()
    {
        return new T();                     // wrong
    }
}
```

**Symptom:** The compiler refuses to create an instance of the variable type `T`, because it does not have the `new()` constraint.

**Fix:** Add `where T : new()`. Be aware of what you are promising: any type someone closes this over must have a public parameterless constructor. A `MonoBehaviour` does not qualify in practice, since Unity components are created by the engine rather than with `new`, which is why the object pool will take a prefab rather than construct its own.

### Using a member of `T` that no constraint guarantees

```csharp
public class Pool<T>
{
    public void Return(T item)
    {
        item.Reset();                       // wrong
    }
}
```

**Symptom:** The compiler reports that `T` does not contain a definition for `Reset`, even when every type you intend to use plainly has one. It is checking against what `T` is *promised* to be, not what you happen to pass.

**Fix:** Add the constraint that makes the promise: `where T : IPoolable`. The error is the type system pointing out that the contract was never written down.

### Comparing an unconstrained `T` to null

```csharp
public void Return(T item)
{
    if (item == null)                       // wrong
    {
        return;
    }
}
```

**Symptom:** The compiler reports that `==` cannot be applied to operands of type `T` and null. `T` might be an `int`, which cannot be null.

**Fix:** Add `where T : class` if the type really should always be a reference type. If it should not, compare with `default(T)` instead, though be aware that the result for value types is a comparison against zero rather than a null check.

### Expecting `List<Enemy>` to be usable as `List<IDamageable>`

```csharp
List<Enemy> enemies = new List<Enemy>();
List<IDamageable> targets = enemies;        // wrong
```

**Symptom:** The compiler reports that it cannot implicitly convert `List<Enemy>` to `List<IDamageable>`. This feels wrong, because every `Enemy` is an `IDamageable`.

**Fix:** The conversion is rejected because it would be unsafe. If it were allowed, you could then call `targets.Add(new Barrel())` and there would be a barrel in a list of enemies. Declare the list as `List<IDamageable>` from the start if that is what callers need, or build a second list and copy the items across.

## Check yourself

1. What is the advantage of `Pool<T>` over storing items as `object` and casting?
2. Why will `return new T();` not compile without a constraint?
3. Write the constraint clause for a class whose `T` must implement `IState` and be creatable with `new`.
4. When do you have to write `Method<Enemy>(x)` rather than just `Method(x)`?
5. Why is `List<Enemy>` not assignable to a `List<IDamageable>` variable?

<details>
<summary>Answers</summary>

**1.** Type checking. `Pool<Bullet>.Get()` returns a `Bullet` that needs no cast and cannot be an `Impact`. The `object` version compiles happily when you return the wrong thing and fails at run time instead.

**2.** Because `T` is unconstrained, so the compiler cannot know the type has a public parameterless constructor. `where T : new()` supplies that guarantee and the compiler then enforces it wherever the class is used.

**3.** `where T : IState, new()` - interface constraints first, `new()` always last.

**4.** When the type parameter cannot be inferred from the arguments, which usually means it appears only in the return type. Inference looks at arguments and nothing else.

**5.** Because it would not be safe. A `List<IDamageable>` accepts any damageable item, so the assignment would allow a `Barrel` to be added to a list that other code believes contains only enemies.

</details>

## Further reading

- [Generic classes and methods](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [Constraints on type parameters](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Generic methods](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-methods)

## Lesson Context

```yaml
previous_lesson:
  topic_code: t08_lambdas_closures

this_lesson:
  topic_code: t09_generics
  difficulty_tier: Intermediate
mlos: [MLO3]
```
