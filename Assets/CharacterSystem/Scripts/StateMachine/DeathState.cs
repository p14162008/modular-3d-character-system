using UnityEngine;

namespace CharacterSystem.StateMachine
{
    /// <summary>
    /// Terminal state entered upon health exhaustion. Disables input processing,
    /// triggers ragdoll/death animation, and freezes motor inputs.
    /// </summary>
    public class DeathState : CharacterBaseState
    {
        public override string Name => "Death";

        public DeathState(CharacterContext context) : base(context) { }

        public override void Enter()
        {
            base.Enter();

            if (Context.Animation != null)
            {
                Context.Animation.TriggerDeath();
            }

            if (Context.Controller != null)
            {
                Context.Controller.SetTargetMovement(Vector3.zero, 0f);
            }
        }

        public override void Update(float deltaTime)
        {
            // Terminal state - no further transitions
        }
    }
}
