using CharacterSystem.Utilities;
using NUnit.Framework;
using UnityEngine;

namespace CharacterSystem.Tests.EditMode
{
    [TestFixture]
    public class Math3DTests
    {
        [Test]
        public void CalculateSlopeAngle_FlatSurface_ReturnsZeroDegrees()
        {
            Vector3 normal = Vector3.up;
            float angle = Math3DUtils.CalculateSlopeAngle(normal);
            Assert.AreEqual(0f, angle, 0.001f);
        }

        [Test]
        public void CalculateSlopeAngle_45DegreeSlope_Returns45Degrees()
        {
            Vector3 normal = new Vector3(1f, 1f, 0f).normalized;
            float angle = Math3DUtils.CalculateSlopeAngle(normal);
            Assert.AreEqual(45f, angle, 0.01f);
        }

        [Test]
        public void CalculateSlopeAngle_VerticalWall_Returns90Degrees()
        {
            Vector3 normal = Vector3.right;
            float angle = Math3DUtils.CalculateSlopeAngle(normal);
            Assert.AreEqual(90f, angle, 0.001f);
        }

        [Test]
        public void IsSlopeTooSteep_ExceedingLimit_ReturnsTrue()
        {
            Assert.IsTrue(Math3DUtils.IsSlopeTooSteep(50f, 45f));
            Assert.IsFalse(Math3DUtils.IsSlopeTooSteep(40f, 45f));
        }

        [Test]
        public void CalculateJumpVelocity_ValidInputs_MatchesPhysicsFormula()
        {
            float height = 2.0f;
            float gravity = 20.0f;
            // v = sqrt(2 * g * h) = sqrt(2 * 20 * 2) = sqrt(80) ≈ 8.94427 m/s
            float expected = Mathf.Sqrt(2f * gravity * height);
            float actual = Math3DUtils.CalculateJumpVelocity(height, gravity);
            Assert.AreEqual(expected, actual, 0.001f);
        }

        [Test]
        public void ProjectOnContactPlane_SlopeSurface_PreservesPlanarMagnitude()
        {
            Vector3 direction = Vector3.forward;
            Vector3 normal = new Vector3(0f, 1f, -1f).normalized; // 45 degree slope forward

            Vector3 projected = Math3DUtils.ProjectOnContactPlane(direction, normal);

            Assert.Greater(projected.y, 0f, "Projected slope vector should point upward along the slope.");
            Assert.AreEqual(direction.x, projected.x, 0.001f);
        }

        [Test]
        public void TransformInputToCameraSpace_NullCamera_UsesWorldAxes()
        {
            Vector2 input = new Vector2(0f, 1f); // Forward
            Vector3 worldDir = Math3DUtils.TransformInputToCameraSpace(input, null);

            Assert.AreEqual(Vector3.forward, worldDir);
        }
    }
}
