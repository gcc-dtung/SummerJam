using System;
using UnityEngine;
using PrimeTween;

public class PersonMovement : MonoBehaviour
{
   [SerializeField] private PersonEventHandler eventHandler;
   [SerializeField] private float Duration;
   private Tween tweenPosition;

   private void OnEnable()
   {
      eventHandler.OnDraggingWithMousePosition += MoveToPosition;
      eventHandler.OnMoveToSeat += MoveToPosition; // nếu có thêm transition onSeat thì đky event lại
   }

   private void OnDisable()
   {
      eventHandler.OnDraggingWithMousePosition -= MoveToPosition;
      eventHandler.OnMoveToSeat -= MoveToPosition;
   }

   public void MoveToPosition(Vector3 position)
   {
      if(tweenPosition.isAlive)
         tweenPosition.Stop();
      if (transform.position == position) return;
      tweenPosition = Tween.Position(transform, position, duration: Duration);
   }
   
   
}
