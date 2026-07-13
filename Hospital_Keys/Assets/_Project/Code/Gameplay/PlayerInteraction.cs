using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;
    
    [Header("Box Cast Configuration")]
    [Tooltip("The half-extents (half of the total width, height, and depth) of your interaction box.")]
    [SerializeField] private Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.2f);

    private PlayerControls _controls;
    private PlayerInventory _inventory;
    private IInteractable _currentInteractable; // Tracks what we are looking at

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
        _controls.Player.Interaction.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
        _controls.Player.Interaction.performed -= OnInteractPressed;
    }

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        RaycastHit hit;

        // Continuously sweep the box to check what the player is looking at
        bool hasHit = Physics.BoxCast(
            transform.position,
            boxHalfExtents,
            transform.forward,
            out hit,
            transform.rotation,
            interactionDistance,
            interactableLayer
        );

        if (hasHit)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable == null && hit.transform.parent != null)
            {
                interactable = hit.transform.parent.GetComponent<IInteractable>();

                if (interactable == null)
                {
                    foreach (Transform sibling in hit.transform.parent)
                    {
                        interactable = sibling.GetComponent<IInteractable>();
                        if (interactable != null) break;
                    }
                }
            }

            // If we found something interactable, update the UI prompt
            if (interactable != null)
            {
                _currentInteractable = interactable;
                
                // Safe check for when we build the UI manager next
                if (GameUIManager.Instance != null)
                {
                    GameUIManager.Instance.ShowPrompt(_currentInteractable.GetInteractionPrompt());
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void ClearCurrentInteractable()
    {
        _currentInteractable = null;
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.HidePrompt();
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        // If our Update loop has already found a valid target, interact immediately!
        if (_currentInteractable != null)
        {
            _currentInteractable.Interact(_inventory);
            
            // Refresh the prompt text immediately (e.g., changes to "Empty")
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.ShowPrompt(_currentInteractable.GetInteractionPrompt());
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
        Vector3 endCenter = Vector3.forward * interactionDistance;
        Gizmos.DrawWireCube(endCenter, boxHalfExtents * 2f);
        
        Gizmos.DrawLine(new Vector3(-boxHalfExtents.x, 0f, boxHalfExtents.z), new Vector3(-boxHalfExtents.x, 0f, boxHalfExtents.z) + endCenter);
        Gizmos.DrawLine(new Vector3(boxHalfExtents.x, 0f, boxHalfExtents.z), new Vector3(boxHalfExtents.x, 0f, boxHalfExtents.z) + endCenter);

        Gizmos.matrix = oldMatrix;
    }
}