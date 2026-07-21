using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : Editor
{
    private bool _showBoardGridSettings = true;
    private bool _showWaitLineGridSettings = true;

    public override void OnInspectorGUI()
    {
        LevelConfig levelConfig = (LevelConfig)target;

        // Update serialized level config
        serializedObject.Update();

        SerializedProperty idProp = serializedObject.FindProperty("<ID>k__BackingField");
        SerializedProperty moveLimitProp = serializedObject.FindProperty("<MoveLimit>k__BackingField");
        SerializedProperty boardGridProp = serializedObject.FindProperty("<BoardGrid>k__BackingField");
        SerializedProperty waitLineGridProp = serializedObject.FindProperty("<WaitLineGrid>k__BackingField");

        GUILayout.Label("Level General Config", EditorStyles.boldLabel);
        if (idProp != null) EditorGUILayout.PropertyField(idProp);
        if (moveLimitProp != null) EditorGUILayout.PropertyField(moveLimitProp);
        
        if (boardGridProp != null) EditorGUILayout.PropertyField(boardGridProp);
        if (waitLineGridProp != null) EditorGUILayout.PropertyField(waitLineGridProp);

        serializedObject.ApplyModifiedProperties();

        GridConfig boardGrid = boardGridProp != null ? boardGridProp.objectReferenceValue as GridConfig : null;
        GridConfig waitLineGrid = waitLineGridProp != null ? waitLineGridProp.objectReferenceValue as GridConfig : null;

        if (boardGrid == null || waitLineGrid == null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This level is missing its Board Grid or Wait Line Grid references.", MessageType.Warning);
            if (GUILayout.Button("Generate & Link Missing Grid Configs", GUILayout.Height(30)))
            {
                GenerateAndLinkGrids(levelConfig, boardGridProp, waitLineGridProp);
                boardGrid = boardGridProp.objectReferenceValue as GridConfig;
                waitLineGrid = waitLineGridProp.objectReferenceValue as GridConfig;
            }
        }

        // 1. Board Grid Settings
        if (boardGrid != null)
        {
            GUILayout.Space(15);
            _showBoardGridSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showBoardGridSettings, "Board Grid Settings");
            if (_showBoardGridSettings)
            {
                EditorGUI.indentLevel++;
                DrawGridInspectorFields("Board Grid Properties", boardGrid);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // 2. Wait Line Grid Settings
        if (waitLineGrid != null)
        {
            GUILayout.Space(15);
            _showWaitLineGridSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showWaitLineGridSettings, "Wait Line Grid Settings");
            if (_showWaitLineGridSettings)
            {
                EditorGUI.indentLevel++;
                DrawGridInspectorFields("Wait Line Grid Properties", waitLineGrid);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // 3. Screen Simulation
        if (boardGrid != null || waitLineGrid != null)
        {
            GUILayout.Space(25);
            GUILayout.Label("--- Screen Simulation ---", EditorStyles.centeredGreyMiniLabel);
            
            if (boardGrid != null)
            {
                DrawGridPreview("Main Board Grid (Click cell to configure)", boardGrid, false);
            }

            if (waitLineGrid != null)
            {
                DrawGridPreview("Wait Line Grid (Click cell to configure)", waitLineGrid, true);
            }
        }
    }

    private void DrawGridInspectorFields(string header, GridConfig grid)
    {
        SerializedObject gridObj = new SerializedObject(grid);
        gridObj.Update();

        SerializedProperty sizeProp = gridObj.FindProperty("<Size>k__BackingField");
        SerializedProperty cellSizeProp = gridObj.FindProperty("originalCellSize");
        SerializedProperty cellDistProp = gridObj.FindProperty("originalCellDistance");
        SerializedProperty posXProp = gridObj.FindProperty("PosX");
        SerializedProperty posYProp = gridObj.FindProperty("PosY");

        if (sizeProp != null && cellSizeProp != null && cellDistProp != null && posXProp != null && posYProp != null)
        {
            EditorGUI.BeginChangeCheck();

            Vector2Int sizeVal = EditorGUILayout.Vector2IntField("Grid Size (X, Y)", sizeProp.vector2IntValue);
            
            // Limit minimum size to 1x1
            if (sizeVal.x < 1) sizeVal.x = 1;
            if (sizeVal.y < 1) sizeVal.y = 1;

            Vector2 cellSizeVal = EditorGUILayout.Vector2Field("Cell Size", cellSizeProp.vector2Value);
            Vector2 cellDistVal = EditorGUILayout.Vector2Field("Cell Spacing", cellDistProp.vector2Value);

            float posXVal = EditorGUILayout.Slider("Pos X (Viewport)", posXProp.floatValue, 0f, 1f);
            float posYVal = EditorGUILayout.Slider("Pos Y (Viewport)", posYProp.floatValue, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(grid, "Modify Grid Properties");

                bool sizeChanged = (sizeVal != sizeProp.vector2IntValue);
                sizeProp.vector2IntValue = sizeVal;
                cellSizeProp.vector2Value = cellSizeVal;
                cellDistProp.vector2Value = cellDistVal;
                posXProp.floatValue = posXVal;
                posYProp.floatValue = posYVal;

                gridObj.ApplyModifiedProperties();

                if (sizeChanged || NeedsResize(grid))
                {
                    ResizeGrid(grid);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Failed to find some serialized properties in GridConfig.", MessageType.Error);
        }
    }

    private bool NeedsResize(GridConfig config)
    {
        if (config == null) return false;
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
        if (config == null) return;
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

    private void DrawGridPreview(string label, GridConfig grid, bool isWaitLine)
    {
        GUILayout.Space(15);
        GUILayout.Label(label, EditorStyles.boldLabel);

        GUIStyle cellStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        if (grid.BaseGrid != null && grid.BaseGrid.Length == grid.Size.y)
        {
            for (int y = grid.Size.y - 1; y >= 0; y--)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int x = 0; x < grid.Size.x; x++)
                {
                    CellDataSO currentCell = grid.BaseGrid[y].Values[x];

                    string cellText = ".";
                    Color cellColor = Color.gray;

                    if (currentCell != null)
                    {
                        if (isWaitLine)
                        {
                            cellText = currentCell.name.Replace("Seat", "");
                            cellColor = new Color(0f, 0.6f, 0.5f); // Teal
                        }
                        else
                        {
                            switch (currentCell.Type)
                            {
                                case CellType.Block:
                                    cellText = "X";
                                    cellColor = Color.red;
                                    break;
                                case CellType.Dish:
                                    cellText = currentCell.name.Substring(0, Mathf.Min(4, currentCell.name.Length));
                                    cellColor = new Color(0.6f, 0f, 0.8f); // Purple
                                    break;
                                case CellType.Seat:
                                    cellText = "Seat";
                                    cellColor = new Color(0.2f, 0.4f, 0.8f); // Blue
                                    break;
                            }
                        }
                    }

                    GUI.backgroundColor = cellColor;
                    GUIContent cellContent = new GUIContent(cellText, currentCell != null ? currentCell.name : "Empty");

                    if (GUILayout.Button(cellContent, cellStyle, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        ShowCellContextMenu(grid, x, y, isWaitLine);
                    }

                    GUI.backgroundColor = Color.white;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Grid sizes mismatched or not initialized. Modify grid size above to initialize.", MessageType.Info);
        }
    }

    private void ShowCellContextMenu(GridConfig grid, int x, int y, bool isWaitLine)
    {
        GenericMenu menu = new GenericMenu();

        // Option 1: Empty
        menu.AddItem(new GUIContent("Empty / Clear"), grid.BaseGrid[y].Values[x] == null, () =>
        {
            Undo.RecordObject(grid, "Set Cell Empty");
            grid.BaseGrid[y].Values[x] = null;
            EditorUtility.SetDirty(grid);
        });

        if (!isWaitLine)
        {
            // Normal Seat
            CellDataSO normalSeat = AssetDatabase.LoadAssetAtPath<CellDataSO>("Assets/Data/Cell/NozmalSeat.asset");
            if (normalSeat != null)
            {
                menu.AddItem(new GUIContent("Chỗ ngồi thường"), grid.BaseGrid[y].Values[x] == normalSeat, () =>
                {
                    Undo.RecordObject(grid, "Set Normal Seat");
                    grid.BaseGrid[y].Values[x] = normalSeat;
                    EditorUtility.SetDirty(grid);
                });
            }

            // Blocked
            CellDataSO blocked = AssetDatabase.LoadAssetAtPath<CellDataSO>("Assets/Data/Cell/New Blocked.asset");
            if (blocked != null)
            {
                menu.AddItem(new GUIContent("Blocked"), grid.BaseGrid[y].Values[x] == blocked, () =>
                {
                    Undo.RecordObject(grid, "Set Blocked");
                    grid.BaseGrid[y].Values[x] = blocked;
                    EditorUtility.SetDirty(grid);
                });
            }

            // Dishes sub-menu
            string[] dishGuids = AssetDatabase.FindAssets("t:Dishes");
            foreach (string guid in dishGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CellDataSO dishAsset = AssetDatabase.LoadAssetAtPath<CellDataSO>(path);
                if (dishAsset != null)
                {
                    string displayName = $"Các món ăn/{dishAsset.name}";
                    menu.AddItem(new GUIContent(displayName), grid.BaseGrid[y].Values[x] == dishAsset, () =>
                    {
                        Undo.RecordObject(grid, "Set Dish");
                        grid.BaseGrid[y].Values[x] = dishAsset;
                        EditorUtility.SetDirty(grid);
                    });
                }
            }
        }
        else
        {
            // WaitLine options - Person Seats
            string[] seatGuids = AssetDatabase.FindAssets("t:Seat");
            foreach (string guid in seatGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("WaitLineSeat") || path.Contains("SeatInWaitLine"))
                {
                    CellDataSO seatAsset = AssetDatabase.LoadAssetAtPath<CellDataSO>(path);
                    if (seatAsset != null)
                    {
                        string displayName = $"Person/{seatAsset.name}";
                        menu.AddItem(new GUIContent(displayName), grid.BaseGrid[y].Values[x] == seatAsset, () =>
                        {
                            Undo.RecordObject(grid, "Set WaitLine Seat");
                            grid.BaseGrid[y].Values[x] = seatAsset;
                            EditorUtility.SetDirty(grid);
                        });
                    }
                }
            }
        }

        menu.ShowAsContext();
    }

    private void GenerateAndLinkGrids(LevelConfig levelConfig, SerializedProperty boardGridProp, SerializedProperty waitLineGridProp)
    {
        string levelPath = AssetDatabase.GetAssetPath(levelConfig);
        string levelName = Path.GetFileNameWithoutExtension(levelPath);
        
        string levelSuffix = levelName;
        if (levelName.StartsWith("Level"))
        {
            levelSuffix = levelName.Substring("Level".Length);
        }
        if (string.IsNullOrEmpty(levelSuffix))
        {
            levelSuffix = levelName;
        }

        string parentFolder = "Assets/Data/GridData";
        string targetFolder = $"{parentFolder}/{levelName}";

        // Ensure target folder exists
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Data"))
                {
                    AssetDatabase.CreateFolder("Assets", "Data");
                }
                AssetDatabase.CreateFolder("Assets/Data", "GridData");
            }
            AssetDatabase.CreateFolder(parentFolder, levelName);
        }

        // Generate BoardGrid if missing
        if (boardGridProp.objectReferenceValue == null)
        {
            string boardPath = $"{targetFolder}/Layout{levelSuffix}.asset";
            GridConfig newBoard = ScriptableObject.CreateInstance<GridConfig>();
            AssetDatabase.CreateAsset(newBoard, boardPath);
            
            SerializedObject boardObj = new SerializedObject(newBoard);
            boardObj.Update();
            SerializedProperty sizeProp = boardObj.FindProperty("<Size>k__BackingField");
            if (sizeProp != null)
            {
                sizeProp.vector2IntValue = new Vector2Int(5, 5);
            }
            boardObj.ApplyModifiedProperties();
            newBoard.ResetGrid();

            boardGridProp.objectReferenceValue = newBoard;
        }

        // Generate WaitLineGrid if missing
        if (waitLineGridProp.objectReferenceValue == null)
        {
            string waitLinePath = $"{targetFolder}/WaitLV{levelSuffix}.asset";
            GridConfig newWaitLine = ScriptableObject.CreateInstance<GridConfig>();
            AssetDatabase.CreateAsset(newWaitLine, waitLinePath);
            
            SerializedObject waitLineObj = new SerializedObject(newWaitLine);
            waitLineObj.Update();
            SerializedProperty sizeProp = waitLineObj.FindProperty("<Size>k__BackingField");
            if (sizeProp != null)
            {
                sizeProp.vector2IntValue = new Vector2Int(5, 1);
            }
            waitLineObj.ApplyModifiedProperties();
            newWaitLine.ResetGrid();

            waitLineGridProp.objectReferenceValue = newWaitLine;
        }

        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
