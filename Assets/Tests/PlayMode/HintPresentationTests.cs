using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Hints;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class HintPresentationTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private InvestigationHintController hints;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Hint Test Player");
            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = Vector3.up * .9f;
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            var content = CaseContent.LoadBaseline();
            session = player.AddComponent<CaseSession>();
            session.Configure(content, new CaseState(content));
            hints = player.AddComponent<InvestigationHintController>();
            hints.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(player);

        [Test]
        public void ExplicitRequestsEscalateAndStateChangeReturnsToLeadWithoutChangingCaseState()
        {
            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Lead));
            Assert.That(modalInput.IsOwner(hints), Is.True);
            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Comparison));
            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Explanation));

            session.State.AcquireSource("test:T01", "T01");
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Lead));
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(new[] { "T01" }));
            Assert.That(session.State.IdentityConfirmed, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
        }

        [Test]
        public void CancelClosesOptionalPanelAndRestoresModalOwnership()
        {
            Assert.That(hints.RequestHint(), Is.True);
            modalInput.HandleCancel();
            Assert.That(hints.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
