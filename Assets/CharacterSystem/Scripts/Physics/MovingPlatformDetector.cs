using UnityEngine;

namespace CharacterSystem.Physics
{
    /// <summary>
    /// Detects moving or rotating platforms beneath the character and computes
    /// world-space delta displacements so the character moves synchronously without sliding.
    /// </summary>
    public class MovingPlatformDetector : MonoBehaviour
    {
        private Transform activePlatform;
        private Vector3 lastPlatformPosition;
        private Quaternion lastPlatformRotation;

        public bool HasActivePlatform => activePlatform != null;
        public Transform ActivePlatform => activePlatform;

        public void SetPlatform(Transform platform)
        {
            if (activePlatform != platform)
            {
                activePlatform = platform;
                if (activePlatform != null)
                {
                    lastPlatformPosition = activePlatform.position;
                    lastPlatformRotation = activePlatform.rotation;
                }
            }
        }

        public void ClearPlatform()
        {
            activePlatform = null;
        }

        public Vector3 ComputePlatformDelta(Vector3 characterPosition)
        {
            if (activePlatform == null)
                return Vector3.zero;

            // Translation delta
            Vector3 positionDelta = activePlatform.position - lastPlatformPosition;

            // Rotation delta around platform origin
            Quaternion rotationDelta = activePlatform.rotation * Quaternion.Inverse(lastPlatformRotation);
            Vector3 offsetFromOrigin = characterPosition - activePlatform.position;
            Vector3 rotatedOffset = rotationDelta * offsetFromOrigin;
            Vector3 rotationalDelta = (activePlatform.position + rotatedOffset) - characterPosition;

            // Update cache
            lastPlatformPosition = activePlatform.position;
            lastPlatformRotation = activePlatform.rotation;

            return positionDelta + rotationalDelta;
        }
    }
}
