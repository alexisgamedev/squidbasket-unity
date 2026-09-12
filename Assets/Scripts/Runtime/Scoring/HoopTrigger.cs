using Squidbasket.Gameplay;
using UnityEngine;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Detects a ball passing through the hoop and notifies it; the Ball itself carries the
    /// Zone/points recorded at release and raises the make event (ADR-0003).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class HoopTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var ball = other.GetComponentInParent<Ball>();
            ball?.NotifyPassedThroughHoop();
        }
    }
}
