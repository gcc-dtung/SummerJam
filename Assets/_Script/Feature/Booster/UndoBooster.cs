using UnityEngine;

public class UndoBooster : MonoBehaviour
{
    public bool TryUndo()
    {
       return UndoManager.Instance.TryUndoMove();
    }
}
