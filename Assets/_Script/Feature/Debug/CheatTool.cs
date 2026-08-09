#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using UnityEngine;

public class CheatTool : MonoBehaviour
{
    private Rect _windowRect = new Rect(20, 20, 250, 320);
    private bool _isMinimized = true;
    private bool _showGUI = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // Kiểm tra xem đã có CheatTool trong scene chưa
        if (FindAnyObjectByType<CheatTool>() != null) return;

        GameObject cheatObject = new GameObject("[Dev_CheatTool]");
        cheatObject.AddComponent<CheatTool>();
        DontDestroyOnLoad(cheatObject);
        Debug.Log("<color=green>[CheatTool] Khởi tạo thành công! F5 Thắng, F6 Thua, F7 Màn Tiếp, F8 Chơi Lại, F9 Panel, F10 Reset Tutorial.</color>");
    }

    private void Update()
    {
        // 1. Phím tắt Ẩn/Hiện Panel (F9)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            _showGUI = !_showGUI;
        }

        // Nếu game không chạy thì không làm gì tiếp theo
        if (!Application.isPlaying) return;

        // 2. Phím tắt Thắng Màn (F5)
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TriggerWin();
        }

        // 3. Phím tắt Thua Màn (F6)
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TriggerLose();
        }

        // 4. Phím tắt Màn Tiếp Theo (F7)
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TriggerNextLevel();
        }

        // 5. Phím tắt Tải Lại Màn (F8)
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TriggerRestart();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            ResetTutorial();
        }
    }

    private void OnGUI()
    {
        if (!_showGUI) return;

        // Đổi skin GUI cho đẹp hơn tí
        GUI.color = Color.white;
        string windowTitle = _isMinimized ? "🐞 Debug" : "🐞 DEV CHEAT TOOL";
        
        // Điều chỉnh kích thước cửa sổ dựa trên việc thu gọn hay phóng to
        float width = _isMinimized ? 100 : 260;
        float height = _isMinimized ? 60 : 410;
        _windowRect.width = width;
        _windowRect.height = height;

        // Vẽ cửa sổ có thể kéo di chuyển
        _windowRect = GUI.Window(9999, _windowRect, DrawWindow, windowTitle);
    }

    private void DrawWindow(int windowId)
    {
        // Nút thu gọn / mở rộng góc phải
        if (GUI.Button(new Rect(_windowRect.width - 25, 2, 20, 20), _isMinimized ? "□" : "_"))
        {
            _isMinimized = !_isMinimized;
        }

        if (_isMinimized)
        {
            if (GUI.Button(new Rect(10, 25, 80, 25), "Mở rộng"))
            {
                _isMinimized = false;
            }
            GUI.DragWindow();
            return;
        }

        GUILayout.Space(15);

        // Hiển thị trạng thái hiện tại
        int currentLevelIdx = (LevelManager.Instance != null) ? LevelManager.Instance.CurrentLevelIndex + 1 : 0;
        string gameStateStr = (GameManager.Instance != null) ? GameManager.Instance.currentState.ToString() : "N/A";
        GUILayout.Label($"Level hiện tại: {currentLevelIdx}", GUILayout.Width(240));
        GUILayout.Label($"Trạng thái game: {gameStateStr}", GUILayout.Width(240));
        
        GUILayout.Space(10);

        // Nhóm nút thay đổi trạng thái Level
        GUILayout.Label("== LEVEL CONTROL ==", GUILayout.Width(240));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Thắng Màn (F5)", GUILayout.Height(30)))
        {
            TriggerWin();
        }
        if (GUILayout.Button("Thua Màn (F6)", GUILayout.Height(30)))
        {
            TriggerLose();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Màn Tiếp (F7)", GUILayout.Height(30)))
        {
            TriggerNextLevel();
        }
        if (GUILayout.Button("Chơi Lại (F8)", GUILayout.Height(30)))
        {
            TriggerRestart();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Nhóm nút Kinh Tế & Booster
        GUILayout.Label("== TÀI NGUYÊN & BOOSTER ==", GUILayout.Width(240));
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1K Vàng", GUILayout.Height(25)))
        {
            AddGold(1000);
        }
        if (GUILayout.Button("+1K Gem", GUILayout.Height(25)))
        {
            AddGem(1000);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("+5 Tất Cả Boosters", GUILayout.Height(30)))
        {
            AddAllBoosters(5);
        }

        GUILayout.Space(10);
        GUILayout.Label("== TUTORIAL ==", GUILayout.Width(240));
        if (GUILayout.Button("Reset Tutorial (F10)", GUILayout.Height(30)))
        {
            ResetTutorial();
        }

        GUILayout.Space(10);
        GUILayout.Label("F9: Ẩn/Hiện toàn bộ Panel", GUILayout.Width(240));

        // Cho phép kéo di chuyển cửa sổ bằng cách click giữ vùng trống
        GUI.DragWindow();
    }

    private void TriggerWin()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Win);
            Debug.Log("[CheatTool] Đã kích hoạt thắng màn chơi!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy GameManager!");
        }
    }

    private void TriggerLose()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Lose);
            Debug.Log("[CheatTool] Đã kích hoạt thua màn chơi!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy GameManager!");
        }
    }

    private void TriggerNextLevel()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevelButton();
            Debug.Log("[CheatTool] Đã chuyển qua màn tiếp theo!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy LevelManager!");
        }
    }

    private void TriggerRestart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateGameState(GameState.Replay);
            Debug.Log("[CheatTool] Đã tải lại màn chơi hiện tại!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy GameManager!");
        }
    }

    private void AddGold(int amount)
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.GetGold(amount);
            Debug.Log($"[CheatTool] Đã thêm {amount} Gold!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy EconomyManager!");
        }
    }

    private void AddGem(int amount)
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.GetGem(amount);
            Debug.Log($"[CheatTool] Đã thêm {amount} Gem!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy EconomyManager!");
        }
    }

    private void AddAllBoosters(int amount)
    {
        if (BoosterManager.Instance != null)
        {
            var holder = BoosterManager.Instance.BoosterHolder;
            foreach (Booster b in Enum.GetValues(typeof(Booster)))
            {
                if (!holder.ContainsKey(b)) holder[b] = 0;
                holder[b] += amount;
            }
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame();
            }
            Debug.Log($"[CheatTool] Đã thêm {amount} cho tất cả các loại Booster!");
        }
        else
        {
            Debug.LogWarning("[CheatTool] Không tìm thấy BoosterManager!");
        }
    }

    private void ResetTutorial()
    {
        SaveLoadManager saveLoadManager = SaveLoadManager.Instance;
        if (saveLoadManager == null || saveLoadManager.GameData == null)
        {
            Debug.LogWarning("[CheatTool] Không thể reset tutorial vì chưa có save data.");
            return;
        }

        saveLoadManager.GameData.tutorialVersionCompleted = 0;

        if (LevelManager.Instance != null)
            LevelManager.Instance.CurrentLevelIndex = 0;

        saveLoadManager.SaveGame();
        Debug.Log("[CheatTool] Đã reset tutorial. Hãy thoát và vào lại Play Mode để chạy từ đầu.");
    }
}
#endif
