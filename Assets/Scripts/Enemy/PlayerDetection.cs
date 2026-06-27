using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    [SerializeField] private BaseEnemyMovement enemyMovement;

    [SerializeField]private LayerMask obstacleMask; // Layers that block sight

    private Transform player;

    private Vector3 lastSeenPosition;

    private bool playerInside;

    private void Update()
    {
        if (!playerInside)
            return;

        Vector2 direction = player.position - transform.position;

        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            distance,
            obstacleMask);

        bool playerInLOS = hit.collider == null;

        if (playerInLOS)
        {
            lastSeenPosition = player.position;

            if (enemyMovement.currentState != BaseEnemyMovement.State.Pursue)
            {
                enemyMovement.EnterPursue(player);
            }
        }
        else
        {
            if (enemyMovement.currentState == BaseEnemyMovement.State.Pursue)
            {
                enemyMovement.Search(lastSeenPosition);
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

            if (enemyMovement.currentState == BaseEnemyMovement.State.Pursue)
            {
                enemyMovement.Search(lastSeenPosition);
            }
        }
    }
}