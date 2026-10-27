using UnityEngine;

namespace CharacterSystem.Combat
{
    /// <summary>
    /// Payload carrying combat damage details, impact point, knockback vector, and attacker identity.
    /// </summary>
    public struct DamageData
    {
        public float Amount;
        public Vector3 HitPoint;
        public Vector3 KnockbackDirection;
        public float KnockbackForce;
        public bool IsCritical;
        public GameObject Instigator;

        public DamageData(
            float amount,
            Vector3 hitPoint,
            Vector3 knockbackDirection,
            float knockbackForce,
            bool isCritical,
            GameObject instigator)
        {
            Amount = amount;
            HitPoint = hitPoint;
            KnockbackDirection = knockbackDirection;
            KnockbackForce = knockbackForce;
            IsCritical = isCritical;
            Instigator = instigator;
        }
    }
}
