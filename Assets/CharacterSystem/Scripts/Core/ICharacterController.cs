using CharacterSystem.Attributes;
using UnityEngine;

namespace CharacterSystem.Core
{
    /// <summary>
    /// Interface exposing motor state, kinematics, and movement commands.
    /// </summary>
    public interface ICharacterController
    {
        Vector3 Velocity { get; }
        Vector3 HorizontalVelocity { get; }
        float CurrentSpeed { get; }
        bool IsGrounded { get; }
        Vector3 GroundNormal { get; }
        float SlopeAngle { get; }
        bool IsOnSteepSlope { get; }
        MovementConfigSO Config { get; }

        void SetTargetMovement(Vector3 worldDirection, float targetSpeed);
        void Jump();
        void CutJump();
        void ApplyImpulse(Vector3 impulse);
        void ResetVerticalVelocity();
    }
}
