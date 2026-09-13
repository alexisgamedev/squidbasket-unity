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
        [SerializeField] ParticleSystem score2pointParticles;
        [SerializeField] ParticleSystem score3pointParticles;

        private void OnTriggerEnter(Collider other)
        {
            var ball = other.GetComponentInParent<Ball>();
            if(ball != null)
            {
                switch(ball.NotifyPassedThroughHoop())
                {
                    case 2:
                        score2pointParticles.Clear();
                        score2pointParticles.Play();
                        break;
                    case 3:
                        score3pointParticles.Clear();
                        score3pointParticles.Play();
                        break;
                }
            }
        }
    }
}
