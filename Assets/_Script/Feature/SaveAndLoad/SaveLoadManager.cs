using UnityEngine;
using System.IO;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private IDataService dataService;
    private readonly string saveFileName = "/player_save.json"; 
    
    private GameData gameData;
    public GameData GameData => gameData;
    protected override void Awake()
    {
        base.Awake();
        dataService = new JsonDataService();
        LoadGame();
    }
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (gameData == null) gameData = new GameData();
        if (LevelManager.Instance != null)
        {
            gameData.currentLevelIndex = LevelManager.Instance.CurrentLevelIndex;
        }
        if (BoosterManager.Instance != null)
        {
            gameData.boosterCounts = BoosterManager.Instance.BoosterHolder;
        }
        if (EconomyManager.Instance != null)
        {
            gameData.currentGold = EconomyManager.Instance.CurrentGold;
            gameData.currentGem = EconomyManager.Instance.CurrentGem;
        }
        dataService.SaveData(saveFileName, gameData, false);
    }

    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        string fullPath = Application.persistentDataPath + saveFileName;

        if (!File.Exists(fullPath))
        {
            Debug.Log("No save file found. Creating new game data.");

            gameData = new GameData();

            // Optional: tạo file save ngay lần đầu chạy.
            dataService.SaveData(saveFileName, gameData, false);

            ApplyDataToManagers();
            return;
        }

        try
        {
            gameData = dataService.LoadData<GameData>(saveFileName, false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save file exists but failed to load. Creating default data. Reason: {e.Message}");
            gameData = new GameData();
        }

        ApplyDataToManagers();
    }

    private void ApplyDataToManagers()
    {
        if (gameData == null)
        {
            gameData = new GameData();
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CurrentLevelIndex = gameData.currentLevelIndex;
        }
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.BoosterHolder = gameData.boosterCounts;
        }
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.InitializeData(gameData.currentGold, gameData.currentGem);
        }
    }
}
