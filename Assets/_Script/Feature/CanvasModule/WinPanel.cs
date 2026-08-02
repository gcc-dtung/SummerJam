using System;
using Coffee.UIExtensions;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Transform winPanelParent;

    [Header("VFX")]
    [Tooltip("Assign the UIParticle component used by the win effect.")]
    [SerializeField] private UIParticle uiParticle;
    [Tooltip("Optional fallback. Assign every ParticleSystem that should play when UI Particle is not used.")]
    [SerializeField] private ParticleSystem[] winVfxSystems = new ParticleSystem[0];
    [SerializeField, Min(0f)] private float vfxStartDelay = 0.2f;
    [SerializeField, Min(0.1f)] private float vfxRenderScale = 40f;

    [Header("Win Layout")]
    [Tooltip("Assign the complete Win Layout root.")]
    [SerializeField] private Transform winLayoutRoot;
    [Tooltip("Add a CanvasGroup to Win Layout and assign it here so the complete layout can fade during outro.")]
    [SerializeField] private CanvasGroup winLayoutCanvasGroup;
    [SerializeField] private Image lightImage;
    [SerializeField] private Image dropLightImage;
    [SerializeField] private Transform firstCharacter;
    [SerializeField] private Transform secondCharacter;
    [SerializeField] private Transform wellDoneRoot;
    [SerializeField] private Image[] wellDoneLetters = new Image[0];

    [Header("Win Layout Timing")]
    [SerializeField, Min(0f)] private float lightStartTime;
    [SerializeField, Min(0f)] private float dropLightStartTime = 0.08f;
    [SerializeField, Min(0f)] private float lightFadeDuration = 0.3f;
    [SerializeField, Min(0f)] private float characterStartTime = 0.25f;
    [SerializeField, Min(0f)] private float characterStagger = 0.08f;
    [SerializeField, Min(0f)] private float characterPopDuration = 0.35f;
    [SerializeField] private Ease characterPopEase = Ease.OutBack;
    [SerializeField, Min(0f)] private float textStartTime = 0.55f;
    [SerializeField, Min(0f)] private float letterStagger = 0.07f;
    [SerializeField, Min(0f)] private float letterPopDuration = 0.28f;
    [SerializeField] private Ease letterPopEase = Ease.OutBack;
    [SerializeField, Min(0f)]
    [Tooltip("How many seconds before the Win Layout finishes Phase 1 should begin. Set to 0 to wait for the layout to finish.")]
    private float phase1Overlap = 0.15f;

    [Header("Intro Animation")]
    [Tooltip("Assign the Image on the WinPanel background.")]
    [SerializeField] private Image winPanelImage;
    [SerializeField] private float panelStartAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float panelTargetAlpha = 0.93f;
    [SerializeField] private float panelFadeDuration = 0.35f;
    [SerializeField] private Ease panelFadeEase = Ease.OutCubic;
    [SerializeField] private Vector3 phase1StartScale = new Vector3(1.2f, 1.2f, 1f);
    [SerializeField] private float phase1IntroDuration = 0.35f;
    [SerializeField] private Ease phase1ScaleEase = Ease.OutBack;
    [SerializeField] private Ease phase1FadeEase = Ease.OutCubic;

    [Header("Phase Transition")]
    [SerializeField] private PhaseTransitionAnimation phaseTransitionAnimation = new PhaseTransitionAnimation();

    [Header("Outro Animation")]
    [SerializeField, Min(0f)] private float outroDuration = 0.35f;
    [SerializeField] private Ease outroFadeEase = Ease.OutCubic;
    [SerializeField] private Ease outroScaleEase = Ease.OutCubic;

    [Header("Phase 1")] 
    [SerializeField] private Transform rootPhase1;
    [SerializeField] private CanvasGroup phase1CanvasGroup;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button adsButton;

    [Header("Phase 2")] 
    [SerializeField] private Transform rootPhase2;
    [SerializeField] private CanvasGroup phase2CanvasGroup;

    private Sequence introSequence;
    private Vector3 firstCharacterTargetScale = Vector3.one;
    private Vector3 secondCharacterTargetScale = Vector3.one;
    private Vector3[] letterTargetScales = new Vector3[0];
    private Sequence outroSequence;
    private bool isClosing;

    public bool IsVisible => winPanelParent != null && winPanelParent.gameObject.activeInHierarchy;

    private void Awake()
    {
        wellDoneLetters ??= Array.Empty<Image>();
        winVfxSystems ??= Array.Empty<ParticleSystem>();
        CacheWinLayoutTargetScales();

        if (uiParticle != null)
            uiParticle.scale = vfxRenderScale;

        StopAndClearVfx();
        HideImmediate();
    }

    private void OnEnable()
    {
        rewardButton.onClick.AddListener(OnPressRewardButton);
        adsButton.onClick.AddListener(OnPressAdsButton);
    }

    private void OnDisable()
    {
        introSequence.Stop();
        phaseTransitionAnimation.Stop();
        outroSequence.Stop();

        rewardButton.onClick.RemoveListener(OnPressRewardButton);
        adsButton.onClick.RemoveListener(OnPressAdsButton);
    }

    private void Start()
    {
        HideImmediate();
    }

    public void OnWin()
    {
        isClosing = false;
        introSequence.Stop();
        phaseTransitionAnimation.Stop();
        outroSequence.Stop();

        winPanelParent.gameObject.SetActive(true);

        rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        SetPanelAlpha(panelStartAlpha);
        StopAndClearVfx();

        PlayWinIntro();
    }

    private void OnPressRewardButton()
    {
        if (!phase1CanvasGroup.interactable)
            return;

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);

        // TODO : Anim Cong Vang
        EconomyManager.Instance.GetGold(LevelManager.Instance.CurrentLevel.Gold);
        NextPhase();
    }

    private void OnPressAdsButton()
    {
        if (!phase1CanvasGroup.interactable)
            return;

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);

        // TODO : Ads + Anim
        NextPhase();
    }

    public void HideImmediate()
    {
        isClosing = false;
        introSequence.Stop();
        phaseTransitionAnimation.Stop();
        outroSequence.Stop();
        StopAndClearVfx();

        if (rootPhase1 != null)
            rootPhase1.gameObject.SetActive(false);

        if (rootPhase2 != null)
            rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        if (winPanelImage != null)
            SetPanelAlpha(panelStartAlpha);

        if (winLayoutCanvasGroup != null)
            winLayoutCanvasGroup.alpha = 0f;

        if (winPanelParent != null)
            winPanelParent.gameObject.SetActive(false);
    }

    public void PlayOutro(Action onComplete)
    {
        if (isClosing)
            return;

        if (!IsVisible)
        {
            onComplete?.Invoke();
            return;
        }

        isClosing = true;
        introSequence.Stop();
        phaseTransitionAnimation.Stop();
        outroSequence.Stop();

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        Sequence sequence = Sequence.Create()
            .Group(Tween.Alpha(
                winPanelImage,
                endValue: panelStartAlpha,
                duration: outroDuration,
                ease: outroFadeEase
            ));

        if (winLayoutRoot != null && winLayoutCanvasGroup != null && winLayoutRoot.gameObject.activeSelf)
        {
            sequence = sequence.Group(Tween.Alpha(
                winLayoutCanvasGroup,
                endValue: 0f,
                duration: outroDuration,
                ease: outroFadeEase
            ));
        }

        sequence = AddPhaseOutro(sequence, rootPhase1, phase1CanvasGroup, true);
        sequence = AddPhaseOutro(sequence, rootPhase2, phase2CanvasGroup, false);

        outroSequence = sequence.ChainCallback(() =>
        {
            HideImmediate();
            onComplete?.Invoke();
        });
    }

    private void NextPhase()
    {
        phaseTransitionAnimation.Play(
            rootPhase1,
            phase1CanvasGroup,
            rootPhase2,
            phase2CanvasGroup
        );
    }

    private Sequence AddPhaseOutro(
        Sequence sequence,
        Transform phaseRoot,
        CanvasGroup phaseCanvasGroup,
        bool scaleToStart)
    {
        if (phaseRoot == null || phaseCanvasGroup == null || !phaseRoot.gameObject.activeSelf)
            return sequence;

        sequence = sequence.Group(Tween.Alpha(
            phaseCanvasGroup,
            endValue: 0f,
            duration: outroDuration,
            ease: outroFadeEase
        ));

        if (scaleToStart)
        {
            sequence = sequence.Group(Tween.Scale(
                phaseRoot,
                endValue: phase1StartScale,
                duration: outroDuration,
                ease: outroScaleEase
            ));
        }

        return sequence;
    }

    private void PlayWinIntro()
    {
        PrepareWinIntro();

        Sequence sequence = Sequence.Create()
            .Insert(0f, Tween.Alpha(
                winPanelImage,
                endValue: panelTargetAlpha,
                duration: panelFadeDuration,
                ease: panelFadeEase
            ))
            .InsertCallback(vfxStartDelay, this, target => target.PlayVfx());

        float layoutEndTime = Mathf.Max(panelFadeDuration, vfxStartDelay);

        if (lightImage != null)
        {
            sequence.Insert(lightStartTime, Tween.Alpha(
                lightImage,
                endValue: 1f,
                duration: lightFadeDuration,
                ease: Ease.OutCubic
            ));

            layoutEndTime = Mathf.Max(layoutEndTime, lightStartTime + lightFadeDuration);
        }

        if (dropLightImage != null)
        {
            sequence.Insert(dropLightStartTime, Tween.Alpha(
                dropLightImage,
                endValue: 1f,
                duration: lightFadeDuration,
                ease: Ease.OutCubic
            ));

            layoutEndTime = Mathf.Max(layoutEndTime, dropLightStartTime + lightFadeDuration);
        }

        if (firstCharacter != null)
        {
            sequence.Insert(characterStartTime, Tween.Scale(
                firstCharacter,
                endValue: firstCharacterTargetScale,
                duration: characterPopDuration,
                ease: characterPopEase
            ));

            layoutEndTime = Mathf.Max(layoutEndTime, characterStartTime + characterPopDuration);
        }

        if (secondCharacter != null)
        {
            float secondCharacterStartTime = characterStartTime + characterStagger;

            sequence.Insert(secondCharacterStartTime, Tween.Scale(
                secondCharacter,
                endValue: secondCharacterTargetScale,
                duration: characterPopDuration,
                ease: characterPopEase
            ));

            layoutEndTime = Mathf.Max(layoutEndTime, secondCharacterStartTime + characterPopDuration);
        }

        for (int index = 0; index < wellDoneLetters.Length; index++)
        {
            Image letter = wellDoneLetters[index];
            if (letter == null)
                continue;

            float letterStartTime = textStartTime + index * letterStagger;

            sequence.Insert(letterStartTime, Tween.Alpha(
                letter,
                endValue: 1f,
                duration: letterPopDuration * 0.5f,
                ease: Ease.OutCubic
            ));

            sequence.Insert(letterStartTime, Tween.Scale(
                letter.transform,
                endValue: letterTargetScales[index],
                duration: letterPopDuration,
                ease: letterPopEase
            ));

            layoutEndTime = Mathf.Max(layoutEndTime, letterStartTime + letterPopDuration);
        }

        float phase1StartTime = Mathf.Max(0f, layoutEndTime - phase1Overlap);

        sequence
            .Insert(phase1StartTime, Tween.Alpha(
                phase1CanvasGroup,
                endValue: 1f,
                duration: phase1IntroDuration,
                ease: phase1FadeEase
            ))
            .Insert(phase1StartTime, Tween.Scale(
                rootPhase1,
                endValue: Vector3.one,
                duration: phase1IntroDuration,
                ease: phase1ScaleEase
            ))
            .InsertCallback(
                phase1StartTime + phase1IntroDuration,
                phase1CanvasGroup,
                canvasGroup => CanvasGroupUtility.SetInteractable(canvasGroup, true)
            );

        introSequence = sequence;
    }

    private void StopAndClearVfx()
    {
        if (uiParticle != null)
        {
            uiParticle.Stop();
            uiParticle.Clear();
            return;
        }

        if (winVfxSystems == null)
            return;

        foreach (ParticleSystem particleSystem in winVfxSystems)
        {
            if (particleSystem == null)
                continue;

            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void PlayVfx()
    {
        if (uiParticle != null)
        {
            uiParticle.Clear();
            uiParticle.Play();
            return;
        }

        if (winVfxSystems == null)
            return;

        foreach (ParticleSystem particleSystem in winVfxSystems)
        {
            if (particleSystem == null)
                continue;

            particleSystem.Clear(false);
            particleSystem.Play(false);
        }
    }

    private void PrepareWinIntro()
    {
        if (winLayoutRoot != null)
            winLayoutRoot.gameObject.SetActive(true);

        if (winLayoutCanvasGroup != null)
            winLayoutCanvasGroup.alpha = 1f;

        SetActive(lightImage);
        SetActive(dropLightImage);
        SetActive(firstCharacter);
        SetActive(secondCharacter);
        SetActive(wellDoneRoot);

        SetImageAlpha(lightImage, 0f);
        SetImageAlpha(dropLightImage, 0f);

        if (firstCharacter != null)
            firstCharacter.localScale = Vector3.zero;

        if (secondCharacter != null)
            secondCharacter.localScale = Vector3.zero;

        for (int index = 0; index < wellDoneLetters.Length; index++)
        {
            Image letter = wellDoneLetters[index];
            if (letter == null)
                continue;

            letter.gameObject.SetActive(true);
            letter.transform.localScale = Vector3.zero;
            SetImageAlpha(letter, 0f);
        }

        rootPhase1.gameObject.SetActive(true);
        rootPhase1.localScale = phase1StartScale;
        phase1CanvasGroup.alpha = 0f;
        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
    }

    private void CacheWinLayoutTargetScales()
    {
        if (firstCharacter != null)
            firstCharacterTargetScale = firstCharacter.localScale;

        if (secondCharacter != null)
            secondCharacterTargetScale = secondCharacter.localScale;

        letterTargetScales = new Vector3[wellDoneLetters.Length];

        for (int index = 0; index < wellDoneLetters.Length; index++)
        {
            Image letter = wellDoneLetters[index];
            letterTargetScales[index] = letter != null
                ? letter.transform.localScale
                : Vector3.one;
        }
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private static void SetActive(Image image)
    {
        if (image != null)
            image.gameObject.SetActive(true);
    }

    private static void SetActive(Transform target)
    {
        if (target != null)
            target.gameObject.SetActive(true);
    }

    private void SetPanelAlpha(float alpha)
    {
        Color color = winPanelImage.color;
        color.a = alpha;
        winPanelImage.color = color;
    }

}
