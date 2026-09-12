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
    }
}
