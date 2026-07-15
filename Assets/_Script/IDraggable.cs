using UnityEngine;

public interface IDraggable
{
    void StartDrag();
    void Drag(Vector3 dragPosition);
    void Drop(Vector3 endPosition);
}
