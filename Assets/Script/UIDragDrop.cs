using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    public GameObject worldPrefab;
    public bool snapToGrid = true;
    public LayerMask blockingLayers;

    [Header("Drag Preview")] 
    public GameObject dragPreviewPrefab;
    private GameObject currentPreview;
    public Color validPreviewColor = new Color(1, 1, 1, 0.7f);
    public Color invalidPreviewColor = new Color(1, 0.5f, 0.5f, 0.5f);

    [Header("Visual Feedback")]
    public float dropShakeDuration = 0.5f;
    public float dropShakeIntensity = 5f;
    public float dropShakeSpeed = 10f;
    public ParticleSystem invalidDropParticles;

    [Header("Manual Bounds Control")]
    public bool useManualBounds = false; // Toggle between auto/manual bounds
    public Vector3Int minBounds = new Vector3Int(-5, -5, 0); // Custom grid bounds (min)
    public Vector3Int maxBounds = new Vector3Int(5, 5, 0);    // Custom grid bounds (max)
    public Vector3 manualOffset = new Vector3(2, 0, 0);

    [Header("Audio Feedback")]
    public AudioClip validDropSound;
    public AudioClip invalidDropSound;
    [Range(0, 1)] public float volume = 0.8f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private GameObject draggedClone;
    private Vector3 originalPosition;
    private AudioSource audioSource;
    private Tilemap tilemap;
    private bool isValidDrop;
    private Vector3 lastDropPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        TiledGridVisualizer grid = FindFirstObjectByType<TiledGridVisualizer>();
        if (grid != null) tilemap = grid.GetComponent<Tilemap>();
    }

  public void OnBeginDrag(PointerEventData eventData)
{
    originalPosition = rectTransform.position;
    canvasGroup.alpha = 0.5f;
    canvasGroup.blocksRaycasts = false;

    Vector3 worldPos = GetWorldPosition(eventData.position);
    draggedClone = new GameObject("DragClone");
    
    SpriteRenderer sr = draggedClone.AddComponent<SpriteRenderer>();
    Image originalImage = GetComponent<Image>();
    if (originalImage != null)
    {
        sr.sprite = originalImage.sprite;
        sr.color = originalImage.color;
    }

    sr.sortingLayerName = "UI"; 
    sr.sortingOrder = 100; 


    draggedClone.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    draggedClone.transform.localScale = Vector3.one;

    if (dragPreviewPrefab != null)
    {
        currentPreview = Instantiate(dragPreviewPrefab, worldPos, Quaternion.identity);
        SpriteRenderer previewSr = currentPreview.GetComponent<SpriteRenderer>();
        if (previewSr != null)
        {
            previewSr.sortingLayerName = "UI";
            previewSr.sortingOrder = 101; 
        }
        UpdatePreviewValidity(worldPos);
    }
}

     public void OnDrag(PointerEventData eventData)
{
    if (draggedClone == null) return;

    Vector3 worldPos = GetWorldPosition(eventData.position);
    
    if (snapToGrid && tilemap != null)
    {
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);
        Debug.Log($"Checking cell: {cellPos} | World pos: {worldPos} | HasTile: {tilemap.HasTile(cellPos)}");
        worldPos = tilemap.GetCellCenterWorld(cellPos);
    }

    draggedClone.transform.position = worldPos;
    lastDropPosition = worldPos;
    
    if (currentPreview != null)
    {
        currentPreview.transform.position = worldPos;
        UpdatePreviewValidity(worldPos);
    }
}
    

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedClone == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        isValidDrop = IsValidDropPosition(lastDropPosition);

        if (isValidDrop)
        {
            HandleValidDrop(lastDropPosition);
        }
        else
        {
            HandleInvalidDrop();
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;
        }
    }

    private void HandleValidDrop(Vector3 position)
    {
        if (validDropSound != null)
        {
            audioSource.PlayOneShot(validDropSound);
        }

        if (worldPrefab != null)
        {
            SpawnWorldCharacter(position);
        }

        Destroy(draggedClone);
    }

    private void HandleInvalidDrop()
    {
        if (invalidDropSound != null)
        {
            audioSource.PlayOneShot(invalidDropSound);
        }

        if (invalidDropParticles != null)
        {
            Instantiate(invalidDropParticles, lastDropPosition, Quaternion.identity);
        }

        StartCoroutine(InvalidDropRoutine());
    }

    private IEnumerator InvalidDropRoutine()
    {
        if (draggedClone == null) yield break;

        Vector3 startPos = draggedClone.transform.position;
        Image cloneImage = draggedClone.GetComponent<Image>();
        Color originalColor = cloneImage != null ? cloneImage.color : Color.white;

        float elapsed = 0f;
        while (elapsed < dropShakeDuration)
        {
            float shakeX = Mathf.Sin(elapsed * dropShakeSpeed) * dropShakeIntensity;
            float shakeY = Mathf.Cos(elapsed * dropShakeSpeed * 0.7f) * dropShakeIntensity;
            
            draggedClone.transform.position = startPos + new Vector3(shakeX, shakeY, 0);

            if (cloneImage != null)
            {
                float flash = Mathf.PingPong(elapsed * 10f, 1f);
                cloneImage.color = Color.Lerp(originalColor, invalidPreviewColor, flash);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (cloneImage != null) cloneImage.color = originalColor;
        Destroy(draggedClone);
    }

     private Vector3 GetWorldPosition(Vector2 screenPos)
{
    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return Vector3.zero;

        float zDistance = Mathf.Abs(mainCam.transform.position.z);
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(
            screenPos.x,
            screenPos.y,
            zDistance)); 

        worldPos.z = 0; 
        return worldPos;
    }
    else 
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out Vector3 worldPos);

        worldPos.z = 0; 
        return worldPos;
    }
}

    private bool IsValidDropPosition(Vector3 worldPos)
{
    if (tilemap == null) return false;

    Vector3Int cellPos = tilemap.WorldToCell(worldPos);

    // Apply manual offset in grid space if needed
    if (useManualBounds)
    {
        cellPos += Vector3Int.FloorToInt(manualOffset);
    }

    // Bounds check (auto or manual)
    bool inBounds;
    if (useManualBounds)
    {
        inBounds = cellPos.x >= minBounds.x && cellPos.x <= maxBounds.x &&
                   cellPos.y >= minBounds.y && cellPos.y <= maxBounds.y;
    }
    else
    {
        inBounds = tilemap.cellBounds.Contains(cellPos);
    }

    if (!inBounds)
    {
        Debug.Log($"Out of bounds: {cellPos}");
        return false;
    }

    // Tile existence check - use the original cell position (without manual offset)
    Vector3Int tileCheckPos = tilemap.WorldToCell(worldPos);
    if (!tilemap.HasTile(tileCheckPos))
    {
        Debug.Log($"No tile at: {tileCheckPos}");
        return true;
    }

    // Convert back to world position with offset
    if (snapToGrid)
    {
        worldPos = tilemap.GetCellCenterWorld(cellPos);
        if (useManualBounds)
        {
            worldPos += manualOffset - Vector3Int.FloorToInt(manualOffset); // Fractional offset
        }
    }

    // Collision check
    Collider2D[] overlaps = Physics2D.OverlapCircleAll(worldPos, 0.2f, blockingLayers);
    foreach (Collider2D col in overlaps)
    {
        if (col.gameObject != draggedClone && col.gameObject != currentPreview)
            return false;
    }

    return true;
}

    private void UpdatePreviewValidity(Vector3 worldPos)
    {
        if (currentPreview == null) return;
        
        isValidDrop = IsValidDropPosition(worldPos);
        SpriteRenderer previewRenderer = currentPreview.GetComponent<SpriteRenderer>();
        if (previewRenderer != null)
        {
            previewRenderer.color = isValidDrop ? validPreviewColor : invalidPreviewColor;
        }
    }

  private int _characterSortingOrder = 1; 

private void SpawnWorldCharacter(Vector3 position)
{
    if (snapToGrid && tilemap != null)
    {
        Vector3Int cellPos = tilemap.WorldToCell(position);
        position = tilemap.GetCellCenterWorld(cellPos);
    }

    position.y += 0.4f;
    GameObject character = Instantiate(worldPrefab, position, Quaternion.identity);
    
    SpriteRenderer characterSr = character.GetComponent<SpriteRenderer>();
    if (characterSr != null)
    {
        characterSr.sortingLayerName = "Characters"; 
        characterSr.sortingOrder = _characterSortingOrder++;
        if (_characterSortingOrder > 10000) _characterSortingOrder = 1;
    }
}

    void OnDestroy()
    {
        if (draggedClone != null) Destroy(draggedClone);
        if (currentPreview != null) Destroy(currentPreview);
    }
    private void OnDrawGizmos()
{
    if (tilemap == null) return;

    // Draw tilemap bounds
    Gizmos.color = Color.cyan;
    BoundsInt bounds = tilemap.cellBounds;
    Gizmos.DrawWireCube(bounds.center, bounds.size);
}
}