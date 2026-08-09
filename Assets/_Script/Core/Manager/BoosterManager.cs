using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoosterManager : Singleton<BoosterManager>
{
    private Dictionary<Booster, int> boosterHolder = new Dictionary<Booster, int>();

    [Header("Booster HUD")]
    [SerializeField] private TextMeshProUGUI undoCountText;
    [SerializeField] private TextMeshProUGUI removeCountText;
    [SerializeField] private TextMeshProUGUI moveCountText;

    public Dictionary<Booster, int> BoosterHolder
    {
        get => boosterHolder;
        set
        {
            boosterHolder = value ?? new Dictionary<Booster, int>();
            RefreshBoosterCountTexts();
        }
    }

    private void Start()
    {
        ResolveBoosterCountTexts();
        RefreshBoosterCountTexts();
    }

    public void AddMoreBooster(Booster boost,int amount)
    {
        if(!boosterHolder.ContainsKey(boost)) boosterHolder.Add(boost,0);
        boosterHolder[boost]+= amount;
        RefreshBoosterCountTexts();
        if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
    }

    public void Undo()
    {
        if(boosterHolder[Booster.Undo] <= 0) return;
        if (UndoManager.Instance.TryUndoMove())
        {
            boosterHolder[Booster.Undo]--;
            RefreshBoosterCountTexts();
            EventBus.Notify(GameEventType.BoosterUsed, Booster.Undo);
            if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
        }
    }

    public void MoreMove()
    {
        if(boosterHolder[Booster.Move] <= 0) return;
        if(MoveManager.Instance.TryIncreaseMove())
        {
            boosterHolder[Booster.Move]--;
            RefreshBoosterCountTexts();
            EventBus.Notify(GameEventType.BoosterUsed, Booster.Move);
            if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
        }
    }

    public bool CanRemove()
    {
        if(boosterHolder[Booster.Remove] <= 0) return false;
        return true;
    }

    public void RemoveHandle()
    {
        if (boosterHolder[Booster.Remove] <= 0) return;

        boosterHolder[Booster.Remove]--;
        RefreshBoosterCountTexts();
        EventBus.Notify(GameEventType.BoosterUsed, Booster.Remove);
        if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.SaveGame();
    }

    public int GetBoosterCount(Booster boost)
    {
        return boosterHolder.TryGetValue(boost, out int count) ? count : 0;
    }

    private void ResolveBoosterCountTexts()
    {
        TextMeshProUGUI[] countTexts = FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (TextMeshProUGUI countText in countTexts)
        {
            if (countText == null || countText.transform.parent == null)
                continue;

            switch (countText.transform.parent.name)
            {
                case "Booster 1":
                    undoCountText ??= countText;
                    break;
                case "Booster 2":
                    removeCountText ??= countText;
                    break;
                case "Booster 3":
                    moveCountText ??= countText;
                    break;
            }
        }
    }

    private void RefreshBoosterCountTexts()
    {
        SetCountText(undoCountText, Booster.Undo);
        SetCountText(removeCountText, Booster.Remove);
        SetCountText(moveCountText, Booster.Move);
    }

    private void SetCountText(TextMeshProUGUI countText, Booster boost)
    {
        if (countText != null)
            countText.text = GetBoosterCount(boost).ToString();
    }
   
}

public enum Booster
{
    Move = 0,
    Undo = 1,
    Remove = 2
}
