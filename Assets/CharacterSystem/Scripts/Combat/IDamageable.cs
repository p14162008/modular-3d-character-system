namespace CharacterSystem.Combat
{
    /// <summary>
    /// Interface for any entity that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        bool IsDead { get; }
        float TakeDamage(DamageData damage);
    }

    /// <summary>
    /// Interface for hurtbox components that forward impacts to their root character damageable.
    /// </summary>
    public interface IHurtbox
    {
        IDamageable Owner { get; }
        bool ReceiveHit(DamageData damage);
    }
}
