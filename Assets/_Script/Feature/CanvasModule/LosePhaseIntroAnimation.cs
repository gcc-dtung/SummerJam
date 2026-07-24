using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LosePhaseIntroAnimation
{
    [Header("Lose Panel Fade")]
    [SerializeField] private Image losePanelImage;
    [SerializeField] private float losePanelStartAlpha = 0f;
    [SerializeField] private float losePanelTargetAlpha = 0.65f;
    [SerializeField] private float losePanelFadeDuration = 0.35f;
    [SerializeField] private Ease losePanelFadeEase = Ease.OutCubic;

    [Header("Phase 1 Intro")]
    [SerializeField] private Vector3 phase1StartScale = new Vector3(1.2f, 1.2f, 1f);
    [SerializeField] private Vector3 phase1TargetScale = Vector3.one;
    [SerializeField] private float phase1IntroDuration = 0.35f;
    [SerializeField] private Ease phase1ScaleEase = Ease.OutBack;
    [SerializeField] private Ease phase1FadeEase = Ease.OutCubic;

    private Sequence sequence;

    public void PrepareLosePanel()
    {
        SetLosePanelAlpha(losePanelStartAlpha);
    }

    public void Stop()
    {
        sequence.Stop();
    }

    public void Play(
        Transform rootPhase1,
        Transform rootPhase2,
        CanvasGroup phase1CanvasGroup,
        CanvasGroup phase2CanvasGroup)
    {
        Stop();

        rootPhase1.gameObject.SetActive(true);
        rootPhase2.gameObject.SetActive(false);

        phase1CanvasGroup.alpha = 0f;
        rootPhase1.localScale = phase1StartScale;

        CanvasGroupUtility.SetInteractable(phase1CanvasGroup, false);
        CanvasGroupUtility.SetInteractable(phase2CanvasGroup, false);

        sequence = Sequence.Create()
            .Group(Tween.Alpha(
                losePanelImage,
                endValue: losePanelTargetAlpha,
                duration: losePanelFadeDuration,
                ease: losePanelFadeEase
            ))
            .Group(Tween.Alpha(
                phase1CanvasGroup,
                endValue: 1f,
                duration: phase1IntroDuration,
                ease: phase1FadeEase
            ))
            .Group(Tween.Scale(
                rootPhase1,
                endValue: phase1TargetScale,
                duration: phase1IntroDuration,
                ease: phase1ScaleEase
            ))
            .ChainCallback(() =>
            {
                CanvasGroupUtility.SetInteractable(phase1CanvasGroup, true);
            });
    }

    private void SetLosePanelAlpha(float alpha)
    {
        Color color = losePanelImage.color;
        color.a = alpha;
        losePanelImage.color = color;
    }
}
