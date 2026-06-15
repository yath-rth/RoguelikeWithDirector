using UnityEngine;

[ExecuteAlways]
public class AspectRatioComplianceModeSwitcher : MonoBehaviour
{
    [SerializeField] private LayoutAspectMaintainer layoutAspectMaintainer;
    [SerializeField] private RectTransform reference;
    [SerializeField] private LayoutAspectMaintainerMode lesser;
    [SerializeField] private LayoutAspectMaintainerMode more;
    [SerializeField] private float breakpoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        UpdateMode();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateMode();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateMode();
    }
#endif

    private void UpdateMode()
    {
        if (layoutAspectMaintainer == null) return;
        
        
   

        if ((float)reference.rect.width / reference.rect.height >= breakpoint)
        {
            layoutAspectMaintainer.ChangeLayoutMode(more);
        }
        else
        {
            layoutAspectMaintainer.ChangeLayoutMode(lesser);
        }

    }
}
