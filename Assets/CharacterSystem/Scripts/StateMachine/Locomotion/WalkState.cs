using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// Locomotion state driving normal walking speed, steering along camera axes,
    /// and transitioning to sprint or airborne states.
    /// </summary>
    public class WalkState : CharacterBaseState
    {
        public override string Name => "Walk";

        public WalkState(CharacterContext context) : base(context) { }

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

            // Cessation of movement -> Idle
            if (moveDir.sqrMagnitude < 0.001f)
            {
                Context.StateMachine.ChangeState(Context.IdleState);
                return;
            }

            // Sprint held and stamina available -> Sprint
            if (Context.Input != null && Context.Input.IsSprintHeld && Context.Attributes != null && Context.Attributes.HasStamina(2f))
            {
                Context.StateMachine.ChangeState(Context.SprintState);
                return;
            }

            // Drive motor
            float speed = Context.Attributes != null ? Context.Attributes.MoveSpeed : 4.5f;
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(moveDir, speed);
            }

            base.Update(deltaTime);
        }
    }
}
