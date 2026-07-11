using UnityEngine;
using PrimeTween;

public class PersonMovement : MonoBehaviour
{
   [SerializeField] private float Duration;

   private Tween tweenPosition;
   public void MoveToPosition(Vector3 position)
   {
      if(tweenPosition.isAlive)
         tweenPosition.Stop();
      tweenPosition = Tween.Position(transform, position, duration: Duration);
   }
   
   
}
