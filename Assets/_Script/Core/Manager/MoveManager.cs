using System;
using TMPro;
using UnityEngine;
public class MoveManager : Singleton<MoveManager>
{
    private LevelConfig data;
    [SerializeField] private TextMeshProUGUI text;
    public int StepRemain { get; private set; }
    public int Limit => data != null ? data.MoveLimit : 0;
    public event Action<int> OnStepRemainChanged;

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.PlacePerson,DetuctMove);
        LevelManager.Instance.OnLevelConfigChange += ReloadData;
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.PlacePerson,DetuctMove);
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelConfigChange -= ReloadData;
    }
    
    public void ReloadData(LevelConfig data)
    {
        this.data = data;
        StepRemain = this.data.MoveLimit;
        NotifyStepRemainChanged();
    }

    public void DetuctMove()
    {
        if(IsOutOfMove()) return;
        StepRemain--;
        NotifyStepRemainChanged();
    }

    public void AddMoreMove(int amount)
    {
        StepRemain = Mathf.Clamp(StepRemain + amount, 0, data.MoveLimit);
        NotifyStepRemainChanged();
    }

    public bool TryIncreaseMove()
    {
        if (StepRemain + 1 > data.MoveLimit) return false;
        StepRemain = Mathf.Clamp(StepRemain + 1, 0, data.MoveLimit);
        NotifyStepRemainChanged();
        return true;
    }

    private void NotifyStepRemainChanged()
    {
        if (text != null)
            text.SetText("{0}", StepRemain);

        OnStepRemainChanged?.Invoke(StepRemain);
    }

    public bool IsOutOfMove() => (StepRemain <= 0);

}
