using System;
using UnityEngine;

public class GridManager : MonoBehaviour
{
   [SerializeField] private GridConfig config;
   public Grid<int> Grid;
   private void Awake()
   {
      Grid = new Grid<int>(config);
   }
   
   
}
