using UnityEngine;

public class SearchableItem : MonoBehaviour, IInteractable
{
    [Header("Container Configuration")]
    [SerializeField] private string containerName = "File Cabinet";
    [SerializeField] private KeycardLevel hiddenCard = KeycardLevel.None;

    private bool hasBeenSearched = false;

    public string GetInteractionPrompt()
    {
        if (hasBeenSearched) return $"{containerName} (Empty)";
        return $"Press E to Search {containerName}";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (hasBeenSearched)
        {
            Debug.Log($"{containerName} has already been searched.");
            
            // UI Hook: Inform the player it's already cleared
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"{containerName} is already empty.");
            }
            return;
        }

        hasBeenSearched = true;
        Debug.Log($"Searching {containerName}...");

        if (hiddenCard != KeycardLevel.None)
        {
            // Safely upgrade the player's keycard rank in their inventory (fires UI alert automatically)
            playerInventory.UpgradeKeycard(hiddenCard);
            Debug.Log($"Success! Found a {hiddenCard} access card.");
        }
        else
        {
            Debug.Log("Nothing found inside.");

            // UI Hook: Display empty container feedback text log
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"Found nothing inside the {containerName}.");
            }
        }
    }
}