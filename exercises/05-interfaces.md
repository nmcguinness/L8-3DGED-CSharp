# Exercises: Interfaces

Read [note 05](../notes/05-interfaces.md) first. Work in a plain C# console project; none of these require Unity.

---

## A - Mechanical

### Goal

Get the basic shape under your fingers: one interface, two classes with nothing else in common, and a loop that treats them identically without knowing what either of them is.

### Constraints

- The interface declares a single method, `void Interact()`.
- Neither implementing class may derive from the other or from a shared base.
- The loop in `Main` must not mention either concrete type name.

### Starter code

```csharp
public class Program
{
    public static void Main()
    {
        List<IInteractable> interactables = new List<IInteractable>();

        // TODO: add one Door and one Lever to the list

        foreach (IInteractable interactable in interactables)
        {
            // TODO: interact with it
        }
    }
}

// TODO: declare IInteractable

// TODO: declare Door, which prints "The door swings open."

// TODO: declare Lever, which prints "The lever clicks."
```

### Done when

- The program prints both messages.
- The word `Door` appears exactly once in your file, and the word `Lever` exactly once, both on the lines that add them to the list.
- Deleting `Lever` entirely requires changing only the line that adds it.

---

## B - Applied

### Goal

Below is an interface that tried to cover everything, and three classes paying for it. Break it up so that nothing is forced to implement something it cannot actually do.

### Starter code

This compiles and is wrong. Your job is to fix the design, not to make it run.

```csharp
public interface IGameEntity
{
    void TakeDamage(int amount);
    void Repair(int amount);
    void Move(float x, float y, float z);
    void Save(string path);
}

public class Crate : IGameEntity
{
    public void TakeDamage(int amount) { /* reduce integrity */ }
    public void Repair(int amount) { /* restore integrity */ }
    public void Move(float x, float y, float z) { throw new NotImplementedException(); }
    public void Save(string path) { throw new NotImplementedException(); }
}

public class Guard : IGameEntity
{
    public void TakeDamage(int amount) { /* reduce health */ }
    public void Repair(int amount) { throw new NotImplementedException(); }
    public void Move(float x, float y, float z) { /* walk */ }
    public void Save(string path) { /* write state */ }
}

public class Checkpoint : IGameEntity
{
    public void TakeDamage(int amount) { throw new NotImplementedException(); }
    public void Repair(int amount) { throw new NotImplementedException(); }
    public void Move(float x, float y, float z) { throw new NotImplementedException(); }
    public void Save(string path) { /* write state */ }
}
```

### Constraints

- After your changes, no class contains `NotImplementedException`.
- Every class keeps the behaviour it genuinely had. Do not delete `Guard.Move`.
- Write one method for each of the following, each taking the narrowest parameter type that will do the job:
  - `ApplySplashDamage`, which damages a set of things.
  - `SaveAll`, which saves a set of things.
- `Crate` must be passable to `ApplySplashDamage` and must not be passable to `SaveAll`. The compiler should enforce this, not a comment.

### Done when

- The project compiles with no `NotImplementedException` anywhere.
- Passing a `Crate` to `SaveAll` is a compile error.
- Passing a `Checkpoint` to `ApplySplashDamage` is a compile error.
- Adding a new type that can only be saved requires implementing exactly one interface.

---

## C - Design

### Goal

A turret has to pick which enemy to shoot at. Different turrets around the level should pick differently, and a designer should be able to change how any one of them chooses without anybody reopening the turret class.

Your job is to build the mechanism that makes that possible.

### Constraints

- `Turret` must have no knowledge of any specific way of choosing. Its code must not contain the words `nearest`, `weakest`, or any other selection rule, and it must contain no `switch` or `if` chain over a selection kind.
- Provide at least three different selection rules. Suggestions: the closest enemy, the one with the least health, the one that most recently attacked the turret. Invent your own if you prefer.
- It must be possible to change a turret's rule at run time, not only at construction.
- The selection rules must be testable without constructing a `Turret`.

### Starter code

Use this `Enemy` so that everyone is working from the same shape. Distance is a plain `float` on the enemy rather than a real position, so that this stays a console exercise.

```csharp
public class Enemy
{
    public string Name { get; set; }
    public int Health { get; set; }
    public float DistanceFromTurret { get; set; }
    public float SecondsSinceItAttackedUs { get; set; }
}
```

### Alternative brief: obstacle modifiers

If you would rather work with a different domain, this version is equivalent and marked the same way. A player moves through a level and meets things that change their condition:

- a lava pit reduces health;
- laughing gas raises resistance;
- a sad story lowers mood.

Build `IModifyObject` with a single method that applies an effect to a `Player`, implement at least three modifiers, and have the level hold a `List<IModifyObject>` it applies in order. `Player` exposes `Health`, `Resistance` and `Mood` as properties.

The constraints above carry over unchanged: the level must not name any specific modifier, a fourth must cost no edit to the level, and each modifier must be testable on its own. The justification below applies in the same way - the question becomes whether a modifier receives the whole `Player` and mutates it, or returns a description of the change for the level to apply.

### The choice you must justify

There is more than one defensible design here, and they differ in what the selection rule is handed and what it gives back. At minimum, decide:

- Does the rule receive the whole list of candidates and return one, or does it score a single candidate and let the turret compare scores?
- What should happen when the candidate list is empty?

Write a comment at the top of your main file, five to ten lines, stating which design you chose, what the alternative was, and what the alternative would have cost. A comment that only describes what your code does scores nothing; the comparison is the point.

### Done when

- Three or more rules exist and produce different answers for the same enemy list.
- A turret's rule can be swapped at run time and the next selection reflects the change.
- Adding a fourth rule requires no edit to `Turret`.
- Each rule can be exercised directly in a test without a `Turret` present.
- The justification comment is present and names a specific alternative.
