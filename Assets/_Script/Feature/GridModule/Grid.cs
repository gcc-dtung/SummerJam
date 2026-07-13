using System;
using UnityEngine;

public class Grid<T>
{
    private GridConfig config;
    private Vector2 anchor => config.GetWorldPosition();
    private Vector2 cellOffSet => 0.5f * config.CellSize;
    private T[,] grid;

    public Grid(GridConfig config,Func<int,int,T> createGridObject)
    {
        ReloadNewData(config,createGridObject);
    }

    void ReloadNewData(GridConfig config,Func<int,int,T> createGridObject)
    {
        this.config = config;
        grid = new T[config.Size.x, config.Size.y];

        for (int x = 0; x < config.Size.x; x++)
        {
            for (int y = 0; y < config.Size.y; y++)
            {
                SetValue(x,y,createGridObject(x,y));
            }
        }
    }

    public void MatrixTraversal(Action<int,int,T> action)
    {
        for(int x = 0;x<config.Size.x;x++)
        for (int y = 0; y < config.Size.y; y++)
        {
            action(x, y, grid[x, y]);
        }
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

    public T GetValueFromWorldPosition(Vector2 worldPos)
    {
        int x, y;
        if (TryGetCellFromWorldPos(worldPos, out x, out y)) return GetValue(x, y);
        return default(T);
    }

    public bool IsOnRange(int x,int y)
    {
        if (x < 0 || y < 0 || x >= config.Size.x || y >= config.Size.y) return false;
        return true;
    }

    public bool TryGetCellFromWorldPos(Vector2 worldPos, out int x, out int y)
    {
        Vector2 step = config.CellSize + config.CellDistance;

        x = Mathf.FloorToInt(((worldPos - anchor).x + cellOffSet.x) / step.x);
        y = Mathf.FloorToInt(((worldPos - anchor).y + cellOffSet.y) / step.y);

        if (!IsOnRange(x,y)) return false;

        Vector2 cellCenter = anchor + new Vector2(x * step.x, y * step.y);
        bool insideX = Mathf.Abs(worldPos.x - cellCenter.x) <= config.CellSize.x * 0.5f;
        bool insideY = Mathf.Abs(worldPos.y - cellCenter.y) <= config.CellSize.y * 0.5f;

        return insideX && insideY;
    }

    public Vector2 GetWorldPosition(int x,int y)
    {
        if (!IsOnRange(x, y))
        {
            Debug.LogWarning("GetWorldPosition + Grid:  IndexOutOfRange");
        }
        Vector2 step = config.CellSize + config.CellDistance;
        return anchor + new Vector2(x * step.x, y * step.y);
    }
}