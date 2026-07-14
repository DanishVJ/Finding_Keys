using System.Collections;
using UnityEngine;

public class CardboardBox : MonoBehaviour, IInteractable
{
    // --- NEW: Global static tracking for the fail-safe ---
    public static int BlockedHatchCount { get; private set; } = 0;

    [Header("Movement Target")]
    [Tooltip("Drag the TargetPosition child object here.")]
    [SerializeField] private Transform targetPoint;
    
    [Header("Speed Configuration")]
    [Tooltip("How fast the box slides.")]
    [SerializeField] private float slideSpeed = 5f;

    [Header("Audio Clips")]
    [Tooltip("Sound played when the player interacts and the box begins to slide")]
    [SerializeField] private AudioClip cardboardSlideSound;

    private Vector3 startPosition;
    private bool isAtTarget = false;
    private bool isMoving = false; 

    private void Start()
    {
        startPosition = transform.position;
        
        if (targetPoint == null)
        {
            Debug.LogError($"[CardboardBox] Target Point is missing on {gameObject.name}!");
        }
    }

    // Reset static variables if the scene reloads
    private void OnDestroy()
    {
        BlockedHatchCount = 0;
    }

    public string GetInteractionPrompt()
    {
        if (isMoving) return "Moving...";
        
        if (isAtTarget)
        {
            return "Press E to pull the box away from the hatch.";
        }
        return "Press E to move the cardboard box over the hatch.";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (isMoving || targetPoint == null) return;

        if (cardboardSlideSound != null)
        {
            AudioSource.PlayClipAtPoint(cardboardSlideSound, transform.position);
        }
        
        Vector3 destination = isAtTarget ? startPosition : targetPoint.position;
        
        StartCoroutine(SlideTo(destination));
    }

    private IEnumerator SlideTo(Vector3 destination)
    {
        isMoving = true;

        // If it was previously at the target, it's moving AWAY, so subtract from count now
        if (isAtTarget)
        {
            BlockedHatchCount = Mathf.Max(0, BlockedHatchCount - 1);
        }

        while (Vector3.Distance(transform.position, destination) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                destination, 
                slideSpeed * Time.deltaTime
            );
            
            yield return null; 
        }

        transform.position = destination;
        isAtTarget = !isAtTarget;
        isMoving = false;

        // If it arrived at the target, it's covering a hatch, so add to count now
        if (isAtTarget)
        {
            BlockedHatchCount++;
        }
        
        Debug.Log($"[CardboardBox] {gameObject.name} settled. Total Blocked Hatches: {BlockedHatchCount}");

        if (GameUIManager.Instance != null)
        {
            if (isAtTarget)
            {
                GameUIManager.Instance.DisplayNotification("Box moved over the hatch.");
            }
            else
            {
                GameUIManager.Instance.DisplayNotification("Hatch is exposed!");
            }
        }
    }
}