using System;

namespace Squidbasket.Scoring
{
    /// <summary>
    /// Static event bus routing Ball/HoopTrigger outcomes to subscribers like ScoreSystem, which
    /// stays a "dumb subscriber" decoupled from how a make or miss was actually detected (ADR-0003).
    /// </summary>
    public static class GameEvents
    {
        public static event Action<Zone, int> ShotMade;
        public static event Action ShotMissed;

        public static void RaiseShotMade(Zone zone, int points) => ShotMade?.Invoke(zone, points);
        public static void RaiseShotMissed() => ShotMissed?.Invoke();
    }
}
