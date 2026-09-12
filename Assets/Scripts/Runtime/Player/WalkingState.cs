using Squidbasket.Input;
using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// Default state (CONTEXT.md "Walking"): WASD movement relative to the camera, ball carried.
    /// </summary>
    public sealed class WalkingState : IPlayerState
    {
        private readonly PlayerMovement _movement;
        private readonly UnityInputPlayerInputSource _input;
        private readonly Transform _cameraTransform;

        public WalkingState(PlayerMovement movement, UnityInputPlayerInputSource input, Transform cameraTransform)
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
            _movement.TickWithInput(_input.MoveInput, _cameraTransform.forward, deltaTime);
        }

        public void Exit()
        {
        }
    }
}
