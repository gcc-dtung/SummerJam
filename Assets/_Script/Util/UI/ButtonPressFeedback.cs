using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ButtonPressFeedback : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    ICancelHandler
{
    [Header("Target")]
    [SerializeField, Tooltip("Transform that will be scaled. Leave empty to use this GameObject.")]
    private Transform target;

    [Header("Press Feedback")]
    [SerializeField, Range(0.5f, 1f)]
    private float pressedScale = 0.9f;

    [SerializeField, Min(0f)]
    private float pressDuration = 0.08f;

    [SerializeField, Min(0f)]
    private float releaseDuration = 0.16f;

    [SerializeField]
    private Ease pressEase = Ease.OutCubic;

    [SerializeField]
    private Ease releaseEase = Ease.OutBack;

    [SerializeField, Tooltip("Keeps the feedback working when Time.timeScale is zero.")]
    private bool useUnscaledTime = true;

    [SerializeField, Tooltip("Do not animate when an attached Button/Selectable is not interactable.")]
    private bool respectInteractableState = true;

    private Selectable selectable;
    private Tween scaleTween;
    private Vector3 normalScale;
    private bool isPressed;
    private bool hasCachedScale;

    private void Reset()
    {
        target = transform;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        CacheNormalScale();
    }

    private void OnDisable()
    {
        isPressed = false;
        StopScaleTween();

        if (hasCachedScale && target != null)
            target.localScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !CanPlayFeedback())
            return;

        isPressed = true;
        AnimateScale(normalScale * pressedScale, pressDuration, pressEase);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Release();
    }

    public void OnCancel(BaseEventData eventData)
    {
        Release();
    }

    [ContextMenu("Use Current Scale As Normal")]
    public void CacheNormalScale()
    {
        if (target == null)
            target = transform;

        normalScale = target.localScale;
        hasCachedScale = true;
    }

    private void Initialize()
    {
        if (target == null)
            target = transform;

        if (selectable == null)
            selectable = GetComponent<Selectable>();
    }

    private bool CanPlayFeedback()
    {
        return target != null &&
               hasCachedScale &&
               (!respectInteractableState || selectable == null || selectable.IsInteractable());
    }

    private void Release()
    {
        if (!isPressed)
            return;

        isPressed = false;
        AnimateScale(normalScale, releaseDuration, releaseEase);
    }

    private void AnimateScale(Vector3 endScale, float duration, Ease ease)
    {
        StopScaleTween();

        if (duration <= 0f)
        {
            target.localScale = endScale;
            return;
        }

        scaleTween = Tween.Scale(
            target,
            endValue: endScale,
            duration: duration,
            ease: ease,
            useUnscaledTime: useUnscaledTime);
    }

    private void StopScaleTween()
    {
        if (scaleTween.isAlive)
            scaleTween.Stop();
    }
}
