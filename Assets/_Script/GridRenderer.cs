using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private GridConfig config;
    
    void DrawGrid(int xGridLength, int yGridLength)
    {
        for (int x = 0; x < xGridLength; x++)
        for (int y = 0; y < yGridLength; y++)
        {
            Vector2 center = config.GetCellWorldPosition(x, y);
            Gizmos.DrawWireCube(center, config.CellSize);
        }
    }

    void OnDrawGizmos()
    {
        if (config == null || config.Size.x <= 0 || config.Size.y <= 0) return;
        DrawGrid(config.Size.x, config.Size.y);
    }
}
