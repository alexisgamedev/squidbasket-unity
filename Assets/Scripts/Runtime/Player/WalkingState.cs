using Squidbasket.Input;
using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// Default state (CONTEXT.md "Walking"): WASD movement relative to the camera, ball carried.
    /// </summary>
    [System.Serializable]
    public sealed class WalkingState : IPlayerState
    {
        private PlayerMovement _movement;
        private PlayerInputSource _input;
        private Transform _cameraTransform;

        public void Init(PlayerMovement movement, PlayerInputSource input, Transform cameraTransform)
        {
            _movement = movement;
            _input = input;
            _cameraTransform = cameraTransform;
        }

        public void Enter()
        {
        }

        public void Tick(float deltaTime)
        {
            _movement.TickWithInput(_input.MoveInput, _cameraTransform.forward, deltaTime, _input.JumpPressed);
        }

        public void Exit()
        {
        }
    }
}
