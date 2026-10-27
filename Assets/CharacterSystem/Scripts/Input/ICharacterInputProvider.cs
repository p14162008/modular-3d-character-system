using UnityEngine;

namespace CharacterSystem.Input
{
    /// <summary>
    /// Abstract input interface decoupling player hardware input from character motor actions.
    /// Allows the exact same modular character controller to be piloted by human players,
    /// AI behaviour trees, or automated testing harnesses.
    /// </summary>
    public interface ICharacterInputProvider
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsSprintHeld { get; }
        bool IsJumpTriggered { get; }
        bool IsJumpHeld { get; }
        bool IsAttackTriggered { get; }
        bool IsBlockHeld { get; }

        void ConsumeJumpTrigger();
        void ConsumeAttackTrigger();
    }
}
