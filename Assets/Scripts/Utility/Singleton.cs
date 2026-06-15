using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
    where T:Singleton<T>
{

    private static T _instance;
    public static bool instanceExists => instance != null;
    public static T instance => _instance;
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            DontDestroyOnLoad(instance);
        }
        else {
            Destroy(gameObject);
        }
    }

}
