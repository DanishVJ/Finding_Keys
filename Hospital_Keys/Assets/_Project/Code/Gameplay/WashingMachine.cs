using UnityEngine;

public class WashingMachine : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [Tooltip("Check this ONLY on the specific machine you want to hide the key in.")]
    [SerializeField] private bool hasKey = false; 

    // --- NEW: Audio Field ---
    [Header("Audio Clips")]
    [Tooltip("Metallic/hollow clatter sound when opening or searching the washing machine")]
    [SerializeField] private AudioClip searchSound;

    private bool hasBeenOpened = false;

    public string GetInteractionPrompt()
    {
        if (hasBeenOpened) 
        {
            return "The washing machine is empty.";
        }
        return "Press E to search the washing machine.";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (hasBeenOpened) return;

        // --- NEW: Play washing machine search sound instantly ---
        if (searchSound != null)
        {
            AudioSource.PlayClipAtPoint(searchSound, transform.position);
        }

        hasBeenOpened = true;

        if (hasKey)
        {
            playerInventory.AddSimpleKey();
        }
        else
        {
            Debug.Log("[WashingMachine] Looked inside... Empty.");
            
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification("Washing machine is empty...");
            }
        }
    }
}