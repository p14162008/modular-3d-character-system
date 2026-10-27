using System;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Type of modifier applied to an attribute calculation.
    /// </summary>
    public enum ModifierType
    {
        /// <summary>
        /// Added directly to the base value: (Base + Flat)
        /// </summary>
        Flat = 100,

        /// <summary>
        /// Multiplied against the base + flat sum: (Base + Flat) * (1 + PercentSum)
        /// </summary>
        Percentage = 200
    }

    /// <summary>
    /// Represents an immutable stat modifier sourced from equipment, buffs, or environmental effects.
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public readonly float Value;
        public readonly ModifierType Type;
        public readonly int Order;
        public readonly object Source;

        public StatModifier(float value, ModifierType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, ModifierType type) : this(value, type, (int)type, null) { }

        public StatModifier(float value, ModifierType type, object source) : this(value, type, (int)type, source) { }
    }
}
