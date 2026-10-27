using UnityEngine;

namespace CharacterSystem.Input
{
    /// <summary>
    /// AI/Scripted input provider for NPCs, dummy targets, stress test simulations,
    /// and automated unit/integration tests.
    /// </summary>
    public class AICharacterInput : MonoBehaviour, ICharacterInputProvider
    {
        [SerializeField] private Vector2 moveInput;
        [SerializeField] private Vector2 lookInput;
        [SerializeField] private bool isSprintHeld;
        [SerializeField] private bool isJumpTriggered;
        [SerializeField] private bool isJumpHeld;
        [SerializeField] private bool isAttackTriggered;
        [SerializeField] private bool isBlockHeld;

        public Vector2 MoveInput
        {
            get => moveInput;
            set => moveInput = Vector2.ClampMagnitude(value, 1.0f);
        }

        public Vector2 LookInput
        {
            get => lookInput;
            set => lookInput = value;
        }

        public bool IsSprintHeld
        {
            get => isSprintHeld;
            set => isSprintHeld = value;
        }

        public bool IsJumpTriggered
        {
            get => isJumpTriggered;
            set => isJumpTriggered = value;
        }

        public bool IsJumpHeld
        {
            get => isJumpHeld;
            set => isJumpHeld = value;
        }

        public bool IsAttackTriggered
        {
            get => isAttackTriggered;
            set => isAttackTriggered = value;
        }

        public bool IsBlockHeld
        {
            get => isBlockHeld;
            set => isBlockHeld = value;
        }

        public void ConsumeJumpTrigger()
        {
            isJumpTriggered = false;
        }

        public void ConsumeAttackTrigger()
        {
            isAttackTriggered = false;
        }

        public void ResetAll()
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isSprintHeld = false;
            isJumpTriggered = false;
            isJumpHeld = false;
            isAttackTriggered = false;
            isBlockHeld = false;
        }
    }
}
