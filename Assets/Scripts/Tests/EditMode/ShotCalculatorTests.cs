using NUnit.Framework;
using Squidbasket.Scoring;
using UnityEngine;

namespace Squidbasket.Tests
{
    public class ShotCalculatorTests
    {
        [Test]
        public void DetermineZone_InsideArcRadius_IsTwoPoint()
        {
            var releasePosition = new Vector3(3f, 0f, 0f);
            var hoopPosition = Vector3.zero;

            var zone = ShotCalculator.DetermineZone(releasePosition, hoopPosition, threePointRadius: 7f);

            Assert.AreEqual(Zone.TwoPoint, zone);
        }

        [Test]
        public void DetermineZone_BeyondArcRadius_IsThreePoint()
        {
            var releasePosition = new Vector3(8f, 0f, 0f);
            var hoopPosition = Vector3.zero;

            var zone = ShotCalculator.DetermineZone(releasePosition, hoopPosition, threePointRadius: 7f);

            Assert.AreEqual(Zone.ThreePoint, zone);
        }

        [Test]
        public void DetermineZone_ExactlyOnArcRadius_IsThreePoint()
        {
            var releasePosition = new Vector3(7f, 0f, 0f);
            var hoopPosition = Vector3.zero;

            var zone = ShotCalculator.DetermineZone(releasePosition, hoopPosition, threePointRadius: 7f);

            Assert.AreEqual(Zone.ThreePoint, zone);
        }

        [Test]
        public void DetermineZone_IgnoresHeightDifference()
        {
            // Zones are a flat court split (CONTEXT.md): a release point directly below/above
            // the hoop is never out of range just because of a height difference.
            var releasePosition = new Vector3(0f, 50f, 0f);
            var hoopPosition = Vector3.zero;

            var zone = ShotCalculator.DetermineZone(releasePosition, hoopPosition, threePointRadius: 7f);

            Assert.AreEqual(Zone.TwoPoint, zone);
        }

        [Test]
        public void PointsForZone_TwoPoint_IsTwo()
        {
            Assert.AreEqual(2, ShotCalculator.PointsForZone(Zone.TwoPoint));
        }

        [Test]
        public void PointsForZone_ThreePoint_IsThree()
        {
            Assert.AreEqual(3, ShotCalculator.PointsForZone(Zone.ThreePoint));
        }

        [Test]
        public void ComputeLaunchVelocity_ZeroPower_IsPureForwardAtBaseSpeed()
        {
            var velocity = ShotCalculator.ComputeLaunchVelocity(
                cameraForward: Vector3.forward,
                power: 0f,
                upArcScale: 1f,
                speed: 5f);

            Assert.AreEqual(new Vector3(0f, 0f, 5f), velocity, "expected pure forward velocity at zero power");
        }

        [Test]
        public void ComputeLaunchVelocity_HigherPower_ArcsHigher()
        {
            var lowPower = ShotCalculator.ComputeLaunchVelocity(Vector3.forward, power: 0.2f, upArcScale: 1f, speed: 5f);
            var highPower = ShotCalculator.ComputeLaunchVelocity(Vector3.forward, power: 0.8f, upArcScale: 1f, speed: 5f);

            Assert.Greater(highPower.y, lowPower.y);
        }

        [Test]
        public void ComputeLaunchVelocity_ScalesWithSpeed()
        {
            var velocity = ShotCalculator.ComputeLaunchVelocity(Vector3.forward, power: 0f, upArcScale: 1f, speed: 10f);

            Assert.AreEqual(10f, velocity.magnitude, 1e-4f);
        }

        [Test]
        public void SampleTrajectory_FirstPointIsOrigin()
        {
            var origin = new Vector3(1f, 2f, 3f);
            var velocity = new Vector3(0f, 5f, 5f);
            var buffer = new Vector3[30];

            ShotCalculator.SampleTrajectory(origin, velocity, Physics.gravity, groundY: -100f, timeStep: 0.05f, buffer);

            Assert.AreEqual(origin, buffer[0]);
        }

        [Test]
        public void SampleTrajectory_FollowsProjectileMotionFormula()
        {
            var origin = Vector3.zero;
            var velocity = new Vector3(2f, 6f, 0f);
            var gravity = new Vector3(0f, -9.81f, 0f);
            const float timeStep = 0.1f;
            var buffer = new Vector3[30];

            ShotCalculator.SampleTrajectory(origin, velocity, gravity, groundY: 0f, timeStep, buffer);

            Vector3 expectedSecondPoint = origin + velocity * timeStep + 0.5f * gravity * (timeStep * timeStep);
            Assert.AreEqual(expectedSecondPoint, buffer[1]);
        }

        [Test]
        public void SampleTrajectory_StopsOnceItReachesGroundHeight()
        {
            var origin = Vector3.zero;
            var velocity = new Vector3(3f, 5f, 0f);
            var buffer = new Vector3[1000];

            int count = ShotCalculator.SampleTrajectory(origin, velocity, Physics.gravity, groundY: 0f, timeStep: 0.05f, buffer);

            Assert.Less(count, 1000, "an upward arc under gravity should land well before the buffer fills");
            Assert.LessOrEqual(buffer[count - 1].y, 0f);
        }

        [Test]
        public void SampleTrajectory_NeverExceedsBufferLength()
        {
            var origin = Vector3.zero;
            var velocity = new Vector3(5f, 0f, 0f);
            var buffer = new Vector3[12];

            // Zero gravity + no vertical velocity never reaches ground level, so the buffer size
            // is the only thing that can end the loop.
            int count = ShotCalculator.SampleTrajectory(origin, velocity, Vector3.zero, groundY: 0f, timeStep: 0.1f, buffer);

            Assert.AreEqual(12, count);
        }

        [Test]
        public void SampleTrajectory_KeepsTracingDownToGroundWhenReleasedAboveIt()
        {
            // Regression: a Shot released from hand height with little/no upward velocity (e.g.
            // Power at its minimum) must keep tracing down to the actual ground, not stop the
            // instant it dips below release height — which used to collapse the preview to a
            // near-invisible stub for exactly this (very common) case.
            var origin = new Vector3(0f, 1.2f, 0f);
            var velocity = new Vector3(3f, 0f, 0f);
            var buffer = new Vector3[200];

            int count = ShotCalculator.SampleTrajectory(origin, velocity, Physics.gravity, groundY: 0f, timeStep: 0.02f, buffer);

            Assert.Greater(count, 5, "should keep sampling down to true ground level, not stop one step below release height");
            Assert.LessOrEqual(buffer[count - 1].y, 0f);
        }
    }
}
