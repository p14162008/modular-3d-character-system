using CharacterSystem.Equipment;
using UnityEngine;

namespace CharacterSystem.StateMachine.Combat
{
    /// <summary>
    /// Combat state orchestrating multi-step weapon combo chains, active hitbox windows,
    /// attack root step impulses, and chaining transitions without visual hitching.
    /// </summary>
    public class AttackState : CharacterBaseState
    {
        private bool isComboDone;

        public override string Name => "Attack";

        public AttackState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            isComboDone = false;

            if (Context.Combat != null)
            {
                Context.Combat.OnAttackExecuted += HandleAttackExecuted;
                Context.Combat.OnComboFinished += HandleComboFinished;

                if (!Context.Combat.IsAttacking)
                {
                    Context.Combat.TriggerAttack();
                }
            }

            // Bring base locomotion to stop during swing
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

            // Buffer follow-up combo step if player clicks attack
            if (Context.Input != null && Context.Input.IsAttackTriggered)
            {
                Context.Input.ConsumeAttackTrigger();
                if (Context.Combat != null)
                {
                    Context.Combat.TriggerAttack();
                }
            }

            // Update combat timers and hitbox windows
            if (Context.Combat != null)
            {
                Context.Combat.UpdateCombat(deltaTime);
            }

            // Combo chain finished
            if (isComboDone || (Context.Combat != null && !Context.Combat.IsAttacking))
            {
                Vector3 moveDir = GetCameraRelativeMoveDirection();
                if (moveDir.sqrMagnitude > 0.001f)
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

        public override void Exit()
        {
            base.Exit();

            if (Context.Combat != null)
            {
                Context.Combat.OnAttackExecuted -= HandleAttackExecuted;
                Context.Combat.OnComboFinished -= HandleComboFinished;
            }
        }

        private void HandleAttackExecuted(int comboIndex, WeaponAttackData attackData)
        {
            // Visual trigger
            if (Context.Animation != null)
            {
                Context.Animation.TriggerAttack(comboIndex, attackData.animationTrigger);
            }

            // Apply forward step impulse
            if (Context.Controller != null && attackData.forwardImpulse > 0f)
            {
                Vector3 stepVector = Context.Transform.forward * attackData.forwardImpulse;
                Context.Controller.ApplyImpulse(stepVector);
            }
        }

        private void HandleComboFinished()
        {
            isComboDone = true;
        }
    }
}
