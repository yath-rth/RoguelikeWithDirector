using UnityEngine;

public struct DamageData
{
    public float Amount;
    public object Source;
    public string DamageType;

    public DamageData(float amount, object source, string damageType = "Normal")
    {
        Amount = amount;
        Source = source;
        DamageType = damageType;
    }
}