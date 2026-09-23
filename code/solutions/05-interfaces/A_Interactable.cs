// Exercise 05 A - worked solution.
//
// DESIGN CHOICE: Interact returns a string rather than printing, so the behaviour
// can be asserted. The loop holds IInteractable and names no concrete type.
//
// THE ALTERNATIVE - Console.WriteLine inside each Interact - matches the exercise
// wording but leaves nothing a test can check without capturing console output,
// which is more machinery than the exercise is worth.
//
// Each class carries a name so that ToString identifies which door or lever a log
// line is about. The parameterless constructor chains to the named one, so the
// default is written once.

namespace Solutions.T05.A
{
    /// <summary>Something the player can interact with.</summary>
    public interface IInteractable
    {
        /// <summary>Performs this object's interaction.</summary>
        /// <returns>A description of what happened.</returns>
        string Interact();
    }

    /// <summary>A door that swings open.</summary>
    public class Door : IInteractable
    {
        private readonly string _name;

        /// <summary>Creates a door named "door".</summary>
        public Door() : this("door")
        {
        }

        /// <summary>Creates a named door.</summary>
        /// <param name="name">The name used when this door is printed.</param>
        public Door(string name)
        {
            _name = name;
        }

        /// <inheritdoc />
        public string Interact()
        {
            return "The door swings open.";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Door(" + _name + ")";
        }
    }

    /// <summary>A lever that clicks.</summary>
    public class Lever : IInteractable
    {
        private readonly string _name;

        /// <summary>Creates a lever named "lever".</summary>
        public Lever() : this("lever")
        {
        }

        /// <summary>Creates a named lever.</summary>
        /// <param name="name">The name used when this lever is printed.</param>
        public Lever(string name)
        {
            _name = name;
        }

        /// <inheritdoc />
        public string Interact()
        {
            return "The lever clicks.";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "Lever(" + _name + ")";
        }
    }
}
