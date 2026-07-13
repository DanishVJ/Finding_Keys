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
        if (hasBeenOpened)
        {
            // UI Hook: Remind the player that they've already looted this cabinet
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification("The cabinet is empty.");
            }
            return;
        }

        hasBeenOpened = true;
        
        // This automatically handles the UI icon update and "Collected a Simple Key!" notification
        playerInventory.AddSimpleKey(); 
        Debug.Log("[Cabinet] Dispensed 1 Simple Key to the player.");
    }
}