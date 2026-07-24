using System;
using PrimeTween;
using UnityEngine;

[Serializable]
public class PhaseTransitionAnimation
{
    [Header("Scale")]
    [SerializeField] private Vector3 phaseOutScale = new Vector3(0.92f, 0.92f, 1f);
    [SerializeField] private Vector3 phaseInStartScale = new Vector3(1.15f, 1.15f, 1f);
    [SerializeField] private Vector3 phaseNormalScale = Vector3.one;

    [Header("Duration")]
    [SerializeField] private float phaseOutDuration = 0.18f;
    [SerializeField] private float phaseInDuration = 0.3f;

    [Header("Ease")]
    [SerializeField] private Ease phaseOutEase = Ease.InCubic;
    [SerializeField] private Ease phaseInScaleEase = Ease.OutBack;
    [SerializeField] private Ease phaseInFadeEase = Ease.OutCubic;

    private Sequence sequence;

    public void Stop()
    {
        sequence.Stop();
    }

    public void Play(
        Transform fromRoot,
        CanvasGroup fromCanvasGroup,
        Transform toRoot,
        CanvasGroup toCanvasGroup)
    {
        Stop();

        CanvasGroupUtility.SetInteractable(fromCanvasGroup, false);
        CanvasGroupUtility.SetInteractable(toCanvasGroup, false);

        toRoot.gameObject.SetActive(true);
        toRoot.localScale = phaseInStartScale;
        toCanvasGroup.alpha = 0f;

        sequence = Sequence.Create()
            .Group(Tween.Alpha(
                fromCanvasGroup,
                endValue: 0f,
                duration: phaseOutDuration,
                ease: phaseOutEase
            ))
            .Group(Tween.Scale(
                fromRoot,
                endValue: phaseOutScale,
                duration: phaseOutDuration,
                ease: phaseOutEase
            ))
            .ChainCallback(() =>
            {
                fromRoot.gameObject.SetActive(false);
            })
            .Chain(Tween.Alpha(
                toCanvasGroup,
                endValue: 1f,
                duration: phaseInDuration,
                ease: phaseInFadeEase
            ))
            .Group(Tween.Scale(
                toRoot,
                endValue: phaseNormalScale,
                duration: phaseInDuration,
                ease: phaseInScaleEase
            ))
            .ChainCallback(() =>
            {
                CanvasGroupUtility.SetInteractable(toCanvasGroup, true);
            });
    }
}
