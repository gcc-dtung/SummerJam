using System.Collections;
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
    [SerializeField, Min(0f)]
    [Tooltip("Seconds after the VFX starts before Phase 1 appears. The VFX keeps playing.")]
    private float phase1ShowDelay = 1.5f;

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
    [SerializeField] private Button contiueButton;

    private Coroutine winFlowCoroutine;
    private Sequence introSequence;
    private ParticleSystem[] winVfxSystems = new ParticleSystem[0];

    private void Awake()
    {
        winPanelImage ??= winPanelParent.GetComponent<Image>();
        phase1CanvasGroup = GetOrAddCanvasGroup(rootPhase1, phase1CanvasGroup);
        phase2CanvasGroup = GetOrAddCanvasGroup(rootPhase2, phase2CanvasGroup);
        uiParticle ??= winPanelParent.GetComponentInChildren<UIParticle>(true);
        winVfx ??= FindWinVfx();

        ConfigureVfx();
        StopAndClearVfx();
        HideImmediate();
    }

    private void OnEnable()
    {
        rewardButton.onClick.AddListener(OnPressRewardButton);
        adsButton.onClick.AddListener(OnPressAdsButton);
        contiueButton.onClick.AddListener(OnPressContiueButton);
    }

    private void OnDisable()
    {
        StopWinFlow();
        introSequence.Stop();
        phaseTransitionAnimation.Stop();

        rewardButton.onClick.RemoveListener(OnPressRewardButton);
        adsButton.onClick.RemoveListener(OnPressAdsButton);
        contiueButton.onClick.RemoveListener(OnPressContiueButton);
    }

    private void Start()
    {
        ConfigureVfx();
        HideImmediate();
    }

    public void OnWin()
    {
        StopWinFlow();
        introSequence.Stop();
        phaseTransitionAnimation.Stop();

        winPanelParent.gameObject.SetActive(true);

        rootPhase1.gameObject.SetActive(false);
        rootPhase2.gameObject.SetActive(false);

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        SetPanelAlpha(panelStartAlpha);
        ConfigureVfx();
        StopAndClearVfx();

        winFlowCoroutine = StartCoroutine(PlayWinFlow());
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

    private void OnPressContiueButton()
    {
        if (!phase2CanvasGroup.interactable)
            return;

        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        if (FlowManager.Instance != null)
            FlowManager.Instance.BackToMainMenu();
        else if (CanvasManager.Instance != null)
            CanvasManager.Instance.ChangeToMainMenu();

        HideImmediate();
    }

    public void HideImmediate()
    {
        StopWinFlow();
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

    private IEnumerator PlayWinFlow()
    {
        if (vfxStartDelay > 0f)
            yield return new WaitForSeconds(vfxStartDelay);

        PlayVfx();

        if (phase1ShowDelay > 0f)
            yield return new WaitForSeconds(phase1ShowDelay);

        winFlowCoroutine = null;

        PlayPhase1Intro();
    }

    private void StopWinFlow()
    {
        if (winFlowCoroutine == null)
            return;

        StopCoroutine(winFlowCoroutine);
        winFlowCoroutine = null;
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

    private void PlayPhase1Intro()
    {
        introSequence.Stop();

        rootPhase1.gameObject.SetActive(true);
        rootPhase1.localScale = phase1StartScale;
        phase1CanvasGroup.alpha = 0f;
        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);

        introSequence = Sequence.Create()
            .Group(Tween.Alpha(
                winPanelImage,
                endValue: panelTargetAlpha,
                duration: panelFadeDuration,
                ease: panelFadeEase
            ))
            .Group(Tween.Alpha(
                phase1CanvasGroup,
                endValue: 1f,
                duration: phase1IntroDuration,
                ease: phase1FadeEase
            ))
            .Group(Tween.Scale(
                rootPhase1,
                endValue: Vector3.one,
                duration: phase1IntroDuration,
                ease: phase1ScaleEase
            ))
            .ChainCallback(() =>
            {
                CanvasGroupUtility.SetInteractable(phase1CanvasGroup, true);
            });
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
