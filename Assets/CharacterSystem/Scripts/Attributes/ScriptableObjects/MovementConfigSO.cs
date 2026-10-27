using UnityEngine;

namespace CharacterSystem.Attributes
{
    /// <summary>
    /// Data asset configuring character motor physics, acceleration/deceleration response curves,
    /// jump dynamics, coyote time, and slope traversal limits.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMovementConfig", menuName = "Character System/Movement/Movement Configuration Asset")]
    public class MovementConfigSO : ScriptableObject
    {
        [Header("Acceleration & Snappiness")]
        [Tooltip("Rate of acceleration to target speed in m/s^2.")]
        [SerializeField] private float acceleration = 18f;

        [Tooltip("Rate of braking deceleration when input ceases in m/s^2. Higher values eliminate floaty sliding.")]
        [SerializeField] private float deceleration = 24f;

        [Tooltip("Air acceleration multiplier scaling control while airborne (0 = no air control, 1 = full control).")]
        [SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.5f;

        [Tooltip("Angular speed in degrees per second for turning to face movement direction.")]
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Jumping & Gravity Dynamics")]
        [Tooltip("Peak apex jump height in meters.")]
        [SerializeField] private float jumpApexHeight = 1.8f;

        [Tooltip("Downward gravity acceleration (positive magnitude) in m/s^2.")]
        [SerializeField] private float gravityMagnitude = 22f;

        [Tooltip("Multiplier applied to gravity when falling downward for snappy platforming feel.")]
        [SerializeField] private float fallGravityMultiplier = 1.6f;

        [Tooltip("Velocity dampening factor applied when jump button is released before apex (variable jump height).")]
        [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.5f;

        [Tooltip("Duration in seconds the character can jump after walking off a ledge (Coyote Time).")]
        [SerializeField] private float coyoteTime = 0.15f;

        [Tooltip("Duration in seconds an early jump input is buffered prior to landing (Jump Buffering).")]
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Environment & Slope Physics")]
        [Tooltip("Maximum traversable slope angle in degrees.")]
        [SerializeField, Range(0f, 89f)] private float maxSlopeAngle = 45f;

        [Tooltip("Downward slide acceleration speed when standing on an overly steep slope.")]
        [SerializeField] private float slopeSlideSpeed = 12f;

        [Tooltip("Step height offset the character can step over automatically.")]
        [SerializeField] private float stepOffset = 0.35f;

        [Tooltip("Ground check spherecast distance below controller bounds.")]
        [SerializeField] private float groundCheckDistance = 0.25f;

        [Tooltip("Push force multiplier applied to dynamic non-kinematic Rigidbodies upon collision.")]
        [SerializeField] private float pushForce = 2.0f;

        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirControlMultiplier => airControlMultiplier;
        public float RotationSpeed => rotationSpeed;
        public float JumpApexHeight => jumpApexHeight;
        public float GravityMagnitude => gravityMagnitude;
        public float FallGravityMultiplier => fallGravityMultiplier;
        public float JumpCutMultiplier => jumpCutMultiplier;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public float MaxSlopeAngle => maxSlopeAngle;
        public float SlopeSlideSpeed => slopeSlideSpeed;
        public float StepOffset => stepOffset;
        public float GroundCheckDistance => groundCheckDistance;
        public float PushForce => pushForce;
    }
}
