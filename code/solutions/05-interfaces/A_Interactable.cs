// Exercise 05 A - worked solution.
//
// DESIGN CHOICE: Interact returns a string rather than printing, so the behaviour
// can be asserted. The loop holds IInteractable and names no concrete type.
//
// THE ALTERNATIVE - Console.WriteLine inside each Interact - matches the exercise
// wording but leaves nothing a test can check without capturing console output,
// which is more machinery than the exercise is worth.

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
        /// <inheritdoc />
        public string Interact()
        {
            return "The door swings open.";
        }
    }

    /// <summary>A lever that clicks.</summary>
    public class Lever : IInteractable
    {
        /// <inheritdoc />
        public string Interact()
        {
            return "The lever clicks.";
        }
    }
}
