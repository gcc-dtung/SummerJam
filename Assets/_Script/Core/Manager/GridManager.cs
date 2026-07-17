using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
   private GridConfig boardConfig;
   private GridConfig waitConfig;
   [SerializeField] private Cell Template;
   [SerializeField] private Transform holder;
   public Grid<Cell> Board { get; private set; }
   public Grid<Cell> WaitLine { get; private set; }

   private void OnEnable()
   {
       LevelManager.Instance.OnLevelConfigChange += UpdateGrid;
   }

   private void OnDisable()
   {
       if (LevelManager.Instance != null) LevelManager.Instance.OnLevelConfigChange -= UpdateGrid;
   }

    private void ClearGrid()
    {
        if (Board != null)
        {
            Board.MatrixTraversal((x, y, cell) =>
            {
                if (cell != null)
                {
                    if (cell.CurrentPerson != null)
                    {
                        Destroy(cell.CurrentPerson.gameObject);
                    }
                    Destroy(cell.gameObject);
                }
            });
            Board = null;
        }

        if (WaitLine != null)
        {
            WaitLine.MatrixTraversal((x, y, cell) =>
            {
                if (cell != null)
                {
                    if (cell.CurrentPerson != null)
                    {
                        Destroy(cell.CurrentPerson.gameObject);
                    }
                    Destroy(cell.gameObject);
                }
            });
            WaitLine = null;
        }
    }

    void UpdateGrid(LevelConfig config)
    {
        ClearGrid();
        boardConfig = config.BoardGrid;
        waitConfig = config.WaitLineGrid;
        Board = new Grid<Cell>(config.BoardGrid, InitializeBoard);
        WaitLine = new Grid<Cell>(waitConfig, InitializeWaitLine);
        Board.MatrixTraversal(FillItemToBoard);
        WaitLine.MatrixTraversal(FillItemToWaitLine);
        WaitLine.MatrixTraversal(FillPersonToWaitLine);
    }
   
   private Cell InitializeBoard(int x,int y)
   {
       Cell tmp = Instantiate<Cell>(Template,holder);
       tmp.Initialize(boardConfig.BaseGrid[y].Values[x]);
       tmp.SetGridIndex(x, y);
       return tmp;
   }   
   
   private Cell InitializeWaitLine(int x,int y)
   {
      Cell tmp = Instantiate<Cell>(Template,holder);
      tmp.Initialize(waitConfig.BaseGrid[y].Values[x]);
      InitializePerson(tmp,waitConfig.BaseGrid[y].Values[x]);
      tmp.SetGridIndex(x, y);
      return tmp;
   }
   
   private void FillItemToBoard(int x,int y,Cell cell)
   {
       cell.transform.position = Board.GetWorldPosition(x, y);
       cell.transform.localScale = new Vector3(boardConfig.CellSize.x,boardConfig.CellSize.y,1);
   }   
   
   private void FillItemToWaitLine(int x,int y,Cell cell)
   {
       cell.transform.position = WaitLine.GetWorldPosition(x, y);
       cell.transform.localScale = new Vector3(waitConfig.CellSize.x,waitConfig.CellSize.y,1);
   }

   private void FillPersonToWaitLine(int x, int y,Cell cell)
   {
       if(cell.Type != CellType.Seat || cell.CanSeat || cell.CurrentPerson == null) return;
       Person person = cell.CurrentPerson;
       person.transform.position = WaitLine.GetWorldPosition(x, y);
       person.transform.SetParent(cell.transform);
       person.transform.localScale = Vector3.one;
       person.SetOutSideState(true);
   }
   
   private void InitializePerson(Cell cell,CellDataSO data)
   {
        if(data.Type != CellType.Seat || !(data is Seat) ) return;
        Seat seat = data as Seat;
        if(seat.DefaultPerson == null) return;
        Person person = Instantiate<Person>(seat.DefaultPerson);
        cell.SetPersonToSeat(person);
   }
}

