using UnityEngine;

namespace CharacterSystem.Animation
{
    /// <summary>
    /// Static cache of Animator parameter and state hash IDs.
    /// Eliminates string hashing overhead during runtime updates for maximum multi-character performance.
    /// </summary>
    public static class AnimationHashIDs
    {
        // Float Parameters (Blend Trees)
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int HorizontalSpeed = Animator.StringToHash("HorizontalSpeed");
        public static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
        public static readonly int TurnAngle = Animator.StringToHash("TurnAngle");

        // Boolean Parameters
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int IsFalling = Animator.StringToHash("IsFalling");
        public static readonly int IsBlocking = Animator.StringToHash("IsBlocking");
        public static readonly int IsDead = Animator.StringToHash("IsDead");

        // Triggers
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Land = Animator.StringToHash("Land");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int ComboIndex = Animator.StringToHash("ComboIndex");
        public static readonly int Hurt = Animator.StringToHash("Hurt");
        public static readonly int HurtDirection = Animator.StringToHash("HurtDirection"); // 0: Front, 1: Back, 2: Left, 3: Right
        public static readonly int Die = Animator.StringToHash("Die");
    }
}
