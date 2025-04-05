using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap tilemap;
    private Grid grid; 

    void Awake()
{
    if (tilemap == null)
    {
        tilemap = FindFirstObjectByType<Tilemap>(); 
    }
    
    if (tilemap == null)
    {
        Debug.LogError("GridManager: No Tilemap found in the scene!");
    }
}


    public Vector3 GetSnappedPosition(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition); 
        Vector3 snapped = grid.CellToWorld(cellPosition) + (grid.cellSize / 2); 
        Debug.Log("Snapped position: " + snapped);
        return snapped;
    }
}
