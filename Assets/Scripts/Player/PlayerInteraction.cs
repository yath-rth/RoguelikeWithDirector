using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private bool playerCanInteract;
    private PlayerInput playerInput;
    private InputAction interactAction;

    // Update is called once per frame
    void Update()
    {
        if (!playerCanInteract || interactAction == null)
        {
            return;
        }

        if (interactAction.WasPressedThisFrame())
        {
            Debug.Log("Player is interacting");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerCanInteract = true;
            playerInput = other.GetComponentInParent<PlayerInput>();

            if (playerInput != null)
            {
                interactAction = playerInput.actions["Interact"];
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactAction = null;
            playerInput = null;
            playerCanInteract = false;
        }
    }
}
