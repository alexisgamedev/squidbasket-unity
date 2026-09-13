using NUnit.Framework;
using Squidbasket.Input;
using UnityEngine;

namespace Squidbasket.Tests
{
    public class GamepadLookScalerTests
    {
        [Test]
        public void Scale_MultipliesByLookSpeedAndDeltaTime()
        {
            var result = GamepadLookScaler.Scale(new Vector2(1f, 0f), lookSpeed: 180f, deltaTime: 0.1f);

            Assert.AreEqual(new Vector2(18f, 0f), result);
        }

        [Test]
        public void Scale_PreservesStickDirection()
        {
            var result = GamepadLookScaler.Scale(new Vector2(-0.5f, 0.25f), lookSpeed: 100f, deltaTime: 0.02f);

            Assert.AreEqual(new Vector2(-1f, 0.5f), result);
        }

        [Test]
        public void Scale_ZeroDeltaTime_IsZero()
        {
            // A stuck/zero deltaTime (e.g. paused frame) must never spike look rotation regardless
            // of stick deflection.
            var result = GamepadLookScaler.Scale(new Vector2(1f, 1f), lookSpeed: 180f, deltaTime: 0f);

            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void Scale_ZeroStickInput_IsZero()
        {
            var result = GamepadLookScaler.Scale(Vector2.zero, lookSpeed: 180f, deltaTime: 0.016f);

            Assert.AreEqual(Vector2.zero, result);
        }

        [Test]
        public void Scale_LargeDeltaTime_ClampsInsteadOfSnapping()
        {
            // A frame-time hitch (GC pause, load stall) must not turn a held-over stick deflection
            // into a huge single-frame look delta.
            var hitchResult = GamepadLookScaler.Scale(new Vector2(1f, 0f), lookSpeed: 180f, deltaTime: 2f);
            var clampedResult = GamepadLookScaler.Scale(new Vector2(1f, 0f), lookSpeed: 180f, deltaTime: 1f / 15f);

            Assert.AreEqual(clampedResult, hitchResult);
        }
    }
}
