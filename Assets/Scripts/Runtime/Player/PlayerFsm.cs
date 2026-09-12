namespace Squidbasket.Player
{
    public enum PlayerLifecycleState
    {
        Walking,
        Shooting
    }

    /// <summary>
    /// Pure transition core for the player's state-object FSM (ADR-0003). PlayerStateController
    /// drives this to decide when to swap its WalkingState/ShootingState objects and side
    /// effects (camera switch, Bullet Time); this class holds no engine references so the
    /// transition rules — including "Reset only fires during Walking" (ADR-0002) — are testable
    /// without a scene.
    /// </summary>
    public sealed class PlayerFsm
    {
        public PlayerLifecycleState Current { get; private set; } = PlayerLifecycleState.Walking;

        public bool TryEnterShooting(bool shootHeld)
        {
            if (Current == PlayerLifecycleState.Walking && shootHeld)
            {
                Current = PlayerLifecycleState.Shooting;
                return true;
            }

            return false;
        }

        public bool TryExitShooting(bool shootReleased, bool timedOut)
        {
            if (Current == PlayerLifecycleState.Shooting && (shootReleased || timedOut))
            {
                Current = PlayerLifecycleState.Walking;
                return true;
            }

            return false;
        }

        public bool CanReset() => Current == PlayerLifecycleState.Walking;
    }
}
