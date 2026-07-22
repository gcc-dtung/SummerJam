using UnityEngine;

public class MoreMoveBooster : MonoBehaviour
{
    public bool TakeMoreMove()
    {
       return MoveManager.Instance.TryIncreaseMove();
    }
}
