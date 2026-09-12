using NUnit.Framework;
using Squidbasket.Gameplay;
using Squidbasket.Player;
using UnityEngine;

namespace Squidbasket.Tests
{
    public class ShootingStateTests
    {
        private GameObject _playerObject;
        private PlayerMovement _movement;
        private GameObject _ballObject;
        private Ball _ball;
        private Rigidbody _rigidbody;
        private GameObject _handObject;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("Player");
            _movement = _playerObject.AddComponent<PlayerMovement>();

            _ballObject = new GameObject("Ball");
            _ball = _ballObject.AddComponent<Ball>();
            _rigidbody = _ballObject.GetComponent<Rigidbody>();

            _handObject = new GameObject("Hand");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_ballObject);
            Object.DestroyImmediate(_handObject);
        }

        [Test]
        public void Enter_AttachesBallToHandAndMakesItKinematic()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState(_movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform);

            try
            {
                state.Enter();

                Assert.IsTrue(_ball.IsHeldByPlayer, "Shooting must not depend on Walking/Retrieval having already attached the ball");
                Assert.IsTrue(_rigidbody.isKinematic);
            }
            finally
            {
                state.Exit(); // restores Time.timeScale
            }
        }

        [Test]
        public void Enter_WithNoBallAssigned_DoesNotThrow()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState(_movement, powerBar, bulletTimeScale: 0.3f, ball: null, handAnchor: null);

            try
            {
                Assert.DoesNotThrow(() => state.Enter());
            }
            finally
            {
                state.Exit();
            }
        }
    }
}
