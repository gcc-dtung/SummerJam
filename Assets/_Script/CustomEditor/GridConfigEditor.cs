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
        if (EditorGUI.EndChangeCheck())
        {
            ResizeGrid(config);
        }
        if (NeedsResize(config))
        {
            ResizeGrid(config);
        }
        GUILayout.Space(20);
        GUILayout.Label("Grid", EditorStyles.boldLabel);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };

        if (config.BaseGrid != null && config.BaseGrid.Length == config.Size.y)
        {
            for (int y = config.Size.y - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < config.Size.x; x++)
                {
                    CellType currentType = config.BaseGrid[y].Values[x];
                    
                    string btnText = "";
                    Color btnColor = Color.white;

                    switch (currentType)
                    {
                        case CellType.Block:
                            btnText = "X";
                            btnColor = Color.red;
                            break;
                        case CellType.Dish:
                            btnText = "D";
                            btnColor = new Color(0.6f, 0f, 0.8f); 
                            break;
                        case CellType.Seat:
                            btnText = "S";
                            btnColor = new Color(0.2f, 0f, 0.8f); 
                            break;
                    }

                    GUI.contentColor = btnColor;

                    if (GUILayout.Button(btnText, buttonStyle, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Undo.RecordObject(config, "Change Cell Type");
                        config.BaseGrid[y].Values[x] = (CellType)(((int)currentType + 1) % 3);
                        EditorUtility.SetDirty(config);
                    }
                    
                    GUI.contentColor = Color.white;
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
        
        Wrapper<CellType>[] newGrid = new Wrapper<CellType>[config.Size.y];

        for (int y = 0; y < config.Size.y; y++)
        {
            newGrid[y] = new Wrapper<CellType>();
            newGrid[y].Values = new CellType[config.Size.x];

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
