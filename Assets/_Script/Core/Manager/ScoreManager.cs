using System;
using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    [SerializeField,Range(0,1)] private float twoStarThresHold;
    [SerializeField,Range(0,1)] private float threeStarThresHold;
    public int lastStart { get; private set; }
    [SerializeField] private TextMeshProUGUI text;
  
    private void Update()
    {
        // Calculate();
        // text.text = "Score: " + lastStart.ToString(); // để tạm
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
