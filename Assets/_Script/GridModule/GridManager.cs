using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
   [SerializeField] private GridConfig boardConfig;
   [SerializeField] private GridConfig waitConfig;
   [SerializeField] private Transform holder;
   public Grid<Cell> Board { get; private set; }
   public Grid<Cell> WaitLine { get; private set; }

   protected override void Awake()
   {
      base.Awake();
      Board = new Grid<Cell>(boardConfig,InitializeBoard);
      WaitLine = new Grid<Cell>(waitConfig, InitializeWaitLine);
   }

   private void Start()
   {
      Board.MatrixTraversal(FillItemToBoard);
      WaitLine.MatrixTraversal(FillItemToWaitLine);
   }

   private Cell InitializeBoard(int x,int y)
   {
      Cell tmp = Instantiate<Cell>(boardConfig.BaseGrid[y].Values[x],holder);
      return tmp;
   }   
   
   private Cell InitializeWaitLine(int x,int y)
   {
      Cell tmp = Instantiate<Cell>(waitConfig.BaseGrid[y].Values[x],holder);
      return tmp;
   }

   private void FillItemToBoard(int x,int y,Cell item)
   {
       item.transform.position = Board.GetWorldPosition(x, y);
       item.transform.localScale = new Vector3(boardConfig.CellSize.x,boardConfig.CellSize.y,1);
   }   
   
   private void FillItemToWaitLine(int x,int y,Cell item)
   {
       item.transform.position = WaitLine.GetWorldPosition(x, y);
       item.transform.localScale = new Vector3(waitConfig.CellSize.x,waitConfig.CellSize.y,1);
   }
}

