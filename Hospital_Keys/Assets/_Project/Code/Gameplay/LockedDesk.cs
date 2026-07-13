using UnityEngine;

public class LockedDesk : MonoBehaviour, IInteractable
{
    [Header("Reward Settings")]
    [SerializeField] private KeycardLevel cardToGive = KeycardLevel.Gold;

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
            // 1. Spend the key (fires a UI alert automatically)
            playerInventory.ConsumeSimpleKey();
            isUnlocked = true;
            
            // 2. Award the gold card! (fires a UI upgrade alert automatically)
            playerInventory.UpgradeKeycard(cardToGive);
            Debug.Log($"[Desk] Drawer unlocked! Awarded {cardToGive} Card.");
            
            // Optional: You could play a drawer opening sound or trigger a brief animation here
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