using CommonHouse.Content;
using System.Collections;
using System.Linq;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class SendConfirmationTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private SendConfirmationController send;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Send Confirmation Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            var content = CaseContent.LoadBaseline();
            session.Configure(content, new CaseState(content));
            send = player.AddComponent<SendConfirmationController>();
            send.Configure(session, modalInput);
        }

        [TearDown]
        public void TearDown()
        {
            if (player != null) Object.DestroyImmediate(player);
        }

        [Test]
        public void UnresolvedIdentityDisplaysTheDomainBlockAndCannotSend()
        {
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.PrivateHeadset), Is.True);
            Assert.That(send.Summary.CanSend, Is.False);
            Assert.That(send.Summary.BlockingReason, Does.Contain("Recipient unresolved"));
            Assert.That(send.Confirm(), Is.False);
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(session.State.Disclosures, Is.Empty);
        }

        [Test]
        public void PublicSummaryShowsExactMaterialRecipientsAndNeedsSeparateAcknowledgement()
        {
            MakeIdentityKnown();
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.RoomSpeaker), Is.True);
            var summary = send.Summary;

            Assert.That(summary.OutcomeId, Is.EqualTo("R3"));
            Assert.That(summary.SourceId, Is.EqualTo("T01"));
            Assert.That(summary.MaterialId, Is.EqualTo("public-request"));
            Assert.That(summary.Text, Is.EqualTo("Ada requests Mara: the road is clear."));
            Assert.That(summary.Recipients, Is.EqualTo(new[] { "ren", "ivo", "player" }));
            Assert.That(summary.Warning, Is.EqualTo("This room-speaker disclosure is not authorized."));
            Assert.That(send.Confirm(), Is.False);
            Assert.That(session.State.OutcomeId, Is.Null);

            send.Tick(new SendConfirmationInputFrame(false, false, true, true, false, false));
            Assert.That(send.PublicDisclosureAcknowledged, Is.True);
            Assert.That(session.State.OutcomeId, Is.Null, "Acknowledging and confirming in one frame must not send.");
            Assert.That(send.Confirm(), Is.True);
            Assert.That(send.Confirm(), Is.False);
            Assert.That(session.State.OutcomeId, Is.EqualTo("R3"));
            var outgoing = session.State.Disclosures.Where(record => record.EventId == "radio-send:R3").ToArray();
            Assert.That(outgoing, Has.Length.EqualTo(1));
            Assert.That(outgoing[0].SourceId, Is.EqualTo("T01"));
            Assert.That(outgoing[0].MaterialId, Is.EqualTo("public-request"));
            Assert.That(outgoing[0].Text, Is.EqualTo(summary.Text));
            Assert.That(outgoing[0].Recipients, Is.EqualTo(new[] { "ivo", "player", "ren" }));
        }

        [Test]
        public void CancelAndDeferPreserveAnEligibleRefusalDraftAndAllLeads()
        {
            MakeIdentityKnown();
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.RefusalRelay), Is.True);
            Assert.That(send.CancelReview(), Is.True);
            Assert.That(session.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01", "C01" }));

            Assert.That(send.Prepare(ResponseRoute.RefusalRelay), Is.True);
            Assert.That(send.Defer(), Is.True);
            Assert.That(session.State.IsDeferred, Is.True);
            Assert.That(session.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(send.ResumeDraft(), Is.True);
            Assert.That(session.State.IsDeferred, Is.False);
            Assert.That(send.Summary.OutcomeId, Is.EqualTo("R2"));
        }

        [Test]
        public void RefusalConfirmationUsesOnlyTheAuthorizedRefusalMaterialOnce()
        {
            MakeIdentityKnown();
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.RefusalRelay), Is.True);
            Assert.That(send.Summary.SourceId, Is.EqualTo("C01"));
            Assert.That(send.Summary.MaterialId, Is.EqualTo("authorized-refusal"));
            Assert.That(send.Summary.Text, Is.EqualTo("Ren declines immediate contact."));
            Assert.That(send.Summary.Recipients, Is.EqualTo(new[] { "ada" }));
            Assert.That(send.Confirm(), Is.True);
            Assert.That(session.State.OutcomeId, Is.EqualTo("R2"));
            var outgoing = session.State.Disclosures.Where(record => record.EventId == "radio-send:R2").ToArray();
            Assert.That(outgoing, Has.Length.EqualTo(1));
            Assert.That(outgoing[0].Text, Is.EqualTo("Ren declines immediate contact."));
        }

        [Test]
        public void DeferringBeforeChoosingARouteResumesToTheFreshRouteList()
        {
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Defer(), Is.True);
            Assert.That(session.State.IsDeferred, Is.True);
            Assert.That(session.State.DraftRoute, Is.Null);

            Assert.That(send.ResumeDraft(), Is.True);
            Assert.That(session.State.IsDeferred, Is.False);
            Assert.That(send.IsReviewing, Is.False);
            Assert.That(send.LastActionMessage, Does.Contain("No response draft"));
        }

        [Test]
        public void WorldTargetUsesTheSameModalControllerWithoutGlobalOpening()
        {
            var radioConsole = new GameObject("Radio Console");
            try
            {
                var target = radioConsole.AddComponent<SendConfirmationWorldTarget>();
                target.Configure(send);
                Assert.That(target.Prompt, Is.EqualTo("Prepare a radio response"));
                Assert.That(target.Interact(), Is.True);
                Assert.That(send.IsOpen, Is.True);
                Assert.That(modalInput.IsOwner(send), Is.True);
                Assert.That(session.State.OutcomeId, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(radioConsole);
            }
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostReachesTheRadioConsoleThroughTheSharedFocusPath()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;

            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var inspection = Object.FindAnyObjectByType<InspectionController>();
            var sceneSend = Object.FindAnyObjectByType<SendConfirmationController>();
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var target = Object.FindAnyObjectByType<SendConfirmationWorldTarget>();
            Assert.That(opening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(sceneSend, Is.Not.Null);
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(target, Is.Not.Null);

            opening.Close();
            Focus(inspection, target.transform.position);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.FocusedPrompt, Is.EqualTo("[E] Prepare a radio response"));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(sceneSend.IsOpen, Is.True);
            Assert.That(sceneSession.State.OutcomeId, Is.Null);
        }

        private void MakeIdentityKnown()
        {
            session.State.AcquireSource("opening-call", "T01");
            session.State.AcquireSource("private-account", "C01");
            session.State.RecordRefusalAuthorization("initial-refusal");
        }

        private static void Focus(InspectionController inspection, Vector3 targetPosition)
        {
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            Assert.That(capsule, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            capsule.enabled = false;
            inspection.transform.position = targetPosition + new Vector3(0f, -1.4f, -0.9f);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.LookAt(targetPosition);
            capsule.enabled = true;
            Physics.SyncTransforms();
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
