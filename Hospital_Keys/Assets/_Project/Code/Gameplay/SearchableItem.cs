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
            // Give the card to the player
            playerInventory.UpgradeKeycard(hiddenCard);
            Debug.Log($"Success! Found a {hiddenCard} access card.");
        }
        else
        {
            Debug.Log("Nothing found inside.");
        }

        // Visual feedback placeholder: turn grey when empty so your partner knows it works
        GetComponent<Renderer>().material.color = Color.gray;
    }
}