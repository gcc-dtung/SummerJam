using System;
using TMPro;
using UnityEngine;
public class MoveManager : MonoBehaviour
{
    [SerializeField] private MoveDataSO data;
    [SerializeField] private TextMeshProUGUI text;
    public int StepRemain { get; private set; }

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
        Reload();
    }

    private void Update()
    {
        text.text = "Move: " + StepRemain.ToString(); // để tạm
    }

    void Reload()
    {
        StepRemain = data.Limit;
    }

    void DetuctMove()
    {
        if(IsOutOfMove()) return;
        StepRemain--;
    }

    void IncreaseMove()
    {
        if(IsOutOfMove()) return;
        StepRemain = Mathf.Clamp(StepRemain + 1, 0, data.Limit);
    }
    bool IsOutOfMove() => (StepRemain <= 0);

}
