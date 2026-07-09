using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactionDistance = 2f;
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
        // Hook into the modern Input System action perform event
        _controls.Player.Interaction.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
        _controls.Player.Interaction.performed -= OnInteractPressed;
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        // Cast a ray forward from the player's position
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Visual aid in the scene view to debug ray distance
        Debug.DrawRay(transform.position, transform.forward * interactionDistance, Color.yellow, 1f);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Check if the object hit implements our interface
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(_inventory);
            }
        }
    }
}