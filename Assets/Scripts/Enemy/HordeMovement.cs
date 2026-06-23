using UnityEngine;
using Pathfinding;

public class HordeMovement : BaseEnemyMovement
{
    [SerializeField] private AIPath aiPath;
    [SerializeField] public Vector2 DashForce;

    private Transform target;
    private Rigidbody2D rb;

    private void Start()
    {
        aiPath.canMove = false;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Pursue:
                if (target != null)
                {
                    aiPath.destination = target.position;
                }
                break;

            case State.Search:
                if (!aiPath.pathPending && aiPath.reachedDestination)
                {
                    EnterIdle();
                }
                break;
        }
    }

    public override void EnterPursue(Transform player)
    {
        target = player;
        currentState = State.Pursue;
        aiPath.canMove = true;
    }

    public override void Search(Vector3 lastSeenPosition)
    {
        target = null;
        currentState = State.Search;
        aiPath.canMove = true;
        aiPath.destination = lastSeenPosition;
    }

    public override void EnterIdle()
    {
        currentState = State.Idle;
        aiPath.canMove = false;
        target = null;
    }
}