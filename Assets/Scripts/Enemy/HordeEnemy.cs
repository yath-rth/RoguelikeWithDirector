using UnityEngine;
using Pathfinding;

public class HordeMovement : MonoBehaviour
{
    public enum State
    {
        Idle,
        Pursue,
        Search
    }

    [SerializeField] private AIPath aiPath;

    public State currentState = State.Idle;

    private Transform target;

    private void Start()
    {
        aiPath.canMove = false;
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

                if (!aiPath.pathPending &&
                    aiPath.reachedDestination)
                {
                    EnterIdle();
                }

                break;
        }
    }

    public void EnterPursue(Transform player)
    {
        target = player;
        currentState = State.Pursue;

        aiPath.canMove = true;
    }

    public void Search(Vector3 lastSeenPosition)
    {
        target = null;

        currentState = State.Search;

        aiPath.canMove = true;
        aiPath.destination = lastSeenPosition;
    }

    public void EnterIdle()
    {
        currentState = State.Idle;

        aiPath.canMove = false;
        target = null;
    }
}