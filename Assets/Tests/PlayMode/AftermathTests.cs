using System.Collections;
using System.Linq;
using CommonHouse.Aftermath;
using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Player;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class AftermathTests
    {
        private GameObject player;
        private CaseSession session;
        private ModalInputCoordinator modalInput;
        private AftermathHouseholdController aftermath;
        private AftermathWorldTarget target;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Aftermath Test Player");
            player.AddComponent<CharacterController>();
            var movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            var content = CaseContent.LoadBaseline();
            session.Configure(content, new CaseState(content));
            aftermath = player.AddComponent<AftermathHouseholdController>();
            aftermath.Configure(session, modalInput);
            target = new GameObject("Aftermath Check-in Target").AddComponent<AftermathWorldTarget>();
            target.Configure(aftermath);
        }

        [TearDown]
        public void TearDown()
        {
            if (target != null) Object.DestroyImmediate(target.gameObject);
            if (player != null) Object.DestroyImmediate(player);
        }

        [Test]
        public void UnresolvedCaseHasNoAftermathToOpenOrAcknowledge()
        {
            Assert.That(aftermath.CanOpen, Is.False);
            Assert.That(target.Prompt, Is.Empty);
            Assert.That(target.Interact(), Is.False);
            Assert.That(aftermath.IsOpen, Is.False);
            Assert.That(aftermath.Acknowledge(), Is.False);
            Assert.That(session.State.Phase, Is.Not.EqualTo(CasePhase.AftermathCompleted));
        }

        [TestCase("R1")]
        [TestCase("R2")]
        [TestCase("R3")]
        public void MatchingHouseholdCheckInCompletesOnceAndRemainsViewable(string outcomeId)
        {
            Resolve(session.State, outcomeId);
            var disclosures = session.State.Disclosures.ToArray();
            Assert.That(session.State.Phase, Is.EqualTo(CasePhase.Resolved));
            Assert.That(aftermath.ActiveEntry.OutcomeId, Is.EqualTo(outcomeId));
            Assert.That(target.Interact(), Is.True);
            Assert.That(aftermath.IsOpen, Is.True);
            Assert.That(modalInput.IsOwner(aftermath), Is.True);

            AssertOutcomeCopy(outcomeId, aftermath.ActiveEntry.Text);
            aftermath.Tick(new AftermathInputFrame(true));
            Assert.That(session.State.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            Assert.That(session.State.Disclosures, Is.EqualTo(disclosures), "Aftermath cannot add or erase a sent disclosure.");
            Assert.That(aftermath.Acknowledge(), Is.False, "The fixed aftermath event is idempotent on repeat input.");

            aftermath.Close();
            Assert.That(target.Interact(), Is.True, "A completed household consequence remains readable on re-entry.");
            Assert.That(aftermath.IsAcknowledged, Is.True);
            Assert.That(session.State.Disclosures, Is.EqualTo(disclosures));
        }

        [UnityTest]
        public IEnumerator GeneratedKitchenCheckInUsesSharedFocusAfterResolution()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var inspection = Object.FindAnyObjectByType<InspectionController>();
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var sceneAftermath = Object.FindAnyObjectByType<AftermathHouseholdController>();
            var sceneTarget = Object.FindAnyObjectByType<AftermathWorldTarget>();
            Assert.That(opening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(sceneAftermath, Is.Not.Null);
            Assert.That(sceneTarget, Is.Not.Null);
            opening.Close();
            Resolve(sceneSession.State, "R2");

            Focus(inspection, sceneTarget.transform.position);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(sceneTarget));
            Assert.That(inspection.FocusedPrompt, Is.EqualTo("[E] Check in with the household"));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(sceneAftermath.ActiveEntry.OutcomeId, Is.EqualTo("R2"));
            sceneAftermath.Tick(new AftermathInputFrame(true));
            Assert.That(sceneSession.State.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
        }

        private static void Resolve(CaseState state, string outcomeId)
        {
            state.AcquireSource("test:call", "T01");
            state.AcquireSource("test:private-account", "C01");
            if (outcomeId == "R1" || outcomeId == "R2") state.RecordRefusalAuthorization("test:refusal");
            if (outcomeId == "R1")
            {
                state.AcquireSource("test:ada-repair", "T03");
                state.AcquireSource("test:ren-repair", "C02");
                state.InspectPrivateRoute("test:route");
                state.RecordPrivateContactAgreement("test:agreement");
                state.PrepareDraft(ResponseRoute.PrivateHeadset);
            }
            else if (outcomeId == "R2") state.PrepareDraft(ResponseRoute.RefusalRelay);
            else if (outcomeId == "R3") state.PrepareDraft(ResponseRoute.RoomSpeaker);
            else throw new System.ArgumentOutOfRangeException(nameof(outcomeId));

            var summary = state.PresentSendSummary();
            Assert.That(state.ConfirmSend("test:send:" + outcomeId, summary, true, outcomeId == "R3"), Is.True);
        }

        private static void AssertOutcomeCopy(string outcomeId, string text)
        {
            switch (outcomeId)
            {
                case "R1":
                    Assert.That(text, Does.Contain("routine repair"));
                    Assert.That(text, Does.Contain("Ren's to share"));
                    return;
                case "R2":
                    Assert.That(text, Does.Contain("complete"));
                    Assert.That(text, Does.Contain("ordinary household life"));
                    return;
                case "R3":
                    Assert.That(text, Does.Contain("distance"));
                    Assert.That(text, Does.Contain("does not erase"));
                    return;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(outcomeId));
            }
        }

        private static void Focus(InspectionController inspection, Vector3 targetPosition)
        {
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            capsule.enabled = false;
            inspection.transform.position = targetPosition + new Vector3(0f, -1.4f, -1.9f);
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
