using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Configuration")]
    [SerializeField] private KeycardLevel requiredLevel = KeycardLevel.Blue;

    [Header("Orientation Adjustment")]
    [Tooltip("Check this box if the door swings toward the player instead of away!")]
    [SerializeField] private bool isFlipped = false;

    [Header("Movement Settings")]
    [SerializeField] private float swingSpeed = 6f;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float holdOpenTime = 5f; // Hard-coded to 5 seconds

    // --- NEW: Sound Effect Field ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when the door starts opening and when it automatically closes")]
    [SerializeField] private AudioClip doorMovementSound;

    private bool isOpen = false;
    private float targetYRotation = 0f;
    private float currentYRotation = 0f;
    
    private Transform doorHingeGrandparent;
    private Coroutine closeTimerCoroutine;

    private void Start()
    {
        // Step 1: Find the parent folder (the old prefab root)
        Transform parentFolder = transform.parent;
        
        if (parentFolder != null)
        {
            // Step 2: Find the grandparent folder (your new custom DoorHinge parent)
            doorHingeGrandparent = parentFolder.parent;
        }

        if (doorHingeGrandparent == null)
        {
            Debug.LogError($"[Door] {gameObject.name} (Grandchild) cannot find its DoorHinge (Grandparent)! " +
                           "Please check that this prefab instance is correctly nested under your new scene parent.");
        }
    }

    private void Update()
    {
        if (doorHingeGrandparent == null) return;

        // Smoothly interpolate the rotation angle toward our current target
        currentYRotation = Mathf.MoveTowards(currentYRotation, targetYRotation, swingSpeed * 20f * Time.deltaTime);
        
        // Directly rotate the beautiful hinge anchor point you set up in the scene
        doorHingeGrandparent.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }

    public string GetInteractionPrompt()
    {
        if (isOpen) return "Door is open";
        return $"Press E to open. Requires {requiredLevel} Card";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isOpen) return;

        if (playerInventory.CurrentHighestCard >= requiredLevel)
        {
            Debug.Log($"Access Granted! Opening door requiring {requiredLevel} clearance.");
            
            // UI Hook: Inform the player access was approved
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"Access Granted. Opening {requiredLevel} Door.");
            }

            Vector3 playerPos = playerInventory.transform.position;
            DetermineSwingDirection(playerPos);

            if (closeTimerCoroutine != null) StopCoroutine(closeTimerCoroutine);
            closeTimerCoroutine = StartCoroutine(AutoCloseCountdown());
        }
        else
        {
            Debug.Log($"Access Denied! You have {playerInventory.CurrentHighestCard} but need {requiredLevel}.");

            // UI Hook: Clear, visual layout restriction feedback
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.DisplayNotification($"Access Denied! Requires {requiredLevel} keycard or higher.");
            }
        }
    }

    private void DetermineSwingDirection(Vector3 interactorPosition)
    {
        isOpen = true;

        // --- NEW: Play the door sound when it begins to open ---
        PlayDoorSound();

        // Use the grandparent hinge's orientation matrix to calculate side-of-approach
        Vector3 dirToPlayer = (interactorPosition - doorHingeGrandparent.position).normalized;
        float dot = Vector3.Dot(doorHingeGrandparent.forward, dirToPlayer);

        // If the door's forward alignment is backwards, reverse the mathematical sign
        if (isFlipped)
        {
            dot *= -1f;
        }

        if (dot > 0f)
        {
            targetYRotation = -openAngle; // Approached from front -> Swing inward/away
        }
        else
        {
            targetYRotation = openAngle;  // Approached from back -> Swing outward/away
        }
    }

    private IEnumerator AutoCloseCountdown()
    {
        yield return new WaitForSeconds(holdOpenTime);
        
        // --- NEW: Play the door sound again right as the automatic close begins ---
        PlayDoorSound();

        targetYRotation = 0f;
        isOpen = false;
        
        // Optional UI Hook: Let them know it shut
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.DisplayNotification("Door automatically closed.");
        }
    }

    // --- NEW: Private audio helper ---
    private void PlayDoorSound()
    {
        if (doorMovementSound != null)
        {
            AudioSource.PlayClipAtPoint(doorMovementSound, transform.position);
        }
    }
}