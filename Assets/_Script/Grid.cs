using UnityEngine;

public class Grid<T>
{
    private GridConfig config;
    private Vector2 startPosition;
    private Vector2 cellOffSet;
    private T[,] grid;

    public Grid(GridConfig config)
    {
        ReloadNewData(config);
    }

    void ReloadNewData(GridConfig config)
    {
        this.config = config;
        grid = new T[config.Size.x, config.Size.y];
        startPosition = config.GetStartWorldPosition();
        cellOffSet = 0.5f * config.CellSize;
    }

    public void SetValue(int x,int y,T value)
    {
        if (!IsOnRange(x, y))
        {
            Debug.LogError("SetValue + Grid: IndexOutOfRange");
            return;
        }

        grid[x, y] = value;
    }

    public T GetValue(int x, int y)
    {
        if (!IsOnRange(x,y))
        {
            Debug.LogError("GetValue + Grid: IndexOutOfRange");
            return default(T);
        }

        return grid[x, y];
    }

    public T GetValueInScreenPosition(Vector2 worldPos)
    {
        int x, y;
        if (TryGetCellFromWorldPos(worldPos, out x, out y)) return GetValue(x, y);
        return default(T);
    }

    private bool IsOnRange(int x,int y)
    {
        if (x < 0 || y < 0 || x >= config.Size.x || y >= config.Size.y) return false;
        return true;
    }

    public bool TryGetCellFromWorldPos(Vector2 worldPos, out int x, out int y)
    {
        Vector2 step = config.CellSize + config.CellDistance;

        x = Mathf.FloorToInt(((worldPos - startPosition).x + cellOffSet.x) / step.x);
        y = Mathf.FloorToInt(((worldPos - startPosition).y + cellOffSet.y) / step.y);

        if (x < 0 || y < 0 || x >= config.Size.x || y >= config.Size.y)
        {
            return false;
        }

        Vector2 cellCenter = startPosition + new Vector2(x * step.x, y * step.y);
        bool insideX = Mathf.Abs(worldPos.x - cellCenter.x) <= config.CellSize.x * 0.5f;
        bool insideY = Mathf.Abs(worldPos.y - cellCenter.y) <= config.CellSize.y * 0.5f;

        return insideX && insideY;
    }
}