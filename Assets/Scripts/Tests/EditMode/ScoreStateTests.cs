using NUnit.Framework;
using Squidbasket.Scoring;

namespace Squidbasket.Tests
{
    public class ScoreStateTests
    {
        [Test]
        public void InitialState_IsZero()
        {
            var state = new ScoreState();

            Assert.AreEqual(0, state.Score);
            Assert.AreEqual(0, state.Streak);
        }

        [Test]
        public void RegisterMake_AddsPointsAndIncrementsStreak()
        {
            var state = new ScoreState();

            state.RegisterMake(3);
            state.RegisterMake(2);

            Assert.AreEqual(5, state.Score);
            Assert.AreEqual(2, state.Streak);
        }

        [Test]
        public void RegisterMiss_ResetsStreakButNotScore()
        {
            var state = new ScoreState();
            state.RegisterMake(2);
            state.RegisterMake(2);

            state.RegisterMiss();

            Assert.AreEqual(4, state.Score, "a Miss carries no scoring penalty (CONTEXT.md)");
            Assert.AreEqual(0, state.Streak);
        }

        [Test]
        public void Restart_ZeroesScoreAndStreak()
        {
            var state = new ScoreState();
            state.RegisterMake(3);

            state.Restart();

            Assert.AreEqual(0, state.Score);
            Assert.AreEqual(0, state.Streak);
        }
    }
}
