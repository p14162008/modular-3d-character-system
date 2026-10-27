using UnityEngine;

namespace CharacterSystem.StateMachine.Locomotion
{
    /// <summary>
    /// Ground impact recovery state. Can be immediately interrupted by movement inputs
    /// for responsive, competitive game feel.
    /// </summary>
    public class LandState : CharacterBaseState
    {
        private float recoveryTimer;
        private const float DefaultLandDuration = 0.15f;

        public override string Name => "Land";

        public LandState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            recoveryTimer = DefaultLandDuration;

            if (Context.Animation != null)
            {
                Context.Animation.TriggerLand();
            }

            if (Context.Controller != null)
            {
                Context.Controller.ResetVerticalVelocity();
            }
        }

        public override void Update(float deltaTime)
        {
            if (CheckUniversalTransitions())
                return;

            Vector3 moveDir = GetCameraRelativeMoveDirection();

            // Responsive input cancellation: immediately step into locomotion
            if (moveDir.sqrMagnitude > 0.001f)
            {
                if (Context.Input != null && Context.Input.IsSprintHeld && Context.Attributes != null && Context.Attributes.HasStamina(2f))
                {
                    Context.StateMachine.ChangeState(Context.SprintState);
                }
                else
                {
                    Context.StateMachine.ChangeState(Context.WalkState);
                }
                return;
            }

            recoveryTimer -= deltaTime;
            if (recoveryTimer <= 0f)
            {
                Context.StateMachine.ChangeState(Context.IdleState);
                return;
            }

            base.Update(deltaTime);
        }
    }
}
