using Squidbasket.Scoring;
using UnityEngine;

namespace Squidbasket.Player
{
    /// <summary>
    /// Draws the Shot's flight arc on a <see cref="LineRenderer"/> while Shooting, from the same
    /// projectile-motion math (<see cref="ShotCalculator.SampleTrajectory"/>) that governs the
    /// real ball once released — so the preview never diverges from where the Shot will actually
    /// go. Owns only rendering; PlayerStateController drives when it's shown/hidden and feeds it
    /// the current origin/velocity each frame Power changes.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        [SerializeField] private float timeStep = 0.05f;
        [SerializeField] private int maxSamples = 60;
        [SerializeField] private float groundY;

        private LineRenderer _lineRenderer;
        private Vector3[] _sampleBuffer;

        private LineRenderer LineRendererComponent =>
            _lineRenderer != null ? _lineRenderer : (_lineRenderer = GetComponent<LineRenderer>());

        private void Awake()
        {
            LineRendererComponent.enabled = false;
            _sampleBuffer = new Vector3[maxSamples];
        }

        public void Show()
        {
            LineRendererComponent.enabled = true;
        }

        public void Hide()
        {
            LineRendererComponent.enabled = false;
        }

        /// <summary>Redraws the arc for the given release point/launch velocity.</summary>
        public void UpdateTrajectory(Vector3 origin, Vector3 velocity)
        {
            int count = ShotCalculator.SampleTrajectory(origin, velocity, Physics.gravity, groundY, timeStep, _sampleBuffer);
            LineRendererComponent.positionCount = count;
            LineRendererComponent.SetPositions(_sampleBuffer);
        }
    }
}
