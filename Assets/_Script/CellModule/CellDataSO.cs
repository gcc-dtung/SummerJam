using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CellDataSO : ScriptableObject
{
    public abstract string Name { get; protected set; }
    public abstract CellType Type { get; protected set; }
    public abstract bool DefaultCanSeat { get; protected set; }
    public abstract bool DefaultCanInteract { get; protected set; }
    public abstract Sprite sprite { get; protected set; }
}


