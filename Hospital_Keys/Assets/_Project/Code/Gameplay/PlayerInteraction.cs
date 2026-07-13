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

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        RaycastHit hit;

        // Physics.BoxCast sweeps a 3D box forward from the player's position
        bool hasHit = Physics.BoxCast(
            transform.position,              // Center of the starting box
            boxHalfExtents,                  // Half the size of the box in each axis
            transform.forward,               // Direction to sweep the box
            out hit,                         // Store hitting data
            transform.rotation,              // Orientation of the box (matches player)
            interactionDistance,             // How far forward to project the sweep
            interactableLayer                // Layer mask restriction
        );

        if (hasHit)
        {
            // 1. Check ONLY the exact child object we hit ("Colliders")
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            // 2. STRICT SIBLING CHECK: If it's not there, step up to the immediate parent, 
            // and look ONLY at its direct children (its siblings) without climbing higher.
            if (interactable == null && hit.transform.parent != null)
            {
                foreach (Transform sibling in hit.transform.parent)
                {
                    interactable = sibling.GetComponent<IInteractable>();
                    if (interactable != null) break; // Found the "Code" sibling! Stop looking.
                }
            }

            if (interactable != null)
            {
                interactable.Interact(_inventory);
            }
            else
            {
                Debug.LogWarning($"[PlayerInteraction] Hit {hit.collider.gameObject.name}, but no IInteractable script found among its immediate siblings.");
            }
        }
    }

    // Draws the interactive volume inside the Scene tab so you can visualize the size
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        
        // Align the gizmo rendering space with the player's position and orientation matrix
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // Draw the starting volume bounds
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
        
        // Draw the terminal target bounds where the sweep ends
        Vector3 endCenter = Vector3.forward * interactionDistance;
        Gizmos.DrawWireCube(endCenter, boxHalfExtents * 2f);
        
        // Connect the corners with directional vector lines
        Gizmos.DrawLine(new Vector3(-boxHalfExtents.x, 0f, boxHalfExtents.z), new Vector3(-boxHalfExtents.x, 0f, boxHalfExtents.z) + endCenter);
        Gizmos.DrawLine(new Vector3(boxHalfExtents.x, 0f, boxHalfExtents.z), new Vector3(boxHalfExtents.x, 0f, boxHalfExtents.z) + endCenter);

        Gizmos.matrix = oldMatrix;
    }
}