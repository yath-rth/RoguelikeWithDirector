public interface IHealer
{
    float HealOutput { get; }
    void ApplyHealing(IHealable target);
}