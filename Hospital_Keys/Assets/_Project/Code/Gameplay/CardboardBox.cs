using System.Collections;
using UnityEngine;

public class CardboardBox : MonoBehaviour, IInteractable
{
    [Header("Movement Target")]
    [Tooltip("Drag the TargetPosition child object here.")]
    [SerializeField] private Transform targetPoint;
    
    [Header("Speed Configuration")]
    [Tooltip("How fast the box slides.")]
    [SerializeField] private float slideSpeed = 5f;

    // --- NEW: Sound Effect Field ---
    [Header("Audio Clips")]
    [Tooltip("Sound played when the player interacts and the box begins to slide")]
    [SerializeField] private AudioClip cardboardSlideSound;

    private Vector3 startPosition;
    private bool isAtTarget = false;
    private bool isMoving = false; // Prevents spamming E while it's mid-slide

    private void Start()
    {
        // Automatically remember exactly where the level designer placed the box
        startPosition = transform.position;
        
        if (targetPoint == null)
        {
            Debug.LogError($"[CardboardBox] Target Point is missing on {gameObject.name}!");
        }
    }

    public string GetInteractionPrompt()
    {
        if (isMoving) return "Moving...";
        
        // Dynamically change the text based on where the box currently is
        if (isAtTarget)
        {
            return "Press E to pull the box away from the hatch.";
        }
        return "Press E to move the cardboard box over the hatch.";
    }

    public void Interact(PlayerInventory playerInventory)
    {
        // Safety check: if the box is already mid-slide, ignore extra inputs
        if (isMoving || targetPoint == null) return;

        // --- NEW: Play the slide sound at the box's current location ---
        if (cardboardSlideSound != null)
        {
            AudioSource.PlayClipAtPoint(cardboardSlideSound, transform.position);
        }
        
        // Determine the next destination based on our current state toggle
        Vector3 destination = isAtTarget ? startPosition : targetPoint.position;
        
        // Start the smooth movement routine
        StartCoroutine(SlideTo(destination));
    }

    private IEnumerator SlideTo(Vector3 destination)
    {
        isMoving = true;

        // Keep moving until we are close enough to snap
        while (Vector3.Distance(transform.position, destination) > 0.001f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                destination, 
                slideSpeed * Time.deltaTime
            );
            
            yield return null; 
        }

        // Lock precisely to the coordinates
        transform.position = destination;
        
        // Flip our state toggle since it successfully arrived
        isAtTarget = !isAtTarget;
        isMoving = false;
        
        Debug.Log($"[CardboardBox] {gameObject.name} arrived. Is at hatch target: {isAtTarget}");

        // UI Hooks: Alert the player to the state of the hatch layout
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