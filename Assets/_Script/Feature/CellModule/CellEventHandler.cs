using System;
using UnityEngine;

public class CellEventHandler : MonoBehaviour
{
    public event Action OnSelected;
    public event Action OnDeselected;

    public void OnSelectedNotify()
    {
        OnSelected?.Invoke();
    }

    public void OnDeselectedNotify()
    {
        OnDeselected?.Invoke();
    }

}
