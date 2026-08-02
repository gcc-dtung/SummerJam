using System.Collections.Generic;
using Coffee.UIExtensions;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Transform winPanelParent;

    [Header("VFX")]
    [SerializeField] private UIParticle uiParticle;
    [SerializeField] private ParticleSystem winVfx;
    [SerializeField, Min(0f)] private float vfxStartDelay = 0.2f;
    [SerializeField, Min(0.1f)] private float vfxRenderScale = 40f;

    [Header("Win Layout")]
    [SerializeField] private Transform winLayoutRoot;
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

    [Header("Phase 1")] 
    [SerializeField] private Transform rootPhase1;
    [SerializeField] private CanvasGroup phase1CanvasGroup;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Button adsButton;

    [Header("Phase 2")] 
    [SerializeField] private Transform rootPhase2;
    [SerializeField] private CanvasGroup phase2CanvasGroup;

    private Sequence introSequence;
    private ParticleSystem[] winVfxSystems = new ParticleSystem[0];
    private Vector3 firstCharacterTargetScale = Vector3.one;
    private Vector3 secondCharacterTargetScale = Vector3.one;
    private Vector3[] letterTargetScales = new Vector3[0];

    private void Awake()
    {
        winPanelImage ??= winPanelParent.GetComponent<Image>();
        phase1CanvasGroup = GetOrAddCanvasGroup(rootPhase1, phase1CanvasGroup);
        phase2CanvasGroup = GetOrAddCanvasGroup(rootPhase2, phase2CanvasGroup);
        uiParticle ??= winPanelParent.GetComponentInChildren<UIParticle>(true);
        winVfx ??= FindWinVfx();
        ResolveWinLayoutReferences();
        CacheWinLayoutTargetScales();

        ConfigureVfx();
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

        rewardButton.onClick.RemoveListener(OnPressRewardButton);
        adsButton.onClick.RemoveListener(OnPressAdsButton);
    }

    private void Start()
    {
        ConfigureVfx();
        HideImmediate();
    }

    public void OnWin()
    {
        introSequence.Stop();
        phaseTransitionAnimation.Stop();

        winPanelParent.gameObject.SetActive(true);

        rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        SetPanelAlpha(panelStartAlpha);
        ConfigureVfx();
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
        introSequence.Stop();
        phaseTransitionAnimation.Stop();
        StopAndClearVfx();

        if (rootPhase1 != null)
            rootPhase1.gameObject.SetActive(false);

        if (rootPhase2 != null)
            rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        if (winPanelImage != null)
            SetPanelAlpha(panelStartAlpha);

        if (winPanelParent != null)
            winPanelParent.gameObject.SetActive(false);
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

        foreach (ParticleSystem particleSystem in winVfxSystems)
            particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void PlayVfx()
    {
        if (uiParticle != null)
        {
            uiParticle.Clear();
            uiParticle.Play();
            return;
        }

        foreach (ParticleSystem particleSystem in winVfxSystems)
        {
            particleSystem.Clear(false);
            particleSystem.Play(false);
        }
    }

    private void ConfigureVfx()
    {
        List<ParticleSystem> validParticleSystems = new List<ParticleSystem>();

        if (uiParticle != null)
            uiParticle.scale = vfxRenderScale;

        if (winVfx != null)
        {
            ParticleSystem[] particleSystems = winVfx.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (HasRenderableMaterial(particleSystem))
                {
                    validParticleSystems.Add(particleSystem);
                    continue;
                }

                DisableInvisibleEmitter(particleSystem);
            }
        }

        winVfxSystems = validParticleSystems.ToArray();
    }

    private static void DisableInvisibleEmitter(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
            return;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = false;
        particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private static bool HasRenderableMaterial(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
            return false;

        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        return particleRenderer != null && particleRenderer.sharedMaterial != null;
    }

    private void PrepareWinIntro()
    {
        if (winLayoutRoot != null)
            winLayoutRoot.gameObject.SetActive(true);

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

    private void ResolveWinLayoutReferences()
    {
        winLayoutRoot ??= FindChildByName(winPanelParent, "Win Layout");
        lightImage ??= FindChildImage(winLayoutRoot, "light");
        dropLightImage ??= FindChildImage(winLayoutRoot, "drop light");
        firstCharacter ??= FindChildByName(winLayoutRoot, "char 1");
        secondCharacter ??= FindChildByName(winLayoutRoot, "char 2");
        wellDoneRoot ??= FindChildByName(winLayoutRoot, "well done text");
        wellDoneRoot ??= FindChildByName(winLayoutRoot, "Well Done root");

        if ((wellDoneLetters == null || wellDoneLetters.Length == 0) && wellDoneRoot != null)
            wellDoneLetters = FindLetterImages(wellDoneRoot);

        wellDoneLetters ??= new Image[0];
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

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return null;
    }

    private static Image FindChildImage(Transform root, string childName)
    {
        Transform child = FindChildByName(root, childName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private static Image[] FindLetterImages(Transform root)
    {
        List<Image> letters = new List<Image>();

        for (int index = 0; index < root.childCount; index++)
        {
            Transform child = root.GetChild(index);
            if (child.TryGetComponent(out Image letter))
                letters.Add(letter);
        }

        if (letters.Count == 0 && root.TryGetComponent(out Image completeTextImage))
            letters.Add(completeTextImage);

        return letters.ToArray();
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

    private ParticleSystem FindWinVfx()
    {
        ParticleSystem[] particleSystems = winPanelParent.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem.name == "WinPartical_UI")
                return particleSystem;
        }

        return particleSystems.Length > 0 ? particleSystems[0] : null;
    }

    private static CanvasGroup GetOrAddCanvasGroup(Transform root, CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
            return canvasGroup;

        if (root.TryGetComponent(out CanvasGroup existingCanvasGroup))
            return existingCanvasGroup;

        return root.gameObject.AddComponent<CanvasGroup>();
    }
}
