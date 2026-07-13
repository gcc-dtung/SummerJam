using UnityEngine;

public class Cell : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public CellType Type { get; private set; }
    public bool CanSeat { get; set; }
    public bool CanInteract { get; set; }
    public CellDataSO Data { get; private set; }
    public Person CurrentPerson { get; private set; }
    private SpriteRenderer _spriteRenderer;
    public CellEventHandler CellEventHandler { get; private set; }
    private void Awake()
    {
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        CellEventHandler = this.GetComponent<CellEventHandler>();
    }

    public void Initialize(CellDataSO data)
    {
        Data = data;
        CanSeat = data.DefaultCanSeat;
        CanInteract = data.DefaultCanInteract;
        Type = data.Type;
        if (data.sprite != null) _spriteRenderer.sprite = data.sprite;
    }

    public void SetPersonToSeat(Person person)
    {
        CurrentPerson = person;
    }

    public void SetGridIndex(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }
}