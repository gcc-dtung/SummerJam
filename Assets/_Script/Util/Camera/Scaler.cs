using System;
using UnityEngine;

public class Scaler : MonoBehaviour
{
    private Vector2 referenceResolution = new Vector2(1080, 1920);
    private Vector3 OriginalScale;
    private Camera mainCam;
    private Vector2 designViewportPos;
    private void Start()
    {
        OriginalScale = this.transform.localScale;
        mainCam = Camera.main;
        Calculate();
        UpdateNeo();
    }

    private void LateUpdate()
    {
        transform.localScale = OriginalScale * ScalerCalculation.ScaleFactor;
        UpdateNeo();
    }

    void UpdateNeo()
    {
        Vector3 worldPosition = mainCam.ViewportToWorldPoint(new Vector3(designViewportPos.x,designViewportPos.y, mainCam.nearClipPlane + 1f));
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
    }

    void Calculate()
    {
        float designHeight = 2f * mainCam.orthographicSize;
        float designWidth = designHeight * (referenceResolution.x / referenceResolution.y);
        Vector3 relativePos = transform.position - mainCam.transform.position;
        float u = (relativePos.x / designWidth) + 0.5f;
        float v = (relativePos.y / designHeight) + 0.5f; 
        designViewportPos = new Vector2(u, v);
    }
}