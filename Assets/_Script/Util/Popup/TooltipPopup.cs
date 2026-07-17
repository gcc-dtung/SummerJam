using System;
using PrimeTween;
using TMPro;
using UnityEngine;

public class TooltipPopup : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private SpriteRenderer top;
    [SerializeField] private SpriteRenderer middle;
    [SerializeField] private SpriteRenderer bottom;


    [Header("Text")]
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private TextMeshPro contentText;

    [Header("Layout")]
    [SerializeField] private float textWidth = 2f;
    [SerializeField] private float verticalPadding = 0.15f;
    [SerializeField] private int textSortingOrder = 10;

    [Header("Content (Test)")]
    [SerializeField] private float contentHeight = 1f;

    [Header("Tween")]
    [SerializeField] private Transform tooltipTransform;

    [SerializeField] private float durationTween = 0.2f;
    
    private Sequence tooltipSequence;
    private bool isShow = false;

    private void Start()
    {
        Restart();
    }

    private void OnEnable()
    {
        EventBus.AddListener(GameEventType.PressOutSide, Hide);
        EventBus.AddListener(GameEventType.StartDragPerson, Hide);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener(GameEventType.PressOutSide, Hide);
        EventBus.RemoveListener(GameEventType.StartDragPerson, Hide);
    }


    public void Show(string personName, string content)
    {
        if (isShow)
        {
            Hide();
            return;
        }

        isShow = true;
        tooltipTransform.gameObject.SetActive(true);

        nameText.text = personName;
        contentText.text = content;

        contentText.textWrappingMode = TextWrappingModes.Normal;
        contentText.overflowMode = TextOverflowModes.Overflow;

        contentText.rectTransform.sizeDelta = new Vector2(textWidth, 100f);
        contentText.ForceMeshUpdate();

        float textHeight = contentText.GetPreferredValues(content, textWidth, Mathf.Infinity).y;
        float middleHeight = textHeight + verticalPadding * 2f;

        ResizeInternal(middleHeight);

        float bottomH  = bottom.sprite.bounds.size.y * bottom.transform.localScale.y;
        float actualMiddleH = middleHeight;
        float topH     = top.sprite.bounds.size.y    * top.transform.localScale.y;

        float middleCenterY = bottomH * 0.5f + actualMiddleH * 0.5f;

        contentText.alignment = TextAlignmentOptions.TopLeft;
        contentText.rectTransform.pivot = new Vector2(0f, 1f);
        contentText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
        contentText.transform.localPosition = new Vector3(
            -textWidth * 0.5f,
            middleCenterY + actualMiddleH * 0.5f - verticalPadding,
            -0.1f
        );

        float topCenterY = bottomH * 0.5f + actualMiddleH + topH * 0.5f;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.transform.localPosition = new Vector3(0f, topCenterY, -0.1f);

        nameText.sortingOrder  = textSortingOrder;
        contentText.sortingOrder = textSortingOrder;
        
        PlayTransition(toScale: 1f, toAlpha: 1f);
    }

    public void Hide()
    {
        isShow = false;
        PlayTransition(toScale: 0f, toAlpha: 0f,
            onComplete: () => tooltipTransform.gameObject.SetActive(false));
    }
    
    private void PlayTransition(float toScale, float toAlpha, System.Action onComplete = null)
    {
        if (tooltipSequence.isAlive)
            tooltipSequence.Stop();

        Sequence seq = Sequence.Create();
        bool hasAnyTween = false;

        if (!Mathf.Approximately(tooltipTransform.localScale.x, toScale))
        {
            Tween scaleTween = Tween.Scale(tooltipTransform, toScale, durationTween);
            seq = hasAnyTween ? seq.Group(scaleTween) : seq.Chain(scaleTween);
            hasAnyTween = true;
        }

        if (!Mathf.Approximately(top.color.a, toAlpha))
        {
            Color oldColor = top.color;
            Color newColor = oldColor;
            newColor.a = toAlpha;
            Tween alphaTween = Tween.Custom(oldColor, newColor, durationTween / 2, ChangeColor);
            seq = hasAnyTween ? seq.Group(alphaTween) : seq.Chain(alphaTween);
            hasAnyTween = true;
        }

        if (!hasAnyTween)
        {
            onComplete?.Invoke();
            return;
        }

        tooltipSequence = seq;
        if (onComplete != null)
            tooltipSequence.OnComplete(onComplete);
    }

    [ContextMenu("Test Resize")]
    public void TestResize()
    {
        ResizeInternal(contentHeight);
    }
    
    private void ResizeInternal(float desiredMiddleHeight)
    {
        float bottomH       = bottom.sprite.bounds.size.y * bottom.transform.localScale.y;
        float middleBaseH   = middle.sprite.bounds.size.y;
        float topH          = top.sprite.bounds.size.y    * top.transform.localScale.y;

        // Scale middle theo Y
        float scaleY = desiredMiddleHeight / middleBaseH;
        middle.transform.localScale = new Vector3(
            middle.transform.localScale.x,
            scaleY,
            middle.transform.localScale.z
        );

        float actualMiddleH = middleBaseH * scaleY;

        // Đặt vị trí local — Bottom làm gốc (0, 0, 0)
        bottom.transform.localPosition = Vector3.zero;

        middle.transform.localPosition = new Vector3(
            0f,
            bottomH * 0.5f + actualMiddleH * 0.5f,
            0f
        );

        top.transform.localPosition = new Vector3(
            0f,
            bottomH * 0.5f + actualMiddleH + topH * 0.5f,
            0f
        );      
    }
    private void ChangeColor(Color color)
    {
        top.color = color;
        bottom.color = color;
        middle.color = color;
    }

    void Restart()
    {
        Color newColor = Color.white;
        newColor.a = 0f;
        ChangeColor(newColor);
        tooltipTransform.localScale = Vector3.zero;
        tooltipTransform.gameObject.SetActive(false);
    }
}