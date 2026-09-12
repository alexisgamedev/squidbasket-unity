using Squidbasket.Camera;
using Squidbasket.Gameplay;
using Squidbasket.Scoring;
using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// State-object FSM context (ADR-0003): owns WalkingState/ShootingState, drives
    /// Movement/camera/Ball directly, and owns Reset (which only fires during Walking,
    /// preserving the "no player-initiated cancel" rule from ADR-0002).
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityEngine.Camera sharedCamera;
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private FirstPersonCamera firstPersonCamera;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Ball ball;
        [SerializeField] private PlayerBallHand ballHand;
        [SerializeField] private Transform freeThrowLineAnchor;
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        [Header("Shooting")]
        [SerializeField] private float powerBarMin;
        [SerializeField] private float powerBarMax = 1f;
        [SerializeField] private float powerBarSpeed = 1f;
        [SerializeField] private float bulletTimeScale = 0.3f;
        [SerializeField] private float shotSpeed = 12f;
        [SerializeField] private float shotUpArcScale = 0.6f;
        [SerializeField] private float cameraBlendSpeed = 8f;

        private IPlayerInputSource _input;
        private PlayerMovement _movement;
        private PlayerFsm _fsm;
        private WalkingState _walking;
        private ShootingState _shooting;
        private IPlayerState _current;

        public bool IsShooting => _fsm.Current == PlayerLifecycleState.Shooting;

        public float PowerBarNormalizedValue => IsShooting
            ? Mathf.InverseLerp(powerBarMin, powerBarMax, _shooting.PowerBar.CurrentValue)
            : 0f;

        private void Awake()
        {
            _input = inputSourceBehaviour as IPlayerInputSource;
            _movement = GetComponent<PlayerMovement>();
            _fsm = new PlayerFsm();

            var powerBar = new PowerBarOscillator(powerBarMin, powerBarMax, powerBarSpeed);
            _walking = new WalkingState(_movement, _input, sharedCamera.transform);
            _shooting = new ShootingState(_movement, powerBar, bulletTimeScale);
            _current = _walking;
        }

        private void OnEnable()
        {
            _current.Enter();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            if (_fsm.Current == PlayerLifecycleState.Walking)
            {
                TickWalking(deltaTime);
            }
            else
            {
                TickShooting(deltaTime);
            }

            UpdateCamera(deltaTime);
        }

        private void TickWalking(float deltaTime)
        {
            bool canShoot = ball != null && ball.IsHeldByPlayer;
            if (canShoot && _fsm.TryEnterShooting(_input.ShootHeld))
            {
                firstPersonCamera.SetOrientation(thirdPersonCamera.Yaw, thirdPersonCamera.Pitch);
                _current.Exit();
                _current = _shooting;
                _current.Enter();
                return;
            }

            _current.Tick(deltaTime);

            if (_fsm.CanReset() && _input.ResetPressed)
            {
                PerformReset();
            }
        }

        private void TickShooting(float deltaTime)
        {
            _current.Tick(deltaTime);

            bool shootReleased = !_input.ShootHeld;
            bool timedOut = _shooting.PowerBar.HasTimedOut;

            if (_fsm.TryExitShooting(shootReleased, timedOut))
            {
                if (timedOut)
                {
                    ball?.Drop();
                }
                else
                {
                    FireShot();
                }

                thirdPersonCamera.SetOrientation(firstPersonCamera.Yaw, firstPersonCamera.Pitch);
                _current.Exit();
                _current = _walking;
                _current.Enter();
            }
        }

        private void FireShot()
        {
            if (ball == null)
            {
                return;
            }

            float normalizedPower = PowerBarNormalizedValue;
            Vector3 velocity = ShotCalculator.ComputeLaunchVelocity(
                sharedCamera.transform.forward, normalizedPower, shotUpArcScale, shotSpeed);
            ball.Shoot(velocity);
        }

        private void PerformReset()
        {
            if (freeThrowLineAnchor != null)
            {
                transform.SetPositionAndRotation(freeThrowLineAnchor.position, freeThrowLineAnchor.rotation);
            }

            if (ball != null && ballHand != null)
            {
                ball.AttachTo(ballHand.HandAnchor);
            }
        }

        private void UpdateCamera(float deltaTime)
        {
            CameraPose desired = IsShooting
                ? firstPersonCamera.GetDesiredPose(cameraPivot, _input.LookInput, deltaTime)
                : thirdPersonCamera.GetDesiredPose(cameraPivot, _input.LookInput, deltaTime);

            Transform cam = sharedCamera.transform;
            float t = 1f - Mathf.Exp(-cameraBlendSpeed * deltaTime);
            cam.position = Vector3.Lerp(cam.position, desired.Position, t);
            cam.rotation = Quaternion.Slerp(cam.rotation, desired.Rotation, t);
        }
    }
}
