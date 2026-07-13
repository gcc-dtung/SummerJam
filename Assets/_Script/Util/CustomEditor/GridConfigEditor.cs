using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridConfig))] 
public class GridConfigEditor : Editor
{
   public override void OnInspectorGUI()
    {
        GridConfig config = (GridConfig)target;
        
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck() || NeedsResize(config))
        {
            ResizeGrid(config);
        }
        
        GUILayout.Space(20);
        GUILayout.Label("Grid Preview", EditorStyles.boldLabel);

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        if (config.BaseGrid != null && config.BaseGrid.Length == config.Size.y)
        {
            for (int y = config.Size.y - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < config.Size.x; x++)
                {
                    CellDataSO currentCell = config.BaseGrid[y].Values[x];
                    
                    string boxText = ".";
                    Color boxColor = Color.gray; 
                    if (currentCell != null)
                    {
                        switch (currentCell.Type)
                        {
                            case CellType.Block:
                                boxText = "X";
                                boxColor = Color.red;
                                break;
                            case CellType.Dish:
                                boxText = "D";
                                boxColor = new Color(0.6f, 0f, 0.8f);
                                break;
                            case CellType.Seat:
                                boxText = "S";
                                boxColor = new Color(0.2f, 0f, 0.8f);
                                break;
                        }
                    }

                    GUI.backgroundColor = boxColor;
                    GUILayout.Box(boxText, boxStyle, GUILayout.Width(40), GUILayout.Height(40));
                    
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("Reset Board"))
        {
            Undo.RecordObject(config, "Reset Board");
            config.ResetGrid(); 
            EditorUtility.SetDirty(config);
        }
    }

    private bool NeedsResize(GridConfig config)
    {
        if (config.BaseGrid == null || config.BaseGrid.Length != config.Size.y) return true;
        
        for (int y = 0; y < config.Size.y; y++)
        {
            if (config.BaseGrid[y] == null || config.BaseGrid[y].Values == null || config.BaseGrid[y].Values.Length != config.Size.x)
            {
                return true;
            }
        }
        return false;
    }

    private void ResizeGrid(GridConfig config)
    {
        Undo.RecordObject(config, "Resize Grid");
        
        Wrapper<CellDataSO>[] newGrid = new Wrapper<CellDataSO>[config.Size.y];

        for (int y = 0; y < config.Size.y; y++)
        {
            newGrid[y] = new Wrapper<CellDataSO>();
            newGrid[y].Values = new CellDataSO[config.Size.x];

            if (config.BaseGrid != null && y < config.BaseGrid.Length && config.BaseGrid[y] != null && config.BaseGrid[y].Values != null)
            {
                int copyLength = Mathf.Min(config.Size.x, config.BaseGrid[y].Values.Length);
                System.Array.Copy(config.BaseGrid[y].Values, newGrid[y].Values, copyLength);
            }
        }

        config.BaseGrid = newGrid;
        EditorUtility.SetDirty(config);
    }
}
