using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class TransmissionLogTests
    {
        private GameObject player;
        private FirstPersonMovement movement;
        private ModalInputCoordinator modalInput;
        private CaseSession session;
        private TransmissionLog log;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Transmission Log Test Player");
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = Vector3.up * 0.9f;
            movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            session.Configure(CaseContent.LoadBaseline(), new CaseState(CaseContent.LoadBaseline()));
            log = player.AddComponent<TransmissionLog>();
            log.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(player);
        }

        [Test]
        public void ExplicitReceiveMakesE01ReviewableWithoutTreatingTheCallerAssertionAsConfirmedIdentity()
        {
            Assert.That(log.Open(), Is.True);
            Assert.That(log.Records, Is.Empty, "Opening a log must not invent a received transmission.");
            log.Close();

            Assert.That(log.Receive("T01"), Is.EqualTo(TransmissionReceiveResult.Received));
            Assert.That(session.State.HasEvidence("E01"), Is.True);
            Assert.That(session.State.IdentityConfirmed, Is.False);
            Assert.That(log.Receive("T01"), Is.EqualTo(TransmissionReceiveResult.AlreadyReceived));
            Assert.That(log.Records, Has.Count.EqualTo(1));
            var record = log.Records[0];
            Assert.That(record.SourceId, Is.EqualTo("T01"));
            Assert.That(record.SpeakerId, Is.EqualTo("ada"));
            Assert.That(record.SpeakerLabel, Does.Contain("Ada"));
            Assert.That(record.Transcript, Does.Contain("road is clear"));
            Assert.That(record.AssertedContactIdentity, Is.EqualTo("Mara"));
            Assert.That(record.IsAssertedContactIdentityConfirmed, Is.False);

            session.State.AcquireSource("private-conversation", "C01");
            log.RefreshRecords();
            Assert.That(log.Records[0].AssertedContactIdentity, Is.EqualTo("Mara"));
            Assert.That(log.Records[0].IsAssertedContactIdentityConfirmed, Is.True, "E04, not Ada's caller assertion, confirms Ren/Mara identity.");
        }

        [Test]
        public void SelectingAndReplayingARecordUsesOneModalOwnerAndNeverRepeatsEffects()
        {
            log.Receive("T01");
            log.Receive("T02");
            var disclosures = session.State.Disclosures.Count;
            Assert.That(log.Open(), Is.True);
            Assert.That(modalInput.IsOwner(log), Is.True);
            Assert.That(movement.MenuOpen, Is.True);
            Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T01"));

            log.Tick(new TransmissionLogInputFrame(false, false, true, false));
            Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T02"));
            log.Tick(new TransmissionLogInputFrame(false, false, false, true));
            Assert.That(log.IsShowingTranscript, Is.True);
            Assert.That(log.SelectedRecord.SoundCaption, Does.Contain("Kitchen bell"));
            Assert.That(session.State.Disclosures, Has.Count.EqualTo(disclosures));
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01", "T02" }));

            modalInput.HandleCancel();
            Assert.That(log.IsShowingTranscript, Is.False);
            Assert.That(modalInput.IsOwner(log), Is.True);
            modalInput.HandleCancel();
            Assert.That(log.IsOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(movement.MenuOpen, Is.False);

            Assert.That(log.Open(), Is.True);
            Assert.That(log.OpenSelectedTranscript(), Is.True);
            log.Close();
            Assert.That(session.State.Disclosures, Has.Count.EqualTo(disclosures));
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01", "T02" }));
        }

        [Test]
        public void NonTransmissionReceiveIsRejectedAndAnotherModalPanelCannotBeDisplaced()
        {
            Assert.That(log.Receive("C01"), Is.EqualTo(TransmissionReceiveResult.Rejected));
            Assert.That(session.State.AcquiredSourceIds, Is.Empty);
            var other = new TestModalPanel();
            Assert.That(modalInput.TryOpen(other), Is.True);
            Assert.That(log.Open(), Is.False);
            Assert.That(modalInput.IsOwner(other), Is.True);
            Assert.That(log.IsOpen, Is.False);
            modalInput.Release(other);
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }

        private sealed class TestModalPanel : IModalPanel
        {
            public void CloseFromModalInput() { }
        }
    }
}
