using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;

    private void Start()
    {
        UpdateSafeArea();
    }

    
    [ContextMenu("UpdateSafeArea")]
    public void UpdateSafeArea()
    {
        rectTransform = GetComponent<RectTransform>();
        safeArea = Screen.safeArea;
        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        //Debug.Log(safeArea.position);
        //Debug.Log(safeArea.size);


        minAnchor.x /= Screen.width;
        //Debug.Log("Min x: "+minAnchor.x);
        minAnchor.y /= Screen.height;
        //Debug.Log("Min y: " + minAnchor.y);
        maxAnchor.x /= Screen.width;
        //Debug.Log("Max x: " + maxAnchor.x);
        maxAnchor.y /= Screen.height;
        //Debug.Log("Max y: " + maxAnchor.y);

        rectTransform.anchorMin = minAnchor;
        // Debug.Log(rectTransform.anchorMin);
        rectTransform.anchorMax = maxAnchor;
        //Debug.Log(rectTransform.anchorMax);
    }

}
