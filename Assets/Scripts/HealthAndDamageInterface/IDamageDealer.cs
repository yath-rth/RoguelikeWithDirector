using UnityEngine;

public interface IDamageDealer
{
    float DamageOutput {get;}
    void DealDamage(IDamagable target);
}
