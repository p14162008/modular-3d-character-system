using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Runtime attribute manager component handling stat calculations, damage mitigation,
    /// health/stamina vitals lifecycle, and event dispatching.
    /// </summary>
    public class AttributeSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CharacterStatsSO initialStats;

        private readonly Dictionary<AttributeType, CharacterAttribute> attributes =
            new Dictionary<AttributeType, CharacterAttribute>();

        private float currentHealth;
        private float currentStamina;
        private float staminaRegenCooldown;
        private bool isDead;

        // Events
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action<float, float> OnStaminaChanged; // (current, max)
        public event Action<float, bool> OnDamageTaken; // (amount, isCritical)
        public event Action<float> OnHealed; // (amount)
        public event Action OnDeath;

        public bool IsDead => isDead;
        public float CurrentHealth => currentHealth;
        public float CurrentStamina => currentStamina;

        public float MaxHealth => GetAttributeValue(AttributeType.MaxHealth, 100f);
        public float MaxStamina => GetAttributeValue(AttributeType.MaxStamina, 100f);
        public float MoveSpeed => GetAttributeValue(AttributeType.MoveSpeed, 4.5f);
        public float SprintSpeed => GetAttributeValue(AttributeType.SprintSpeed, 8.0f);
        public float AttackPower => GetAttributeValue(AttributeType.AttackPower, 20f);
        public float Defense => GetAttributeValue(AttributeType.Defense, 10f);
        public float AttackSpeed => GetAttributeValue(AttributeType.AttackSpeed, 1.0f);
        public float CriticalChance => GetAttributeValue(AttributeType.CriticalChance, 0.05f);
        public float CriticalMultiplier => GetAttributeValue(AttributeType.CriticalMultiplier, 1.5f);

        private void Awake()
        {
            if (initialStats != null)
            {
                InitializeFromStats(initialStats);
            }
        }

        private void Update()
        {
            if (isDead)
                return;

            HandleStaminaRegeneration(Time.deltaTime);
        }

        /// <summary>
        /// Initializes all attributes based on a designer-configured CharacterStatsSO asset.
        /// </summary>
        public void InitializeFromStats(CharacterStatsSO stats)
        {
            if (stats == null)
                return;

            initialStats = stats;
            attributes.Clear();

            RegisterAttribute(AttributeType.MaxHealth, stats.MaxHealth);
            RegisterAttribute(AttributeType.MaxStamina, stats.MaxStamina);
            RegisterAttribute(AttributeType.MoveSpeed, stats.BaseMoveSpeed);
            RegisterAttribute(AttributeType.SprintSpeed, stats.BaseSprintSpeed);
            RegisterAttribute(AttributeType.AttackPower, stats.BaseAttackPower);
            RegisterAttribute(AttributeType.Defense, stats.BaseDefense);
            RegisterAttribute(AttributeType.AttackSpeed, stats.AttackSpeed);
            RegisterAttribute(AttributeType.CriticalChance, stats.CriticalChance);
            RegisterAttribute(AttributeType.CriticalMultiplier, stats.CriticalMultiplier);

            currentHealth = MaxHealth;
            currentStamina = MaxStamina;
            isDead = false;

            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
        }

        public CharacterAttribute GetAttribute(AttributeType type)
        {
            attributes.TryGetValue(type, out CharacterAttribute attr);
            return attr;
        }

        public float GetAttributeValue(AttributeType type, float fallback = 0f)
        {
            if (attributes.TryGetValue(type, out CharacterAttribute attr))
            {
                return attr.Value;
            }
            return fallback;
        }

        public void AddModifier(AttributeType type, StatModifier modifier)
        {
            if (attributes.TryGetValue(type, out CharacterAttribute attr))
            {
                attr.AddModifier(modifier);

                // Re-clamp health or stamina if max values changed
                if (type == AttributeType.MaxHealth)
                {
                    currentHealth = Mathf.Min(currentHealth, attr.Value);
                    OnHealthChanged?.Invoke(currentHealth, attr.Value);
                }
                else if (type == AttributeType.MaxStamina)
                {
                    currentStamina = Mathf.Min(currentStamina, attr.Value);
                    OnStaminaChanged?.Invoke(currentStamina, attr.Value);
                }
            }
        }

        public bool RemoveModifier(AttributeType type, StatModifier modifier)
        {
            if (attributes.TryGetValue(type, out CharacterAttribute attr))
            {
                return attr.RemoveModifier(modifier);
            }
            return false;
        }

        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var kvp in attributes)
            {
                kvp.Value.RemoveAllModifiersFromSource(source);
            }
        }

        /// <summary>
        /// Applies incoming raw damage factored against character Defense rating.
        /// Formula: EffectiveDamage = Max(1, RawDamage * 100 / (100 + Defense))
        /// </summary>
        public float ApplyDamage(float rawDamage, bool isCritical = false)
        {
            if (isDead || rawDamage <= 0f)
                return 0f;

            float def = Defense;
            float mitigationFactor = 100f / (100f + Mathf.Max(0f, def));
            float effectiveDamage = Mathf.Max(1f, rawDamage * mitigationFactor);

            currentHealth = Mathf.Max(0f, currentHealth - effectiveDamage);
            OnDamageTaken?.Invoke(effectiveDamage, isCritical);
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);

            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                OnDeath?.Invoke();
            }

            return effectiveDamage;
        }

        /// <summary>
        /// Heals the character by a specified flat amount, bounded by MaxHealth.
        /// </summary>
        public float Heal(float healAmount)
        {
            if (isDead || healAmount <= 0f)
                return 0f;

            float prevHealth = currentHealth;
            currentHealth = Mathf.Min(MaxHealth, currentHealth + healAmount);
            float actualHeal = currentHealth - prevHealth;

            if (actualHeal > 0f)
            {
                OnHealed?.Invoke(actualHeal);
                OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            }

            return actualHeal;
        }

        /// <summary>
        /// Consumes stamina (e.g., for sprinting, jumping, or combat actions).
        /// </summary>
        public bool ConsumeStamina(float amount)
        {
            if (isDead || currentStamina < amount || amount < 0f)
                return false;

            currentStamina = Mathf.Max(0f, currentStamina - amount);
            staminaRegenCooldown = initialStats != null ? initialStats.StaminaRegenDelay : 1.0f;
            OnStaminaChanged?.Invoke(currentStamina, MaxStamina);
            return true;
        }

        /// <summary>
        /// Checks if character has enough stamina for an action.
        /// </summary>
        public bool HasStamina(float amount)
        {
            return currentStamina >= amount;
        }

        private void HandleStaminaRegeneration(float deltaTime)
        {
            if (staminaRegenCooldown > 0f)
            {
                staminaRegenCooldown -= deltaTime;
                return;
            }

            float max = MaxStamina;
            if (currentStamina < max)
            {
                float regenRate = initialStats != null ? initialStats.StaminaRegenRate : 20f;
                currentStamina = Mathf.Min(max, currentStamina + (regenRate * deltaTime));
                OnStaminaChanged?.Invoke(currentStamina, max);
            }
        }

        private void RegisterAttribute(AttributeType type, float baseValue)
        {
            attributes[type] = new CharacterAttribute(type, baseValue);
        }
    }
}
