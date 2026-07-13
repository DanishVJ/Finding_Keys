using System.Collections;
using UnityEngine;

public class JanitorDoor : MonoBehaviour, IInteractable
{
    [Header("Orientation Adjustment")]
    [Tooltip("Check this box if the door swings toward the player instead of away!")]
    [SerializeField] private bool isFlipped = false;

    [Header("Movement Settings")]
    [SerializeField] private float swingSpeed = 6f;
    [SerializeField] private float openAngle = 90f;

    private bool isOpen = false;
    private float targetYRotation = 0f;
    private float currentYRotation = 0f;
    
    private Transform doorHingeGrandparent;

    private void Start()
    {
        Transform parentFolder = transform.parent;
        if (parentFolder != null)
        {
            doorHingeGrandparent = parentFolder.parent;
        }

        if (doorHingeGrandparent == null)
        {
            Debug.LogError($"[JanitorDoor] {gameObject.name} cannot find its DoorHinge (Grandparent)!");
        }
    }

    private void Update()
    {
        if (doorHingeGrandparent == null) return;

        currentYRotation = Mathf.MoveTowards(currentYRotation, targetYRotation, swingSpeed * 20f * Time.deltaTime);
        doorHingeGrandparent.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }

    public string GetInteractionPrompt()
    {
        // Once opened, it stays open, so we don't need an interaction prompt anymore
        if (isOpen) return ""; 
        return "Press E to open. Requires Simple Key";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isOpen) return;

        if (playerInventory.HasSimpleKey())
        {
            playerInventory.ConsumeSimpleKey();
            Debug.Log("[JanitorDoor] Key accepted. Opening janitor room permanently.");
            
            Vector3 playerPos = playerInventory.transform.position;
            DetermineSwingDirection(playerPos);
        }
        else
        {
            Debug.Log("[JanitorDoor] Access Denied! You do not have a Simple Key.");
        }
    }

    private void DetermineSwingDirection(Vector3 interactorPosition)
    {
        isOpen = true;

        Vector3 dirToPlayer = (interactorPosition - doorHingeGrandparent.position).normalized;
        float dot = Vector3.Dot(doorHingeGrandparent.forward, dirToPlayer);

        if (isFlipped)
        {
            dot *= -1f;
        }

        if (dot > 0f)
        {
            targetYRotation = -openAngle; 
        }
        else
        {
            targetYRotation = openAngle;  
        }
    }
}