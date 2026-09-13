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
        private bool _previewEnabled = true;

        // Tracks PlayerStateController's Show()/Hide() intent separately from _previewEnabled so
        // toggling the menu Toggle back on mid-Shooting (Show() already having been called for
        // this shot) re-displays the line immediately rather than waiting for the next Shooting
        // entry.
        private bool _showRequested;

        private LineRenderer LineRendererComponent =>
            _lineRenderer != null ? _lineRenderer : (_lineRenderer = GetComponent<LineRenderer>());

        private void Awake()
        {
            LineRendererComponent.enabled = false;
            _sampleBuffer = new Vector3[maxSamples];
        }

        // Gates Show()/Hide() rather than replacing them: PlayerStateController still owns *when*
        // the preview would be shown during Shooting (CONTEXT.md); this only owns *whether* the
        // player has opted into seeing it at all (menu Toggle).
        public void SetPreviewEnabled(bool enabled)
        {
            _previewEnabled = enabled;
            LineRendererComponent.enabled = _previewEnabled && _showRequested;
        }

        public void Show()
        {
            _showRequested = true;
            LineRendererComponent.enabled = _previewEnabled;
        }

        public void Hide()
        {
            _showRequested = false;
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
