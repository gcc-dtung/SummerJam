using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropController : MonoBehaviour
{
    private Camera mainCam;
    [SerializeField] private LayerMask draggableLayer;
    private float depthDistance = 10f;
    private IDraggable currentDragItem;
    private Vector2 currentPosition;
    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 screenMousePos = Input.mousePosition;
        screenMousePos.z = depthDistance; 
        currentPosition = mainCam.ScreenToWorldPoint(screenMousePos);
        if (Input.GetMouseButtonDown(0))
        {
            Detect();
            currentDragItem?.StartDrag();
        }
    
        if (Input.GetMouseButton(0))
        {
           currentDragItem?.Drag(currentPosition);
        }
    
        if (Input.GetMouseButtonUp(0))
        {
           currentDragItem?.Drop(currentPosition);
           currentDragItem = null;
        }
    }
    
    void Detect()
    {
      Collider2D col =  Physics2D.OverlapPoint(currentPosition,draggableLayer,-depthDistance,depthDistance);
      if (col != null && col.TryGetComponent<IDraggable>(out var draggable))
      {
          currentDragItem = draggable;
      }
    }


}
