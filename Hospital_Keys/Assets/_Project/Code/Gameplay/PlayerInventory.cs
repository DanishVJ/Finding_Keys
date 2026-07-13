using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Keycard Security")]
    // Shows as a clean dropdown in the Unity Inspector
    public KeycardLevel CurrentHighestCard = KeycardLevel.None;

    [Header("Special Items")]
    [SerializeField] private int simpleKeyCount = 0; // Tracks how many simple keys you have

    public void UpgradeKeycard(KeycardLevel newLevel)
    {
        if (newLevel > CurrentHighestCard)
        {
            CurrentHighestCard = newLevel;
            Debug.Log($"Inventory Updated! Highest card is now: {CurrentHighestCard}");
        }
    }

    // Call this when the player searches the cabinet
    public void AddSimpleKey()
    {
        simpleKeyCount++;
        Debug.Log($"Collected a Simple Key! Total keys: {simpleKeyCount}");
    }

    // Call this to check if the player can open a simple lock
    public bool HasSimpleKey()
    {
        return simpleKeyCount > 0;
    }

    // Call this immediately after successfully unlocking a simple lock
    public void ConsumeSimpleKey()
    {
        if (simpleKeyCount > 0)
        {
            simpleKeyCount--;
            Debug.Log($"Used a Simple Key. Keys remaining: {simpleKeyCount}");
        }
    }
}