namespace Squidbasket.Scoring
{
    /// <summary>
    /// Pure accumulation logic for Score and Streak (CONTEXT.md). A Miss carries no scoring
    /// penalty but resets Streak; Restart zeroes both without requiring an app relaunch.
    /// </summary>
    public sealed class ScoreState
    {
        public int Score { get; private set; }
        public int Streak { get; private set; }

        public void RegisterMake(int points)
        {
            Score += points;
            Streak += 1;
        }

        public void RegisterMiss()
        {
            Streak = 0;
        }

        public void Restart()
        {
            Score = 0;
            Streak = 0;
        }
    }
}
