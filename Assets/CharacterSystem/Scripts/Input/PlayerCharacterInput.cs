using UnityEngine;

namespace CharacterSystem.Input
{
    /// <summary>
    /// Player input implementation polling standard hardware axes with deadzone filtering
    /// and input consumption mechanisms.
    /// </summary>
    public class PlayerCharacterInput : MonoBehaviour, ICharacterInputProvider
    {
        [Header("Axis Mapping")]
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private string mouseXAxis = "Mouse X";
        [SerializeField] private string mouseYAxis = "Mouse Y";

        [Header("Key Bindings")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode blockKey = KeyCode.Mouse1;

        [Header("Tuning")]
        [SerializeField, Range(0.01f, 0.2f)] private float inputDeadzone = 0.05f;

        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool sprintHeld;
        private bool jumpTriggered;
        private bool jumpHeld;
        private bool attackTriggered;
        private bool blockHeld;

        public Vector2 MoveInput => moveInput;
        public Vector2 LookInput => lookInput;
        public bool IsSprintHeld => sprintHeld;
        public bool IsJumpTriggered => jumpTriggered;
        public bool IsJumpHeld => jumpHeld;
        public bool IsAttackTriggered => attackTriggered;
        public bool IsBlockHeld => blockHeld;

        private void Update()
        {
            PollInput();
        }

        private void PollInput()
        {
            float rawX = UnityEngine.Input.GetAxisRaw(horizontalAxis);
            float rawY = UnityEngine.Input.GetAxisRaw(verticalAxis);

            Vector2 rawMove = new Vector2(rawX, rawY);
            if (rawMove.magnitude < inputDeadzone)
            {
                moveInput = Vector2.zero;
            }
            else
            {
                moveInput = Vector2.ClampMagnitude(rawMove, 1.0f);
            }

            lookInput = new Vector2(
                UnityEngine.Input.GetAxis(mouseXAxis),
                UnityEngine.Input.GetAxis(mouseYAxis)
            );

            sprintHeld = UnityEngine.Input.GetKey(sprintKey);

            if (UnityEngine.Input.GetKeyDown(jumpKey))
            {
                jumpTriggered = true;
            }
            jumpHeld = UnityEngine.Input.GetKey(jumpKey);

            if (UnityEngine.Input.GetKeyDown(attackKey))
            {
                attackTriggered = true;
            }

            blockHeld = UnityEngine.Input.GetKey(blockKey);
        }

        public void ConsumeJumpTrigger()
        {
            jumpTriggered = false;
        }

        public void ConsumeAttackTrigger()
        {
            attackTriggered = false;
        }
    }
}
