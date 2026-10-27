using System;
using UnityEngine;

namespace CharacterSystem.Utilities
{
    /// <summary>
    /// High-performance 3D vector and trigonometric math utilities for character physics and kinematics.
    /// Provides non-allocating, numerical-stability guaranteed operations.
    /// </summary>
    public static class Math3DUtils
    {
        private const float Epsilon = 1e-5f;

        /// <summary>
        /// Projects a movement direction vector onto a surface plane defined by its normal,
        /// ensuring the projected direction preserves horizontal magnitude without sinking into the ground.
        /// </summary>
        /// <param name="direction">The raw world-space direction vector.</param>
        /// <param name="planeNormal">The surface normal from a ground hit.</param>
        /// <returns>Slope-aligned normalized or scaled direction vector.</returns>
        public static Vector3 ProjectOnContactPlane(Vector3 direction, Vector3 planeNormal)
        {
            if (planeNormal.sqrMagnitude < Epsilon)
                return direction;

            return direction - Vector3.Dot(direction, planeNormal) * planeNormal;
        }

        /// <summary>
        /// Calculates the slope angle in degrees relative to the global Up vector (0, 1, 0).
        /// </summary>
        /// <param name="surfaceNormal">The normal of the ground surface.</param>
        /// <returns>Angle in degrees between 0 (flat) and 90 (vertical wall).</returns>
        public static float CalculateSlopeAngle(Vector3 surfaceNormal)
        {
            if (surfaceNormal.sqrMagnitude < Epsilon)
                return 0f;

            return Vector3.Angle(Vector3.up, surfaceNormal);
        }

        /// <summary>
        /// Calculates the initial vertical launch velocity required to achieve a target apex height under gravity.
        /// Formula: v0 = sqrt(2 * g * h)
        /// </summary>
        /// <param name="jumpHeight">Desired jump apex height in meters.</param>
        /// <param name="gravity">Effective gravity acceleration (positive magnitude).</param>
        /// <returns>Initial vertical velocity in m/s.</returns>
        public static float CalculateJumpVelocity(float jumpHeight, float gravity)
        {
            if (jumpHeight <= 0f || gravity <= 0f)
                return 0f;

            return Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }

        /// <summary>
        /// Transforms 2D planar input (X = horizontal strafe, Y = vertical forward) into world-space
        /// movement direction aligned relative to the camera's horizontal gaze.
        /// </summary>
        /// <param name="input">Input vector (X: horizontal, Y: vertical).</param>
        /// <param name="cameraTransform">Camera transform reference (null defaults to world axes).</param>
        /// <returns>World-space movement direction vector.</returns>
        public static Vector3 TransformInputToCameraSpace(Vector2 input, Transform cameraTransform)
        {
            if (input.sqrMagnitude < Epsilon)
                return Vector3.zero;

            Vector3 forward;
            Vector3 right;

            if (cameraTransform != null)
            {
                forward = cameraTransform.forward;
                right = cameraTransform.right;

                // Flatten on XZ plane
                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }

            Vector3 direction = (forward * input.y) + (right * input.x);
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            return direction;
        }

        /// <summary>
        /// Smoothly rotates a source rotation towards a target direction on the XZ plane.
        /// </summary>
        /// <param name="currentRotation">Current character rotation.</param>
        /// <param name="targetDirection">Target forward direction vector (world space).</param>
        /// <param name="rotationSpeed">Angular speed in degrees per second.</param>
        /// <param name="deltaTime">Time step.</param>
        /// <returns>New interpolated Quaternion rotation.</returns>
        public static Quaternion RotateTowardsDirection(
            Quaternion currentRotation,
            Vector3 targetDirection,
            float rotationSpeed,
            float deltaTime)
        {
            targetDirection.y = 0f;
            if (targetDirection.sqrMagnitude < Epsilon)
                return currentRotation;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            return Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * deltaTime);
        }

        /// <summary>
        /// Checks if a slope angle exceeds the maximum traversable slope limit.
        /// </summary>
        public static bool IsSlopeTooSteep(float slopeAngle, float maxSlopeLimit)
        {
            return slopeAngle > maxSlopeLimit;
        }

        /// <summary>
        /// Computes the slide direction and acceleration down a steep slope.
        /// </summary>
        public static Vector3 CalculateSlopeSlideVelocity(Vector3 surfaceNormal, float gravity)
        {
            Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, surfaceNormal).normalized;
            return slideDirection * Mathf.Abs(gravity);
        }
    }
}
