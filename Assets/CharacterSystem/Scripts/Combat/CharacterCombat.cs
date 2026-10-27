using System;
using System.Collections.Generic;
using CharacterSystem.Attributes;
using CharacterSystem.Equipment;
using UnityEngine;

namespace CharacterSystem.Combat
{
    /// <summary>
    /// Core combat manager coordinating combo chains, hitbox triggering,
    /// block stance damage absorption, and hit reactions.
    /// </summary>
    public class CharacterCombat : MonoBehaviour, IDamageable
    {
        [Header("Dependencies")]
        [SerializeField] private AttributeSystem attributeSystem;
        [SerializeField] private EquipmentInventory equipmentInventory;
        [SerializeField] private Hitbox weaponHitbox;

        [Header("Defensive Stance")]
        [Tooltip("Percentage of damage absorbed while in active block state (0.0 to 1.0).")]
        [SerializeField, Range(0f, 1f)] private float blockDamageMitigation = 0.75f;

        [Tooltip("Stamina consumed when absorbing an incoming attack while blocking.")]
        [SerializeField] private float blockStaminaCost = 15f;

        [Header("Fallback Unarmed Combo")]
        [SerializeField] private List<WeaponAttackData> unarmedCombo = new List<WeaponAttackData>();

        // Runtime Combo State
        private int currentComboIndex = 0;
        private float comboTimer = 0f;
        private bool isAttacking = false;
        private bool isBlocking = false;
        private bool comboBuffered = false;

        // Knockback cache
        private Vector3 currentKnockbackVelocity = Vector3.zero;

        // Events
        public event Action<int, WeaponAttackData> OnAttackExecuted;
        public event Action OnComboFinished;
        public event Action<DamageData> OnHitReceived;
        public event Action<DamageData> OnBlockSuccessful;

        public bool IsDead => attributeSystem != null && attributeSystem.IsDead;
        public bool IsAttacking => isAttacking;
        public bool IsBlocking { get => isBlocking; set => isBlocking = value; }
        public int CurrentComboIndex => currentComboIndex;
        public Vector3 KnockbackVelocity => currentKnockbackVelocity;

        private void Awake()
        {
            if (attributeSystem == null)
                attributeSystem = GetComponent<AttributeSystem>();

            if (equipmentInventory == null)
                equipmentInventory = GetComponent<EquipmentInventory>();

            SetupDefaultUnarmedCombo();
        }

        private void SetupDefaultUnarmedCombo()
        {
            if (unarmedCombo.Count == 0)
            {
                // Step 1: Left Jab
                unarmedCombo.Add(new WeaponAttackData
                {
                    attackName = "Jab",
                    animationTrigger = "Attack1",
                    damageMultiplier = 1.0f,
                    hitboxRadius = 0.5f,
                    attackDuration = 0.5f,
                    comboWindowStart = 0.4f,
                    comboWindowEnd = 0.85f,
                    hitboxActiveStart = 0.2f,
                    hitboxActiveEnd = 0.5f,
                    forwardImpulse = 2.0f,
                    staminaCost = 10f,
                    knockbackForce = 3f
                });

                // Step 2: Right Cross
                unarmedCombo.Add(new WeaponAttackData
                {
                    attackName = "Cross",
                    animationTrigger = "Attack2",
                    damageMultiplier = 1.3f,
                    hitboxRadius = 0.55f,
                    attackDuration = 0.55f,
                    comboWindowStart = 0.35f,
                    comboWindowEnd = 0.85f,
                    hitboxActiveStart = 0.2f,
                    hitboxActiveEnd = 0.5f,
                    forwardImpulse = 2.5f,
                    staminaCost = 12f,
                    knockbackForce = 4.5f
                });

                // Step 3: Heavy Kick
                unarmedCombo.Add(new WeaponAttackData
                {
                    attackName = "FinisherKick",
                    animationTrigger = "Attack3",
                    damageMultiplier = 1.8f,
                    hitboxRadius = 0.7f,
                    attackDuration = 0.75f,
                    comboWindowStart = 1.0f, // No combo follow-up
                    comboWindowEnd = 1.0f,
                    hitboxActiveStart = 0.3f,
                    hitboxActiveEnd = 0.65f,
                    forwardImpulse = 4.0f,
                    staminaCost = 18f,
                    knockbackForce = 8.0f
                });
            }
        }

        public IReadOnlyList<WeaponAttackData> GetCurrentComboSequence()
        {
            if (equipmentInventory != null && equipmentInventory.EquippedWeapon != null)
            {
                var seq = equipmentInventory.EquippedWeapon.ComboSequence;
                if (seq != null && seq.Count > 0)
                    return seq;
            }
            return unarmedCombo;
        }

        public bool CanInitiateAttack()
        {
            if (IsDead || isBlocking)
                return false;

            var sequence = GetCurrentComboSequence();
            if (sequence.Count == 0)
                return false;

            int targetIndex = isAttacking ? (currentComboIndex + 1) : 0;
            if (targetIndex >= sequence.Count)
                return false;

            WeaponAttackData attack = sequence[targetIndex];
            return attributeSystem == null || attributeSystem.HasStamina(attack.staminaCost);
        }

        /// <summary>
        /// Attempts to execute an attack or buffer it during an active combo window.
        /// </summary>
        public bool TriggerAttack()
        {
            if (isAttacking)
            {
                var sequence = GetCurrentComboSequence();
                if (currentComboIndex < sequence.Count - 1)
                {
                    WeaponAttackData currentAttack = sequence[currentComboIndex];
                    float normalizedTime = Mathf.Clamp01(comboTimer / Mathf.Max(0.01f, currentAttack.attackDuration));

                    if (normalizedTime >= currentAttack.comboWindowStart && normalizedTime <= currentAttack.comboWindowEnd)
                    {
                        comboBuffered = true;
                        return true;
                    }
                }
                return false;
            }

            return StartAttackStep(0);
        }

        public void UpdateCombat(float deltaTime)
        {
            // Decay knockback
            if (currentKnockbackVelocity.sqrMagnitude > 0.01f)
            {
                currentKnockbackVelocity = Vector3.MoveTowards(
                    currentKnockbackVelocity,
                    Vector3.zero,
                    15f * deltaTime);
            }

            if (!isAttacking)
                return;

            comboTimer += deltaTime;
            var sequence = GetCurrentComboSequence();
            if (currentComboIndex >= sequence.Count)
            {
                EndAttack();
                return;
            }

            WeaponAttackData attack = sequence[currentComboIndex];
            float normalized = comboTimer / Mathf.Max(0.01f, attack.attackDuration);

            // Manage Hitbox Active Window
            if (weaponHitbox != null)
            {
                if (normalized >= attack.hitboxActiveStart && normalized <= attack.hitboxActiveEnd && !weaponHitbox.IsActive)
                {
                    float basePower = attributeSystem != null ? attributeSystem.AttackPower : 20f;
                    float totalDamage = basePower * attack.damageMultiplier;

                    bool isCrit = UnityEngine.Random.value < (attributeSystem != null ? attributeSystem.CriticalChance : 0.05f);
                    if (isCrit)
                    {
                        totalDamage *= (attributeSystem != null ? attributeSystem.CriticalMultiplier : 1.5f);
                    }

                    DamageData data = new DamageData(
                        totalDamage,
                        transform.position + transform.forward,
                        transform.forward,
                        attack.knockbackForce,
                        isCrit,
                        gameObject);

                    weaponHitbox.Radius = attack.hitboxRadius;
                    weaponHitbox.Activate(data);
                }
                else if (normalized > attack.hitboxActiveEnd && weaponHitbox.IsActive)
                {
                    weaponHitbox.Deactivate();
                }
            }

            // Chaining buffered combo
            if (comboBuffered && normalized >= attack.comboWindowStart)
            {
                comboBuffered = false;
                StartAttackStep(currentComboIndex + 1);
                return;
            }

            // Attack complete
            if (comboTimer >= attack.attackDuration)
            {
                EndAttack();
            }
        }

        private bool StartAttackStep(int stepIndex)
        {
            var sequence = GetCurrentComboSequence();
            if (stepIndex >= sequence.Count)
            {
                EndAttack();
                return false;
            }

            WeaponAttackData attack = sequence[stepIndex];

            // Consume stamina
            if (attributeSystem != null && !attributeSystem.ConsumeStamina(attack.staminaCost))
            {
                EndAttack();
                return false;
            }

            currentComboIndex = stepIndex;
            comboTimer = 0f;
            isAttacking = true;
            comboBuffered = false;

            if (weaponHitbox != null)
            {
                weaponHitbox.Deactivate();
            }

            OnAttackExecuted?.Invoke(currentComboIndex, attack);
            return true;
        }

        public void InterruptAttack()
        {
            if (weaponHitbox != null)
                weaponHitbox.Deactivate();

            isAttacking = false;
            comboBuffered = false;
            currentComboIndex = 0;
            comboTimer = 0f;
        }

        private void EndAttack()
        {
            if (weaponHitbox != null)
                weaponHitbox.Deactivate();

            isAttacking = false;
            comboBuffered = false;
            currentComboIndex = 0;
            comboTimer = 0f;

            OnComboFinished?.Invoke();
        }

        public float TakeDamage(DamageData damage)
        {
            if (IsDead)
                return 0f;

            // Handle Active Block
            if (isBlocking)
            {
                // Check if attack came from front 180 degrees
                Vector3 toAttacker = (damage.Instigator != null)
                    ? (damage.Instigator.transform.position - transform.position).normalized
                    : -damage.KnockbackDirection;

                toAttacker.y = 0f;
                if (Vector3.Dot(transform.forward, toAttacker) > 0f)
                {
                    // Blocked!
                    float blockedDamage = damage.Amount * (1f - blockDamageMitigation);
                    if (attributeSystem != null)
                    {
                        attributeSystem.ConsumeStamina(blockStaminaCost);
                        attributeSystem.ApplyDamage(blockedDamage, false);
                    }

                    currentKnockbackVelocity = damage.KnockbackDirection * (damage.KnockbackForce * 0.35f);
                    OnBlockSuccessful?.Invoke(damage);
                    return blockedDamage;
                }
            }

            // Full impact
            InterruptAttack();

            float applied = 0f;
            if (attributeSystem != null)
            {
                applied = attributeSystem.ApplyDamage(damage.Amount, damage.IsCritical);
            }

            currentKnockbackVelocity = damage.KnockbackDirection * damage.KnockbackForce;
            OnHitReceived?.Invoke(damage);
            return applied;
        }
    }
}
