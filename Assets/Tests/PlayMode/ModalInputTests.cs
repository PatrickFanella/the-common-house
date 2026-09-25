using CommonHouse.Interaction;
using CommonHouse.Player;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class ModalInputTests
    {
        [Test]
        public void CompetingPanelsCannotStealPauseOrReleaseAnotherOwner()
        {
            var player = new GameObject("Modal ownership test");
            try
            {
                var movement = player.AddComponent<FirstPersonMovement>();
                var coordinator = player.AddComponent<ModalInputCoordinator>();
                var first = new Panel(coordinator);
                var second = new Panel(coordinator);
                Assert.That(coordinator.TryOpen(first), Is.True);
                Assert.That(coordinator.TryOpen(second), Is.False);
                Assert.That(coordinator.Release(second), Is.False);
                Assert.That(movement.MenuOpen, Is.True);
                coordinator.HandleCancel();
                Assert.That(first.CloseCount, Is.EqualTo(1));
                Assert.That(second.CloseCount, Is.Zero);
                Assert.That(coordinator.HasOwner, Is.False);
                Assert.That(movement.MenuOpen, Is.False);
                Assert.That(coordinator.TryOpen(second), Is.True);
                Assert.That(coordinator.Release(first), Is.False);
                Assert.That(coordinator.IsOwner(second), Is.True);
                coordinator.HandleCancel();
                Assert.That(second.CloseCount, Is.EqualTo(1));
                Assert.That(movement.MenuOpen, Is.False);
                coordinator.HandleCancel();
                Assert.That(movement.MenuOpen, Is.True);
                coordinator.HandleCancel();
                Assert.That(movement.MenuOpen, Is.False);
            }
            finally { Object.DestroyImmediate(player); }
        }

        private sealed class Panel : IModalPanel
        {
            private readonly ModalInputCoordinator coordinator;
            public int CloseCount { get; private set; }
            public Panel(ModalInputCoordinator coordinator) { this.coordinator = coordinator; }
            public void CloseFromModalInput() { CloseCount++; coordinator.Release(this); }
        }
    }
}
