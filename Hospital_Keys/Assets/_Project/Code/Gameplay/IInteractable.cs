public interface IInteractable
{
    string GetInteractionPrompt(); // What shows on screen (e.g., "Search Desk")
    void Interact(PlayerInventory playerInventory); // What happens when pressing E
}