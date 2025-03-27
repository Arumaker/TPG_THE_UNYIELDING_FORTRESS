using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector3 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        canvasGroup.alpha = 0.6f; 
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position += (Vector3)eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
{
    canvasGroup.alpha = 1f;
    canvasGroup.blocksRaycasts = true;
    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(rectTransform.position);
    worldPosition.z = 0f; 

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
        if (hit.collider.CompareTag("GridCell")) 
        {
            GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
            if (gridManager != null)
            {
                transform.position = gridManager.GetSnappedPosition(worldPosition);
                return;
            }
        }
    }
    rectTransform.position = originalPosition;
}

}
