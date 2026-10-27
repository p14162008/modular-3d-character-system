using UnityEngine;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Data asset defining baseline character statistics.
    /// Easily authorable in the Inspector and swappable across archetypes (Warrior, Rogue, Mage, Boss).
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Character System/Stats/Character Stats Asset")]
    public class CharacterStatsSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string statsProfileName = "Default Stats";

        [Header("Vitals")]
        [Tooltip("Maximum health pool.")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Health points regenerated per second when out of combat.")]
        [SerializeField] private float healthRegenRate = 0f;

        [Tooltip("Maximum stamina pool for sprinting, dodging, and heavy combos.")]
        [SerializeField] private float maxStamina = 100f;

        [Tooltip("Stamina recovered per second.")]
        [SerializeField] private float staminaRegenRate = 20f;

        [Tooltip("Delay in seconds after stamina expenditure before regen kicks in.")]
        [SerializeField] private float staminaRegenDelay = 1.0f;

        [Header("Locomotion")]
        [Tooltip("Base walking speed in m/s.")]
        [SerializeField] private float baseMoveSpeed = 4.5f;

        [Tooltip("Base sprinting speed in m/s.")]
        [SerializeField] private float baseSprintSpeed = 8.0f;

        [Tooltip("Stamina consumed per second while sprinting.")]
        [SerializeField] private float sprintStaminaCostPerSec = 15f;

        [Header("Combat & Defense")]
        [Tooltip("Base attack power scaling weapon damage.")]
        [SerializeField] private float baseAttackPower = 20f;

        [Tooltip("Flat defense mitigating incoming damage.")]
        [SerializeField] private float baseDefense = 10f;

        [Tooltip("Attack speed multiplier scaling animation playback.")]
        [SerializeField] private float attackSpeed = 1.0f;

        [Tooltip("Critical strike chance between 0.0 (0%) and 1.0 (100%).")]
        [SerializeField, Range(0f, 1f)] private float criticalChance = 0.05f;

        [Tooltip("Damage multiplier applied on critical strikes.")]
        [SerializeField] private float criticalMultiplier = 1.5f;

        public string StatsProfileName => statsProfileName;
        public float MaxHealth => maxHealth;
        public float HealthRegenRate => healthRegenRate;
        public float MaxStamina => maxStamina;
        public float StaminaRegenRate => staminaRegenRate;
        public float StaminaRegenDelay => staminaRegenDelay;
        public float BaseMoveSpeed => baseMoveSpeed;
        public float BaseSprintSpeed => baseSprintSpeed;
        public float SprintStaminaCostPerSec => sprintStaminaCostPerSec;
        public float BaseAttackPower => baseAttackPower;
        public float BaseDefense => baseDefense;
        public float AttackSpeed => attackSpeed;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;
    }
}
