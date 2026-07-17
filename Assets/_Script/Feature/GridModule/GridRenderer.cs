using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private LevelConfig debugLevelConfig;
    private LevelConfig currentLevelConfig;

    private void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelConfigChange += OnLevelConfigChanged;
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelConfigChange -= OnLevelConfigChanged;
        }
    }

    private void OnLevelConfigChanged(LevelConfig newConfig)
    {
        currentLevelConfig = newConfig;
    }

    void DrawGrid(GridConfig gridConfig, Color color)
    {
        if (gridConfig == null || gridConfig.Size.x <= 0 || gridConfig.Size.y <= 0) return;
        
        Gizmos.color = color;
        for (int x = 0; x < gridConfig.Size.x; x++)
        for (int y = 0; y < gridConfig.Size.y; y++)
        {
            Vector2 center = gridConfig.GetCellWorldPosition(x, y);
            Gizmos.DrawWireCube(center, gridConfig.CellSize);
        }
    }

    void OnDrawGizmos()
    {
        LevelConfig activeConfig = (Application.isPlaying && currentLevelConfig != null) ? currentLevelConfig : debugLevelConfig;
        
        if (activeConfig != null)
        {
            DrawGrid(activeConfig.BoardGrid, Color.white);
            DrawGrid(activeConfig.WaitLineGrid, Color.white);
        }
    }
}
