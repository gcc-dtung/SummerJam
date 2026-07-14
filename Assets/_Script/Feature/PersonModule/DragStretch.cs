using UnityEngine;
using PrimeTween;

public class DragStretch : MonoBehaviour
{
    [SerializeField] float stretchStrength = 0.25f;
    [SerializeField] float maxStretch = 1.6f;
    [SerializeField] Camera gameCamera;

    Vector3 baseScale;
    bool dragging;

    void Awake()
    {
        if(gameCamera == null) gameCamera = Camera.main;
        baseScale = transform.localScale;
    }

    public void OnMouseDown() => dragging = true;

    public void OnMouseDrag()
    {
        Vector3 mouse = gameCamera.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = transform.position.z;

        Vector2 drag = mouse - transform.position;
        float amount = Mathf.Clamp(drag.magnitude * stretchStrength, 0f, maxStretch - 1f);

        // Xoay hướng thân nhân vật theo hướng kéo
        float angle = Mathf.Atan2(drag.y, drag.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // Kéo dài trục Y, bóp ngang trục X
        transform.localScale = new Vector3(
            baseScale.x * (1f - amount * 0.45f),
            baseScale.y * (1f + amount),
            baseScale.z
        );
    }

    public void OnMouseUp()
    {
        dragging = false;
        Tween.Scale(transform, baseScale, 0.18f, Ease.OutBack);
    }
}