using PrimeTween;
using UnityEngine;

public class DragStretch : MonoBehaviour
{
    [SerializeField] private PersonEventHandler eventHandler;
    [SerializeField] private float stretchStrength = 0.25f;
    [SerializeField] private float maxStretch = 1.6f;
    [SerializeField] private float returnDuration = 0.18f;
    [SerializeField] private float maxTilt = 12f;
    [SerializeField] private float tiltSensitivity = 250f;
    [SerializeField, Min(0f)] private float velocityResponsiveness = 12f;
    [SerializeField] private float followResponsiveness = 20f;
    [SerializeField] private Transform hoverTransform;
    [SerializeField] private Vector3 gripLocalPosition = new(0f, 1.25f, 0f);

    private Vector3 baseScale;
    private Vector3 baseLocalPosition;
    private Quaternion baseRotation;
    private Vector3 latestDragPosition;
    private Vector3 previousDragPosition;
    private Vector2 smoothedDragVelocity;
    private bool dragging;
    private bool hasPreviousDragPosition;
    private Tween scaleTween;
    private Tween rotationTween;

    private void Awake()
    {
        if (hoverTransform == null) hoverTransform = transform;
        PersonVisual personVisual = hoverTransform.GetComponent<PersonVisual>();
        if (personVisual != null) gripLocalPosition = personVisual.HandPosition;

        baseScale = hoverTransform.localScale;
        baseLocalPosition = hoverTransform.localPosition;
        baseRotation = hoverTransform.rotation;
    }

    private void OnEnable()
    {
        eventHandler.OnStartDrag += StartStretch;
        eventHandler.OnDraggingWithMousePosition += UpdateDragPosition;
        eventHandler.OnDrop += EndStretch;
    }

    private void OnDisable()
    {
        eventHandler.OnStartDrag -= StartStretch;
        eventHandler.OnDraggingWithMousePosition -= UpdateDragPosition;
        eventHandler.OnDrop -= EndStretch;
    }

    private void LateUpdate()
    {
        if (!dragging || !hasPreviousDragPosition) return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector2 instantVelocity = (latestDragPosition - previousDragPosition) / deltaTime;
        previousDragPosition = latestDragPosition;

        float velocityBlend = 1f - Mathf.Exp(-velocityResponsiveness * deltaTime);
        smoothedDragVelocity = Vector2.Lerp(smoothedDragVelocity, instantVelocity, velocityBlend);

        // Preserve the old Inspector tuning at a 60 FPS reference while making it frame-rate independent.
        Vector2 dragDelta = smoothedDragVelocity / 60f;
        float poseBlend = 1f - Mathf.Exp(-followResponsiveness * deltaTime);

        if (dragDelta.sqrMagnitude < 0.0001f)
        {
            ReturnToRestPose(poseBlend);
            return;
        }

        float amount = Mathf.Clamp(dragDelta.magnitude * stretchStrength, 0f, maxStretch - 1f);
        float tilt = Mathf.Clamp(-dragDelta.x * tiltSensitivity, -maxTilt, maxTilt);
        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);
        Vector3 targetScale = new Vector3(
            baseScale.x * (1f - amount * 0.45f),
            baseScale.y * (1f + amount),
            baseScale.z);

        ApplyPose(
            Quaternion.Slerp(hoverTransform.rotation, targetRotation, poseBlend),
            Vector3.Lerp(hoverTransform.localScale, targetScale, poseBlend));
    }

    private void StartStretch()
    {
        if (scaleTween.isAlive) scaleTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();

        baseRotation = hoverTransform.rotation;
        dragging = true;
        hasPreviousDragPosition = false;
        smoothedDragVelocity = Vector2.zero;
    }

    private void UpdateDragPosition(Vector3 dragPosition)
    {
        if (!dragging) return;

        latestDragPosition = dragPosition;
        if (!hasPreviousDragPosition)
        {
            previousDragPosition = dragPosition;
            hasPreviousDragPosition = true;
        }
    }

    private void EndStretch()
    {
        dragging = false;
        hasPreviousDragPosition = false;
        smoothedDragVelocity = Vector2.zero;

        if (scaleTween.isAlive) scaleTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();

        hoverTransform.localPosition = baseLocalPosition;
        if (hoverTransform.localScale != baseScale)
            scaleTween = Tween.Scale(hoverTransform, baseScale, returnDuration, Ease.OutBack);
        if (hoverTransform.rotation != baseRotation)
            rotationTween = Tween.Rotation(hoverTransform, baseRotation, returnDuration, Ease.OutBack);
    }

    private void ReturnToRestPose(float blend)
    {
        ApplyPose(
            Quaternion.Slerp(hoverTransform.rotation, baseRotation, blend),
            Vector3.Lerp(hoverTransform.localScale, baseScale, blend));
    }

    private void ApplyPose(Quaternion rotation, Vector3 scale)
    {
        Vector3 gripWorldPosition = hoverTransform.TransformPoint(gripLocalPosition);
        hoverTransform.rotation = rotation;
        hoverTransform.localScale = scale;
        hoverTransform.position += gripWorldPosition - hoverTransform.TransformPoint(gripLocalPosition);
    }
}
