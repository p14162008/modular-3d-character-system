using CharacterSystem.Utilities;
using UnityEngine;

namespace CharacterSystem.StateMachine
{
    /// <summary>
    /// Abstract base state providing common transition checks, input decoding,
    /// and locomotion helpers to all specialized states.
    /// </summary>
    public abstract class CharacterBaseState : IState
    {
        protected readonly CharacterContext Context;

        public abstract string Name { get; }

        protected CharacterBaseState(CharacterContext context)
        {
            Context = context;
        }

        public virtual void Enter() { }

        public virtual void Update(float deltaTime)
        {
            // Default locomotion sync with animation
            if (Context.Animation != null && Context.Controller != null)
            {
                Context.Animation.SetLocomotion(
                    Context.Controller.CurrentSpeed,
                    Context.Controller.Velocity.y,
                    Context.Controller.IsGrounded,
                    deltaTime);
            }
        }

        public virtual void FixedUpdate(float fixedDeltaTime) { }

        public virtual void Exit() { }

        /// <summary>
        /// Decodes 2D input into world-space movement vector projected relative to the camera gaze.
        /// </summary>
        protected Vector3 GetCameraRelativeMoveDirection()
        {
            if (Context.Input == null)
                return Vector3.zero;

            return Math3DUtils.TransformInputToCameraSpace(Context.Input.MoveInput, Context.CameraTransform);
        }

        /// <summary>
        /// Checks common combat and airborne interruption transitions.
        /// </summary>
        protected bool CheckUniversalTransitions()
        {
            // Death Check
            if (Context.Attributes != null && Context.Attributes.IsDead)
            {
                Context.StateMachine.ChangeState(Context.DeathState);
                return true;
            }

            // Attack Trigger Check
            if (Context.Input != null && Context.Input.IsAttackTriggered && Context.Combat != null && Context.Combat.CanInitiateAttack())
            {
                Context.Input.ConsumeAttackTrigger();
                Context.StateMachine.ChangeState(Context.AttackState);
                return true;
            }

            // Block Check
            if (Context.Input != null && Context.Input.IsBlockHeld && Context.Combat != null && !Context.Combat.IsAttacking)
            {
                Context.StateMachine.ChangeState(Context.BlockState);
                return true;
            }

            return false;
        }
    }
}
