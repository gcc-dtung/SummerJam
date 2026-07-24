using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelSelectorWindow : EditorWindow
{
    private int targetLevel1Based = 1;
    private int targetMoveCount = 1;
    private int targetUndoCount = 1;
    private int targetRemoveCount = 1;
    private bool initialized = false;

    [MenuItem("Tools/Level Selector & Debug Tool")]
    public static void ShowWindow()
    {
        GetWindow<LevelSelectorWindow>("Game Debugger");
    }

    private void OnEnable()
    {
        FetchCurrentData();
    }

    private void OnGUI()
    {
        GUILayout.Label("Game Debugger & Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        bool isPlaying = Application.isPlaying;
        if (isPlaying)
        {
            int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.CurrentLevelIndex + 1 : 1;
            EditorGUILayout.HelpBox($"Game đang chạy (Play Mode)\nLevel hiện tại: {currentLevel}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Game đang dừng (Edit Mode)\nThay đổi ở đây sẽ cập nhật trực tiếp vào file save JSON.", MessageType.None);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("1. Cài đặt Level", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetLevel1Based = EditorGUILayout.IntField("Level mong muốn (1-based):", targetLevel1Based);
        if (targetLevel1Based < 1) targetLevel1Based = 1;

        EditorGUILayout.Space();

        if (isPlaying)
        {
            if (LevelManager.Instance != null && targetLevel1Based > LevelManager.Instance.LevelConfigs.Count)
            {
                EditorGUILayout.HelpBox($"Cảnh báo: Level {targetLevel1Based} vượt quá số lượng màn hiện có ({LevelManager.Instance.LevelConfigs.Count}).", MessageType.Warning);
            }

            if (GUILayout.Button("Load Level Ngay Lập Tức", GUILayout.Height(30)))
            {
                int index = targetLevel1Based - 1;
                if (LevelManager.Instance != null && index >= 0 && index < LevelManager.Instance.LevelConfigs.Count)
                {
                    GameManager.Instance.StartCoroutine(LoadSelectedLevelFlow(index));
                }
                else
                {
                    Debug.LogError($"Level {targetLevel1Based} không hợp lệ!");
                }
            }
        }
        else
        {
            if (GUILayout.Button("Lưu Level khởi đầu vào File Save", GUILayout.Height(30)))
            {
                SaveLevelToSaveFile(targetLevel1Based - 1);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("2. Cài đặt Booster", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!initialized)
        {
            if (GUILayout.Button("Tải dữ liệu Booster hiện tại"))
            {
                FetchCurrentData();
            }
        }
        else
        {
            DrawBoosterRow("Move Booster (+Move)", ref targetMoveCount);
            DrawBoosterRow("Undo Booster (Undo)", ref targetUndoCount);
            DrawBoosterRow("Remove Booster (Bỏ Condition)", ref targetRemoveCount);

            EditorGUILayout.Space();

            if (isPlaying)
            {
                if (GUILayout.Button("Áp Dụng Booster Ngay Lập Tức", GUILayout.Height(30)))
                {
                    ApplyBoostersInGame();
                }
            }
            else
            {
                if (GUILayout.Button("Lưu Số Lượng Booster vào File Save", GUILayout.Height(30)))
                {
                    SaveBoostersToSaveFile();
                }
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
        if (GUILayout.Button("Đọc lại toàn bộ dữ liệu từ Game/File Save", GUILayout.Height(25)))
        {
            FetchCurrentData();
            Debug.Log("Đã làm mới dữ liệu trên Debug Tool!");
        }
    }
    private void DrawBoosterRow(string label, ref int count)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(180));
        count = EditorGUILayout.IntField(count, GUILayout.Width(60));
        
        if (count < 0) count = 0;

        if (GUILayout.Button("-5", GUILayout.Width(35))) count = Mathf.Max(0, count - 5);
        if (GUILayout.Button("-1", GUILayout.Width(25))) count = Mathf.Max(0, count - 1);
        if (GUILayout.Button("+1", GUILayout.Width(25))) count++;
        if (GUILayout.Button("+5", GUILayout.Width(35))) count += 5;
        if (GUILayout.Button("+99", GUILayout.Width(38))) count += 99;
        if (GUILayout.Button("Set 0", GUILayout.Width(50))) count = 0;

        EditorGUILayout.EndHorizontal();
    }

    private void FetchCurrentData()
    {
        if (Application.isPlaying)
        {
            if (LevelManager.Instance != null)
            {
                targetLevel1Based = LevelManager.Instance.CurrentLevelIndex + 1;
            }
            
            if (BoosterManager.Instance != null && BoosterManager.Instance.BoosterHolder != null)
            {
                var holder = BoosterManager.Instance.BoosterHolder;
                targetMoveCount = holder.ContainsKey(Booster.Move) ? holder[Booster.Move] : 0;
                targetUndoCount = holder.ContainsKey(Booster.Undo) ? holder[Booster.Undo] : 0;
                targetRemoveCount = holder.ContainsKey(Booster.Remove) ? holder[Booster.Remove] : 0;
                initialized = true;
            }
        }
        else
        {
            string relativePath = "/player_save.json";
            string fullPath = Application.persistentDataPath + relativePath;

            if (File.Exists(fullPath))
            {
                try
                {
                    JsonDataService dataService = new JsonDataService();
                    GameData gameData = dataService.LoadData<GameData>(relativePath, false);

                    if (gameData != null)
                    {
                        targetLevel1Based = gameData.currentLevelIndex + 1;

                        if (gameData.boosterCounts != null)
                        {
                            targetMoveCount = gameData.boosterCounts.ContainsKey(Booster.Move) ? gameData.boosterCounts[Booster.Move] : 0;
                            targetUndoCount = gameData.boosterCounts.ContainsKey(Booster.Undo) ? gameData.boosterCounts[Booster.Undo] : 0;
                            targetRemoveCount = gameData.boosterCounts.ContainsKey(Booster.Remove) ? gameData.boosterCounts[Booster.Remove] : 0;
                        }
                        initialized = true;
                    }
                }
                catch
                {
                    initialized = false;
                }
            }
            else
            {
                GameData defaultData = new GameData();
                targetLevel1Based = defaultData.currentLevelIndex + 1;
                targetMoveCount = defaultData.boosterCounts[Booster.Move];
                targetUndoCount = defaultData.boosterCounts[Booster.Undo];
                targetRemoveCount = defaultData.boosterCounts[Booster.Remove];
                initialized = true;
            }
        }
    }

    private void ApplyBoostersInGame()
    {
        if (BoosterManager.Instance != null && BoosterManager.Instance.BoosterHolder != null)
        {
            var holder = BoosterManager.Instance.BoosterHolder;
            
            holder[Booster.Move] = targetMoveCount;
            holder[Booster.Undo] = targetUndoCount;
            holder[Booster.Remove] = targetRemoveCount;

            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame();
            }

            Debug.Log($"Đã cập nhật booster trong game: Move={targetMoveCount}, Undo={targetUndoCount}, Remove={targetRemoveCount}");
        }
        else
        {
            Debug.LogError("Không tìm thấy BoosterManager trong Scene!");
        }
    }

    private void SaveBoostersToSaveFile()
    {
        string relativePath = "/player_save.json";
        string fullPath = Application.persistentDataPath + relativePath;

        try
        {
            JsonDataService dataService = new JsonDataService();
            GameData gameData;

            if (File.Exists(fullPath))
            {
                gameData = dataService.LoadData<GameData>(relativePath, false);
            }
            else
            {
                gameData = new GameData();
            }

            if (gameData.boosterCounts == null)
            {
                gameData.boosterCounts = new Dictionary<Booster, int>();
            }

            gameData.boosterCounts[Booster.Move] = targetMoveCount;
            gameData.boosterCounts[Booster.Undo] = targetUndoCount;
            gameData.boosterCounts[Booster.Remove] = targetRemoveCount;

            dataService.SaveData(relativePath, gameData, false);
            Debug.Log($"Đã lưu số lượng booster vào file save thành công: Move={targetMoveCount}, Undo={targetUndoCount}, Remove={targetRemoveCount}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Không thể ghi đè booster vào file save: {e.Message}");
        }
    }

    private void SaveLevelToSaveFile(int index)
    {
        string relativePath = "/player_save.json";
        string fullPath = Application.persistentDataPath + relativePath;

        try
        {
            JsonDataService dataService = new JsonDataService();
            GameData gameData;

            if (File.Exists(fullPath))
            {
                gameData = dataService.LoadData<GameData>(relativePath, false);
            }
            else
            {
                gameData = new GameData();
            }

            gameData.currentLevelIndex = index;
            dataService.SaveData(relativePath, gameData, false);

            Debug.Log($"Đã cập nhật level khởi đầu trong file save thành Level {index + 1} tại: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Không thể ghi đè level vào file save: {e.Message}");
        }
    }

    private System.Collections.IEnumerator LoadSelectedLevelFlow(int index)
    {
        GameManager.Instance.UpdateGameState(GameState.SetUp);
        
        LevelManager.Instance.CurrentLevelIndex = index;
        LevelManager.Instance.LoadCurrentLevel();
        
        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SaveGame();
        }
        
        yield return null;
        
        GameManager.Instance.UpdateGameState(GameState.GamePlay);
        Debug.Log($"Đã nhảy đến Level {index + 1} thành công!");
    }
}
