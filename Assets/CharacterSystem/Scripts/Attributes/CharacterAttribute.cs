using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Represents a dynamic character attribute with modifier stacking,
    /// dirty-flag caching, and event notifications.
    /// </summary>
    [Serializable]
    public class CharacterAttribute
    {
        [SerializeField] private AttributeType type;
        [SerializeField] private float baseValue;

        private float cachedValue;
        private bool isDirty = true;

        private readonly List<StatModifier> modifiers = new List<StatModifier>();
        public readonly ReadOnlyCollection<StatModifier> Modifiers;

        public event Action<CharacterAttribute, float, float> OnValueChanged;

        public AttributeType Type => type;

        public float BaseValue
        {
            get => baseValue;
            set
            {
                if (Mathf.Approximately(baseValue, value))
                    return;

                float oldVal = Value;
                baseValue = value;
                isDirty = true;
                float newVal = Value;
                OnValueChanged?.Invoke(this, oldVal, newVal);
            }
        }

        public float Value
        {
            get
            {
                if (isDirty)
                {
                    cachedValue = CalculateFinalValue();
                    isDirty = false;
                }
                return cachedValue;
            }
        }

        public CharacterAttribute(AttributeType type, float baseValue)
        {
            this.type = type;
            this.baseValue = baseValue;
            this.cachedValue = baseValue;
            Modifiers = modifiers.AsReadOnly();
        }

        public void AddModifier(StatModifier mod)
        {
            if (mod == null)
                return;

            float oldVal = Value;
            modifiers.Add(mod);
            modifiers.Sort(CompareModifierOrder);
            isDirty = true;
            float newVal = Value;
            OnValueChanged?.Invoke(this, oldVal, newVal);
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (mod == null)
                return false;

            float oldVal = Value;
            if (modifiers.Remove(mod))
            {
                isDirty = true;
                float newVal = Value;
                OnValueChanged?.Invoke(this, oldVal, newVal);
                return true;
            }
            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            if (source == null)
                return false;

            float oldVal = Value;
            bool didRemove = false;

            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (modifiers[i].Source == source)
                {
                    modifiers.RemoveAt(i);
                    didRemove = true;
                }
            }

            if (didRemove)
            {
                isDirty = true;
                float newVal = Value;
                OnValueChanged?.Invoke(this, oldVal, newVal);
            }

            return didRemove;
        }

        public void ClearModifiers()
        {
            if (modifiers.Count == 0)
                return;

            float oldVal = Value;
            modifiers.Clear();
            isDirty = true;
            float newVal = Value;
            OnValueChanged?.Invoke(this, oldVal, newVal);
        }

        private float CalculateFinalValue()
        {
            float finalValue = baseValue;
            float sumPercentAdd = 0f;

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier mod = modifiers[i];
                if (mod.Type == ModifierType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == ModifierType.Percentage)
                {
                    sumPercentAdd += mod.Value;
                }
            }

            // (Base + Flat) * (1 + SumPercentage)
            finalValue *= (1f + sumPercentAdd);
            return Mathf.Max(0f, finalValue);
        }

        private static int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order)
                return -1;
            if (a.Order > b.Order)
                return 1;
            return 0;
        }
    }
}
