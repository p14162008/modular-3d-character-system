using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterSystem.Equipment
{
    public enum WeaponType
    {
        OneHandedSword,
        TwoHandedGreatsword,
        DualDaggers,
        MagicStaff,
        Polearm
    }

    [Serializable]
    public struct WeaponAttackData
    {
        [Tooltip("Identifier name of the attack (e.g. Slash1, Finisher).")]
        public string attackName;

        [Tooltip("Animator trigger or state name for this attack.")]
        public string animationTrigger;

        [Tooltip("Multiplier scaled by character base AttackPower.")]
        public float damageMultiplier;

        [Tooltip("Radius of the hitbox sphere during active attack frames.")]
        public float hitboxRadius;

        [Tooltip("Total duration of the attack in seconds.")]
        public float attackDuration;

        [Tooltip("Normalized start time (0.0 to 1.0) when next combo input is accepted.")]
        [Range(0f, 1f)] public float comboWindowStart;

        [Tooltip("Normalized end time (0.0 to 1.0) before combo window closes.")]
        [Range(0f, 1f)] public float comboWindowEnd;

        [Tooltip("Normalized start time for active damage dealing hitbox frames.")]
        [Range(0f, 1f)] public float hitboxActiveStart;

        [Tooltip("Normalized end time for active damage dealing hitbox frames.")]
        [Range(0f, 1f)] public float hitboxActiveEnd;

        [Tooltip("Forward step impulse velocity applied on attack execution.")]
        public float forwardImpulse;

        [Tooltip("Stamina consumed to perform this attack step.")]
        public float staminaCost;

        [Tooltip("Knockback impulse applied to targets hit.")]
        public float knockbackForce;
    }

    /// <summary>
    /// Specialized equipment asset for weapons defining attack combos, timing windows,
    /// and dynamic hitbox configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Character System/Equipment/Weapon Asset")]
    public class WeaponItemSO : EquipmentItemSO
    {
        [Header("Weapon Configuration")]
        [SerializeField] private WeaponType weaponType = WeaponType.OneHandedSword;

        [Header("Combo Chain")]
        [SerializeField] private List<WeaponAttackData> comboSequence = new List<WeaponAttackData>();

        public WeaponType WeaponType => weaponType;
        public IReadOnlyList<WeaponAttackData> ComboSequence => comboSequence;

        private void Reset()
        {
            // Default setup for a new weapon asset
            typeof(EquipmentItemSO).GetField("slotType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, EquipmentSlotType.MainHand);
        }
    }
}
