using System;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
   [SerializeField] private GridConfig config;
   [SerializeField] private List<Test> Prefab;
   private Dictionary<CellType, GameObject> prefabDic = new Dictionary<CellType, GameObject>();
   public Grid<CellType> Grid;
   
   private void Awake()
   {
      foreach (var it in Prefab)
      {
            if(!prefabDic.ContainsKey(it.type)) prefabDic.Add(it.type,it.gameObject);
      }

      Grid = new Grid<CellType>(config,GetValuesInGrid);
   }

   private void Start()
   {
       Grid.MatrixTraversal(FillItemToGrid);
   }

   private void FillItemToGrid(int x,int y,CellType type)
   {
       GameObject obj = Instantiate(prefabDic[type]);
       obj.transform.position = Grid.GetWorldPosition(x, y);
   }


   private CellType GetValuesInGrid(int x,int y)
   {
       return config.BaseGrid[y].Values[x];
   }
}

// test script
