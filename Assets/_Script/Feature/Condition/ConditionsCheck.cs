using System;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsCheck : MonoBehaviour
{
    private List<Cell> cellHolder = new List<Cell>();
    List<Cell> adjacency = new List<Cell>();
    
    private List<Vector2Int> eightDirection = new List<Vector2Int>()
    {
        new Vector2Int(0, 1), // up
        new Vector2Int(1, 0), // right
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 1), // top right
        new Vector2Int(1, -1), // down right
        new Vector2Int(-1, -1), // down left
        new Vector2Int(-1, 1) // top left
    };

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.StopDragPerson,Check);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.StopDragPerson,Check);
    }


    public void Check()
    {
        GetPerson();
        for (int i = 0; i < cellHolder.Count; i++)
        {
           adjacency.Clear();
            foreach (Vector2Int direction in eightDirection)
            {
                Vector2Int tmp = new Vector2Int(cellHolder[i].X + direction.x, cellHolder[i].Y + direction.y);
                if (GridManager.Instance.Board.IsOnRange(tmp.x, tmp.y))
                {
                    adjacency.Add(GridManager.Instance.Board.GetValue(tmp.x, tmp.y));
                }
            }
            cellHolder[i].CurrentPerson.CheckConditions(cellHolder[i],adjacency);
        }
        EventBus.Notify(GameEventType.Checking);
    }

    private void GetPerson()
    {
        cellHolder.Clear();
        GridManager.Instance.Board.MatrixTraversal(GetPerson);
    }
    
    private void GetPerson(int x, int y, Cell cell)
    {
        if (cell.Type != CellType.Seat) return;
        if (cell.CurrentPerson != null)
        {
            cellHolder.Add(cell);
        }
    }
}