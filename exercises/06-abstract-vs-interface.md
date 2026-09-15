# Exercises: Abstract classes and interfaces

Read [note 06](../notes/06-abstract-vs-interface.md) first. Plain C# console project throughout.

---

## A - Mechanical

### Goal

Build your first abstract base class and see what each modifier buys you: one member every subclass must supply, and one no subclass is allowed to touch.

### Constraints

- `PickupBase` is abstract, holds a `protected` field for the amount collected, and has a `protected` constructor taking that amount.
- `Collect` is public, non-virtual, and prints how much was collected before calling `OnCollected`.
- `OnCollected` is `protected abstract`.
- `HealthPickup` and `AmmoPickup` derive from it and each print something different from `OnCollected`.

### Starter code

```csharp
public class Program
{
    public static void Main()
    {
        List<PickupBase> pickups = new List<PickupBase>
        {
            new HealthPickup(25),
            new AmmoPickup(12)
        };

        foreach (PickupBase pickup in pickups)
        {
            pickup.Collect();
        }
    }
}

// TODO: PickupBase, HealthPickup, AmmoPickup
```

### Done when

- The program prints four lines: the shared line and the specific line for each pickup, in that order.
- `new PickupBase(10)` is a compile error.
- Removing `override` from `OnCollected` in `HealthPickup` is a compile error.

---

## B - Applied

### Goal

Two of the classes below repeat the same bookkeeping, and the third has nothing to do with either of them. Pull the shared part into a base class without shutting the odd one out.

### Starter code

```csharp
public interface ISaveable
{
    string Serialise();
    bool IsDirty { get; }
}

public class PlayerProfile : ISaveable
{
    private bool _isDirty;
    public bool IsDirty { get { return _isDirty; } }

    public void MarkDirty() { _isDirty = true; }

    public string Serialise()
    {
        _isDirty = false;
        return "profile:" + Name;
    }

    public string Name { get; set; }
}

public class LevelProgress : ISaveable
{
    private bool _isDirty;
    public bool IsDirty { get { return _isDirty; } }

    public void MarkDirty() { _isDirty = true; }

    public string Serialise()
    {
        _isDirty = false;
        return "level:" + Index;
    }

    public int Index { get; set; }
}

public class SettingsFile : ISaveable
{
    // Always considered dirty; it is rewritten every time regardless.
    public bool IsDirty { get { return true; } }

    public string Serialise()
    {
        return "settings:" + Volume;
    }

    public float Volume { get; set; }
}
```

The dirty-flag handling in the first two classes is identical, and it is subtly important: `Serialise` must clear the flag. If a third author adds a fourth saveable type and forgets that line, the object is saved on every frame forever.

### Constraints

- Introduce an abstract base class that `PlayerProfile` and `LevelProgress` derive from, holding the dirty flag and its handling.
- The clearing of the flag must happen in a place a subclass cannot forget or override. Subclasses supply only the type-specific part of the serialised string.
- `SettingsFile` must continue to implement `ISaveable` directly and must not derive from the new base class.
- Write a `SaveAll` method that takes a collection and saves only the dirty items. Its parameter type must accept all three classes.

### Done when

- The dirty-flag field and the line that clears it each appear exactly once in the whole project.
- `SettingsFile` compiles unchanged apart from formatting.
- `SaveAll` accepts a collection containing all three types.
- A new saveable type deriving from your base class cannot fail to clear its dirty flag, no matter what its author writes.

---

## C - Design

### Goal

This one is all design. You are working out the type structure for a weapon system, and there are four weapons to account for:

- **Pistol** - fires one shot per trigger press, has an ammunition count, reloads.
- **Rifle** - fires continuously while held, has an ammunition count, reloads.
- **Crowbar** - swings, has no ammunition, cannot reload.
- **Turret** - not carried by the player. It is placed in the level, acquires its own targets, fires continuously, and has infinite ammunition.

Several systems need to talk to these:

- The HUD displays remaining ammunition, for weapons that have any.
- The input handler tells the currently held weapon to start and stop firing.
- The save system records the state of everything that can be reloaded.

### Constraints

- Produce compiling C# with method bodies left as comments or trivial `Console.WriteLine` calls. The design is what is being assessed, not the ballistics.
- No class may contain a member it cannot meaningfully implement.
- Each of the three systems above must be written as a method whose parameter type admits exactly the weapons it should and no others. If the HUD method can be passed a crowbar, the design is wrong.
- At least one abstract class and at least two interfaces must appear, and each must be there because it is the right tool rather than because this sentence asked for it. Be prepared to defend any you could have done without.

### The choice you must justify

The interesting decision is where `Turret` sits. It fires like a rifle but is not held, not reloaded, and not displayed on the HUD. You can place it inside the same family as the carried weapons, or outside it sharing only interfaces.

Both are defensible and they fail differently later. Write a comment at the top of your main file, five to ten lines, naming the route you took, the route you rejected, and the specific thing the rejected route would have made harder. State what would have to change in your design if a fifth weapon - a thrown grenade - were added next week.

### Done when

- Four weapon types exist and compile.
- Three system methods exist, each with a parameter type that admits exactly the right subset, enforced by the compiler.
- No `NotImplementedException` and no member that returns a meaningless value to satisfy a contract.
- The justification comment names a specific rejected alternative and a specific cost.
- You can state, in one sentence, which of your types is a family and which is a capability.
