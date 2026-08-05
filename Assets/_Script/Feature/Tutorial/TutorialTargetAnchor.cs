using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum TutorialTargetId
{
    None = 0,
    PlayButton = 1,
    WaitLine = 2,
    TargetPerson = 3,
    TargetCondition = 4,
    ValidSeat = 5,
    DestinationSeat = 6,
    BoosterArea = 7,
    RemoveBooster = 8,
    MoreMoveBooster = 9,
    UndoBooster = 10,
    MoveCounter = 11,
    CakeItem = 12
}

[DisallowMultipleComponent]
public sealed class TutorialTargetAnchor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TutorialTargetId targetId;
    [Tooltip("Fallback hit radius for world targets that do not have a Collider2D or Renderer.")]
    [SerializeField, Min(0.01f)] private float worldHitRadius = 0.75f;

    public event Action<TutorialTargetAnchor> Clicked;

    public TutorialTargetId TargetId => targetId;
    public Transform TargetTransform => transform;

    public void Configure(TutorialTargetId value)
    {
        targetId = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        Collider2D targetCollider = GetComponentInChildren<Collider2D>();
        if (targetCollider != null)
            return targetCollider.OverlapPoint(worldPoint);

        Renderer targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
        {
            Vector3 point = new Vector3(worldPoint.x, worldPoint.y, targetRenderer.bounds.center.z);
            return targetRenderer.bounds.Contains(point);
        }

        return Vector2.Distance(transform.position, worldPoint) <= worldHitRadius;
    }
}
