using UnityEngine;

public class GridDebugger : MonoBehaviour
{
    public Vector2 gridSize = new Vector2(30, 30); 
    public Vector2 cellSize = new Vector2(1.0f, 0.5f); 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (float x = 0; x < gridSize.x; x++)
        {
            for (float y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize.x, y * cellSize.y, 0);
                Gizmos.DrawWireCube(worldPos, new Vector3(cellSize.x, cellSize.y, 0));
            }
        }
    }
}
