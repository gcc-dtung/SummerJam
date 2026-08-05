using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class TutorialView : MonoBehaviour, IPointerClickHandler
{
    [Header("Dependencies")]
    [SerializeField] private TutorialDirector director;
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Graphic inputCatcher;

    [Header("Dimming")]
    [SerializeField] private RectTransform dimTop;
    [SerializeField] private RectTransform dimBottom;
    [SerializeField] private RectTransform dimLeft;
    [SerializeField] private RectTransform dimRight;
    [SerializeField, Min(0f)] private float spotlightPadding = 28f;

    [Header("Focus Indicators")]
    [SerializeField] private RectTransform highlightFrame;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private RectTransform hand;

    [Header("Copy")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text tapToContinueText;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float bobDistance = 14f;
    [SerializeField, Min(0f)] private float animationSpeed = 3f;
    [SerializeField, Range(0f, 0.25f)] private float pulseAmount = 0.08f;

    private readonly Vector3[] worldCorners = new Vector3[4];
    private CanvasGroup canvasGroup;
    private TutorialTargetAnchor currentTarget;
    private Vector2 arrowBasePosition;
    private Vector2 handBasePosition;
    private RectTransform instructionRect;
    private RectTransform continuePromptRect;
    private Vector2 instructionDefaultPosition;
    private Vector2 continuePromptDefaultPosition;
    private TMP_FontAsset instructionDefaultFont;
    private float instructionDefaultFontSize;
    private FontStyles instructionDefaultFontStyle;
    private Color instructionDefaultColor;
    private TextAlignmentOptions instructionDefaultAlignment;
    private Vector2 currentSpotlightOffset;
    private Vector2 currentSpotlightSizeDelta;
    private Vector2 currentArrowOffset;
    private Vector2 currentHandOffset;
    private TutorialWorldView worldView;
    private bool isVisible;

    public bool IsVisible => isVisible;

    private void Awake()
    {
        EnsureReferences();
        worldView = gameObject.AddComponent<TutorialWorldView>();
        worldView.Initialize(
            director,
            tutorialCanvas,
            highlightFrame,
            arrow,
            hand,
            instructionText);
        HideImmediate();
    }

    private void OnEnable()
    {
        if (director == null)
            return;

        director.OnStepEntered += HandleStepEntered;
        director.OnStepExited += HandleStepExited;
        director.OnTutorialCompleted += HideImmediate;
    }

    private void OnDisable()
    {
        if (director != null)
        {
            director.OnStepEntered -= HandleStepEntered;
            director.OnStepExited -= HandleStepExited;
            director.OnTutorialCompleted -= HideImmediate;
        }

        HideImmediate();
    }

    public void ShowStep(TutorialStepData step)
    {
        if (step == null)
        {
            HideImmediate();
            return;
        }

        isVisible = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (instructionText != null)
            instructionText.text = step.Instruction;

        if (tapToContinueText != null)
        {
            tapToContinueText.text = "Tap anywhere to continue";
            tapToContinueText.gameObject.SetActive(step.RequiredAction == TutorialAction.TapAnywhere);
        }

        ApplyLayoutOffsets(step);

        currentTarget = null;
        TutorialTargetId presentationTarget = step.RequiredAction == TutorialAction.PlacePersonOnTarget
            ? step.DestinationTarget
            : step.SourceTarget;
        if (director != null && presentationTarget != TutorialTargetId.None)
            director.TargetRegistry.TryGetFirst(presentationTarget, out currentTarget);

        bool hasTarget = currentTarget != null;
        bool useWorldPresentation = step.PresentationSpace == TutorialPresentationSpace.World;
        if (instructionText != null)
            instructionText.gameObject.SetActive(!useWorldPresentation);

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(
                !useWorldPresentation && hasTarget && step.KeepTargetHighlighted);

        if (arrow != null)
            arrow.gameObject.SetActive(!useWorldPresentation && hasTarget && step.ShowArrow);

        if (hand != null)
            hand.gameObject.SetActive(!useWorldPresentation && hasTarget && step.ShowHand);

        if (worldView != null)
            worldView.ShowStep(step);

        if (hasTarget)
            RefreshSpotlight();
        else
            ShowFullDim();

        SetDimVisible(step.DimBackground);
        ApplyInputPolicy(step.RequiredAction);
    }

    [ContextMenu("Hide Tutorial UI")]
    public void HideImmediate()
    {
        isVisible = false;
        currentTarget = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(false);

        if (arrow != null)
        {
            arrow.gameObject.SetActive(false);
            arrow.localScale = Vector3.one;
        }

        if (hand != null)
        {
            hand.gameObject.SetActive(false);
            hand.localScale = Vector3.one;
        }

        SetGraphicRaycast(inputCatcher, false);
        SetDimRaycast(false);
        worldView?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isVisible || director == null || inputCatcher == null || !inputCatcher.raycastTarget)
            return;

        director.TryHandleAction(TutorialAction.TapAnywhere);
    }

    [ContextMenu("Preview Tutorial UI")]
    private void PreviewTutorialUI()
    {
        EnsureReferences();
        if (canvasGroup == null || overlayRoot == null)
            return;

        isVisible = true;
        currentTarget = null;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (instructionText != null)
            instructionText.text = "Place this customer in the highlighted seat";

        if (tapToContinueText != null)
        {
            tapToContinueText.text = "Tap anywhere to continue";
            tapToContinueText.gameObject.SetActive(true);
        }

        Rect rootRect = overlayRoot.rect;
        Rect previewRect = new Rect(-180f, -110f, 360f, 220f);
        previewRect.position += rootRect.center;

        SetRect(dimTop, rootRect.xMin, previewRect.yMax, rootRect.xMax, rootRect.yMax);
        SetRect(dimBottom, rootRect.xMin, rootRect.yMin, rootRect.xMax, previewRect.yMin);
        SetRect(dimLeft, rootRect.xMin, previewRect.yMin, previewRect.xMin, previewRect.yMax);
        SetRect(dimRight, previewRect.xMax, previewRect.yMin, rootRect.xMax, previewRect.yMax);
        SetRect(highlightFrame, previewRect.xMin, previewRect.yMin, previewRect.xMax, previewRect.yMax);
        SetDimVisible(true);

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(true);

        arrowBasePosition = previewRect.center + new Vector2(0f, previewRect.height * 0.5f + 70f);
        handBasePosition = previewRect.center + new Vector2(previewRect.width * 0.5f + 64f, -previewRect.height * 0.5f - 64f);

        if (arrow != null)
        {
            arrow.gameObject.SetActive(true);
            arrow.anchoredPosition = arrowBasePosition;
        }

        if (hand != null)
        {
            hand.gameObject.SetActive(true);
            hand.anchoredPosition = handBasePosition;
        }
    }

    private void LateUpdate()
    {
        if (!isVisible)
            return;

        if (director != null)
            ApplyLayoutOffsets(director.CurrentStep);

        if (currentTarget != null)
            RefreshSpotlight();

        float wave = Mathf.Sin(Time.unscaledTime * animationSpeed);
        float scale = 1f + ((wave + 1f) * 0.5f * pulseAmount);

        if (arrow != null && arrow.gameObject.activeSelf)
        {
            arrow.anchoredPosition = arrowBasePosition + (Vector2.up * wave * bobDistance);
            arrow.localScale = Vector3.one * scale;
        }

        if (hand != null && hand.gameObject.activeSelf)
        {
            hand.anchoredPosition = handBasePosition + (Vector2.up * -wave * bobDistance);
            hand.localScale = Vector3.one * scale;
        }
    }

    private void HandleStepEntered(int stepIndex, TutorialStepData step)
    {
        ShowStep(step);
    }

    private void HandleStepExited(int stepIndex, TutorialStepData step)
    {
        HideImmediate();
    }

    private void ApplyLayoutOffsets(TutorialStepData step)
    {
        if (step == null)
            return;

        currentSpotlightOffset = step.SpotlightOffset;
        currentSpotlightSizeDelta = step.SpotlightSizeDelta;
        currentArrowOffset = step.ArrowOffset;
        currentHandOffset = step.HandOffset;

        if (instructionRect != null)
            instructionRect.anchoredPosition = instructionDefaultPosition + step.InstructionOffset;

        if (continuePromptRect != null)
            continuePromptRect.anchoredPosition =
                continuePromptDefaultPosition + step.ContinuePromptOffset;

        ApplyTypography(instructionText, step);
    }

    private void ApplyTypography(TMP_Text text, TutorialStepData step)
    {
        if (text == null || step == null)
            return;

        if (!step.OverrideTypography)
        {
            text.font = instructionDefaultFont;
            text.fontSize = instructionDefaultFontSize;
            text.fontStyle = instructionDefaultFontStyle;
            text.color = instructionDefaultColor;
            text.alignment = instructionDefaultAlignment;
            return;
        }

        text.font = step.FontAsset != null ? step.FontAsset : instructionDefaultFont;
        text.fontSize = step.FontSize;
        text.fontStyle = step.FontStyle;
        text.color = step.FontColor;
        text.alignment = step.TextAlignment;
    }

    private void RefreshSpotlight()
    {
        if (!TryGetTargetRect(currentTarget, out Rect targetRect))
        {
            ShowFullDim();
            return;
        }

        targetRect.xMin -= spotlightPadding;
        targetRect.xMax += spotlightPadding;
        targetRect.yMin -= spotlightPadding;
        targetRect.yMax += spotlightPadding;

        Vector2 adjustedCenter = targetRect.center + currentSpotlightOffset;
        Vector2 adjustedSize = new Vector2(
            Mathf.Max(1f, targetRect.width + currentSpotlightSizeDelta.x),
            Mathf.Max(1f, targetRect.height + currentSpotlightSizeDelta.y));
        targetRect = new Rect(adjustedCenter - (adjustedSize * 0.5f), adjustedSize);

        Rect rootRect = overlayRoot.rect;
        targetRect.xMin = Mathf.Clamp(targetRect.xMin, rootRect.xMin, rootRect.xMax);
        targetRect.xMax = Mathf.Clamp(targetRect.xMax, rootRect.xMin, rootRect.xMax);
        targetRect.yMin = Mathf.Clamp(targetRect.yMin, rootRect.yMin, rootRect.yMax);
        targetRect.yMax = Mathf.Clamp(targetRect.yMax, rootRect.yMin, rootRect.yMax);

        SetRect(dimTop, rootRect.xMin, targetRect.yMax, rootRect.xMax, rootRect.yMax);
        SetRect(dimBottom, rootRect.xMin, rootRect.yMin, rootRect.xMax, targetRect.yMin);
        SetRect(dimLeft, rootRect.xMin, targetRect.yMin, targetRect.xMin, targetRect.yMax);
        SetRect(dimRight, targetRect.xMax, targetRect.yMin, rootRect.xMax, targetRect.yMax);
        SetRect(highlightFrame, targetRect.xMin, targetRect.yMin, targetRect.xMax, targetRect.yMax);

        Vector2 center = targetRect.center;
        arrowBasePosition = center +
                            new Vector2(0f, (targetRect.height * 0.5f) + 70f) +
                            currentArrowOffset;
        handBasePosition = center +
                           new Vector2((targetRect.width * 0.5f) + 64f, -(targetRect.height * 0.5f) - 64f) +
                           currentHandOffset;

        if (arrow != null)
            arrow.anchoredPosition = arrowBasePosition;

        if (hand != null)
            hand.anchoredPosition = handBasePosition;
    }

    private bool TryGetTargetRect(TutorialTargetAnchor target, out Rect localRect)
    {
        localRect = default;
        if (target == null || overlayRoot == null || tutorialCanvas == null)
            return false;

        Camera canvasCamera = tutorialCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : tutorialCanvas.worldCamera;

        RectTransform targetRect = target.TargetTransform as RectTransform;
        if (targetRect != null)
        {
            targetRect.GetWorldCorners(worldCorners);
            Vector2 minScreen = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCorners[0]);
            Vector2 maxScreen = minScreen;

            for (int i = 1; i < worldCorners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCorners[i]);
                minScreen = Vector2.Min(minScreen, screenPoint);
                maxScreen = Vector2.Max(maxScreen, screenPoint);
            }

            return TryScreenRectToLocal(minScreen, maxScreen, canvasCamera, out localRect);
        }

        Camera worldCamera = Camera.main;
        Collider2D targetCollider = target.GetComponentInChildren<Collider2D>();
        Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
        if ((targetCollider != null || targetRenderer != null) && worldCamera != null)
        {
            Bounds bounds = targetCollider != null
                ? targetCollider.bounds
                : targetRenderer.bounds;
            TutorialStepData step = director != null ? director.CurrentStep : null;
            if (step != null && step.PresentationSpace == TutorialPresentationSpace.World)
            {
                bounds.center += (Vector3)step.WorldHighlightOffset;
                Vector3 adjustedSize = bounds.size + (Vector3)step.WorldHighlightSizeDelta;
                bounds.size = new Vector3(
                    Mathf.Max(0.01f, adjustedSize.x),
                    Mathf.Max(0.01f, adjustedSize.y),
                    Mathf.Max(0.01f, adjustedSize.z));
            }

            Vector2 minScreen = worldCamera.WorldToScreenPoint(bounds.min);
            Vector2 maxScreen = worldCamera.WorldToScreenPoint(bounds.max);
            return TryScreenRectToLocal(
                Vector2.Min(minScreen, maxScreen),
                Vector2.Max(minScreen, maxScreen),
                canvasCamera,
                out localRect);
        }

        if (worldCamera == null)
            return false;

        Vector2 fallbackCenter = worldCamera.WorldToScreenPoint(target.TargetTransform.position);
        return TryScreenRectToLocal(
            fallbackCenter - new Vector2(60f, 60f),
            fallbackCenter + new Vector2(60f, 60f),
            canvasCamera,
            out localRect);
    }

    private bool TryScreenRectToLocal(
        Vector2 minScreen,
        Vector2 maxScreen,
        Camera canvasCamera,
        out Rect localRect)
    {
        localRect = default;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRoot,
                minScreen,
                canvasCamera,
                out Vector2 minLocal))
            return false;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRoot,
                maxScreen,
                canvasCamera,
                out Vector2 maxLocal))
            return false;

        localRect = Rect.MinMaxRect(
            Mathf.Min(minLocal.x, maxLocal.x),
            Mathf.Min(minLocal.y, maxLocal.y),
            Mathf.Max(minLocal.x, maxLocal.x),
            Mathf.Max(minLocal.y, maxLocal.y));
        return true;
    }

    private void ShowFullDim()
    {
        if (overlayRoot == null)
            return;

        Rect rootRect = overlayRoot.rect;
        SetRect(dimTop, rootRect.xMin, rootRect.yMin, rootRect.xMax, rootRect.yMax);
        SetRect(dimBottom, 0f, 0f, 0f, 0f);
        SetRect(dimLeft, 0f, 0f, 0f, 0f);
        SetRect(dimRight, 0f, 0f, 0f, 0f);

        if (highlightFrame != null)
            highlightFrame.gameObject.SetActive(false);

        if (arrow != null)
            arrow.gameObject.SetActive(false);

        if (hand != null)
            hand.gameObject.SetActive(false);
    }

    private void SetDimVisible(bool visible)
    {
        if (dimTop != null) dimTop.gameObject.SetActive(visible);
        if (dimBottom != null) dimBottom.gameObject.SetActive(visible);
        if (dimLeft != null) dimLeft.gameObject.SetActive(visible);
        if (dimRight != null) dimRight.gameObject.SetActive(visible);
    }

    private void ApplyInputPolicy(TutorialAction requiredAction)
    {
        bool catchFullScreen = requiredAction == TutorialAction.None ||
                               requiredAction == TutorialAction.TapAnywhere ||
                               requiredAction == TutorialAction.Wait;

        SetGraphicRaycast(inputCatcher, catchFullScreen);
        SetDimRaycast(!catchFullScreen);
    }

    private void SetDimRaycast(bool value)
    {
        SetGraphicRaycast(dimTop, value);
        SetGraphicRaycast(dimBottom, value);
        SetGraphicRaycast(dimLeft, value);
        SetGraphicRaycast(dimRight, value);
    }

    private static void SetGraphicRaycast(Component component, bool value)
    {
        if (component != null && component.TryGetComponent(out Graphic graphic))
            graphic.raycastTarget = value;
    }

    private void EnsureReferences()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Graphic overlayGraphic = GetComponent<Graphic>();
        if (overlayGraphic != null && overlayGraphic != inputCatcher)
            overlayGraphic.raycastTarget = false;

        if (tutorialCanvas == null)
            tutorialCanvas = GetComponentInParent<Canvas>();

        if (overlayRoot == null)
            overlayRoot = transform as RectTransform;

        if (instructionText != null && instructionRect == null)
        {
            instructionRect = instructionText.rectTransform;
            instructionDefaultPosition = instructionRect.anchoredPosition;
            instructionDefaultFont = instructionText.font;
            instructionDefaultFontSize = instructionText.fontSize;
            instructionDefaultFontStyle = instructionText.fontStyle;
            instructionDefaultColor = instructionText.color;
            instructionDefaultAlignment = instructionText.alignment;
        }

        if (tapToContinueText != null && continuePromptRect == null)
        {
            continuePromptRect = tapToContinueText.rectTransform;
            continuePromptDefaultPosition = continuePromptRect.anchoredPosition;
        }
    }

    private static void SetRect(RectTransform target, float xMin, float yMin, float xMax, float yMax)
    {
        if (target == null)
            return;

        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = new Vector2(0.5f, 0.5f);
        target.anchoredPosition = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
        target.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
    }
}
