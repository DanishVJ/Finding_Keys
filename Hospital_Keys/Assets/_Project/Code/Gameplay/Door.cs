using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Configuration")]
    [SerializeField] private KeycardLevel requiredLevel = KeycardLevel.Blue;

    private bool isOpen = false;

    public string GetInteractionPrompt()
    {
        if (isOpen) return "Door is open";
        return $"Press E to open. Requires {requiredLevel} Card";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isOpen) return;

        // Use standard math operators: check if player card rank is greater than or equal to required
        if (playerInventory.CurrentHighestCard >= requiredLevel)
        {
            isOpen = true;
            Debug.Log($"Access Granted! Opening door requiring {requiredLevel} clearance.");
            
            // Rapid prototype fix: slide the door upwards out of the way or simply turn it off
            gameObject.SetActive(false); 
        }
        else
        {
            Debug.Log($"Access Denied! You have {playerInventory.CurrentHighestCard} but need {requiredLevel}.");
        }
    }
}