using System;

namespace CharacterSystem.StateMachine
{
    /// <summary>
    /// Deterministic, non-allocating finite state machine orchestrator.
    /// Drives state entry, continuous tick updates, and clean transition exits.
    /// </summary>
    public class StateMachine
    {
        private IState currentState;
        private IState previousState;
        private bool isTransitioning;

        public IState CurrentState => currentState;
        public IState PreviousState => previousState;

        public event Action<IState, IState> OnStateChanged;

        public void Initialize(IState startingState)
        {
            if (startingState == null)
                return;

            currentState = startingState;
            currentState.Enter();
        }

        public bool ChangeState(IState newState)
        {
            if (newState == null || newState == currentState || isTransitioning)
                return false;

            isTransitioning = true;

            previousState = currentState;
            currentState?.Exit();

            currentState = newState;
            currentState.Enter();

            isTransitioning = false;
            OnStateChanged?.Invoke(previousState, currentState);
            return true;
        }

        public void Update(float deltaTime)
        {
            currentState?.Update(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            currentState?.FixedUpdate(fixedDeltaTime);
        }
    }
}
