using System;
using UnityEngine;

public class PersonEventHandler : MonoBehaviour
{
   public event Action<Vector3> OnDragging;
   public event Action<Vector3> OnMoveToSeat;
   public event Action OnStartDrag;
   public event Action OnDrop;

   public void OnDraggingNotify(Vector3 position)
   {
      OnDragging?.Invoke(position);
   }

   public void OnStartDragNotify()
   {
      OnStartDrag?.Invoke();
   }

   public void OnDropNotify()
   {
      OnDrop?.Invoke();
   }

   public void OnMoveToSeatNotify(Vector3 position)
   {
      OnMoveToSeat?.Invoke(position);
   }
}
