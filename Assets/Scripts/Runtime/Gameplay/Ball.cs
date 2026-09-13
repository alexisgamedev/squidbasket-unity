using Squidbasket.Player;
using Squidbasket.Scoring;
using UnityEngine;

namespace Squidbasket.Gameplay
{
    /// <summary>
    /// Owns Retrieval (proximity pickup while loose, CONTEXT.md) and records the Zone/points for
    /// a Shot at the moment of release, so ScoreSystem never has to reach into another system's
    /// data (ADR-0003). HoopTrigger reports a pass-through to this ball; this ball itself detects
    /// a Miss when it settles without having been reported as a make.
    ///
    /// Starts un-held: whoever first hands the player the ball (PlayerStateController at
    /// startup, or Reset) must call <see cref="AttachTo"/> explicitly, so the Rigidbody's
    /// kinematic state and <see cref="IsHeldByPlayer"/> can never drift out of sync.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Ball : MonoBehaviour
    {
        // Held / InFlight / Loose as one enum (rather than independent bools) makes "can this be
        // picked up right now" unambiguous: only Loose allows Retrieval, so a shot that's still
        // resolving (InFlight, e.g. rebounding off the rim back toward the player) can no longer
        // be silently re-attached to the hand before its make/miss outcome is ever raised.
        private enum BallState
        {
            Held,
            InFlight,
            Loose
        }

        [SerializeField] private Transform hoop;
        [SerializeField] private float threePointRadius = 6.75f;
        [SerializeField] private float restSpeedThreshold = 0.05f;
        [SerializeField] private float restCheckDelay = 0.25f;

        private Rigidbody _rigidbody;
        private Transform _heldAnchor;

        [SerializeField] private BallState _state = BallState.Loose;
        private bool _hasScoredThisShot;
        private Zone _releaseZone;
        private int _releasePoints;
        private float _restTimer;

        public bool IsHeldByPlayer => _state == BallState.Held;

        // Fetched lazily rather than cached in Awake(): AttachTo can legitimately run before
        // Awake has (e.g. PlayerStateController.OnEnable calling it on a sibling component), and
        // RequireComponent already guarantees a Rigidbody exists on this GameObject regardless.
        private Rigidbody RigidbodyComponent => _rigidbody != null ? _rigidbody : (_rigidbody = GetComponent<Rigidbody>());

        private void LateUpdate()
        {
            if (_state == BallState.Held && _heldAnchor != null)
            {
                transform.SetPositionAndRotation(_heldAnchor.position, _heldAnchor.rotation);
            }
        }

        private void FixedUpdate()
        {
            if (_state == BallState.Held)
            {
                return;
            }

            if (RigidbodyComponent.linearVelocity.magnitude <= restSpeedThreshold)
            {
                _restTimer += Time.fixedDeltaTime;
                if (_restTimer >= restCheckDelay)
                {
                    OnSettled();
                }
            }
            else
            {
                _restTimer = 0f;
            }
        }

        /// <summary>Retrieval / Reset: attaches the ball to the player's hand anchor.</summary>
        public void AttachTo(Transform anchor)
        {
            _heldAnchor = anchor;
            _state = BallState.Held;
            _hasScoredThisShot = false;
            _restTimer = 0f;
            RigidbodyComponent.isKinematic = true;
            Debug.Log($"Ball attached to {anchor?.name ?? "null"}");
        }

        /// <summary>Releases the ball as a Shot with the given launch velocity (LMB released).</summary>
        public void Shoot(Vector3 velocity)
        {
            RecordReleaseZone();
            ReleasePhysics(velocity);
            _state = BallState.InFlight;
        }

        /// <summary>Releases the ball with no launch velocity after a Shot Timeout — the ball is lost.</summary>
        public void Drop()
        {
            ReleasePhysics(Vector3.zero);
            _state = BallState.Loose;
        }

        /// <summary>Called by HoopTrigger when this ball passes through the hoop.</summary>
        public int NotifyPassedThroughHoop()
        {
            if (_state != BallState.InFlight || _hasScoredThisShot)
            {
                return 0;
            }

            _hasScoredThisShot = true;
            GameEvents.RaiseShotMade(_releaseZone, _releasePoints);
            return _releasePoints;
        }

        private void RecordReleaseZone()
        {
            Vector3 hoopPosition = hoop != null ? hoop.position : Vector3.zero;
            _releaseZone = ShotCalculator.DetermineZone(transform.position, hoopPosition, threePointRadius);
            _releasePoints = ShotCalculator.PointsForZone(_releaseZone);
        }

        private void ReleasePhysics(Vector3 velocity)
        {
            _heldAnchor = null;
            _restTimer = 0f;
            RigidbodyComponent.isKinematic = false;
            RigidbodyComponent.linearVelocity = velocity;
        }

        private void OnSettled()
        {
            if (_state == BallState.InFlight && !_hasScoredThisShot)
            {
                GameEvents.RaiseShotMissed();
            }

            _state = BallState.Loose;
        }

        /// <summary>Retrieval attempt from a hand overlap; a no-op unless the ball is Loose (not
        /// held, and not still resolving a Shot in flight).</summary>
        public void TryRetrieve(PlayerBallHand hand)
        {
            if (_state != BallState.Loose || hand == null)
            {
                return;
            }

            AttachTo(hand.HandAnchor);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryRetrieve(other.GetComponentInParent<PlayerBallHand>());
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(LayerMask.NameToLayer("Ground") == collision.gameObject.layer)
            {
                OnSettled();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if(hoop != null)
            {
                Gizmos.color = new Color(1,0,0,0.5f);
                Vector3 pos = hoop.position;
                pos.y = 0f;
                Gizmos.DrawSphere(pos, threePointRadius);
            }
        }
    }
}
