using CharacterSystem.Combat;
using UnityEngine;

namespace CharacterSystem.Animation
{
    /// <summary>
    /// Mediates communication between character state logic and Unity's Animator component.
    /// Uses hash lookups, parameter dampening, and fixed-time cross-fading to eliminate visual hitching.
    /// </summary>
    public class CharacterAnimationController : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;

        [Header("Blending & Dampening")]
        [Tooltip("Dampening time in seconds for locomotion speed transitions in blend trees.")]
        [SerializeField] private float speedDampTime = 0.1f;

        [Tooltip("Default fixed-time transition duration for cross-fades.")]
        [SerializeField] private float crossFadeDuration = 0.15f;

        public Animator Animator => animator;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void SetLocomotion(float currentSpeed, float verticalVelocity, bool isGrounded, float deltaTime)
        {
            if (animator == null)
                return;

            animator.SetFloat(AnimationHashIDs.Speed, currentSpeed, speedDampTime, deltaTime);
            animator.SetFloat(AnimationHashIDs.VerticalVelocity, verticalVelocity);
            animator.SetBool(AnimationHashIDs.IsGrounded, isGrounded);
            animator.SetBool(AnimationHashIDs.IsFalling, !isGrounded && verticalVelocity < -0.5f);
        }

        public void TriggerJump()
        {
            if (animator == null)
                return;

            animator.SetTrigger(AnimationHashIDs.Jump);
        }

        public void TriggerLand()
        {
            if (animator == null)
                return;

            animator.SetTrigger(AnimationHashIDs.Land);
        }

        public void TriggerAttack(int comboIndex, string customTrigger = null)
        {
            if (animator == null)
                return;

            animator.SetInteger(AnimationHashIDs.ComboIndex, comboIndex);

            if (!string.IsNullOrEmpty(customTrigger))
            {
                animator.CrossFadeInFixedTime(customTrigger, 0.08f);
            }
            else
            {
                animator.SetTrigger(AnimationHashIDs.Attack);
            }
        }

        public void SetBlocking(bool isBlocking)
        {
            if (animator == null)
                return;

            animator.SetBool(AnimationHashIDs.IsBlocking, isBlocking);
        }

        public void TriggerHurt(Vector3 localHitDirection)
        {
            if (animator == null)
                return;

            // Determine directional sector: Front, Back, Left, Right
            int dirIndex = 0; // 0 = Front
            float dotFwd = localHitDirection.z;
            float dotRight = localHitDirection.x;

            if (dotFwd < -0.5f)
                dirIndex = 1; // Back
            else if (dotRight > 0.5f)
                dirIndex = 3; // Right
            else if (dotRight < -0.5f)
                dirIndex = 2; // Left

            animator.SetInteger(AnimationHashIDs.HurtDirection, dirIndex);
            animator.SetTrigger(AnimationHashIDs.Hurt);
        }

        public void TriggerDeath()
        {
            if (animator == null)
                return;

            animator.SetBool(AnimationHashIDs.IsDead, true);
            animator.CrossFadeInFixedTime("Death", crossFadeDuration);
        }

        public void CrossFadeState(string stateName, float duration = -1f)
        {
            if (animator == null)
                return;

            float blend = duration >= 0f ? duration : crossFadeDuration;
            animator.CrossFadeInFixedTime(stateName, blend);
        }
    }
}
