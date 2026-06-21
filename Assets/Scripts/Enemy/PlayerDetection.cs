using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    [SerializeField] private HordeMovement hordeMovement;

    [SerializeField] private LayerMask obstacleMask;

    private Transform player;

    private Vector3 lastSeenPosition;

    private bool playerInside;

    private void Update()
    {
        if (!playerInside || player == null)
            return;

        Vector2 direction =player.position - transform.position;

        float distance = direction.magnitude;

        RaycastHit2D hit =Physics2D.Raycast(transform.position,direction.normalized, distance, obstacleMask);

        

        if (hit.collider==null)
        {
            lastSeenPosition = player.position;

            if (hordeMovement.currentState != HordeMovement.State.Pursue)
            {
                hordeMovement.EnterPursue(player);
            }
        }
        else
        {
            if (hordeMovement.currentState == HordeMovement.State.Pursue)
            {
                hordeMovement.Search(lastSeenPosition);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            player = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            player = null;

            if (hordeMovement.currentState == HordeMovement.State.Pursue)
            {
                hordeMovement.Search(lastSeenPosition);
            }
        }
    }
}