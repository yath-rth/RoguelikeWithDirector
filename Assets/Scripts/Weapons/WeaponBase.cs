using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon, IAmmoWeapon, IUpgradeableWeapon
{
    [SerializeField] protected string weaponName;
    [SerializeField] protected FireMode fireMode;
    [SerializeField] protected float damage;
    [SerializeField] protected float fireRate;

    [SerializeField] protected bool usesAmmo;
    [SerializeField] protected int magazineSize;
    [SerializeField] protected int startingReserveAmmo;
    [SerializeField] protected float reloadTime;

    protected Transform owner;

    protected float nextFireTime;
    protected bool isFiring;
    protected bool isReloading;
    protected Coroutine reloadRoutine;

    protected float damageBonus;
    protected float fireRateMultiplier = 1f;
    protected int magazineBonus;
    protected float reloadSpeedMultiplier = 1f;

    protected int currentAmmo;
    protected int reserveAmmo;

    public string WeaponName => weaponName;
    public virtual bool CanFire => !isReloading && Time.time >= nextFireTime && (!usesAmmo || currentAmmo > 0);

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => magazineSize + magazineBonus;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;

    protected float EffectiveDamage => damage + damageBonus;
    protected float EffectiveFireRate => fireRate * fireRateMultiplier;
    protected float EffectiveReloadTime => reloadTime / Mathf.Max(0.01f, reloadSpeedMultiplier);

    protected virtual void Awake()
    {
        currentAmmo = magazineSize;
        reserveAmmo = startingReserveAmmo;
    }

    public virtual void Equip(Transform newOwner)
    {
        owner = newOwner;
        gameObject.SetActive(true);
    }

    public virtual void Unequip()
    {
        StopFire();
        gameObject.SetActive(false);
    }

    public virtual void StartFire()
    {
        isFiring = true;
        if (fireMode != FireMode.Automatic)
            TryFireOnce();
    }

    public virtual void StopFire()
    {
        isFiring = false;
    }

    public virtual void TickAutomaticFire()
    {
        if (isFiring && fireMode == FireMode.Automatic && Time.time >= nextFireTime)
            TryFireOnce();
    }

    protected void TryFireOnce()
    {
        if (!CanFire) return;

        nextFireTime = Time.time + (1f / Mathf.Max(0.01f, EffectiveFireRate));

        if (usesAmmo)
            currentAmmo--;

        PerformFire();
    }

    protected abstract void PerformFire();

    public virtual void Reload()
    {
        if (isReloading || !usesAmmo) return;
        if (currentAmmo >= MaxAmmo || reserveAmmo <= 0) return;

        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    protected virtual IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(EffectiveReloadTime);

        int needed = MaxAmmo - currentAmmo;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentAmmo += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
        reloadRoutine = null;
    }

    // --- Upgrade hooks ---

    public virtual void UpgradeDamage(float flatBonus) => damageBonus += flatBonus;

    public virtual void UpgradeFireRate(float multiplier) => fireRateMultiplier *= multiplier;

    public virtual void UpgradeMagazineSize(int flatBonus)
    {
        magazineBonus += flatBonus;
        currentAmmo += flatBonus;
    }

    public virtual void UpgradeReloadSpeed(float multiplier) => reloadSpeedMultiplier *= multiplier;
}
