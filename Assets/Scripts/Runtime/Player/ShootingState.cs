using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// CONTEXT.md "Shooting": Bullet Time engages, the Power Bar starts looping, WASD is locked
    /// but existing momentum keeps carrying the player (ADR-0002). Owns the Bullet Time
    /// enter/exit side effect, per ADR-0003.
    /// </summary>
    public sealed class ShootingState : IPlayerState
    {
        private readonly PlayerMovement _movement;
        private readonly PowerBarOscillator _powerBar;
        private readonly float _bulletTimeScale;
        private float _previousTimeScale;

        public ShootingState(PlayerMovement movement, PowerBarOscillator powerBar, float bulletTimeScale)
        {
            _movement = movement;
            _powerBar = powerBar;
            _bulletTimeScale = bulletTimeScale;
        }

        public PowerBarOscillator PowerBar => _powerBar;

        public void Enter()
        {
            _powerBar.Reset();
            _previousTimeScale = Time.timeScale;
            Time.timeScale = _bulletTimeScale;
        }

        public void Tick(float deltaTime)
        {
            // deltaTime is Time.deltaTime (already Bullet-Time-scaled), so momentum's real-world
            // drift is slowed as a natural consequence; the Power Bar alone uses unscaled time.
            _movement.TickMomentumOnly(deltaTime);
            _powerBar.Advance(Time.unscaledDeltaTime);
        }

        public void Exit()
        {
            Time.timeScale = _previousTimeScale;
        }
    }
}
