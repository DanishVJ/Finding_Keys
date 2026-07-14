using UnityEngine;

public class LockedDesk : MonoBehaviour, IInteractable
{
    [Header("Reward Settings")]
    [SerializeField] private KeycardLevel cardToGive = KeycardLevel.Gold;

    // --- NEW: Sound Effect Field ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when the simple key is successfully used to unlock the desk drawer")]
    [SerializeField] private AudioClip keyUnlockSound;

    private bool isUnlocked = false;

    public string GetInteractionPrompt()
    {
        if (isUnlocked) return "The desk is empty.";
        return "Press E to unlock desk drawer. (Requires Simple Key)";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isUnlocked) return;

        // Check for the consumable key
        if (playerInventory.HasSimpleKey())
        {
            // --- NEW: Play the unlock sound at the desk's location ---
            if (keyUnlockSound != null)
            {
                AudioSource.PlayClipAtPoint(keyUnlockSound, transform.position);
            }

            // 1. Spend the key (fires a UI alert automatically)
            playerInventory.ConsumeSimpleKey();
            isUnlocked = true;
            
            // 2. Award the gold card! (fires a UI upgrade alert automatically)
            playerInventory.UpgradeKeycard(cardToGive);
            Debug.Log($"[Desk] Drawer unlocked! Awarded {cardToGive} Card.");
        }
        else
        {
            Debug.Log("[Desk] The drawer is locked tight. Needs a Simple Key.");

            // UI Hook: Display access restriction alert on screen
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification("The drawer is locked tight! Needs a Simple Key.");
            }
        }
    }
}