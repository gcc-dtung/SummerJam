using PrimeTween;
using UnityEngine;

public class PersonMovement : MonoBehaviour
{
   [SerializeField] private PersonEventHandler eventHandler;
   [SerializeField, Min(0.001f)] private float Duration;
   [SerializeField, Min(0.001f)] private float dragSmoothTime = 0.05f;

   private Tween tweenPosition;
   private Vector3 dragTargetPosition;
   private Vector3 dragVelocity;
   private bool isFollowingDrag;

   private void OnEnable()
   {
      eventHandler.OnStartDrag += StartFollowingDrag;
      eventHandler.OnDraggingWithMousePosition += SetDragTarget;
      eventHandler.OnDrop += StopFollowingDrag;
      eventHandler.OnMoveToSeat += MoveToPosition;
   }

   private void OnDisable()
   {
      eventHandler.OnStartDrag -= StartFollowingDrag;
      eventHandler.OnDraggingWithMousePosition -= SetDragTarget;
      eventHandler.OnDrop -= StopFollowingDrag;
      eventHandler.OnMoveToSeat -= MoveToPosition;

      if (tweenPosition.isAlive)
         tweenPosition.Stop();
   }

   private void LateUpdate()
   {
      if (!isFollowingDrag) return;

      transform.position = Vector3.SmoothDamp(
         transform.position,
         dragTargetPosition,
         ref dragVelocity,
         dragSmoothTime,
         Mathf.Infinity,
         Time.deltaTime);
   }

   private void StartFollowingDrag()
   {
      if (tweenPosition.isAlive)
         tweenPosition.Stop();

      dragTargetPosition = transform.position;
      dragVelocity = Vector3.zero;
      isFollowingDrag = true;
   }

   private void SetDragTarget(Vector3 position)
   {
      dragTargetPosition = position;
   }

   private void StopFollowingDrag()
   {
      isFollowingDrag = false;
      dragVelocity = Vector3.zero;
   }

   public void MoveToPosition(Vector3 position)
   {
      StopFollowingDrag();

      if (tweenPosition.isAlive)
         tweenPosition.Stop();
      if (transform.position == position) return;

      tweenPosition = Tween.Position(transform, position, duration: Duration);
   }
}
