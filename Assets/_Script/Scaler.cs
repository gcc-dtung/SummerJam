using System;
using UnityEngine;

public class Scaler : MonoBehaviour
{
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f); 
    private float referenceOrthoSize; 

    private Vector2 designViewportPos;
    private Vector3 referenceScale;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        referenceScale = transform.localScale;
        referenceOrthoSize = mainCam.orthographicSize;
        CalculateDesignViewport();
        ApplyAnchorAndScale();
    }
    
    // Test
    // void LateUpdate()
    // {
    //     ApplyAnchorAndScale();
    // }

    void CalculateDesignViewport()
    {
        float designHeight = 2f * referenceOrthoSize;
        float designWidth = designHeight * (referenceResolution.x / referenceResolution.y);
        Vector3 pos = transform.position; 
        float u = (pos.x / designWidth) + 0.5f;
        float v = (pos.y / designHeight) + 0.5f;
        designViewportPos = new Vector2(u, v);
    }

    void ApplyAnchorAndScale()
    {
        if (mainCam == null) return;
        Vector3 worldPos = mainCam.ViewportToWorldPoint(new Vector3(designViewportPos.x, designViewportPos.y, mainCam.nearClipPlane + 1f));
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        transform.localScale = referenceScale * ScalerCalculation.UniformScale;
    }
}
