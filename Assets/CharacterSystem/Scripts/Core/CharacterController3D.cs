using CharacterSystem.Attributes;
using CharacterSystem.Physics;
using CharacterSystem.Utilities;
using UnityEngine;

namespace CharacterSystem.Core
{
    /// <summary>
    /// Production-grade 3D character motor handling camera-aligned kinematics,
    /// acceleration/deceleration response curves, slope physics, jumping mechanics,
    /// moving platform riding, and dynamic obstacle interactions.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterController3D : MonoBehaviour, ICharacterController
    {
        [Header("Configuration")]
        [SerializeField] private MovementConfigSO config;

        [Header("Layer Masks")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Header("Runtime References")]
        [SerializeField] private MovingPlatformDetector platformDetector;

        private CharacterController controller;
        private Vector3 currentVelocity;
        private Vector3 targetMoveDirection;
        private float targetSpeed;
        private Vector3 externalImpulse;

        // Ground & Slope telemetry
        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float slopeAngle;
        private bool isOnSteepSlope;

        // Jump & Timing Windows
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool isJumping;

        // Non-alloc buffer for ground checks
        private readonly RaycastHit[] groundHitBuffer = new RaycastHit[4];

        public Vector3 Velocity => currentVelocity;
        public Vector3 HorizontalVelocity => new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        public float CurrentSpeed => HorizontalVelocity.magnitude;
        public bool IsGrounded => isGrounded;
        public Vector3 GroundNormal => groundNormal;
        public float SlopeAngle => slopeAngle;
        public bool IsOnSteepSlope => isOnSteepSlope;
        public MovementConfigSO Config => config;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (platformDetector == null)
            {
                platformDetector = GetComponent<MovingPlatformDetector>();
                if (platformDetector == null)
                {
                    platformDetector = gameObject.AddComponent<MovingPlatformDetector>();
                }
            }

            if (config != null)
            {
                controller.stepOffset = config.StepOffset;
                controller.slopeLimit = config.MaxSlopeAngle;
            }
        }

        public void SetConfiguration(MovementConfigSO newConfig)
        {
            config = newConfig;
            if (controller != null && config != null)
            {
                controller.stepOffset = config.StepOffset;
                controller.slopeLimit = config.MaxSlopeAngle;
            }
        }

        public void SetTargetMovement(Vector3 worldDirection, float speed)
        {
            targetMoveDirection = worldDirection;
            targetSpeed = speed;
        }

        public void ApplyImpulse(Vector3 impulse)
        {
            externalImpulse += impulse;
        }

        public void ResetVerticalVelocity()
        {
            currentVelocity.y = 0f;
        }

        public void BufferJump()
        {
            jumpBufferTimer = config != null ? config.JumpBufferTime : 0.15f;
        }

        public bool CanJump()
        {
            return (isGrounded || coyoteTimer > 0f) && !isOnSteepSlope;
        }

        public void Jump()
        {
            if (config == null)
                return;

            float jumpVelocity = Math3DUtils.CalculateJumpVelocity(config.JumpApexHeight, config.GravityMagnitude);
            currentVelocity.y = jumpVelocity;
            isGrounded = false;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            isJumping = true;
        }

        public void CutJump()
        {
            if (isJumping && currentVelocity.y > 0f)
            {
                float cut = config != null ? config.JumpCutMultiplier : 0.5f;
                currentVelocity.y *= cut;
                isJumping = false;
            }
        }

        public void TickMotor(float deltaTime)
        {
            if (controller == null || config == null)
                return;

            CheckGroundStatus();
            UpdateTimers(deltaTime);
            ProcessHorizontalKinematics(deltaTime);
            ProcessVerticalKinematics(deltaTime);
            ProcessRotation(deltaTime);

            // Compose total displacement
            Vector3 finalVelocity = currentVelocity + externalImpulse;
            Vector3 displacement = finalVelocity * deltaTime;

            // Apply platform displacement
            if (isGrounded && platformDetector != null)
            {
                Vector3 platformDelta = platformDetector.ComputePlatformDelta(transform.position);
                displacement += platformDelta;
            }

            controller.Move(displacement);

            // Decay external impulse
            if (externalImpulse.sqrMagnitude > 0.001f)
            {
                externalImpulse = Vector3.MoveTowards(externalImpulse, Vector3.zero, 25f * deltaTime);
            }
        }

        private void CheckGroundStatus()
        {
            // Center raycast and spherecast for ground normal
            Vector3 origin = transform.position + (Vector3.up * 0.1f);
            float checkDist = (controller.height * 0.5f) - controller.radius + (config.GroundCheckDistance + 0.1f);

            int hits = UnityEngine.Physics.SphereCastNonAlloc(
                origin,
                controller.radius * 0.95f,
                Vector3.down,
                groundHitBuffer,
                checkDist,
                groundLayers,
                QueryTriggerInteraction.Ignore);

            if (hits > 0)
            {
                // Find closest hit
                float closestDist = float.MaxValue;
                RaycastHit groundHit = default;

                for (int i = 0; i < hits; i++)
                {
                    if (groundHitBuffer[i].collider != null && groundHitBuffer[i].collider != controller)
                    {
                        if (groundHitBuffer[i].distance < closestDist)
                        {
                            closestDist = groundHitBuffer[i].distance;
                            groundHit = groundHitBuffer[i];
                        }
                    }
                }

                if (closestDist < float.MaxValue)
                {
                    isGrounded = controller.isGrounded || (closestDist <= config.GroundCheckDistance + 0.1f);
                    groundNormal = groundHit.normal;
                    slopeAngle = Math3DUtils.CalculateSlopeAngle(groundNormal);
                    isOnSteepSlope = Math3DUtils.IsSlopeTooSteep(slopeAngle, config.MaxSlopeAngle);

                    if (platformDetector != null && isGrounded)
                    {
                        platformDetector.SetPlatform(groundHit.transform);
                    }
                    return;
                }
            }

            isGrounded = controller.isGrounded;
            groundNormal = Vector3.up;
            slopeAngle = 0f;
            isOnSteepSlope = false;

            if (platformDetector != null)
            {
                platformDetector.ClearPlatform();
            }
        }

        private void UpdateTimers(float deltaTime)
        {
            if (isGrounded)
            {
                coyoteTimer = config.CoyoteTime;
                isJumping = false;
            }
            else
            {
                coyoteTimer = Mathf.Max(0f, coyoteTimer - deltaTime);
            }

            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer = Mathf.Max(0f, jumpBufferTimer - deltaTime);
                if (CanJump())
                {
                    Jump();
                }
            }
        }

        private void ProcessHorizontalKinematics(float deltaTime)
        {
            Vector3 desiredDir = targetMoveDirection;

            // Handle Steep Slopes
            if (isGrounded && isOnSteepSlope)
            {
                Vector3 slideVelocity = Math3DUtils.CalculateSlopeSlideVelocity(groundNormal, config.SlopeSlideSpeed);
                currentVelocity.x = slideVelocity.x;
                currentVelocity.z = slideVelocity.z;
                return;
            }

            // Slope projection
            if (isGrounded && desiredDir.sqrMagnitude > 0.001f)
            {
                desiredDir = Math3DUtils.ProjectOnContactPlane(desiredDir, groundNormal).normalized;
            }

            Vector3 targetHorizontalVelocity = desiredDir * targetSpeed;
            Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            float accelRate;
            if (targetHorizontalVelocity.sqrMagnitude > 0.001f)
            {
                // Accelerating
                accelRate = config.Acceleration;
                if (!isGrounded)
                {
                    accelRate *= config.AirControlMultiplier;
                }
            }
            else
            {
                // Snappy Deceleration / Braking
                accelRate = config.Deceleration;
            }

            Vector3 updatedHorizontal = Vector3.MoveTowards(
                currentHorizontal,
                targetHorizontalVelocity,
                accelRate * deltaTime);

            currentVelocity.x = updatedHorizontal.x;
            currentVelocity.z = updatedHorizontal.z;
        }

        private void ProcessVerticalKinematics(float deltaTime)
        {
            if (isGrounded && currentVelocity.y < 0f)
            {
                // Slight downward force to keep grounded securely on slopes and stairs
                currentVelocity.y = -2.5f;
            }
            else
            {
                float gravity = config.GravityMagnitude;
                if (currentVelocity.y < 0f)
                {
                    // Snappy falling
                    gravity *= config.FallGravityMultiplier;
                }

                currentVelocity.y -= gravity * deltaTime;
            }
        }

        private void ProcessRotation(float deltaTime)
        {
            if (targetMoveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Math3DUtils.RotateTowardsDirection(
                    transform.rotation,
                    targetMoveDirection,
                    config.RotationSpeed,
                    deltaTime);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Push dynamic rigidbodies (crates, barrels, dynamic obstacles)
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb == null || rb.isKinematic)
                return;

            // Don't push objects below our feet
            if (hit.moveDirection.y < -0.3f)
                return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            if (pushDir.sqrMagnitude < 0.001f)
                return;

            float push = config != null ? config.PushForce : 2.0f;
            rb.AddForceAtPosition(pushDir * push, hit.point, ForceMode.Impulse);
        }

        private void OnDrawGizmosSelected()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (controller != null)
            {
                // Ground check debug sphere
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Vector3 checkCenter = transform.position + (Vector3.up * (controller.radius + 0.05f)) + (Vector3.down * (config != null ? config.GroundCheckDistance : 0.25f));
                Gizmos.DrawWireSphere(checkCenter, controller.radius * 0.95f);

                // Ground normal vector
                if (isGrounded)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(transform.position, groundNormal * 1.5f);
                }
            }
        }
    }
}
