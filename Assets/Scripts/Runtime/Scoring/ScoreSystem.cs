using UnityEngine;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Dumb subscriber to GameEvents that accumulates Score/Streak; separately owns Restart,
    /// which zeroes Score without an app relaunch (CONTEXT.md), distinct from Ball's Retrieval
    /// and the player's Reset action (ADR-0003).
    /// </summary>
    public sealed class ScoreSystem : MonoBehaviour
    {
        private readonly ScoreState _state = new ScoreState();

        public int Score => _state.Score;
        public int Streak => _state.Streak;

        private void OnEnable()
        {
            GameEvents.ShotMade += HandleShotMade;
            GameEvents.ShotMissed += HandleShotMissed;
        }

        private void OnDisable()
        {
            GameEvents.ShotMade -= HandleShotMade;
            GameEvents.ShotMissed -= HandleShotMissed;
        }

        public void Restart() => _state.Restart();

        private void HandleShotMade(Zone zone, int points) => _state.RegisterMake(points);

        private void HandleShotMissed() => _state.RegisterMiss();
    }
}
