using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(AspectRatioFitter))]
[ExecuteAlways]
public class BackgroundFitter : MonoBehaviour
{
    private void Start()
    {
        Fit();
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
        Image img = GetComponent<Image>();
        AspectRatioFitter fitter = GetComponent<AspectRatioFitter>();
        
        if (img.sprite != null)
        {
            float ratio = img.sprite.rect.width / img.sprite.rect.height;
            fitter.aspectRatio = ratio;
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }
    }
}