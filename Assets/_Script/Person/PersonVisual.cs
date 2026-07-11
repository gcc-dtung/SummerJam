using System;
using NaughtyAttributes;
using PrimeTween;
using UnityEngine;

public class PersonVisual : MonoBehaviour
{
    [SerializeField] private float changeViewDuration;
    [SerializeField] private float changeViewScale;
    
    private Person person;
    private SpriteRenderer sprite;
    private Color baseColor;
    private float baseScale;
    private int baseOrderInLayer;
    
    private Tween scaleTween;
    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
        person = this.GetComponent<Person>();
        baseColor = sprite.color;
        // if(sprite == null) Debug.LogError("No sprite renderer");
    }

    private void OnEnable()
    {
        Test.Instance.AddListener("Checking",ChangeStatus);
    }

    private void Start()
    {
        baseScale = sprite.transform.localScale.x;
        baseOrderInLayer = sprite.sortingOrder;
    }

    private void OnDisable()
    {
        Test.Instance.RemoveListener("Checking",ChangeStatus);
    }

    private void ChangeStatus()
    {
        if(person.ConditionChecking.IsHappy) Happy();
        else Sad();
    }

    private void Normal() => sprite.color = baseColor;
    private void Happy() => sprite.color = Color.green;
    private void Sad() => sprite.color = Color.red;

    private void ChangeVisual(float viewScale, int orderInLayer)
    {
        if(scaleTween.isAlive)
            scaleTween.Stop();
        if(sprite.transform.localScale == Vector3.one * viewScale) return;
        sprite.sortingOrder = orderInLayer;
        scaleTween = Tween.Scale(sprite.transform, viewScale, changeViewDuration);
    }
    
    #region Public API
    [Button("VisualOnDrag")]
    public void ChangeVisualOnStartDrag()
    {
        ChangeVisual(changeViewScale, Constaints.MAX_SORTING_LAYER);
    }
    
    [Button("VisualEndDrag")]
    public void ChangeVisualEndDrag()
    {
        ChangeVisual(baseScale, baseOrderInLayer);
    }
    #endregion
}
