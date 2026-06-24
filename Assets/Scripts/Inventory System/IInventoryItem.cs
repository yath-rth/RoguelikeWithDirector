using System;
using UnityEngine;

/**
* Any object that needs to be held in the inventory needs to follow this
*/
public interface IInventoryItem
{
    public int index { get; }
    public string displayName { get; }
    public Sprite image { get; }
    public int quantity { get; }

    public abstract void EquipItem();
    public abstract void AddItem();
}