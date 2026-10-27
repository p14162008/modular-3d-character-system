namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Enumeration of all primary character attributes supported by the modular system.
    /// Easily extensible for gameplay mechanics like Magic, Poise, or Luck.
    /// </summary>
    public enum AttributeType
    {
        MaxHealth,
        CurrentHealth,
        MaxStamina,
        CurrentStamina,
        MoveSpeed,
        SprintSpeed,
        AttackPower,
        Defense,
        AttackSpeed,
        CriticalChance,
        CriticalMultiplier
    }
}
