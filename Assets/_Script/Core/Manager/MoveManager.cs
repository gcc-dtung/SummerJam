using System;
using TMPro;
using UnityEngine;
public class MoveManager : Singleton<MoveManager>
{
    private LevelConfig data;
    [SerializeField] private TextMeshProUGUI text;
    public int StepRemain { get; private set; }
    public int Limit => data.MoveLimit;

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
    
    private void Update()
    {
        text.text = StepRemain.ToString(); // để tạm
    }

    public void ReloadData(LevelConfig data)
    {
        this.data = data;
        StepRemain = this.data.MoveLimit;
    }

    public void DetuctMove()
    {
        if(IsOutOfMove()) return;
        StepRemain--;
    }

    public void IncreaseMove()
    {
        StepRemain = Mathf.Clamp(StepRemain + 1, 0, data.MoveLimit);
    }
    public bool IsOutOfMove() => (StepRemain <= 0);

}
