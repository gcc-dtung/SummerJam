using System;
using UnityEngine;

public class PersonEventHandler : MonoBehaviour
{
   public event Action<Vector3> OnDraggingWithMousePosition;
   public event Action OnDraggingWithoutMousePosition;
   public event Action<Vector3> OnMoveToSeat;
   public event Action OnPress;
   public event Action OnStartDrag;
   public event Action OnDrop;

   public void OnPressNotify()
   {
      OnPress?.Invoke();
   }

   public void OnDraggingNotify(Vector3 position)
   {
      OnDraggingWithMousePosition?.Invoke(position);
      OnDraggingWithoutMousePosition?.Invoke();
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
