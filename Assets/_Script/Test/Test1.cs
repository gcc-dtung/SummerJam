using System;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    [SerializeField] private GridConfig config;
    private Grid<int> grid;
    private Camera mainCam;
    private void Awake()
    {
        grid = new Grid<int>(config, (int x,int y) => 1);
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int x, y;
            Vector2 mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            if (grid.TryGetCellFromWorldPos(mousePosition, out x, out y))
            {
                Debug.Log($"{mousePosition} {x} {y}");
            }

        }
        
    }
}
