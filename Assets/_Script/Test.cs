using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour, IDragHandler
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = mainCam.ScreenToWorldPoint(eventData.position);
    }
}
