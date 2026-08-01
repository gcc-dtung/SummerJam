using System;
using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class EconomyManager : Singleton<EconomyManager>
{
  [SerializeField] private TextMeshProUGUI text;
  [SerializeField] private TextMeshProUGUI Gemtext;
  private const int max = 1000000000;
  public event Action<int> OnGoldChange;
  public event Action<int> OnGemChange;
  public int CurrentGold { get; private set; }
  public int CurrentGem { get; private set; }

  [Header("Debug Settings")]
  [SerializeField] private int debugAddGoldAmount = 1000;
  [SerializeField] private int debugAddGemAmount = 100;

  protected override void Awake()
  {
    base.Awake();
    LoadData();
  }

  private void Update()
  {
    text.text = "Gold: " + CurrentGold.ToString();
    //Gemtext.text = "Gem: " + CurrentGem.ToString();
  }

  public void InitializeData(int gold, int gem)
  {
    CurrentGold = gold;
    CurrentGem = gem;
    OnGoldChange?.Invoke(CurrentGold);
    OnGemChange?.Invoke(CurrentGem);
  }

  public void GetGold(int amount)
  {
    CurrentGold = Mathf.Clamp(CurrentGold + amount, 0, max);
    OnGoldChange?.Invoke(CurrentGold);
    SaveData();
  }

  public void GetGem(int amount)
  {
    CurrentGem = Mathf.Clamp(CurrentGem + amount, 0, max);
    OnGemChange?.Invoke(CurrentGem);
    SaveData();
  }
  
  public bool SpendGold(int amount)
  {
    if(CurrentGold - amount < 0) {Debug.Log("Have Enough Gold"); return false;}
    CurrentGold -= amount;
    OnGoldChange?.Invoke(CurrentGold);
    SaveData();
    return true;
  }

  public bool SpendGem(int amount)
  {
    if(CurrentGem - amount < 0) {Debug.Log("Have Enough Gem"); return false;}
    CurrentGem -= amount;
    OnGemChange?.Invoke(CurrentGem);
    SaveData();
    return true;
  }

  public void ResetGold()
  {
    CurrentGold = 0;
    SaveData();
  }

  public void ResetGem()
  {
    CurrentGem = 0;
    SaveData();
  }

  private void SaveData()
  {
    if (SaveLoadManager.Instance != null)
    {
      var gameData = SaveLoadManager.Instance.GameData;
      if (gameData != null)
      {
        gameData.currentGold = CurrentGold;
        gameData.currentGem = CurrentGem;
      }
      SaveLoadManager.Instance.SaveGame();
    }
    else
    {
      JsonDataService dataService = new JsonDataService();
      GameData gameData;
      try
      {
        gameData = dataService.LoadData<GameData>("/player_save.json", false);
      }
      catch
      {
        gameData = new GameData();
      }
      gameData.currentGold = CurrentGold;
      gameData.currentGem = CurrentGem;
      dataService.SaveData("/player_save.json", gameData, false);
    }
  }

  private void LoadData()
  {
    if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.GameData != null)
    {
      CurrentGold = SaveLoadManager.Instance.GameData.currentGold;
      CurrentGem = SaveLoadManager.Instance.GameData.currentGem;
    }
    else
    {
      JsonDataService dataService = new JsonDataService();
      try
      {
        GameData gameData = dataService.LoadData<GameData>("/player_save.json", false);
        CurrentGold = gameData.currentGold;
        CurrentGem = gameData.currentGem;
      }
      catch
      {
        CurrentGold = 0;
        CurrentGem = 0;
      }
    }
  }

  [Button("Add Gold")]
  private void DebugAddGold()
  {
    GetGold(debugAddGoldAmount);
  }

  [Button("Add Gem")]
  private void DebugAddGem()
  {
    GetGem(debugAddGemAmount);
  }
}
