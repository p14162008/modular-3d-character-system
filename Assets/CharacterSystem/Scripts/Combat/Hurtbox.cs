using UnityEngine;

namespace CharacterSystem.Combat
{
    /// <summary>
    /// Component attached to character colliders that detects incoming attacks
    /// and dispatches damage to the character's core combat/attribute systems.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hurtbox : MonoBehaviour, IHurtbox
    {
        [Tooltip("Damage multiplier applied to hits on this specific hurtbox (e.g. 2.0 for Head, 1.0 for Torso).")]
        [SerializeField] private float damageMultiplier = 1.0f;

        [Tooltip("The root damageable entity that owns this hurtbox.")]
        [SerializeField] private MonoBehaviour ownerComponent;

        private IDamageable owner;
        private Collider col;

        public IDamageable Owner => owner;
        public float DamageMultiplier => damageMultiplier;

        private void Awake()
        {
            col = GetComponent<Collider>();
            if (ownerComponent is IDamageable damageable)
            {
                owner = damageable;
            }
            else
            {
                owner = GetComponentInParent<IDamageable>();
            }
        }

        public void SetOwner(IDamageable newOwner)
        {
            owner = newOwner;
        }

        public bool ReceiveHit(DamageData damage)
        {
            if (owner == null || owner.IsDead)
                return false;

            // Apply body-part multiplier
            damage.Amount *= damageMultiplier;
            owner.TakeDamage(damage);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
            if (col == null)
                col = GetComponent<Collider>();

            if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(sphere.center), sphere.radius * transform.lossyScale.x);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
}
