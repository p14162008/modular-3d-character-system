using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// Stationary state smoothly bringing the motor to rest, listening for movement input,
    /// jump initiations, and combat interactions.
    /// </summary>
    public class IdleState : CharacterBaseState
    {
        public override string Name => "Idle";

        public IdleState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(Vector3.zero, 0f);
            }
        }

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

            // Movement Input Check
            Vector3 moveDir = GetCameraRelativeMoveDirection();
            if (moveDir.sqrMagnitude > 0.001f)
            {
                if (Context.Input != null && Context.Input.IsSprintHeld && Context.Attributes != null && Context.Attributes.HasStamina(5f))
                {
                    Context.StateMachine.ChangeState(Context.SprintState);
                }
                else
                {
                    Context.StateMachine.ChangeState(Context.WalkState);
                }
                return;
            }

            base.Update(deltaTime);
        }
    }
}
