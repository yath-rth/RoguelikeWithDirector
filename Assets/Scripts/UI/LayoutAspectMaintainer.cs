using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum LayoutAspectMaintainerMode
{
    None,
    WidthControlsHeight,
    HeightControlsWidth
}
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class LayoutAspectMaintainer : UIBehaviour,ILayoutElement,ILayoutSelfController
{
    [SerializeField] protected LayoutAspectMaintainerMode mode =  LayoutAspectMaintainerMode.None;
    [SerializeField] private float aspectRatio = -1 ;
    
    private RectTransform rectTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (aspectRatio == -1)
        {
            aspectRatio = rectTransform.rect.width / rectTransform.rect.height;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }
    
    protected override void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

    void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }


    public void CalculateLayoutInputHorizontal() { }

    public void CalculateLayoutInputVertical() { }

    public float minWidth => mode == LayoutAspectMaintainerMode.HeightControlsWidth
        ? rectTransform.rect.height * aspectRatio
        : -1;
    public float preferredWidth => minWidth;
    public float flexibleWidth => -1;
    public float minHeight =>
        mode == LayoutAspectMaintainerMode.WidthControlsHeight
            ? rectTransform.rect.width / aspectRatio
            : -1;
    public float preferredHeight => minHeight;
    public float flexibleHeight => -1;
    public int layoutPriority => 1;
    public void SetLayoutHorizontal()
    {
        if (mode == LayoutAspectMaintainerMode.HeightControlsWidth)
        {
            float width = rectTransform.rect.height * aspectRatio;

            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width
            );
        }
    }

    public void SetLayoutVertical()
    {
        if (mode == LayoutAspectMaintainerMode.WidthControlsHeight)
        {
            float height = rectTransform.rect.width / aspectRatio;

            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                height
            );
        }
    }

    public void ChangeLayoutMode(LayoutAspectMaintainerMode mode)
    {
        this.mode = mode;
    }
    
}
