using System;
using UnityEngine;

namespace CharacterSystem.Core
{
    /// <summary>
    /// Value type containing snapshot telemetry data of character motor kinematics.
    /// </summary>
    [Serializable]
    public struct CharacterMotorData
    {
        public Vector3 Velocity;
        public Vector3 DesiredDirection;
        public float TargetSpeed;
        public float CurrentSpeed;
        public bool IsGrounded;
        public float SlopeAngle;
        public Vector3 GroundNormal;
        public bool IsSliding;
        public float CoyoteTimer;
        public float JumpBufferTimer;
    }
}
