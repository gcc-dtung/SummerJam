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
    //[HideInInspector]
    [SerializeField] private Cell Blocked;
    public Wrapper<Cell>[] BaseGrid;
    
    private void OnEnable()
    {
      if(BaseGrid == null)   ResetGrid();
    }

    public void ResetGrid()
    {
        BaseGrid = new Wrapper<Cell>[Size.y];
        for (int y = 0; y < Size.y; y++)
        {
            BaseGrid[y] = new Wrapper<Cell>();
            BaseGrid[y].Values = new Cell[Size.x];
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




