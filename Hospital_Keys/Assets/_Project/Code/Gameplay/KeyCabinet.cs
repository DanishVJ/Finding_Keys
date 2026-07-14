using UnityEngine;

public class KeyCabinet : MonoBehaviour, IInteractable
{
    // --- NEW: Audio Field ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when searching through this cabinet")]
    [SerializeField] private AudioClip searchSound;

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
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification("The cabinet is empty.");
            }
            return;
        }

        // --- NEW: Play searching sound instantly ---
        if (searchSound != null)
        {
            AudioSource.PlayClipAtPoint(searchSound, transform.position);
        }

        hasBeenOpened = true;
        
        playerInventory.AddSimpleKey(); 
        Debug.Log("[Cabinet] Dispensed 1 Simple Key to the player.");
    }
}