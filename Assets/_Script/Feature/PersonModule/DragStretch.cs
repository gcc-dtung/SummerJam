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
    [SerializeField] private float followResponsiveness = 20f;
    [SerializeField] private Transform hoverTransform;

    private Vector3 baseScale;
    private Quaternion baseRotation;
    private Vector3 previousDragPosition;
    private bool dragging;
    private bool hasPreviousDragPosition;
    private Tween scaleTween;
    private Tween rotationTween;

    private void Awake()
    {
        if (hoverTransform == null) hoverTransform = transform;
        baseScale = hoverTransform.localScale;
        baseRotation = hoverTransform.rotation;
    }

    private void OnEnable()
    {
        eventHandler.OnStartDrag += StartStretch;
        eventHandler.OnDraggingWithMousePosition += UpdateStretch;
        eventHandler.OnDrop += EndStretch;
    }

    private void OnDisable()
    {
        eventHandler.OnStartDrag -= StartStretch;
        eventHandler.OnDraggingWithMousePosition -= UpdateStretch;
        eventHandler.OnDrop -= EndStretch;
    }

    private void StartStretch()
    {
        if (scaleTween.isAlive) scaleTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();

        // Người có thể đã đổi parent khi bắt đầu kéo, nên lưu rotation hiện tại.
        // baseScale được giữ từ Awake để không xung đột với PersonVisual khi nó phóng to lúc drag.
        baseRotation = hoverTransform.rotation;
        dragging = true;
        hasPreviousDragPosition = false;
    }

    private void UpdateStretch(Vector3 dragPosition)
    {
        if (!dragging) return;

        // Hệ thống drag của project gửi vị trí ở event này. Không dùng OnMouseDrag,
        // vì callback đó không được DragAndDropController gọi liên tục.
        if (!hasPreviousDragPosition)
        {
            previousDragPosition = dragPosition;
            hasPreviousDragPosition = true;
            return;
        }

        Vector2 dragDelta = dragPosition - previousDragPosition;
        previousDragPosition = dragPosition;
        if (dragDelta.sqrMagnitude < 0.0001f)
        {
            ReturnToRestPose();
            return;
        }

        float amount = Mathf.Clamp(dragDelta.magnitude * stretchStrength, 0f, maxStretch - 1f);
        float tilt = Mathf.Clamp(-dragDelta.x * tiltSensitivity, -maxTilt, maxTilt);
        float blend = 1f - Mathf.Exp(-followResponsiveness * Time.deltaTime);

        // Kiểu Is This Seat Taken?: nhân vật luôn đứng thẳng, chỉ nghiêng nhẹ khi lướt ngang.
        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);
        hoverTransform.rotation = Quaternion.Slerp(hoverTransform.rotation, targetRotation, blend);

        Vector3 targetScale = new Vector3(
            baseScale.x * (1f - amount * 0.45f),
            baseScale.y * (1f + amount),
            baseScale.z);
        hoverTransform.localScale = Vector3.Lerp(hoverTransform.localScale, targetScale, blend);
    }

    private void EndStretch()
    {
        dragging = false;
        hasPreviousDragPosition = false;
        if (scaleTween.isAlive) scaleTween.Stop();
        if (rotationTween.isAlive) rotationTween.Stop();
        if(hoverTransform.localScale != baseScale)
            scaleTween = Tween.Scale(hoverTransform, baseScale, returnDuration, Ease.OutBack);
        if(hoverTransform.rotation != baseRotation)
            rotationTween = Tween.Rotation(hoverTransform, baseRotation, returnDuration, Ease.OutBack);
    }

    private void ReturnToRestPose()
    {
        float blend = 1f - Mathf.Exp(-followResponsiveness * Time.deltaTime);
        hoverTransform.rotation = Quaternion.Slerp(hoverTransform.rotation, baseRotation, blend);
        hoverTransform.localScale = Vector3.Lerp(hoverTransform.localScale, baseScale, blend);
    }
}
