using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PlayerInput playerInput;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private IInteractable currentInteractable;

    private InputAction moveAction;
    private InputAction interactAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Get actions directly from the referenced PlayerInput component
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            interactAction = playerInput.actions["Interact"];

            // Hook up callback functions
            if (moveAction != null)
            {
                moveAction.performed += Move;
                moveAction.canceled += Move;
            }

            if (interactAction != null)
            {
                interactAction.started += Interact;
            }
        }
    }

   

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    // Move Callback Function
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Interact Callback Function
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started && currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    // Trigger Detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
            }
        }
    }
}