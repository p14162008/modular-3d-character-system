using System.Collections.Generic;
using UnityEngine;

namespace CharacterSystem.Combat
{
    /// <summary>
    /// Active attack volume component. When activated, queries surrounding colliders
    /// using non-allocating sphere casts and delivers damage to valid hurtboxes.
    /// </summary>
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Properties")]
        [SerializeField] private float radius = 0.6f;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private LayerMask targetLayers = ~0;

        private bool isActive;
        private DamageData currentDamageData;
        private readonly HashSet<IDamageable> alreadyHitEntities = new HashSet<IDamageable>();
        private readonly Collider[] hitBuffer = new Collider[16];

        public bool IsActive => isActive;
        public float Radius { get => radius; set => radius = value; }

        public void Activate(DamageData damageData)
        {
            isActive = true;
            currentDamageData = damageData;
            alreadyHitEntities.Clear();
            EvaluateHits();
        }

        public void Deactivate()
        {
            isActive = false;
            alreadyHitEntities.Clear();
        }

        private void Update()
        {
            if (isActive)
            {
                EvaluateHits();
            }
        }

        private void EvaluateHits()
        {
            Vector3 center = transform.TransformPoint(offset);
            int hitCount = UnityEngine.Physics.OverlapSphereNonAlloc(center, radius, hitBuffer, targetLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = hitBuffer[i];
                if (hit == null)
                    continue;

                // Don't hit self
                if (currentDamageData.Instigator != null && hit.transform.IsChildOf(currentDamageData.Instigator.transform))
                    continue;

                IHurtbox hurtbox = hit.GetComponent<IHurtbox>();
                if (hurtbox != null && hurtbox.Owner != null)
                {
                    if (!alreadyHitEntities.Contains(hurtbox.Owner))
                    {
                        alreadyHitEntities.Add(hurtbox.Owner);

                        // Calculate knockback direction
                        Vector3 knockbackDir = (hit.transform.position - transform.position);
                        knockbackDir.y = 0f;
                        if (knockbackDir.sqrMagnitude < 0.001f)
                        {
                            knockbackDir = transform.forward;
                        }
                        else
                        {
                            knockbackDir.Normalize();
                        }

                        DamageData payload = currentDamageData;
                        payload.HitPoint = hit.ClosestPoint(center);
                        payload.KnockbackDirection = knockbackDir;

                        hurtbox.ReceiveHit(payload);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!isActive)
                return;

            Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.6f);
            Gizmos.DrawWireSphere(transform.TransformPoint(offset), radius);
        }
    }
}
