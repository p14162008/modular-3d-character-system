using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// Airborne upward propulsion state. Supports variable jump heights via input release cut
    /// and transitions to FallState at the jump trajectory apex.
    /// </summary>
    public class JumpState : CharacterBaseState
    {
        public override string Name => "Jump";

        public JumpState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();

            if (Context.Controller != null)
            {
                Context.Controller.Jump();
            }

            if (Context.Animation != null)
            {
                Context.Animation.TriggerJump();
            }
        }

        public override void Update(float deltaTime)
        {
            // Early button release -> cut upward velocity for variable jump height
            if (Context.Input != null && !Context.Input.IsJumpHeld)
            {
                if (Context.Controller != null)
                {
                    Context.Controller.CutJump();
                }
            }

            // Airborne steering
            Vector3 moveDir = GetCameraRelativeMoveDirection();
            float speed = Context.Attributes != null ? Context.Attributes.MoveSpeed : 4.5f;
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(moveDir, speed);
            }

            // Apex reached or falling downward -> FallState
            if (Context.Controller != null && Context.Controller.Velocity.y <= 0f)
            {
                Context.StateMachine.ChangeState(Context.FallState);
                return;
            }

            base.Update(deltaTime);
        }
    }
}
