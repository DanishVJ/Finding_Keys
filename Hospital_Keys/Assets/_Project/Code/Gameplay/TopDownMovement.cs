using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;
    private PlayerControls _controls;
    private Vector2 _moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Initialize the generated input class
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
        
        // Optional: If you want to handle interaction trigger right here later
        // controls.Player.Interact.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    void Update()
    {
        // Read the Vector2 value directly from our new WASD mapping
        _moveInput = _controls.Player.Movement.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Translate the Vector2 input (X, Y) to 3D space movement (X, Z)
        Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        
        rb.linearVelocity = new Vector3(movement.x * moveSpeed, rb.linearVelocity.y, movement.z * moveSpeed);

        // Simple Rotation: Face direction of movement
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }
    }
}