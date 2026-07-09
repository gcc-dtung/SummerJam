using DG.Tweening;
using UnityEngine;

public class PersonMovement : MonoBehaviour
{
   [SerializeField] private float Duration;
   public void MoveToPosition(Vector3 position)
   {
      transform.DOMove(position, Duration).SetEase(Ease.OutQuad);
   }
   
   
}
