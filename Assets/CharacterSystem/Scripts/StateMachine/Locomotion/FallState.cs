using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// Airborne descent state applying snappier gravity, caching pre-landing jump buffers,
    /// and transitioning cleanly into LandState upon ground contact.
    /// </summary>
    public class FallState : CharacterBaseState
    {
        public override string Name => "Fall";

        public FallState(CharacterContext context) : base(context) { }

        public override void Update(float deltaTime)
        {
            // Buffer early jump input prior to landing
            if (Context.Input != null && Context.Input.IsJumpTriggered)
            {
                if (Context.Controller != null)
                {
                    Context.Controller.BufferJump();
                }
            }

            // Air steering
            Vector3 moveDir = GetCameraRelativeMoveDirection();
            float speed = Context.Attributes != null ? Context.Attributes.MoveSpeed : 4.5f;
            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(moveDir, speed);
            }

            // Ground Contact -> LandState
            if (Context.Controller != null && Context.Controller.IsGrounded)
            {
                Context.StateMachine.ChangeState(Context.LandState);
                return;
            }

            base.Update(deltaTime);
        }
    }
}
