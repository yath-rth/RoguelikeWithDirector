using UnityEngine;
using UnityEngine.Events;
public class AnimationEventHelper : MonoBehaviour
{
    public UnityEvent triggerFunctions;

    public void triggerEvent() {
        triggerFunctions?.Invoke();
    }
}
