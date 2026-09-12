namespace Squidbasket.Player
{
    /// <summary>A state object owned by PlayerStateController's state-object FSM (ADR-0003).</summary>
    public interface IPlayerState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
