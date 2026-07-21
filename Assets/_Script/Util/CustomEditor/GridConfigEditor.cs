using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridConfig))] 
public class GridConfigEditor : Editor
{
    private List<CellDataSO> _cellAssets;
    private int _selectedBrushIndex = -1; // -1 represents Eraser (Null)

    private void LoadCellAssets()
    {
        if (_cellAssets != null) return;
        
        _cellAssets = new List<CellDataSO>();
        string[] guids = AssetDatabase.FindAssets("t:CellDataSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CellDataSO asset = AssetDatabase.LoadAssetAtPath<CellDataSO>(path);
            if (asset != null)
            {
                _cellAssets.Add(asset);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        GridConfig config = (GridConfig)target;
        
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck() || NeedsResize(config))
        {
            ResizeGrid(config);
        }
        
        LoadCellAssets();

        GUILayout.Space(15);
        GUILayout.Label("Active Paint Brush", EditorStyles.boldLabel);

        int count = _cellAssets.Count;
        string[] brushNames = new string[count + 1];
        brushNames[0] = "[Eraser] Clear";
        for (int i = 0; i < count; i++)
        {
            brushNames[i + 1] = _cellAssets[i].name;
        }

        string selectedName = "Eraser";
        if (_selectedBrushIndex >= 0 && _selectedBrushIndex < count)
        {
            selectedName = _cellAssets[_selectedBrushIndex].name;
        }
        EditorGUILayout.HelpBox($"Selected Brush: {selectedName}", MessageType.Info);

        int currentSelection = _selectedBrushIndex + 1;
        int newSelection = GUILayout.SelectionGrid(currentSelection, brushNames, 3);
        _selectedBrushIndex = newSelection - 1;

        if (GUILayout.Button("Refresh Brush List"))
        {
            _cellAssets = null;
        }

        GUILayout.Space(20);
        GUILayout.Label("Grid Preview (Click cell to paint)", EditorStyles.boldLabel);

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
                    GUIContent cellContent = new GUIContent(boxText, currentCell != null ? currentCell.name : "Empty");
                    
                    if (GUILayout.Button(cellContent, boxStyle, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        Undo.RecordObject(config, "Paint Grid Cell");
                        
                        CellDataSO activeBrush = null;
                        if (_selectedBrushIndex >= 0 && _selectedBrushIndex < _cellAssets.Count)
                        {
                            activeBrush = _cellAssets[_selectedBrushIndex];
                        }
                        
                        config.BaseGrid[y].Values[x] = activeBrush;
                        EditorUtility.SetDirty(config);
                    }
                    
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
