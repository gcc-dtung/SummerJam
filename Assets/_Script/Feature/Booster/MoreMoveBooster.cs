using UnityEngine;

public class MoreMoveBooster : MonoBehaviour
{
    public void TakeMoreMove()
    {
        MoveManager.Instance.IncreaseMove();
    }
}
