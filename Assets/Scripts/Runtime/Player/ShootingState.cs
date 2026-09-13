using Squidbasket.Gameplay;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Squidbasket.Player
{
    /// <summary>
    /// CONTEXT.md "Shooting": Bullet Time engages, the Power Bar starts looping, WASD is locked
    /// but existing momentum keeps carrying the player (ADR-0002). Owns the Bullet Time, motion
    /// blur, and first-person mesh-visibility enter/exit side effects, per ADR-0003 — and
    /// re-asserts the ball is kinematic and attached to the hand on Enter, so Shooting never
    /// depends on some other system having gotten that right first. PlayerStateController owns
    /// making the ball dynamic again (Shoot/Drop) at the moment Shooting is exited.
    /// </summary>
    public sealed class ShootingState : IPlayerState
    {
        private readonly PlayerMovement _movement;
        private readonly PowerBarOscillator _powerBar;
        private readonly float _bulletTimeScale;
        private readonly Ball _ball;
        private readonly Transform _handAnchor;
        private readonly MotionBlur _motionBlur;
        private readonly Renderer _bodyMeshRenderer;
        private readonly Renderer _eyesMeshRenderer;
        private float _previousTimeScale;

        public ShootingState(
            PlayerMovement movement,
            PowerBarOscillator powerBar,
            float bulletTimeScale,
            Ball ball,
            Transform handAnchor,
            MotionBlur motionBlur,
            Renderer bodyMeshRenderer,
            Renderer eyesMeshRenderer)
        {
            _movement = movement;
            _powerBar = powerBar;
            _bulletTimeScale = bulletTimeScale;
            _ball = ball;
            _handAnchor = handAnchor;
            _motionBlur = motionBlur;
            _bodyMeshRenderer = bodyMeshRenderer;
            _eyesMeshRenderer = eyesMeshRenderer;
        }

        public PowerBarOscillator PowerBar => _powerBar;

        public void Enter()
        {
            _ball?.AttachTo(_handAnchor);
            _powerBar.Reset();
            _previousTimeScale = Time.timeScale;
            Time.timeScale = _bulletTimeScale;

            if (_motionBlur != null)
            {
                _motionBlur.active = true;
            }

            // Hidden rather than the third-person Walking view, so the first-person Shooting
            // camera never clips through the player's own body/eyes model.
            if (_bodyMeshRenderer != null)
            {
                _bodyMeshRenderer.enabled = false;
            }

            if (_eyesMeshRenderer != null)
            {
                _eyesMeshRenderer.enabled = false;
            }
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

            if (_motionBlur != null)
            {
                _motionBlur.active = false;
            }

            if (_bodyMeshRenderer != null)
            {
                _bodyMeshRenderer.enabled = true;
            }

            if (_eyesMeshRenderer != null)
            {
                _eyesMeshRenderer.enabled = true;
            }
        }
    }
}
