using System;
using UnityEngine;

public class EconomyManager : Singleton<EconomyManager>
{
  private const int max = 1000000000;
  public event Action<int> OnGoldChange;
  public event Action<int> OnGemChange;
  public int CurrentGold { get; private set; }
  public int CurrentGem { get; private set; }

  protected override void Awake()
  {
    base.Awake();
    LoadData();
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
    PlayerPrefs.SetInt("UserGold",CurrentGold);
    PlayerPrefs.SetInt("UserGem",CurrentGem);
    PlayerPrefs.Save();
  }

  private void LoadData()
  {
    CurrentGold = PlayerPrefs.GetInt("UserGold",0);
    CurrentGem = PlayerPrefs.GetInt("UserGem",0);
  }
}
