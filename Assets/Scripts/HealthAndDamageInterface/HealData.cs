using UnityEngine;

public struct HealData
{
    public float Amount;
    public object Source; // e.g., the potion or the medic

    public HealData(float amount, object source)
    {
        Amount = amount;
        Source = source;
    }
}