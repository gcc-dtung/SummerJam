using UnityEngine;

public class UndoBooster : MonoBehaviour
{
    public void Undo()
    {
        UndoManager.Instance.UndoMove();
    }
}
