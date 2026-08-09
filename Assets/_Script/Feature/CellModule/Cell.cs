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
    private CellVisual _cellVisual;
    public CellEventHandler CellEventHandler { get; private set; }
    private void Awake()
    {
        _spriteRenderer = this.GetComponentInChildren<SpriteRenderer>();
        _cellVisual = this.GetComponent<CellVisual>();
        CellEventHandler = this.GetComponent<CellEventHandler>();
    }

    public void Initialize(CellDataSO data, Vector2 cellSize)
    {
        Data = data;
        CanSeat = data.DefaultCanSeat;
        CanInteract = data.DefaultCanInteract;
        Type = data.Type;
        if (data.sprite != null) _spriteRenderer.sprite = data.sprite;
        _cellVisual?.SetCellSize(cellSize);
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

    public void OverrideCellType(CellType type)
    {
        Type = type;
    }
}
