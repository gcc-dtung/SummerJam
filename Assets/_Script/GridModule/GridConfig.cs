using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Grid/Config", fileName = "GridConfig")]
public class GridConfig : ScriptableObject
{
    [Header("Config Grid")]
    [field:SerializeField] public Vector2Int Size { get; private set; }
    [field:SerializeField] public Vector2 CellSize { get; private set; }
    [field:SerializeField] public Vector2 CellDistance { get; private set; }
    [Header("Grid Position")]
    [SerializeField] private Vector2 OriginPosition;
    [SerializeField] private CellDataSO Blocked;
    public Wrapper<CellDataSO>[] BaseGrid;
    
    private void OnEnable()
    {
      if(BaseGrid == null)   ResetGrid();
    }

    public void ResetGrid()
    {
        BaseGrid = new Wrapper<CellDataSO>[Size.y];
        for (int y = 0; y < Size.y; y++)
        {
            BaseGrid[y] = new Wrapper<CellDataSO>();
            BaseGrid[y].Values = new CellDataSO[Size.x];
        }

        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                BaseGrid[y].Values[x] = Blocked;
            }
        }
    }

    public Vector2 GetStartWorldPosition()
    {
        Vector2 offSet = new Vector2(
            -0.5f * (CellSize.x + CellDistance.x) * (Size.x - 1),
            -0.5f * (CellSize.y + CellDistance.y) * (Size.y - 1)
        );
        return OriginPosition + offSet;
    }
    
    public Vector2 GetCellWorldPosition(int x, int y)
    {
        Vector2 step = CellSize + CellDistance;
        return GetStartWorldPosition() + new Vector2(x * step.x, y * step.y);
    }
    
}

[Serializable]
public class Wrapper<T>
{
    public T[] Values;
}




