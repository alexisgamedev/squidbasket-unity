using NUnit.Framework;
using Squidbasket.Gameplay;
using Squidbasket.Player;
using UnityEngine;

namespace Squidbasket.Tests
{
    public class BallTests
    {
        private GameObject _ballObject;
        private Ball _ball;
        private Rigidbody _rigidbody;
        private GameObject _handObject;
        private PlayerBallHand _hand;

        [SetUp]
        public void SetUp()
        {
            _ballObject = new GameObject("Ball");
            _ball = _ballObject.AddComponent<Ball>();
            _rigidbody = _ballObject.GetComponent<Rigidbody>();
            _handObject = new GameObject("Hand");
            _hand = _handObject.AddComponent<PlayerBallHand>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_ballObject);
            Object.DestroyImmediate(_handObject);
        }

        [Test]
        public void AttachTo_MakesRigidbodyKinematicAndHeld()
        {
            _ball.AttachTo(_handObject.transform);

            Assert.IsTrue(_rigidbody.isKinematic);
            Assert.IsTrue(_ball.IsHeldByPlayer);
        }

        [Test]
        public void AttachTo_ZerosExistingVelocity()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = new Vector3(1f, 2f, 3f);

            _ball.AttachTo(_handObject.transform);

            Assert.AreEqual(Vector3.zero, _rigidbody.linearVelocity);
        }

        [Test]
        public void Shoot_MakesRigidbodyDynamicWithLaunchVelocityAndNotHeld()
        {
            _ball.AttachTo(_handObject.transform);
            var velocity = new Vector3(0f, 5f, 10f);

            _ball.Shoot(velocity);

            Assert.IsFalse(_rigidbody.isKinematic, "a Shot must hand the ball back to physics");
            Assert.IsFalse(_ball.IsHeldByPlayer);
            Assert.AreEqual(velocity, _rigidbody.linearVelocity);
        }

        [Test]
        public void Drop_MakesRigidbodyDynamicWithZeroVelocityAndNotHeld()
        {
            _ball.AttachTo(_handObject.transform);

            _ball.Drop();

            Assert.IsFalse(_rigidbody.isKinematic, "a Shot Timeout still hands the ball back to physics");
            Assert.IsFalse(_ball.IsHeldByPlayer);
            Assert.AreEqual(Vector3.zero, _rigidbody.linearVelocity);
        }

        [Test]
        public void IsHeldByPlayer_IsFalseUntilExplicitlyAttached()
        {
            // A freshly-added Ball must not silently claim to be held before anything has
            // synced the Rigidbody's kinematic state to match (this was the source of the
            // bug where the ball free-fell from frame one instead of starting in the hand).
            Assert.IsFalse(_ball.IsHeldByPlayer);
        }

        [Test]
        public void TryRetrieve_WhileShotIsInFlight_DoesNotReattach()
        {
            // A Shot that rebounds back near the player before settling must not be silently
            // re-picked-up mid-air: that would discard the pending make/miss outcome with no
            // GameEvents ever raised for it.
            _ball.AttachTo(_handObject.transform);
            _ball.Shoot(Vector3.forward);

            _ball.TryRetrieve(_hand);

            Assert.IsFalse(_ball.IsHeldByPlayer);
            Assert.IsFalse(_rigidbody.isKinematic);
        }

        [Test]
        public void TryRetrieve_WhileLoose_Reattaches()
        {
            _ball.AttachTo(_handObject.transform);
            _ball.Drop();

            _ball.TryRetrieve(_hand);

            Assert.IsTrue(_ball.IsHeldByPlayer);
            Assert.IsTrue(_rigidbody.isKinematic);
        }
    }
}
