using UnityEngine;


public interface IDamageable
{
    void TakeDamage(float amount, DamageInfo info);
    bool IsAlive { get; }
}
public interface IWeapon
{
    string WeaponName { get; }
    bool CanFire { get; }

    void Equip(Transform owner);
    void Unequip();

    void StartFire();
    void StopFire();

    void Reload();
}
public interface IAmmoWeapon
{
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    int ReserveAmmo { get; }
    bool IsReloading { get; }
}
public interface IUpgradeableWeapon
{
    void UpgradeDamage(float flatBonus);
    void UpgradeFireRate(float multiplier);
    void UpgradeMagazineSize(int flatBonus);
    void UpgradeReloadSpeed(float multiplier);
}

public struct DamageInfo
{
    public GameObject Source;
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public bool IsCritical;
    public DamageType Type;

    public DamageInfo(GameObject source, Vector3 hitPoint, Vector3 hitNormal, DamageType type = DamageType.Generic, bool isCritical = false)
    {
        Source = source;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
        Type = type;
        IsCritical = isCritical;
    }
}

public enum DamageType
{
    Generic,
    Ballistic,
    Explosive,
    Melee,
    Fire,
    Electric
}

public enum FireMode
{
    Single,
    Burst,
    Automatic
}
