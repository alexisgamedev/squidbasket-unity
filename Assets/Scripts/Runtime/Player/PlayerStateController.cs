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

        // Deliberately not gated on IsShooting: FireShot() reads this at the exact moment the
        // FSM has already flipped back to Walking, so gating here would always read 0 right when
        // it matters most. Callers that need "is the bar currently visible" should check
        // IsShooting themselves alongside this value.
        public float PowerBarNormalizedValue =>
            Mathf.InverseLerp(powerBarMin, powerBarMax, _shooting.PowerBar.CurrentValue);

        private void Awake()
        {
            _input = inputSourceBehaviour as IPlayerInputSource;
            _movement = GetComponent<PlayerMovement>();
            _fsm = new PlayerFsm();

            if (_input == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerStateController)} has no {nameof(inputSourceBehaviour)} assigned, or it " +
                    $"doesn't implement {nameof(IPlayerInputSource)} — the player will not respond to input.",
                    this);
            }

            if (ballHand != null && ballHand.HandAnchor == null)
            {
                Debug.LogWarning(
                    $"{nameof(PlayerBallHand)} on {ballHand.name} has no handAnchor assigned — " +
                    "the ball will attach with no anchor to track and freeze wherever it was released.",
                    ballHand);
            }

            var powerBar = new PowerBarOscillator(powerBarMin, powerBarMax, powerBarSpeed);
            _walking = new WalkingState(_movement, _input, sharedCamera.transform);
            _shooting = new ShootingState(
                _movement, powerBar, bulletTimeScale, ball, ballHand != null ? ballHand.HandAnchor : null);
            _current = _walking;
        }

        private void OnEnable()
        {
            // The player starts a Session already holding the ball (CONTEXT.md), same as after
            // a Reset — establish that explicitly rather than assuming the ball's initial scene
            // state (Rigidbody kinematic flag) already matches.
            if (ball != null && ballHand != null)
            {
                ball.AttachTo(ballHand.HandAnchor);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(PlayerStateController)} is missing its {nameof(ball)}/{nameof(ballHand)} " +
                    "reference — the player will never be able to enter Shooting.", this);
            }

            _current.Enter();
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

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
                // A same-frame release and timeout favors the release (ADR-0002: Shot Timeout is
                // the cost of holding through 2.5 loops *without* releasing, not a way to steal
                // an intentionally-released shot away from the player).
                if (shootReleased)
                {
                    FireShot();
                }
                else
                {
                    ball?.Drop();
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
                ? firstPersonCamera.GetDesiredPose(cameraPivot, _input.LookInput)
                : thirdPersonCamera.GetDesiredPose(cameraPivot, _input.LookInput);

            Transform cam = sharedCamera.transform;
            float t = 1f - Mathf.Exp(-cameraBlendSpeed * deltaTime);
            cam.position = Vector3.Lerp(cam.position, desired.Position, t);
            cam.rotation = Quaternion.Slerp(cam.rotation, desired.Rotation, t);
        }
    }
}
