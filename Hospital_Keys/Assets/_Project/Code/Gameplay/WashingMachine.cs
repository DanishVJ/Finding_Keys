using UnityEngine;

public class WashingMachine : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [Tooltip("Check this ONLY on the specific machine you want to hide the key in.")]
    [SerializeField] private bool hasKey = false; 

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

        hasBeenOpened = true;

        if (hasKey)
        {
            playerInventory.AddSimpleKey();
        }
        else
        {
            Debug.Log("[WashingMachine] Looked inside... Empty.");
        }
    }
}