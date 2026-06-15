using UnityEngine;

public abstract class SingletonNonPersistent<T> : MonoBehaviour
    where T:SingletonNonPersistent<T>
{

    private static T _instance;
    public static bool instanceExists => instance != null;
    public static T instance => _instance;
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
        }
        else {
            Destroy(gameObject);
        }
    }

}
