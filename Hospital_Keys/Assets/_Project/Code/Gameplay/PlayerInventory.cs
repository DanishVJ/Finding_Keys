using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // Shows as a clean dropdown in the Unity Inspector
    public KeycardLevel CurrentHighestCard = KeycardLevel.None;

    public void UpgradeKeycard(KeycardLevel newLevel)
    {
        if (newLevel > CurrentHighestCard)
        {
            CurrentHighestCard = newLevel;
            Debug.Log($"Inventory Updated! Highest card is now: {CurrentHighestCard}");
        }
    }
}