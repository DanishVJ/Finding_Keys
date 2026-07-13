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
            return;
        }

        hasBeenSearched = true;
        Debug.Log($"Searching {containerName}...");

        if (hiddenCard != KeycardLevel.None)
        {
            // Safely upgrade the player's keycard rank in their inventory
            playerInventory.UpgradeKeycard(hiddenCard);
            Debug.Log($"Success! Found a {hiddenCard} access card.");
        }
        else
        {
            Debug.Log("Nothing found inside.");
        }
    }
}