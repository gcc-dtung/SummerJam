using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : Editor
{
    private bool _showBoardGridSettings = true;
    private bool _showWaitLineGridSettings = true;
    private bool _showDifficultyBreakdown = true;
    private LevelDifficultyCalculator.DifficultyResult _cachedDifficultyResult;

    private GridConfig _selectedCellGrid;
    private int _selectedCellX = -1;
    private int _selectedCellY = -1;
    private bool _selectedIsWaitLine;
    private string _hoveredCellTooltip;
    private string _activeTooltipForFrame;

    public override void OnInspectorGUI()
    {
        _activeTooltipForFrame = _hoveredCellTooltip;
        _hoveredCellTooltip = null;

        LevelConfig levelConfig = (LevelConfig)target;

        // Draw Level Difficulty Rating Section with Calculate Button
        DrawDifficultyHeaderSection(levelConfig);

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

            if (!string.IsNullOrEmpty(_activeTooltipForFrame))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox(_activeTooltipForFrame, MessageType.Info);
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
        CellDataSO blockedAsset = AssetDatabase.LoadAssetAtPath<CellDataSO>("Assets/Data/Cell/New Blocked.asset");

        for (int y = 0; y < config.Size.y; y++)
        {
            newGrid[y] = new Wrapper<CellDataSO>();
            newGrid[y].Values = new CellDataSO[config.Size.x];

            for (int x = 0; x < config.Size.x; x++)
            {
                if (config.BaseGrid != null && y < config.BaseGrid.Length && config.BaseGrid[y] != null && config.BaseGrid[y].Values != null && x < config.BaseGrid[y].Values.Length)
                {
                    newGrid[y].Values[x] = config.BaseGrid[y].Values[x];
                }
                else
                {
                    newGrid[y].Values[x] = blockedAsset;
                }
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
                                    if (currentCell.name == "New Dishes")
                                    {
                                        cellText = ""; // chỉ tô màu chứ không hiện chữ
                                    }
                                    else
                                    {
                                        cellText = currentCell.name.Substring(0, Mathf.Min(4, currentCell.name.Length));
                                    }
                                    cellColor = new Color(0.6f, 0f, 0.8f); // Purple
                                    break;
                                case CellType.Seat:
                                    cellText = "Seat";
                                    cellColor = new Color(0.2f, 0.4f, 0.8f); // Blue
                                    break;
                            }
                        }
                    }

                    bool isSelected = (_selectedCellGrid == grid && _selectedCellX == x && _selectedCellY == y);
                    if (isSelected)
                    {
                        cellColor = new Color(1.0f, 0.85f, 0.2f); // Gold highlight
                    }

                    GUI.backgroundColor = cellColor;
                    GUIContent cellContent = new GUIContent(cellText, GetCellTooltipText(currentCell));

                    if (GUILayout.Button(cellContent, cellStyle, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        _selectedCellGrid = grid;
                        _selectedCellX = x;
                        _selectedCellY = y;
                        _selectedIsWaitLine = isWaitLine;

                        ShowCellContextMenu(grid, x, y, isWaitLine);
                    }

                    Rect btnRect = GUILayoutUtility.GetLastRect();
                    if (btnRect.Contains(Event.current.mousePosition))
                    {
                        _hoveredCellTooltip = cellContent.tooltip;
                        if (_hoveredCellTooltip != _activeTooltipForFrame)
                        {
                            Repaint();
                        }
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
        CellDataSO blockedAsset = AssetDatabase.LoadAssetAtPath<CellDataSO>("Assets/Data/Cell/New Blocked.asset");

        // Option 1: Clear (trở về trạng thái Blocked)
        menu.AddItem(new GUIContent("Clear"), grid.BaseGrid[y].Values[x] == blockedAsset, () =>
        {
            Undo.RecordObject(grid, "Clear Cell to Blocked");
            grid.BaseGrid[y].Values[x] = blockedAsset;
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

            // Abstract Dish
            CellDataSO newDishes = AssetDatabase.LoadAssetAtPath<CellDataSO>("Assets/Data/Cell/New Dishes.asset");
            if (newDishes != null)
            {
                menu.AddItem(new GUIContent("Món ăn (Trừu tượng)"), grid.BaseGrid[y].Values[x] == newDishes, () =>
                {
                    Undo.RecordObject(grid, "Set Abstract Dish");
                    grid.BaseGrid[y].Values[x] = newDishes;
                    EditorUtility.SetDirty(grid);
                });
            }

            // Dishes sub-menu
            string[] dishGuids = AssetDatabase.FindAssets("t:Dishes");
            foreach (string guid in dishGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CellDataSO dishAsset = AssetDatabase.LoadAssetAtPath<CellDataSO>(path);
                if (dishAsset != null && dishAsset != newDishes)
                {
                    string displayName = $"Món ăn chi tiết/{dishAsset.name}";
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

    private void DrawDifficultyHeaderSection(LevelConfig levelConfig)
    {
        EditorGUILayout.Space(5);
        GUILayout.Label("Level Difficulty", EditorStyles.boldLabel);

        string buttonText = _cachedDifficultyResult == null ? "Calculate Difficulty" : "Recalculate Difficulty";
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            fixedHeight = 30
        };

        if (GUILayout.Button(buttonText, buttonStyle))
        {
            _cachedDifficultyResult = LevelDifficultyCalculator.Calculate(levelConfig);
        }

        if (_cachedDifficultyResult == null)
        {
            EditorGUILayout.HelpBox("Bấm nút 'Calculate Difficulty' ở trên để tính toán điểm độ khó cho Level này.", MessageType.Info);
            EditorGUILayout.Space(5);
            return;
        }

        LevelDifficultyCalculator.DifficultyResult diff = _cachedDifficultyResult;

        // Header Card Box
        Color defaultBg = GUI.backgroundColor;
        GUI.backgroundColor = diff.CategoryColor * 0.7f + Color.white * 0.3f;

        GUIStyle cardStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(12, 12, 10, 10),
            margin = new RectOffset(0, 0, 5, 10)
        };

        EditorGUILayout.BeginVertical(cardStyle);
        GUI.backgroundColor = defaultBg;

        // Title Row
        EditorGUILayout.BeginHorizontal();
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleLeft
        };

        string titleText = $"Level Difficulty: {diff.TotalScore:F1} / 100 — [{diff.Category}]";
        EditorGUILayout.LabelField(titleText, titleStyle);

        if (diff.IsApproximate)
        {
            GUILayout.Label("(Ước lượng)", EditorStyles.miniBoldLabel, GUILayout.Width(70));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        // Prominent Solvability & Minimum Moves Status Banner
        if (diff.IsSolvable)
        {
            EditorGUILayout.HelpBox(
                $"🎯 KẾT QUẢ KIỂM TRA: CÓ THỂ VƯỢT QUA LEVEL (WINNABLE)\n" +
                $"• Số nước đi tối thiểu để thắng (M_opt): {diff.M_opt} moves\n" +
                $"• Giới hạn nước đi hiện tại (MoveLimit): {diff.MoveLimit} moves\n" +
                $"• Số nước đi dư (Slack): +{diff.Slack} moves",
                MessageType.Info);
        }
        else
        {
            string reason = diff.M_opt >= 999 
                ? "Số ghế trên bàn ít hơn số lượng khách hàng!" 
                : $"Số lượt di chuyển quá ít (Thiếu {-diff.Slack} moves)";
            EditorGUILayout.HelpBox(
                $"⛔ KẾT QUẢ KIỂM TRA: KHÔNG THỂ VƯỢT QUA LEVEL (UNSOLVABLE)\n" +
                $"• Lý do: {reason}\n" +
                $"• Số nước đi tối thiểu cần thiết (M_opt): {(diff.M_opt >= 999 ? "Không khả thi" : diff.M_opt + " moves")}\n" +
                $"• Giới hạn nước đi hiện tại (MoveLimit): {diff.MoveLimit} moves",
                MessageType.Error);
        }

        EditorGUILayout.Space(4);

        // Breakdown Section
        GUILayout.Label("Chi tiết phân tích độ khó (5 thành phần)", EditorStyles.boldLabel);

        float score1 = diff.X1_MoveTightness * 30.0f;
        float score2 = diff.X2_ConditionComplexity * 20.0f;
        float score3 = diff.X3_ConstraintConflict * 30.0f;
        float score4 = diff.X4_GridConstraint * 10.0f;
        float score5 = diff.X5_ResourceScarcity * 10.0f;

        DrawComponentBar($"1. Move Tightness (X1 = {diff.X1_MoveTightness:F2}) — Điểm: {score1:F1} / 30.0", diff.X1_MoveTightness, $"Moves cần (M_opt): {diff.M_opt} | MoveLimit: {diff.MoveLimit} | Dư địa (Slack): {diff.Slack} | Nước sửa (N_correction): {diff.N_correction}");
        DrawComponentBar($"2. Condition Complexity (X2 = {diff.X2_ConditionComplexity:F2}) — Điểm: {score2:F1} / 20.0", diff.X2_ConditionComplexity, $"Phức tạp điều kiện TB (C_avg): {diff.C_avg:F2} / 5.0 (C_MAX) của {diff.N_person} khách hàng");
        DrawComponentBar($"3. Constraint Conflict (X3 = {diff.X3_ConstraintConflict:F2}) — Điểm: {score3:F1} / 30.0", diff.X3_ConstraintConflict, $"Cặp xung đột: {diff.ConflictPairs} | Tranh chấp vị trí: {diff.SharedContention} | Độ sâu chuỗi phụ thuộc: {diff.ChainDepth}");
        DrawComponentBar($"4. Grid Constraint (X4 = {diff.X4_GridConstraint:F2}) — Điểm: {score4:F1} / 10.0", diff.X4_GridConstraint, $"Tỷ lệ ô chặn: {(diff.BlockedRatio * 100):F1}% | Lân cận TB: {diff.AvgNeighbor:F1} / 8.0 | Ghế bị chật: {diff.TightSeatCount} / {diff.N_person}");
        DrawComponentBar($"5. Resource Scarcity (X5 = {diff.X5_ResourceScarcity:F2}) — Điểm: {score5:F1} / 10.0", diff.X5_ResourceScarcity, $"Độ khan hiếm tài nguyên món ăn & nhân vật: {(diff.X5_ResourceScarcity * 100):F0}%");

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private string GetCellTooltipText(CellDataSO cell)
    {
        if (cell == null) return "Empty Cell";

        if (cell is Seat seat)
        {
            if (seat.DefaultPerson != null)
            {
                Person personPrefab = seat.DefaultPerson;
                SerializedObject personSo = new SerializedObject(personPrefab);
                PersonDataSO data = personSo.FindProperty("data")?.objectReferenceValue as PersonDataSO;
                ConditionsSO conds = personSo.FindProperty("conditions")?.objectReferenceValue as ConditionsSO;

                string personName = data != null && !string.IsNullOrEmpty(data.Name) ? data.Name : personPrefab.name;
                List<string> lines = new List<string>();
                lines.Add($"Person: {personName} (ID: {(data != null ? data.ID : 0)})");

                if (data != null && data.Trait != null && data.Trait.Count > 0)
                {
                    lines.Add($"Traits: {string.Join(", ", data.Trait.Select(t => t.ToString()))}");
                }

                lines.Add("----------------------------------------");
                lines.Add("Conditions:");

                if (conds == null)
                {
                    lines.Add("• Không có điều kiện (Luôn luôn Happy)");
                }
                else
                {
                    List<SingleConditionsSO> singleConditions = FlattenSingleConditions(conds);
                    if (singleConditions.Count == 0)
                    {
                        lines.Add("• Không có điều kiện (Luôn luôn Happy)");
                    }
                    else
                    {
                        foreach (var sc in singleConditions)
                        {
                            if (sc == null) continue;
                            string desc = !string.IsNullOrEmpty(sc.Description) ? sc.Description : $"{sc.Scope} {sc.FilterTarget} {sc.Comparator} {sc.Value}";
                            lines.Add($"• {desc}");
                        }
                    }
                }

                return string.Join("\n", lines);
            }
            else
            {
                return $"Seat: {seat.name}\nStatus: Ghế trống (Chưa xếp người)";
            }
        }
        else if (cell is Dishes dish)
        {
            List<string> lines = new List<string>();
            string dishName = !string.IsNullOrEmpty(dish.Name) ? dish.Name : dish.name;
            lines.Add($"Dish: {dishName}");
            lines.Add("----------------------------------------");
            if (dish.Tags != null && dish.Tags.Count > 0)
            {
                lines.Add($"Food Tags: {string.Join(", ", dish.Tags.Select(t => t.ToString()))}");
            }
            else
            {
                lines.Add("Food Tags: (Chưa có Tag)");
            }
            return string.Join("\n", lines);
        }

        return $"Cell: {cell.name}\nType: {cell.Type}";
    }

    private void DrawComponentBar(string label, float scoreFactor, string detail)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(false, 16);
        EditorGUI.ProgressBar(rect, scoreFactor, $"{(scoreFactor * 100):F0}%");

        if (!string.IsNullOrEmpty(detail))
        {
            EditorGUILayout.LabelField(detail, EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectedPersonDetailsPanel()
    {
        if (_selectedCellGrid == null || _selectedCellGrid.BaseGrid == null) return;
        if (_selectedCellY < 0 || _selectedCellY >= _selectedCellGrid.Size.y) return;
        if (_selectedCellX < 0 || _selectedCellX >= _selectedCellGrid.Size.x) return;

        CellDataSO cell = _selectedCellGrid.BaseGrid[_selectedCellY].Values[_selectedCellX];
        if (cell == null) return;

        EditorGUILayout.Space(15);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        string gridTypeName = _selectedIsWaitLine ? "Wait Line Grid" : "Main Board Grid";
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Selected Cell Details [{gridTypeName} ({_selectedCellX}, {_selectedCellY})]", EditorStyles.boldLabel);

        if (GUILayout.Button("Close", GUILayout.Width(60)))
        {
            _selectedCellGrid = null;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (cell is Seat seat && seat.DefaultPerson != null)
        {
            Person personPrefab = seat.DefaultPerson;
            SerializedObject personSo = new SerializedObject(personPrefab);
            PersonDataSO data = personSo.FindProperty("data")?.objectReferenceValue as PersonDataSO;
            ConditionsSO conds = personSo.FindProperty("conditions")?.objectReferenceValue as ConditionsSO;

            string personName = data != null && !string.IsNullOrEmpty(data.Name) ? data.Name : personPrefab.name;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Person: {personName}", EditorStyles.boldLabel);
            if (GUILayout.Button("Ping Person Asset", GUILayout.Width(130)))
            {
                EditorGUIUtility.PingObject(personPrefab);
            }
            EditorGUILayout.EndHorizontal();

            if (data != null && data.Trait != null && data.Trait.Count > 0)
            {
                string traitsText = string.Join(", ", data.Trait.Select(t => t.ToString()));
                EditorGUILayout.LabelField($"Traits: {traitsText}", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Conditions List:", EditorStyles.boldLabel);

            if (conds == null)
            {
                EditorGUILayout.HelpBox("Person này không có điều kiện nào (Luôn luôn Happy).", MessageType.Info);
            }
            else
            {
                List<SingleConditionsSO> singleConditions = FlattenSingleConditions(conds);
                if (singleConditions.Count == 0)
                {
                    EditorGUILayout.HelpBox("Person này không có điều kiện đơn lẻ nào (Luôn luôn Happy).", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < singleConditions.Count; i++)
                    {
                        SingleConditionsSO sc = singleConditions[i];
                        if (sc == null) continue;

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        string desc = !string.IsNullOrEmpty(sc.Description) ? sc.Description : "(No description text)";
                        EditorGUILayout.LabelField($"• {desc}", EditorStyles.wordWrappedLabel);

                        string targetDetails = sc.FilterTarget.ToString();
                        if (sc.FilterTarget == Target.Person && !string.IsNullOrEmpty(sc.Filter.Name))
                        {
                            targetDetails = $"Person ({sc.Filter.Name})";
                        }
                        else if (sc.FilterTarget == Target.Dish && sc.Filter.FoodTags != null && sc.Filter.FoodTags.Count > 0)
                        {
                            targetDetails = $"Dish [{string.Join(", ", sc.Filter.FoodTags.Select(f => f.ToString()))}]";
                        }

                        string techDetail = $"[Scope: {sc.Scope} | Target: {targetDetails} | Comparator: {sc.Comparator} {sc.Value}]";
                        EditorGUILayout.LabelField(techDetail, EditorStyles.miniLabel);

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField($"Cell Name: {cell.name}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {cell.Type} | CanSeat: {cell.DefaultCanSeat} | CanInteract: {cell.DefaultCanInteract}", EditorStyles.miniLabel);
            if (GUILayout.Button("Ping Cell Asset", GUILayout.Width(130)))
            {
                EditorGUIUtility.PingObject(cell);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private List<SingleConditionsSO> FlattenSingleConditions(ConditionsSO cond)
    {
        List<SingleConditionsSO> list = new List<SingleConditionsSO>();
        if (cond is SingleConditionsSO single)
        {
            list.Add(single);
        }
        else if (cond is CompositeConditionsSO composite && composite.SubConditions != null)
        {
            foreach (var sub in composite.SubConditions)
            {
                if (sub != null) list.AddRange(FlattenSingleConditions(sub));
            }
        }
        return list;
    }
}
