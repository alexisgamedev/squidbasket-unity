using Squidbasket.Scoring;
using Squidbasket.Player;
using UnityEngine;
using UnityEngine.UI;
using Squidbasket.Gameplay;

namespace Squidbasket.UI
{
    /// <summary>
    /// Pure display for Score/Streak: reads <see cref="ScoreSystem"/>'s existing public values
    /// every frame rather than tracking its own copy of that state (ADR-0003). Screen-space
    /// overlay, so it stays visible across both Walking and Shooting camera states.
    /// </summary>
    public sealed class ScoreHud : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private PlayerStateController playerStateController;
        [SerializeField] private Ball ball;

        [SerializeField] private Text scoreText;
        [SerializeField] private Text streakText;
        [SerializeField] private Text playerStateText;
        [SerializeField] private Text ballStateText;

        private int _lastScore = -1;
        private int _lastStreak = -1;

        private void Update()
        {
            if (scoreSystem == null)
            {
                return;
            }

            if (scoreSystem.Score != _lastScore)
            {
                _lastScore = scoreSystem.Score;
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {_lastScore}";
                }
            }

            if (scoreSystem.Streak != _lastStreak)
            {
                _lastStreak = scoreSystem.Streak;
                if (streakText != null)
                {
                    streakText.text = $"Streak: {_lastStreak}";
                }
            }

            if (playerStateController != null)
            {
                playerStateText.text = $"Player State: {(playerStateController.IsShooting ? "Shooting" : "Walking")}";
            }

            if (ball != null)
            {
                ballStateText.text = $"Ball Held: {ball.IsHeldByPlayer}";
            }
        }
    }
}
