using UnityEngine;

public class SearchableItem : MonoBehaviour, IInteractable
{
    [Header("Container Configuration")]
    [SerializeField] private string containerName = "File Cabinet";
    [SerializeField] private KeycardLevel hiddenCard = KeycardLevel.None;

    // --- NEW: Audio Field ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when searching through this item")]
    [SerializeField] private AudioClip searchSound;

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
            
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"{containerName} is already empty.");
            }
            return;
        }

        // --- NEW: Play searching sound instantly ---
        if (searchSound != null)
        {
            AudioSource.PlayClipAtPoint(searchSound, transform.position);
        }

        hasBeenSearched = true;
        Debug.Log($"Searching {containerName}...");

        if (hiddenCard != KeycardLevel.None)
        {
            playerInventory.UpgradeKeycard(hiddenCard);
            Debug.Log($"Success! Found a {hiddenCard} access card.");
        }
        else
        {
            Debug.Log("Nothing found inside.");

            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"Found nothing inside the {containerName}.");
            }
        }
    }
}