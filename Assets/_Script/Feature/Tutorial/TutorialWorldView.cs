using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialWorldView : MonoBehaviour
{
    private const float PixelsPerWorldUnit = 100f;

    private TutorialDirector director;
    private Canvas screenCanvas;
    private RectTransform screenRoot;
    private Canvas worldCanvas;
    private RectTransform canvasRect;
    private RectTransform highlightFrame;
    private RectTransform arrow;
    private RectTransform hand;
    private TMP_Text instructionText;
    private TMP_FontAsset defaultFont;
    private float defaultFontSize;
    private FontStyles defaultFontStyle;
    private Color defaultFontColor;
    private TextAlignmentOptions defaultAlignment;
    private TutorialStepData currentStep;
    private TutorialTargetAnchor currentTarget;
    private Vector2 arrowBasePosition;
    private Vector2 handBasePosition;

    public TutorialDirector Director => director;
    public TutorialStepData CurrentStep => currentStep;
    public TutorialTargetAnchor CurrentTarget => currentTarget;

    public bool TryGetCurrentTargetBounds(out Bounds bounds)
    {
        if (currentTarget == null)
        {
            bounds = default;
            return false;
        }

        return TryGetWorldBounds(currentTarget, out bounds);
    }

    public void Initialize(
        TutorialDirector tutorialDirector,
        Canvas screenCanvas,
        RectTransform highlightTemplate,
        RectTransform arrowTemplate,
        RectTransform handTemplate,
        TMP_Text instructionTemplate)
    {
        director = tutorialDirector;
        this.screenCanvas = screenCanvas;
        screenRoot = instructionTemplate != null
            ? instructionTemplate.rectTransform.parent as RectTransform
            : screenCanvas != null ? screenCanvas.transform as RectTransform : null;

        GameObject canvasObject = new GameObject(
            "[TutorialWorldCanvas]",
            typeof(RectTransform),
            typeof(Canvas));
        canvasRect = canvasObject.GetComponent<RectTransform>();
        worldCanvas = canvasObject.GetComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = Camera.main;
        worldCanvas.overrideSorting = true;
        worldCanvas.sortingOrder = screenCanvas != null
            ? screenCanvas.sortingOrder + 10
            : 100;
        canvasRect.sizeDelta = new Vector2(1000f, 1000f);
        canvasRect.localScale = Vector3.one / PixelsPerWorldUnit;

        highlightFrame = CloneRect(highlightTemplate, canvasRect, "WorldHighlight");
        arrow = CloneRect(arrowTemplate, canvasRect, "WorldArrow");
        hand = CloneRect(handTemplate, canvasRect, "WorldHand");

        if (instructionTemplate != null)
        {
            // Screen Space Overlay always renders after a World Space Canvas.
            // Keep the position driven by world coordinates, but render the TMP text
            // on the overlay so the dim panels cannot darken or blur it.
            instructionText = Instantiate(instructionTemplate, screenRoot);
            instructionText.name = "WorldInstructionOverlay";
            instructionText.rectTransform.localScale = Vector3.one;
            instructionText.rectTransform.SetAsLastSibling();
            SetRaycastTarget(instructionText, false);
            defaultFont = instructionText.font;
            defaultFontSize = instructionText.fontSize;
            defaultFontStyle = instructionText.fontStyle;
            defaultFontColor = instructionText.color;
            defaultAlignment = instructionText.alignment;
        }

        Hide();
    }

    public void ShowStep(TutorialStepData step)
    {
        currentStep = step;
        currentTarget = null;

        if (step == null || step.PresentationSpace != TutorialPresentationSpace.World ||
            director == null || worldCanvas == null)
        {
            Hide();
            return;
        }

        TutorialTargetId targetId = step.RequiredAction == TutorialAction.PlacePersonOnTarget
            ? step.DestinationTarget
            : step.SourceTarget;
        if (targetId == TutorialTargetId.None ||
            !director.TargetRegistry.TryGetFirst(targetId, out currentTarget) ||
            currentTarget == null || currentTarget.TargetTransform is RectTransform)
        {
            Hide();
            return;
        }

        worldCanvas.gameObject.SetActive(true);
        RefreshVisuals();
    }

    public void Hide()
    {
        currentStep = null;
        currentTarget = null;
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        if (worldCanvas != null)
            worldCanvas.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentStep == null || currentTarget == null || worldCanvas == null ||
            !worldCanvas.gameObject.activeSelf)
            return;

        RefreshVisuals();

        float wave = Mathf.Sin(Time.unscaledTime * 3f);
        if (arrow != null && arrow.gameObject.activeSelf)
            arrow.anchoredPosition = arrowBasePosition + (Vector2.up * wave * 14f);

        if (hand != null && hand.gameObject.activeSelf)
            hand.anchoredPosition = handBasePosition + (Vector2.up * -wave * 14f);
    }

    private void RefreshVisuals()
    {
        if (!TryGetWorldBounds(currentTarget, out Bounds bounds))
            return;

        if (worldCanvas.worldCamera == null)
            worldCanvas.worldCamera = Camera.main;

        canvasRect.position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z);
        canvasRect.rotation = Quaternion.identity;

        Vector2 targetSize = new Vector2(bounds.size.x, bounds.size.y);
        Vector2 highlightSize = targetSize + currentStep.WorldHighlightSizeDelta;

        if (highlightFrame != null)
        {
            highlightFrame.gameObject.SetActive(currentStep.KeepTargetHighlighted);
            highlightFrame.anchoredPosition = currentStep.WorldHighlightOffset * PixelsPerWorldUnit;
            highlightFrame.sizeDelta = new Vector2(
                Mathf.Max(0.01f, highlightSize.x) * PixelsPerWorldUnit,
                Mathf.Max(0.01f, highlightSize.y) * PixelsPerWorldUnit);
        }

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = currentStep.Instruction;
            UpdateInstructionScreenPosition(bounds);
            ApplyTypography();
        }

        arrowBasePosition =
            (new Vector2(0f, bounds.extents.y + 0.7f) + currentStep.WorldArrowOffset) *
            PixelsPerWorldUnit;
        handBasePosition =
            (new Vector2(bounds.extents.x + 0.64f, -bounds.extents.y - 0.64f) +
             currentStep.WorldHandOffset) * PixelsPerWorldUnit;

        if (arrow != null)
        {
            arrow.gameObject.SetActive(currentStep.ShowArrow);
            arrow.anchoredPosition = arrowBasePosition;
        }

        if (hand != null)
        {
            hand.gameObject.SetActive(currentStep.ShowHand);
            hand.anchoredPosition = handBasePosition;
        }
    }

    private static bool TryGetWorldBounds(TutorialTargetAnchor target, out Bounds bounds)
    {
        Collider2D targetCollider = target.GetComponentInChildren<Collider2D>();
        if (targetCollider != null)
        {
            bounds = targetCollider.bounds;
            return true;
        }

        Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
        if (targetRenderer != null)
        {
            bounds = targetRenderer.bounds;
            return true;
        }

        bounds = new Bounds(target.TargetTransform.position, Vector3.one);
        return true;
    }

    private void ApplyTypography()
    {
        if (instructionText == null || currentStep == null)
            return;

        if (!currentStep.OverrideTypography)
        {
            instructionText.font = defaultFont;
            instructionText.fontSize = defaultFontSize;
            instructionText.fontStyle = defaultFontStyle;
            instructionText.color = defaultFontColor;
            instructionText.alignment = defaultAlignment;
            return;
        }

        instructionText.font = currentStep.FontAsset != null
            ? currentStep.FontAsset
            : defaultFont;
        instructionText.fontSize = currentStep.FontSize;
        instructionText.fontStyle = currentStep.FontStyle;
        instructionText.color = currentStep.FontColor;
        instructionText.alignment = currentStep.TextAlignment;
    }

    private void UpdateInstructionScreenPosition(Bounds targetBounds)
    {
        if (instructionText == null || screenRoot == null)
            return;

        Camera worldCamera = worldCanvas != null && worldCanvas.worldCamera != null
            ? worldCanvas.worldCamera
            : Camera.main;
        if (worldCamera == null)
            return;

        Vector3 instructionWorldPosition = targetBounds.center +
            new Vector3(
                currentStep.WorldInstructionOffset.x,
                currentStep.WorldInstructionOffset.y,
                0f);
        Vector2 screenPoint = worldCamera.WorldToScreenPoint(instructionWorldPosition);
        Camera uiCamera = screenCanvas != null &&
                          screenCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? screenCanvas.worldCamera
            : null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                screenRoot,
                screenPoint,
                uiCamera,
                out Vector2 localPoint))
        {
            instructionText.rectTransform.anchoredPosition = localPoint;
        }
    }

    private static RectTransform CloneRect(
        RectTransform template,
        RectTransform parent,
        string cloneName)
    {
        if (template == null)
            return null;

        RectTransform clone = Instantiate(template, parent);
        clone.name = cloneName;
        clone.localScale = Vector3.one;
        SetRaycastTarget(clone, false);
        return clone;
    }

    private static void SetRaycastTarget(Component component, bool value)
    {
        if (component != null && component.TryGetComponent(out Graphic graphic))
            graphic.raycastTarget = value;
    }

    private void OnDestroy()
    {
        if (instructionText != null)
            Destroy(instructionText.gameObject);

        if (worldCanvas != null)
            Destroy(worldCanvas.gameObject);
    }
}
