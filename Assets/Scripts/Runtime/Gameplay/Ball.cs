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
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Ball : MonoBehaviour
    {
        [SerializeField] private Transform hoop;
        [SerializeField] private float threePointRadius = 6.75f;
        [SerializeField] private float restSpeedThreshold = 0.05f;
        [SerializeField] private float restCheckDelay = 0.25f;

        private Rigidbody _rigidbody;
        private Transform _heldAnchor;
        private bool _isHeldByPlayer = true;
        private bool _isShotPending;
        private bool _hasScoredThisShot;
        private Zone _releaseZone;
        private int _releasePoints;
        private float _restTimer;

        public bool IsHeldByPlayer => _isHeldByPlayer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void LateUpdate()
        {
            if (_isHeldByPlayer && _heldAnchor != null)
            {
                transform.SetPositionAndRotation(_heldAnchor.position, _heldAnchor.rotation);
            }
        }

        private void FixedUpdate()
        {
            if (_isHeldByPlayer)
            {
                return;
            }

            if (_rigidbody.linearVelocity.magnitude <= restSpeedThreshold)
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
            _isHeldByPlayer = true;
            _isShotPending = false;
            _hasScoredThisShot = false;
            _restTimer = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }

        /// <summary>Releases the ball as a Shot with the given launch velocity (LMB released).</summary>
        public void Shoot(Vector3 velocity)
        {
            RecordReleaseZone();
            ReleasePhysics(velocity);
            _isShotPending = true;
        }

        /// <summary>Releases the ball with no launch velocity after a Shot Timeout — the ball is lost.</summary>
        public void Drop()
        {
            ReleasePhysics(Vector3.zero);
            _isShotPending = false;
        }

        /// <summary>Called by HoopTrigger when this ball passes through the hoop.</summary>
        public void NotifyPassedThroughHoop()
        {
            if (!_isShotPending || _hasScoredThisShot)
            {
                return;
            }

            _hasScoredThisShot = true;
            GameEvents.RaiseShotMade(_releaseZone, _releasePoints);
        }

        private void RecordReleaseZone()
        {
            Vector3 hoopPosition = hoop != null ? hoop.position : Vector3.zero;
            _releaseZone = ShotCalculator.DetermineZone(transform.position, hoopPosition, threePointRadius);
            _releasePoints = ShotCalculator.PointsForZone(_releaseZone);
        }

        private void ReleasePhysics(Vector3 velocity)
        {
            _isHeldByPlayer = false;
            _heldAnchor = null;
            _restTimer = 0f;
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = velocity;
        }

        private void OnSettled()
        {
            if (_isShotPending && !_hasScoredThisShot)
            {
                GameEvents.RaiseShotMissed();
            }

            _isShotPending = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isHeldByPlayer)
            {
                return;
            }

            var hand = other.GetComponentInParent<PlayerBallHand>();
            if (hand != null)
            {
                AttachTo(hand.HandAnchor);
            }
        }
    }
}
