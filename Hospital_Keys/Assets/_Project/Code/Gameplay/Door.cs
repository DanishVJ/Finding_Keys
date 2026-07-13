using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Configuration")]
    [SerializeField] private KeycardLevel requiredLevel = KeycardLevel.Blue;

    [Header("Pivot Setup")]
    [Tooltip("Drag your partner's child pivot GameObject here!")]
    [SerializeField] private Transform childPivot;

    [Header("Movement Settings")]
    [SerializeField] private float swingSpeed = 6f;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float holdOpenTime = 10f;

    private bool isOpen = false;
    private float targetYRotation = 0f;
    private float currentYRotation = 0f;
    
    private Quaternion originalRotation;
    private Coroutine closeTimerCoroutine;

    private void Start()
    {
        // Save the baseline starting rotation so we always return to absolute zero
        originalRotation = transform.rotation;

        if (childPivot == null)
        {
            Debug.LogError($"[Door] Child Pivot missing on {gameObject.name}! Drag the child pivot into the Inspector slot.");
        }
    }

    private void Update()
    {
        // Smoothly step the rotation float toward our target angle
        currentYRotation = Mathf.MoveTowards(currentYRotation, targetYRotation, swingSpeed * 20f * Time.deltaTime);
        
        // Feed that angle into our custom child pivot rotation matrix math
        ApplyPivotRotation(currentYRotation);
    }

    public string GetInteractionPrompt()
    {
        if (isOpen) return "Door is open";
        return $"Press E to open. Requires {requiredLevel} Card";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        // If it's already open, don't re-trigger the swing calculation
        if (isOpen) return;

        // Verify keycard credentials
        if (playerInventory.CurrentHighestCard >= requiredLevel)
        {
            Debug.Log($"Access Granted! Opening door requiring {requiredLevel} clearance.");
            
            // Calculate direction using the player's world position relative to this door
            Vector3 playerPos = playerInventory.transform.position;
            DetermineSwingDirection(playerPos);

            // Start the 10-second automatic countdown
            if (closeTimerCoroutine != null) StopCoroutine(closeTimerCoroutine);
            closeTimerCoroutine = StartCoroutine(AutoCloseCountdown());
        }
        else
        {
            Debug.Log($"Access Denied! You have {playerInventory.CurrentHighestCard} but need {requiredLevel}.");
        }
    }

    private void DetermineSwingDirection(Vector3 interactorPosition)
    {
        isOpen = true;

        // Get direction vector from door to player
        Vector3 dirToPlayer = (interactorPosition - transform.position).normalized;
        
        // Dot product checks if player is standing in front or behind the door's local forward axis
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot > 0f)
        {
            // Player is in front -> Swing it backward away from them
            targetYRotation = -openAngle;
        }
        else
        {
            // Player is behind -> Swing it forward away from them
            targetYRotation = openAngle;
        }
    }

    private void ApplyPivotRotation(float targetAngle)
    {
        if (childPivot == null) return;

        // Reset to default baseline frame position before applying rotation matrix offset
        transform.rotation = originalRotation;

        // Force parent mesh matrix coordinates to swing relative to the child position instead of parent origin
        transform.RotateAround(childPivot.position, Vector3.up, targetAngle);
    }

    private IEnumerator AutoCloseCountdown()
    {
        // Hold open for designated time frame
        yield return new WaitForSeconds(holdOpenTime);

        // Reset variables to close the door smoothly back to zero degrees
        targetYRotation = 0f;
        isOpen = false;
    }
}