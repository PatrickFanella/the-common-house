using System.Collections;
using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Opening;
using CommonHouse.Player;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class OpeningSceneTests
    {
        private GameObject player;
        private FirstPersonMovement movement;
        private ModalInputCoordinator modalInput;
        private CaseSession session;
        private TransmissionLog log;
        private OpeningHouseholdController opening;
        private OpeningWorldTarget careTarget;
        private OpeningWorldTarget callTarget;

        [SetUp]
        public void SetUp()
        {
            player = new GameObject("Opening Scene Test Player");
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = Vector3.up * 0.9f;
            movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modalInput = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            var content = CaseContent.LoadBaseline();
            session.Configure(content, new CaseState(content));
            log = player.AddComponent<TransmissionLog>();
            log.Configure(session, modalInput);
            opening = player.AddComponent<OpeningHouseholdController>();
            opening.Configure(session, log, modalInput);

            careTarget = new GameObject("Opening Care Target").AddComponent<OpeningWorldTarget>();
            careTarget.Configure(opening, OpeningWorldAction.Care);
            callTarget = new GameObject("Opening Call Target").AddComponent<OpeningWorldTarget>();
            callTarget.Configure(opening, OpeningWorldAction.ReceiveFirstCall);
        }

        [TearDown]
        public void TearDown()
        {
            if (careTarget != null) Object.DestroyImmediate(careTarget.gameObject);
            if (callTarget != null) Object.DestroyImmediate(callTarget.gameObject);
            if (player != null) Object.DestroyImmediate(player);
        }

        [Test]
        public void CareIsOfferedBeforeTheWaitingCallAndDismissalDoesNotGateReception()
        {
            Assert.That(opening.IsCareOffered, Is.True);
            Assert.That(opening.IsFirstCallAvailable, Is.True);
            Assert.That(callTarget.Prompt, Does.Contain("waiting call"));
            Assert.That(session.State.AcquiredSourceIds, Is.Empty, "Care is reciprocal context, not evidence or a reward.");
            Assert.That(session.State.Disclosures, Is.Empty);

            Assert.That(careTarget.Interact(), Is.True);
            Assert.That(opening.IsCarePanelOpen, Is.True);
            Assert.That(modalInput.IsOwner(opening), Is.True);
            Assert.That(movement.MenuOpen, Is.True);
            modalInput.HandleCancel();

            Assert.That(opening.IsCarePanelOpen, Is.False);
            Assert.That(modalInput.HasOwner, Is.False);
            Assert.That(opening.IsFirstCallAvailable, Is.True, "Leaving or interrupting the welcome must not consume the call.");
            Assert.That(callTarget.Interact(), Is.True);
            Assert.That(session.State.HasEvidence("E01"), Is.True);
            Assert.That(log.IsShowingTranscript, Is.True, "Receiving through the desk should present Ada's call immediately.");
        }

        [Test]
        public void RevisitingCareAndReceivingAgainCannotDuplicateTheFirstCall()
        {
            Assert.That(careTarget.Interact(), Is.True);
            modalInput.HandleCancel();
            Assert.That(careTarget.Interact(), Is.True, "Care can be revisited without becoming a task reward.");
            modalInput.HandleCancel();

            Assert.That(callTarget.Interact(), Is.True);
            var disclosures = session.State.Disclosures.Count;
            var sources = session.State.AcquiredSourceIds;
            Assert.That(callTarget.Interact(), Is.True, "The target remains usable but the received call is idempotent.");
            Assert.That(session.State.Disclosures, Has.Count.EqualTo(disclosures));
            Assert.That(session.State.AcquiredSourceIds, Is.EqualTo(sources));
            Assert.That(log.Records, Has.Count.EqualTo(1));
        }

        [Test]
        public void FirstCallDoesNotAcquireBehindAnotherModalPanel()
        {
            var other = new TestModalPanel();
            Assert.That(modalInput.TryOpen(other), Is.True);
            Assert.That(callTarget.Interact(), Is.False);
            Assert.That(session.State.AcquiredSourceIds, Is.Empty);
            Assert.That(log.Records, Is.Empty);
            modalInput.Release(other);
        }

        [Test]
        public void WelcomeCopyExplainsReplayAndEndpointVisibilityWithoutADeadlineOrPlayerBackstory()
        {
            Assert.That(OpeningContent.CareLines, Has.Count.EqualTo(3));
            Assert.That(OpeningContent.CareLines[1].Text, Does.Contain("replay"));
            Assert.That(OpeningContent.CareLines[2].Text, Does.Contain("private"));
            Assert.That(OpeningContent.CareLines[2].Text, Does.Contain("room speaker"));
            Assert.That(OpeningContent.CallAvailableText, Does.Contain("no countdown"));
            Assert.That(OpeningContent.CareLines[0].Text, Does.Not.Contain("past"));
        }

        [UnityTest]
        public IEnumerator GeneratedOutpostPlacesReachableCareAndFirstCallTargetsAtTheRadioDesk()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            var generatedOpening = Object.FindFirstObjectByType<OpeningHouseholdController>();
            var inspection = Object.FindFirstObjectByType<InspectionController>();
            var generatedSession = Object.FindFirstObjectByType<CaseSession>();
            var generatedLog = Object.FindFirstObjectByType<TransmissionLog>();
            var targets = Object.FindObjectsByType<OpeningWorldTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Assert.That(generatedOpening, Is.Not.Null);
            Assert.That(inspection, Is.Not.Null);
            Assert.That(targets, Has.Length.EqualTo(2));

            generatedOpening.Close();
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            var care = System.Array.Find(targets, target => target.Prompt.Contains("Check in"));
            var call = System.Array.Find(targets, target => target.Prompt.Contains("waiting call"));
            Assert.That(care, Is.Not.Null);
            Assert.That(call, Is.Not.Null);

            Focus(inspection.transform, capsule, camera, care.transform.position);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(care));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(generatedOpening.IsCarePanelOpen, Is.True);
            generatedOpening.Close();

            Focus(inspection.transform, capsule, camera, call.transform.position);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(call));
            Assert.That(inspection.TryActivateFocused(), Is.True);
            Assert.That(generatedSession.State.HasEvidence("E01"), Is.True);
            Assert.That(generatedLog.Records, Has.Count.EqualTo(1));
            Assert.That(generatedLog.IsShowingTranscript, Is.True);
        }

        private static void Focus(Transform playerTransform, CharacterController capsule, Camera camera, Vector3 targetPosition)
        {
            capsule.enabled = false;
            playerTransform.position = targetPosition + new Vector3(0f, -1.1f, 1.9f);
            playerTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
            camera.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            capsule.enabled = true;
            Physics.SyncTransforms();
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
