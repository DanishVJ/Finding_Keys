using UnityEngine;

public class KeyCabinet : MonoBehaviour, IInteractable
{
    private bool hasBeenOpened = false;

    public string GetInteractionPrompt()
    {
        if (hasBeenOpened) return "The cabinet is empty.";
        return "Press E to search the cabinet.";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (hasBeenOpened) return;

        hasBeenOpened = true;
        playerInventory.AddSimpleKey();
        Debug.Log("[Cabinet] Dispensed 1 Simple Key to the player.");
    }
}