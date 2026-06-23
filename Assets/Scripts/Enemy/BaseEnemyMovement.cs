using UnityEngine;

public abstract class BaseEnemyMovement : MonoBehaviour
{
    public enum State
    {
        Idle,
        Pursue,
        Search
    }

    public State currentState;

    public abstract void EnterPursue(Transform target);

    public abstract void Search(Vector3 position);

    public abstract void EnterIdle();
}