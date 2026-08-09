using System;
using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    [SerializeField,Range(0,1)] private float twoStarThresHold;
    [SerializeField,Range(0,1)] private float threeStarThresHold;
    public int lastStart { get; private set; }
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        if (MoveManager.Instance != null)
            MoveManager.Instance.OnStepRemainChanged += HandleStepRemainChanged;
    }

    private void Start()
    {
        RefreshScore();
    }

    private void OnDisable()
    {
        if (MoveManager.Instance != null)
            MoveManager.Instance.OnStepRemainChanged -= HandleStepRemainChanged;
    }

    private void HandleStepRemainChanged(int stepRemain)
    {
        RefreshScore();
    }

    private void RefreshScore()
    {
        Calculate();

        if (text != null)
            text.SetText("Score: {0}", lastStart);
    }

    public void Calculate()
    {
        int remainMove = MoveManager.Instance.StepRemain;
        int LimitMove = MoveManager.Instance.Limit;

        if (remainMove >= threeStarThresHold * LimitMove)
        {
            lastStart = 3;
            return;
        }

        if (remainMove >= twoStarThresHold * LimitMove)
        {
            lastStart = 2;
            return;
        }

        lastStart = 1;
    }
}
