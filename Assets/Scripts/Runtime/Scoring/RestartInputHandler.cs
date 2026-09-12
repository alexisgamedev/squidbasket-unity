using Squidbasket.Player;
using UnityEngine;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Routes the Restart action to <see cref="ScoreSystem.Restart"/> independent of
    /// <see cref="PlayerFsm"/>/<see cref="PlayerStateController"/> — unlike Reset, Restart only
    /// zeroes Score/Streak and has no effect on player/ball state, so it doesn't go through the
    /// player state machine and isn't gated to Walking (CONTEXT.md, ADR-0003).
    /// </summary>
    public sealed class RestartInputHandler : MonoBehaviour
    {
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        private IPlayerInputSource _input;

        private void Awake()
        {
            _input = inputSourceBehaviour as IPlayerInputSource;
        }

        private void Update()
        {
            if (_input != null && _input.RestartPressed)
            {
                scoreSystem?.Restart();
            }
        }
    }
}
