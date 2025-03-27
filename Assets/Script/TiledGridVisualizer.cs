using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic; 

[RequireComponent(typeof(Tilemap))]

public class TiledGridVisualizer : MonoBehaviour
{
    private Vector3 gridOffset = Vector3.zero;
    [Header("Grid Settings")]
    public Material lineMaterial;
    public Color gridColor = Color.green;
    public float lineThickness = 0.05f;
    public float zOffset = -0.5f;

    [Header("Debug Controls")]
    public bool showEmptyCells = false; 

    private Tilemap _tilemap;
    private GameObject _gridContainer;
    private Dictionary<Vector3Int, GameObject> _cellLines = new Dictionary<Vector3Int, GameObject>();

    void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        if (_gridContainer != null)
            Destroy(_gridContainer);

        _gridContainer = new GameObject("Tiled Grid Container");
        _gridContainer.transform.SetParent(transform);
        _cellLines.Clear();

        BoundsInt bounds = _tilemap.cellBounds;
        GridLayout gridLayout = _tilemap.layoutGrid;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                bool hasTile = _tilemap.HasTile(cellPos);

                if (hasTile || showEmptyCells)
                {
                    VisualizeCell(cellPos, gridLayout.cellSize * 2, hasTile);
                }
            }
        }

        ApplyMovement(gridOffset);
    }

    void VisualizeCell(Vector3Int cellPos, Vector3 cellSize, bool isOccupied)
    {
        Vector3 center = _tilemap.GetCellCenterWorld(cellPos);
        center.z = zOffset;

        Color cellColor = isOccupied ? gridColor : new Color(gridColor.r, gridColor.g, gridColor.b, 0.3f);

        GameObject cellGo = new GameObject($"Cell ({cellPos.x},{cellPos.y})");
        cellGo.transform.SetParent(_gridContainer.transform);
        _cellLines[cellPos] = cellGo;

        // Draw diamond shape
        CreateLine(cellGo, center + new Vector3(0, cellSize.y/2, 0), center + new Vector3(cellSize.x/2, 0, 0), cellColor);
        CreateLine(cellGo, center + new Vector3(cellSize.x/2, 0, 0), center - new Vector3(0, cellSize.y/2, 0), cellColor);
        CreateLine(cellGo, center - new Vector3(0, cellSize.y/2, 0), center - new Vector3(cellSize.x/2, 0, 0), cellColor);
        CreateLine(cellGo, center - new Vector3(cellSize.x/2, 0, 0), center + new Vector3(0, cellSize.y/2, 0), cellColor);
    }

    void CreateLine(GameObject parent, Vector3 start, Vector3 end, Color color)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(parent.transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startColor = lr.endColor = color;
        lr.startWidth = lr.endWidth = lineThickness;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.useWorldSpace = false;
    }
    public void ApplyMovement(Vector3 movement)
{
    if (_gridContainer != null)
    {
        _gridContainer.transform.position += movement;
        Debug.Log("Grid moved to: " + _gridContainer.transform.position);
    }
}


    [ContextMenu("Refresh Grid")]
    public void RefreshGrid()
    {
        InitializeGrid();
    }



    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && _tilemap == null)
            _tilemap = GetComponent<Tilemap>();
    }
}