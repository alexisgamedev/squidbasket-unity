using NUnit.Framework;
using Squidbasket.Player;

namespace Squidbasket.Tests
{
    public class PowerBarOscillatorTests
    {
        [Test]
        public void CurrentValue_StartsAtMin()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            Assert.AreEqual(0f, bar.CurrentValue);
        }

        [Test]
        public void CurrentValue_ReachesMaxAfterHalfCycle()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(10f);

            Assert.AreEqual(10f, bar.CurrentValue, 1e-4f);
        }

        [Test]
        public void CurrentValue_PingPongsBackToMinAfterFullCycle()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(20f);

            Assert.AreEqual(0f, bar.CurrentValue, 1e-4f);
        }

        [Test]
        public void CurrentValue_MidwayThroughReturnLeg()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(15f);

            Assert.AreEqual(5f, bar.CurrentValue, 1e-4f);
        }

        [Test]
        public void LoopsCompleted_IsHalfAfterHalfCycle()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(10f);

            Assert.AreEqual(0.5f, bar.LoopsCompleted, 1e-4f);
        }

        [Test]
        public void LoopsCompleted_IsOneAfterAFullCycle()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(20f);

            Assert.AreEqual(1f, bar.LoopsCompleted, 1e-4f);
        }

        [Test]
        public void HasTimedOut_IsFalseBeforeTwoAndAHalfLoops()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(20f + 9f); // 1 loop + 0.9 * 20 => 1.45 loops

            Assert.IsFalse(bar.HasTimedOut);
        }

        [Test]
        public void HasTimedOut_IsTrueAtExactlyTwoAndAHalfLoops()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);

            bar.Advance(50f); // cycle length 20 => 2.5 loops

            Assert.IsTrue(bar.HasTimedOut);
        }

        [Test]
        public void Reset_ReturnsToMinAndZeroLoops()
        {
            var bar = new PowerBarOscillator(min: 0f, max: 10f, speed: 1f);
            bar.Advance(15f);

            bar.Reset();

            Assert.AreEqual(0f, bar.CurrentValue);
            Assert.AreEqual(0f, bar.LoopsCompleted);
            Assert.IsFalse(bar.HasTimedOut);
        }

        [Test]
        public void Advance_IsUnaffectedByCallerSuppliedTimeScale()
        {
            // The Power Bar is exempt from Bullet Time: callers are expected to feed it
            // unscaled delta time, so the oscillator itself applies no additional scaling.
            var bar = new PowerBarOscillator(min: 0f, max: 4f, speed: 2f);

            bar.Advance(1f);

            Assert.AreEqual(2f, bar.CurrentValue, 1e-4f);
        }
    }
}
