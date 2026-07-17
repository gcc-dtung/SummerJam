using System;
using TMPro;
using UnityEngine;
public class MoveManager : Singleton<MoveManager>
{
    [SerializeField] private MoveDataSO data;
    [SerializeField] private TextMeshProUGUI text;
    public int StepRemain { get; private set; }
    public int Limit => data.Limit;

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.PlacePerson,DetuctMove);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.PlacePerson,DetuctMove);
    }

    private void Start()
    {
        ReloadData(data);
    }

    private void Update()
    {
        text.text = "Move: " + StepRemain.ToString(); // để tạm
    }

    public void ReloadData(MoveDataSO data)
    {
        //this.data = data;
        StepRemain = data.Limit;
    }

    public void DetuctMove()
    {
        if(IsOutOfMove()) return;
        StepRemain--;
    }

    public void IncreaseMove()
    {
        StepRemain = Mathf.Clamp(StepRemain + 1, 0, data.Limit);
    }
    public bool IsOutOfMove() => (StepRemain <= 0);

}
