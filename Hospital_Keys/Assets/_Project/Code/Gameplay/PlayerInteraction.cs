using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Vector3 boxCastSize = new Vector3(0.5f, 0.5f, 0.5f); // Thicken the detection zone
    [SerializeField] private LayerMask interactableLayer;

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
        // Start slightly up from the feet so we are dead-center with the meshes
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        // Using BoxCast instead of Raycast so turning angles don't miss narrow colliders
        if (Physics.BoxCast(origin, boxCastSize, transform.forward, out hit, transform.rotation, interactionDistance, interactableLayer))
        {
            // Check if the object hit (or any of its parents) implements our interface
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(_inventory);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizes your interaction box volume in the Scene view when selected
        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawWireCube(origin + transform.forward * interactionDistance, boxCastSize * 2f);
    }
}