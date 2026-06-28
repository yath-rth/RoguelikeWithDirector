[System.Serializable]
public struct PlayerStats
{
    public int level;
    public int currentXP;
    public int xpToLevelUp;

    public float health;
    public float maxHealth;

    public float damageTakenThisroom;

    public int kills;
    public int shotsFired;
    public int shotsHit;

    public void AddHealth(float amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;
    }

    public void ReduceHealth(float amount)
    {
        health -= amount;

        if (health < 0)
            health = 0;
    }

    public void AddKill()
    {
        kills++;
    }
    public enum CurrentWeapon
    {
        Pistol,
        SMG,
        Shotgun,
        Sniper,
        Melee
    }

    public void AddShotFired()
    {
        shotsFired++;
    }

    public void AddShotHit()
    {
        shotsHit++;
    }

    public float GetAccuracy()//maybe we can add accuracy dependent on gun as missing a bullet with a sniper is worse than missing a bullet with an smg
    {
        if (shotsFired == 0)
            return 0;

        return (float)shotsHit / shotsFired;
    }
    public void OnDamage(int amount)
    {

        damageTakenThisroom+=amount;
    }
    public void ResetStats()//can be called to reset stats in order to keep track of current performance of player like maybe every time they enter a new room
    {
        damageTakenThisroom=0;
        shotsHit=0;
        shotsFired=0;
    }
    public void AddXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToLevelUp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentXP -= xpToLevelUp;

        level++;

        xpToLevelUp += 50;

        maxHealth += 10;

        

        health = maxHealth;
    }
}

