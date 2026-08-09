using System;
using TMPro;
using UnityEngine;

public enum TutorialAction
{
    None = 0,
    TapAnywhere = 1,
    TapTarget = 2,
    Wait = 3,
    PressPerson = 4,
    StartDragPerson = 5,
    PlacePersonOnTarget = 6,
    UseBooster = 7
}

public enum TutorialPresentationSpace
{
    Screen = 0,
    World = 1
}

[Serializable]
public sealed class TutorialStepData
{
    [SerializeField] private string id;
    [SerializeField, TextArea(2, 5)] private string instruction;
    [SerializeField] private TutorialAction requiredAction;
    [SerializeField] private TutorialTargetId sourceTarget;
    [SerializeField] private TutorialTargetId destinationTarget;
    [SerializeField, Min(0f)] private float minimumDisplayTime;

    [Header("Presentation")]
    [SerializeField] private bool dimBackground = true;
    [SerializeField] private bool showArrow;
    [SerializeField] private bool showHand;
    [SerializeField] private bool keepTargetHighlighted;

    [Header("Presentation Space")]
    [Tooltip("Screen is intended for menu/UI targets. World follows gameplay targets in world units.")]
    [SerializeField] private TutorialPresentationSpace presentationSpace;

    [Header("Per-Step Layout Offsets")]
    [Tooltip("Moves the spotlight in reference-canvas pixels without changing its target.")]
    [SerializeField] private Vector2 spotlightOffset;
    [Tooltip("Adds to the spotlight width and height. Negative values shrink it.")]
    [SerializeField] private Vector2 spotlightSizeDelta;
    [Tooltip("Moves the main instruction text from its authored RectTransform position.")]
    [SerializeField] private Vector2 instructionOffset;
    [Tooltip("Moves the tap-to-continue prompt from its authored RectTransform position.")]
    [SerializeField] private Vector2 continuePromptOffset;
    [Tooltip("Moves the arrow relative to its automatically calculated position.")]
    [SerializeField] private Vector2 arrowOffset;
    [Tooltip("Moves the hand relative to its automatically calculated position.")]
    [SerializeField] private Vector2 handOffset;

    [Header("World-Space Layout")]
    [Tooltip("Moves the world highlight relative to the target, in world units.")]
    [SerializeField] private Vector2 worldHighlightOffset;
    [Tooltip("Adds to the target bounds width and height, in world units.")]
    [SerializeField] private Vector2 worldHighlightSizeDelta;
    [Tooltip("Instruction position relative to the target, in world units.")]
    [SerializeField] private Vector2 worldInstructionOffset;
    [Tooltip("Additional arrow offset from its automatic position, in world units.")]
    [SerializeField] private Vector2 worldArrowOffset;
    [Tooltip("Additional hand offset from its automatic position, in world units.")]
    [SerializeField] private Vector2 worldHandOffset;

    [Header("Per-Step Typography")]
    [Tooltip("Enable to override the instruction typography for this step only.")]
    [SerializeField] private bool overrideTypography;
    [Tooltip("Optional TMP font. Leave empty to keep the TutorialView font.")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField, Min(1f)] private float fontSize = 48f;
    [SerializeField] private FontStyles fontStyle = FontStyles.Normal;
    [SerializeField] private Color fontColor = Color.white;
    [SerializeField] private TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;

    public string Id => id;
    public string Instruction => instruction;
    public TutorialAction RequiredAction => requiredAction;
    public TutorialTargetId SourceTarget => sourceTarget;
    public TutorialTargetId DestinationTarget => destinationTarget;
    public float MinimumDisplayTime => minimumDisplayTime;
    public bool DimBackground => dimBackground;
    public bool ShowArrow => showArrow;
    public bool ShowHand => showHand;
    public bool KeepTargetHighlighted => keepTargetHighlighted;
    public TutorialPresentationSpace PresentationSpace => presentationSpace;
    public Vector2 SpotlightOffset => spotlightOffset;
    public Vector2 SpotlightSizeDelta => spotlightSizeDelta;
    public Vector2 InstructionOffset => instructionOffset;
    public Vector2 ContinuePromptOffset => continuePromptOffset;
    public Vector2 ArrowOffset => arrowOffset;
    public Vector2 HandOffset => handOffset;
    public Vector2 WorldHighlightOffset => worldHighlightOffset;
    public Vector2 WorldHighlightSizeDelta => worldHighlightSizeDelta;
    public Vector2 WorldInstructionOffset => worldInstructionOffset;
    public Vector2 WorldArrowOffset => worldArrowOffset;
    public Vector2 WorldHandOffset => worldHandOffset;
    public bool OverrideTypography => overrideTypography;
    public TMP_FontAsset FontAsset => fontAsset;
    public float FontSize => fontSize;
    public FontStyles FontStyle => fontStyle;
    public Color FontColor => fontColor;
    public TextAlignmentOptions TextAlignment => textAlignment;
}
