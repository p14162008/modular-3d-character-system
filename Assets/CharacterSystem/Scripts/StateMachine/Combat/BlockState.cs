using UnityEngine;

namespace CharacterSystem.StateMachine.Combat
{
    /// <summary>
    /// Defensive stance mitigating incoming damage, absorbing impact through stamina,
    /// and allowing guarded strafing locomotion.
    /// </summary>
    public class BlockState : CharacterBaseState
    {
        public override string Name => "Block";

        public BlockState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();

            if (Context.Combat != null)
            {
                Context.Combat.IsBlocking = true;
            }

            if (Context.Animation != null)
            {
                Context.Animation.SetBlocking(true);
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

            // Block released or out of stamina -> transition out
            if (Context.Input == null || !Context.Input.IsBlockHeld || (Context.Attributes != null && Context.Attributes.CurrentStamina <= 0f))
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

            // Guarded movement at reduced speed
            Vector3 moveDir = GetCameraRelativeMoveDirection();
            float guardedSpeed = (Context.Attributes != null ? Context.Attributes.MoveSpeed : 4.5f) * 0.4f;

            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(moveDir, guardedSpeed);
            }

            base.Update(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();

            if (Context.Combat != null)
            {
                Context.Combat.IsBlocking = false;
            }

            if (Context.Animation != null)
            {
                Context.Animation.SetBlocking(false);
            }
        }
    }
}
