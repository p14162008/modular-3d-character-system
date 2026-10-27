using CharacterSystem.Combat;
using UnityEngine;

namespace CharacterSystem.StateMachine.Combat
{
    /// <summary>
    /// Hit reaction state triggering directional flinch animations, applying knockback momentum,
    /// and stunning the character momentarily before recovery.
    /// </summary>
    public class HurtState : CharacterBaseState
    {
        private float stunTimer;
        private DamageData pendingHitData;
        private const float DefaultStunDuration = 0.35f;

        public override string Name => "Hurt";

        public HurtState(CharacterContext context) : base(context) { }

        public void SetHitData(DamageData damage)
        {
            pendingHitData = damage;
        }

        public override void Enter()
        {
            base.Enter();
            stunTimer = DefaultStunDuration;

            // Apply knockback impulse
            if (Context.Controller != null && pendingHitData.KnockbackForce > 0f)
            {
                Vector3 knockback = pendingHitData.KnockbackDirection * pendingHitData.KnockbackForce;
                Context.Controller.ApplyImpulse(knockback);
            }

            // Directional hurt animation
            if (Context.Animation != null && Context.Transform != null)
            {
                Vector3 localHitDir = Context.Transform.InverseTransformDirection(pendingHitData.KnockbackDirection);
                Context.Animation.TriggerHurt(localHitDir);
            }

            // Stop manual locomotion
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(Vector3.zero, 0f);
            }
        }

        public override void Update(float deltaTime)
        {
            // Death Check
            if (Context.Attributes != null && Context.Attributes.IsDead)
            {
                Context.StateMachine.ChangeState(Context.DeathState);
                return;
            }

            stunTimer -= deltaTime;
            if (stunTimer <= 0f)
            {
                Vector3 move = GetCameraRelativeMoveDirection();
                if (move.sqrMagnitude > 0.001f)
                {
                    Context.StateMachine.ChangeState(Context.WalkState);
                }
                else
                {
                    Context.StateMachine.ChangeState(Context.IdleState);
                }
                return;
            }

            base.Update(deltaTime);
        }
    }
}
