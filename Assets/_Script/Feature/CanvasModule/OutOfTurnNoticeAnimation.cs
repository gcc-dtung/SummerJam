using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class OutOfTurnNoticeAnimation
{
    [Header("References")]
    [SerializeField] private RectTransform notice;
    [SerializeField] private Image noticeImage;

    [Header("Positions")]
    [SerializeField] private Vector2 startPosition = new Vector2(0f, 700f);
    [SerializeField] private Vector2 showPosition = new Vector2(0f, 180f);
    [SerializeField] private Vector2 jumpPosition = new Vector2(0f, 250f);
    [SerializeField] private Vector2 endPosition = new Vector2(0f, -250f);

    [Header("Durations")]
    [SerializeField] private float dropDuration = 0.45f;
    [SerializeField] private float jumpDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.45f;

    [Header("Delay")]
    [SerializeField] private float readDelay = 0.6f;

    [Header("Alpha")]
    [SerializeField] private float startAlpha = 1f;
    [SerializeField] private float jumpAlpha = 0.65f;
    [SerializeField] private float endAlpha = 0f;

    [Header("Ease")]
    [SerializeField] private Ease dropEase = Ease.OutBounce;
    [SerializeField] private Ease jumpEase = Ease.OutSine;
    [SerializeField] private Ease fallEase = Ease.InCubic;
    [SerializeField] private Ease fadeEase = Ease.InCubic;

    private Sequence sequence;

    public void Hide()
    {
        notice.gameObject.SetActive(false);
    }

    public void Stop()
    {
        sequence.Stop();
    }

    public void Play(Action onComplete)
    {
        Stop();

        notice.gameObject.SetActive(true);
        notice.anchoredPosition = startPosition;
        SetAlpha(startAlpha);

        sequence = Sequence.Create()
            .Chain(Tween.UIAnchoredPosition(
                notice,
                showPosition,
                duration: dropDuration,
                ease: dropEase
            ))
            .ChainDelay(readDelay)
            .Chain(Tween.UIAnchoredPosition(
                notice,
                jumpPosition,
                duration: jumpDuration,
                ease: jumpEase
            ))
            .Group(Tween.Alpha(
                noticeImage,
                endValue: jumpAlpha,
                duration: jumpDuration,
                ease: fadeEase
            ))
            .Chain(Tween.UIAnchoredPosition(
                notice,
                endPosition,
                duration: fallDuration,
                ease: fallEase
            ))
            .Group(Tween.Alpha(
                noticeImage,
                endValue: endAlpha,
                duration: fallDuration,
                ease: fadeEase
            ))
            .ChainCallback(() =>
            {
                notice.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    private void SetAlpha(float alpha)
    {
        Color color = noticeImage.color;
        color.a = alpha;
        noticeImage.color = color;
    }
}
