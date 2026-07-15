using UnityEngine;

public class Booster : MonoBehaviour,IDraggable,IPressable
{
    
    public void Press()
    {
        
    }
    
    public void StartDrag()
    {
        
    }

    public void Drag(Vector3 dragPosition)
    {
        this.transform.position = dragPosition;
    }

    public void Drop(Vector3 endPosition)
    {
        
    }
    
}
