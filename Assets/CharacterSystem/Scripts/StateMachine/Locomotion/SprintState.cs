using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// High-speed locomotion state consuming stamina per second and accelerating
    /// to sprint speed.
    /// </summary>
    public class SprintState : CharacterBaseState
    {
        public override string Name => "Sprint";

        public SprintState(CharacterContext context) : base(context) { }

        public override void Update(float deltaTime)
        {
            if (CheckUniversalTransitions())
                return;

            // Jump Check
            if (Context.Input != null && Context.Input.IsJumpTriggered)
            {
                if (Context.Controller != null && Context.Controller.CanJump())
                {
                    Context.Input.ConsumeJumpTrigger();
                    Context.StateMachine.ChangeState(Context.JumpState);
                    return;
                }
            }

            // Fall Check
            if (Context.Controller != null && !Context.Controller.IsGrounded && Context.Controller.Velocity.y < -1.0f)
            {
                Context.StateMachine.ChangeState(Context.FallState);
                return;
            }

            Vector3 moveDir = GetCameraRelativeMoveDirection();

            // Movement stopped -> Idle
            if (moveDir.sqrMagnitude < 0.001f)
            {
                Context.StateMachine.ChangeState(Context.IdleState);
                return;
            }

            // Sprint released -> Walk
            if (Context.Input != null && !Context.Input.IsSprintHeld)
            {
                Context.StateMachine.ChangeState(Context.WalkState);
                return;
            }

            // Drain stamina
            float staminaCost = 15f * deltaTime;
            if (Context.Attributes != null && !Context.Attributes.ConsumeStamina(staminaCost))
            {
                // Depleted stamina -> drop to normal walk
                Context.StateMachine.ChangeState(Context.WalkState);
                return;
            }

            // Drive sprint speed
            float sprintSpeed = Context.Attributes != null ? Context.Attributes.SprintSpeed : 8.0f;
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(moveDir, sprintSpeed);
            }

            base.Update(deltaTime);
        }
    }
}
