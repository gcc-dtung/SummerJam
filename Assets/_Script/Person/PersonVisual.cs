using System;
using UnityEngine;

public class PersonVisual : MonoBehaviour
{
    private Person person;
    private SpriteRenderer sprite;
    private Color baseColor;
    private void Awake()
    {
        sprite = this.GetComponent<SpriteRenderer>();
        person = this.GetComponent<Person>();
        baseColor = sprite.color;
    }

    private void OnEnable()
    {
        Test.Instance.AddListener("Checking",ChangeStatus);
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
    
}
