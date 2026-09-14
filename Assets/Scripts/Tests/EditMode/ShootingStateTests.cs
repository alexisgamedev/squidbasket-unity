using NUnit.Framework;
using Squidbasket.Gameplay;
using Squidbasket.Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        private MotionBlur _motionBlur;
        private GameObject _bodyMeshObject;
        private GameObject _eyesMeshObject;
        private MeshRenderer _bodyMeshRenderer;
        private MeshRenderer _eyesMeshRenderer;

        [SetUp]
        public void SetUp()
        {
            _playerObject = new GameObject("Player");
            _movement = _playerObject.AddComponent<PlayerMovement>();

            _ballObject = new GameObject("Ball");
            _ball = _ballObject.AddComponent<Ball>();
            _rigidbody = _ballObject.GetComponent<Rigidbody>();

            _handObject = new GameObject("Hand");

            _motionBlur = ScriptableObject.CreateInstance<MotionBlur>();

            _bodyMeshObject = new GameObject("BodyMesh");
            _bodyMeshRenderer = _bodyMeshObject.AddComponent<MeshRenderer>();

            _eyesMeshObject = new GameObject("EyesMesh");
            _eyesMeshRenderer = _eyesMeshObject.AddComponent<MeshRenderer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObject);
            Object.DestroyImmediate(_ballObject);
            Object.DestroyImmediate(_handObject);
            Object.DestroyImmediate(_motionBlur);
            Object.DestroyImmediate(_bodyMeshObject);
            Object.DestroyImmediate(_eyesMeshObject);
        }

        [Test]
        public void Enter_AttachesBallToHandAndMakesItKinematic()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                _bodyMeshRenderer, _eyesMeshRenderer);

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
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, 0.3f, ball: null, handAnchor: null, motionBlur: _motionBlur,
                bodyMeshRenderer: _bodyMeshRenderer, eyesMeshRenderer: _eyesMeshRenderer);

            try
            {
                Assert.DoesNotThrow(() => state.Enter());
            }
            finally
            {
                state.Exit();
            }
        }

        [Test]
        public void Enter_ActivatesMotionBlur()
        {
            _motionBlur.active = false;
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                _bodyMeshRenderer, _eyesMeshRenderer);

            try
            {
                state.Enter();

                Assert.IsTrue(_motionBlur.active);
            }
            finally
            {
                state.Exit();
            }
        }

        [Test]
        public void Exit_DeactivatesMotionBlur()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                _bodyMeshRenderer, _eyesMeshRenderer);
            state.Enter();

            state.Exit();

            Assert.IsFalse(_motionBlur.active, "Motion blur must not still be active once Walking is re-entered");
        }

        [Test]
        public void Enter_WithNoMotionBlurAssigned_DoesNotThrow()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, motionBlur: null,
                bodyMeshRenderer: _bodyMeshRenderer, eyesMeshRenderer: _eyesMeshRenderer);

            try
            {
                Assert.DoesNotThrow(() => state.Enter());
            }
            finally
            {
                Assert.DoesNotThrow(() => state.Exit());
            }
        }

        [Test]
        public void Enter_HidesBodyAndEyesMeshRenderers()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                _bodyMeshRenderer, _eyesMeshRenderer);

            try
            {
                state.Enter();

                Assert.IsFalse(_bodyMeshRenderer.enabled, "BodyMesh must be hidden so it doesn't obstruct the first-person Shooting camera");
                Assert.IsFalse(_eyesMeshRenderer.enabled, "EyesMesh must be hidden so it doesn't obstruct the first-person Shooting camera");
            }
            finally
            {
                state.Exit();
            }
        }

        [Test]
        public void Exit_ShowsBodyAndEyesMeshRenderers()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                _bodyMeshRenderer, _eyesMeshRenderer);
            state.Enter();

            state.Exit();

            Assert.IsTrue(_bodyMeshRenderer.enabled, "BodyMesh must be visible again once Walking is re-entered");
            Assert.IsTrue(_eyesMeshRenderer.enabled, "EyesMesh must be visible again once Walking is re-entered");
        }

        [Test]
        public void Enter_WithNoMeshRenderersAssigned_DoesNotThrow()
        {
            var powerBar = new PowerBarOscillator(0f, 1f, 1f);
            var state = new ShootingState();
            state.Init(
                _movement, powerBar, bulletTimeScale: 0.3f, _ball, _handObject.transform, _motionBlur,
                bodyMeshRenderer: null, eyesMeshRenderer: null);

            try
            {
                Assert.DoesNotThrow(() => state.Enter());
            }
            finally
            {
                Assert.DoesNotThrow(() => state.Exit());
            }
        }
    }
}
