using UnityEngine;
using System.IO;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private IDataService dataService;
    private readonly string saveFileName = "/player_save.json"; 
    
    private GameData gameData;
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
        bool success = dataService.SaveData(saveFileName, gameData, false);
    }
    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        try
        {
            gameData = dataService.LoadData<GameData>(saveFileName, false);
        }
        catch (FileNotFoundException)
        {
            gameData = new GameData();
        }
        catch (System.Exception e)
        {
            gameData = new GameData();
        }
        ApplyDataToManagers();
    }
    private void ApplyDataToManagers()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CurrentLevelIndex = gameData.currentLevelIndex;
        }
        if (BoosterManager.Instance != null)
        {
            BoosterManager.Instance.BoosterHolder = gameData.boosterCounts;
        }
    }
}
