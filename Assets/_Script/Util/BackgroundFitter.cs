using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AspectRatioFitter))]
[ExecuteAlways]
public class BackgroundFitter : MonoBehaviour
{
    private bool isFitting;
    private Image img;
    private AspectRatioFitter fitter;

    private void Awake()
    {
        img = GetComponent<Image>();
        fitter = GetComponent<AspectRatioFitter>();
    }

    private void Start()
    {
        Fit();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (isActiveAndEnabled)
        {
            Fit();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            Fit();
        }
    }
#endif

    private void Fit()
    {
        if (isFitting) return;
        RectTransform imageRect = transform as RectTransform;
        RectTransform cameraRect = imageRect
            ? imageRect.parent as RectTransform
            : null;

        if (!img.sprite || !imageRect || !cameraRect) return;

        isFitting = true;
        try
        {
            float ratio = img.sprite.rect.width / img.sprite.rect.height;
            fitter.aspectRatio = ratio;
            fitter.aspectMode = AspectRatioFitter.AspectMode.None;

            Vector2 imageSize = img.sprite.rect.size / img.pixelsPerUnit;
            Vector2 cameraSize = cameraRect.rect.size;

            if (imageSize.x <= 0f || imageSize.y <= 0f) return;

            float scale = Mathf.Max(
                1f,
                cameraSize.x / imageSize.x,
                cameraSize.y / imageSize.y
            );

            Vector2 targetSize = imageSize * scale;
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetSize.x);
            imageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetSize.y);
        }
        finally
        {
            isFitting = false;
        }
    }
}
