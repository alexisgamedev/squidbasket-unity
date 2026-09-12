using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// Drives the player's CharacterController. Walking recomputes horizontal velocity from
    /// WASD each frame; Shooting instead carries whatever horizontal momentum was already
    /// present without decaying it (CONTEXT.md "Shooting") — its real-world drift is slowed only
    /// as a side effect of Bullet Time scaling the deltaTime both states are fed.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpVelocity = 5f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private bool _isAirborne;

        public Vector3 HorizontalVelocity => _horizontalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        /// <summary>
        /// WASD movement relative to the given facing direction (Walking). While grounded, a true
        /// <paramref name="jumpPressed"/> launches the player upward and freezes horizontal
        /// velocity at its current value until landing — no air control, no double-jump.
        /// </summary>
        public void TickWithInput(Vector2 moveInput, Vector3 facingForward, float deltaTime, bool jumpPressed)
        {
            if (_controller.isGrounded)
            {
                _isAirborne = false;

                if (jumpPressed)
                {
                    _verticalVelocity = jumpVelocity;
                    _isAirborne = true;
                }
            }

            if (!_isAirborne)
            {
                Vector3 forward = Vector3.ProjectOnPlane(facingForward, Vector3.up).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward);
                _horizontalVelocity = (forward * moveInput.y + right * moveInput.x) * moveSpeed;

                transform.localRotation = Quaternion.LookRotation(forward, Vector3.up);
            }

            ApplyMotion(deltaTime);
        }

        /// <summary>Carries whatever horizontal momentum was already present, undecayed (Shooting).</summary>
        public void TickMomentumOnly(float deltaTime)
        {
            ApplyMotion(deltaTime);
        }

        /// <summary>
        /// Clears vertical velocity and any in-progress jump freeze. Used by Reset/Respawn so a
        /// mid-jump Reset doesn't leave the player still rising/falling or with horizontal input
        /// frozen after being teleported.
        /// </summary>
        public void ResetVerticalState()
        {
            _verticalVelocity = 0f;
            _isAirborne = false;
        }

        private void ApplyMotion(float deltaTime)
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * deltaTime;
            Vector3 motion = (_horizontalVelocity + Vector3.up * _verticalVelocity) * deltaTime;
            _controller.Move(motion);
        }
    }
}
