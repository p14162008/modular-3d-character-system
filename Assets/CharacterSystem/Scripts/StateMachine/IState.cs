namespace CharacterSystem.StateMachine
{
    /// <summary>
    /// Contract for all character state nodes in the hierarchical finite state machine.
    /// </summary>
    public interface IState
    {
        string Name { get; }
        void Enter();
        void Update(float deltaTime);
        void FixedUpdate(float fixedDeltaTime);
        void Exit();
    }
}
