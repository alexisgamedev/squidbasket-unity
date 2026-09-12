using NUnit.Framework;
using Squidbasket.Player;

namespace Squidbasket.Tests
{
    public class PlayerFsmTests
    {
        [Test]
        public void InitialState_IsWalking()
        {
            var fsm = new PlayerFsm();

            Assert.AreEqual(PlayerLifecycleState.Walking, fsm.Current);
        }

        [Test]
        public void TryEnterShooting_FromWalkingWithShootHeld_Transitions()
        {
            var fsm = new PlayerFsm();

            bool transitioned = fsm.TryEnterShooting(shootHeld: true);

            Assert.IsTrue(transitioned);
            Assert.AreEqual(PlayerLifecycleState.Shooting, fsm.Current);
        }

        [Test]
        public void TryEnterShooting_WithoutShootHeld_DoesNotTransition()
        {
            var fsm = new PlayerFsm();

            bool transitioned = fsm.TryEnterShooting(shootHeld: false);

            Assert.IsFalse(transitioned);
            Assert.AreEqual(PlayerLifecycleState.Walking, fsm.Current);
        }

        [Test]
        public void TryEnterShooting_WhileAlreadyShooting_DoesNotReenter()
        {
            var fsm = new PlayerFsm();
            fsm.TryEnterShooting(shootHeld: true);

            bool transitioned = fsm.TryEnterShooting(shootHeld: true);

            Assert.IsFalse(transitioned);
        }

        [Test]
        public void TryExitShooting_OnRelease_ReturnsToWalking()
        {
            var fsm = new PlayerFsm();
            fsm.TryEnterShooting(shootHeld: true);

            bool transitioned = fsm.TryExitShooting(shootReleased: true, timedOut: false);

            Assert.IsTrue(transitioned);
            Assert.AreEqual(PlayerLifecycleState.Walking, fsm.Current);
        }

        [Test]
        public void TryExitShooting_OnTimeout_ReturnsToWalking()
        {
            var fsm = new PlayerFsm();
            fsm.TryEnterShooting(shootHeld: true);

            bool transitioned = fsm.TryExitShooting(shootReleased: false, timedOut: true);

            Assert.IsTrue(transitioned);
            Assert.AreEqual(PlayerLifecycleState.Walking, fsm.Current);
        }

        [Test]
        public void TryExitShooting_WhileWalking_DoesNothing()
        {
            var fsm = new PlayerFsm();

            bool transitioned = fsm.TryExitShooting(shootReleased: true, timedOut: true);

            Assert.IsFalse(transitioned);
        }

        [Test]
        public void CanReset_IsTrueOnlyWhileWalking()
        {
            var fsm = new PlayerFsm();
            Assert.IsTrue(fsm.CanReset());

            fsm.TryEnterShooting(shootHeld: true);

            Assert.IsFalse(fsm.CanReset(), "Reset cannot interrupt Shooting (ADR-0002/ADR-0003)");
        }
    }
}
