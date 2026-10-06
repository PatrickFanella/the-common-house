using System;
using System.IO;
using System.Linq;
using CommonHouse.Aftermath;
using CommonHouse.Content;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Notebook;
using CommonHouse.Persistence;
using CommonHouse.Player;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class DecisionReloadTests
    {
        private string root;
        private GameObject player;
        private CaseSession session;
        private CaseRuntimeSaveController saves;
        private TransmissionLog log;
        private NotebookController notebook;
        private SendConfirmationController send;
        private RouteInspectionController route;
        private DialoguePresenter dialogue;
        private AftermathHouseholdController aftermath;
        private FirstPersonMovement movement;
        private ModalInputCoordinator modal;
        private InspectionController inspection;
        private CommonHouse.Hints.InvestigationHintController hints;
        private GameObject inspectionObject;

        [SetUp]
        public void SetUp()
        {
            root = Path.Combine(Path.GetTempPath(), "common-house-reload-" + Guid.NewGuid().ToString("N"));
            player = new GameObject("Decision Reload Test Player");
            player.AddComponent<CharacterController>();
            movement = player.AddComponent<FirstPersonMovement>();
            movement.SetInputSource(new FixedInput());
            modal = player.AddComponent<ModalInputCoordinator>();
            session = player.AddComponent<CaseSession>();
            var content = CaseContent.LoadBaseline();
            session.Configure(content, new CaseState(content));
            log = player.AddComponent<TransmissionLog>();
            log.Configure(session, modal);
            notebook = player.AddComponent<NotebookController>();
            notebook.Configure(session, modal);
            send = player.AddComponent<SendConfirmationController>();
            send.Configure(session, modal);
            route = player.AddComponent<RouteInspectionController>();
            route.Configure(session, modal);
            dialogue = player.AddComponent<DialoguePresenter>();
            dialogue.Configure(session, modal);
            aftermath = player.AddComponent<AftermathHouseholdController>();
            aftermath.Configure(session, modal);
            inspection = player.AddComponent<InspectionController>();
            var cameraObject = new GameObject("Decision Reload Inspection Camera");
            cameraObject.transform.SetParent(player.transform);
            cameraObject.transform.localPosition = Vector3.up;
            cameraObject.transform.localRotation = Quaternion.identity;
            var camera = cameraObject.AddComponent<Camera>();
            inspection.Configure(camera, movement, modal);
            inspectionObject = new GameObject("Decision Reload Inspection Target");
            inspectionObject.transform.position = cameraObject.transform.position + Vector3.forward;
            inspectionObject.AddComponent<BoxCollider>();
            inspectionObject.AddComponent<InspectionTarget>().Configure("reload-inspection", "Reload notice", "A disposable readable notice.");
            hints = player.AddComponent<CommonHouse.Hints.InvestigationHintController>();
            hints.Configure(session, modal);
            saves = player.AddComponent<CaseRuntimeSaveController>();
            saves.Configure(session, new CaseSaveStore(content, root));
            send.ConfigureCheckpoint(saves);
        }

        [TearDown]
        public void TearDown()
        {
            if (inspectionObject != null) Object.DestroyImmediate(inspectionObject);
            if (player != null) Object.DestroyImmediate(player);
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Directory.Delete(root, true);
        }

        [TestCase(KeyCode.L)]
        [TestCase(KeyCode.F9)]
        public void LoadKeyRestoresSavedStateAndIgnoresUnrelatedKeys(KeyCode key)
        {
            session.State.AcquireSource("test:call", "T01");
            Assert.That(saves.Save(), Is.True);
            session.ReplaceState(new CaseState(session.Content));
            var before = session.State;
            Assert.That(saves.HandleLoadKey(KeyCode.K), Is.False);
            Assert.That(session.State, Is.SameAs(before));
            Assert.That(saves.HandleLoadKey(key), Is.True);
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01" }));
            Assert.That(saves.LastStatus, Is.EqualTo("Loaded validated development save."));
        }

        [Test]
        public void PreSendCheckpointRestoresDraftConsentEndpointAndDerivedViewsAfterCancellation()
        {
            var state = session.State;
            state.AcquireSource("test:call", "T01");
            state.AcquireSource("test:private", "C01");
            state.RecordRefusalAuthorization("test:refusal");
            log.RefreshRecords();
            notebook.RefreshEntries();
            Assert.That(route.Open(), Is.True);
            route.Tick(new RouteInspectionInputFrame(false, false, true, false));
            route.Tick(new RouteInspectionInputFrame(false, true, true, false));
            route.Close();
            Assert.That(route.Model.CanVerify, Is.True);

            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.RefusalRelay), Is.True);
            Assert.That(send.IsReviewing, Is.True);
            Assert.That(saves.SaveCheckpoint(), Is.True);
            Assert.That(send.CancelReview(), Is.True, "Checkpoint must survive a pre-send cancellation.");
            Assert.That(send.Defer(), Is.True);
            session.ReplaceState(new CaseState(session.Content));
            log.RefreshRecords();
            notebook.RefreshEntries();

            Assert.That(saves.Load(), Is.True);
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(session.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(session.State.IsDeferred, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01", "C01" }));
            Assert.That(log.Records.Select(record => record.SourceId), Is.EqualTo(new[] { "T01" }));
            Assert.That(notebook.Entries.Select(entry => entry.SourceId), Is.EquivalentTo(new[] { "T01", "C01" }));
            Assert.That(send.IsOpen, Is.False);
            Assert.That(send.Summary, Is.Null, "A pre-load confirmation object must never survive into the loaded state.");
            Assert.That(route.Model.CanVerify, Is.False, "Page review belongs to the old scene session, not loaded state.");
        }

        [TestCase("R1")]
        [TestCase("R2")]
        [TestCase("R3")]
        public void ControllerConfirmationCheckpointsBeforeEachRouteThenManualPostSendSaveRestoresOneDisclosure(string outcomeId)
        {
            PrepareForOutcome(session.State, outcomeId);
            ConfirmThroughController(outcomeId);
            Assert.That(session.State.OutcomeId, Is.EqualTo(outcomeId));
            Assert.That(saves.Load(), Is.True, "The controller's checkpoint must restore the state from immediately before sending.");
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(session.State.DraftRoute, Is.EqualTo(RouteFor(outcomeId)));
            Assert.That(send.Summary, Is.Null);
            Assert.That(send.IsOpen, Is.False);

            ConfirmThroughController(outcomeId);
            Assert.That(session.State.CompleteAftermath("aftermath:household-check-in"), Is.True);
            var disclosures = session.State.Disclosures.ToArray();
            Assert.That(saves.Save(), Is.True);
            Assert.That(send.Confirm(), Is.False, "A duplicate confirmation must not checkpoint over the completed save.");
            session.ReplaceState(new CaseState(session.Content));

            Assert.That(saves.Load(), Is.True);
            Assert.That(session.State.OutcomeId, Is.EqualTo(outcomeId));
            Assert.That(session.State.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            AssertDisclosureSnapshots(session.State.Disclosures, disclosures);
            Assert.That(log.Receive("T01"), Is.EqualTo(TransmissionReceiveResult.AlreadyReceived),
                "Reloaded radio receipt retries must be inert and cannot retransmit the resolved call.");
            Assert.That(send.Open(), Is.False);
            Assert.That(session.State.CompleteAftermath("aftermath:household-check-in"), Is.False);
            Assert.That(aftermath.Open(), Is.True);
            Assert.That(aftermath.Acknowledge(), Is.False);
            AssertDisclosureSnapshots(session.State.Disclosures, disclosures);
        }

        [Test]
        public void LoadClosesInspectionAndHintModalsWithoutRetainingHintEscalation()
        {
            Assert.That(saves.Save(), Is.True);
            inspection.RefreshFocus();
            Assert.That(inspection.TryOpenFocused(), Is.True);
            Assert.That(inspection.IsOpen, Is.True);
            Assert.That(modal.HasOwner, Is.True);
            Assert.That(movement.MenuOpen, Is.True);

            Assert.That(saves.Load(), Is.True);
            Assert.That(inspection.IsOpen, Is.False);
            Assert.That(modal.HasOwner, Is.False);
            Assert.That(movement.MenuOpen, Is.False);

            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Comparison));
            Assert.That(modal.HasOwner, Is.True);
            Assert.That(movement.MenuOpen, Is.True);

            Assert.That(saves.Load(), Is.True);
            Assert.That(hints.IsOpen, Is.False);
            Assert.That(modal.HasOwner, Is.False);
            Assert.That(movement.MenuOpen, Is.False);
            Assert.That(hints.RequestHint(), Is.True);
            Assert.That(hints.CurrentHint.Stage, Is.EqualTo(InvestigationHintStage.Lead));

            var beforeInvalidLoad = session.State;
            File.WriteAllText(Path.Combine(root, "case-01.json"), "{ malformed");
            Assert.That(saves.Load(), Is.False);
            Assert.That(session.State, Is.SameAs(beforeInvalidLoad));
            Assert.That(hints.IsOpen, Is.True, "An invalid save must leave the existing modal untouched.");
            Assert.That(modal.HasOwner, Is.True);
            Assert.That(movement.MenuOpen, Is.True);
        }

        [Test]
        public void FailedPreSendCheckpointBlocksConfirmationWithoutChangingTheCase()
        {
            PrepareForOutcome(session.State, "R2");
            var blockedRoot = Path.Combine(root, "not-a-directory");
            Directory.CreateDirectory(root);
            File.WriteAllText(blockedRoot, "development saves disabled here");
            saves.Configure(session, new CaseSaveStore(session.Content, blockedRoot));
            send.ConfigureCheckpoint(saves);
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(ResponseRoute.RefusalRelay), Is.True);

            Assert.That(send.Confirm(), Is.False);
            Assert.That(session.State.OutcomeId, Is.Null);
            Assert.That(send.LastActionMessage, Does.Contain("checkpoint failed"));
        }

        [Test]
        public void UnrecoverableDisposableSaveReportsFailureAndLeavesCurrentStateUntouched()
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "case-01.json"), "{ broken");
            var before = session.State;

            Assert.That(saves.Load(), Is.False);
            Assert.That(session.State, Is.SameAs(before));
            Assert.That(saves.LastStatus, Does.StartWith("Load failed:"));
            Assert.That(File.Exists(Path.Combine(root, "case-01.json")), Is.True);
        }

        [UnityTest]
        public System.Collections.IEnumerator GeneratedSceneUsesItsDisposableTestRootAndControllerCheckpoint()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
            yield return null;
            var opening = Object.FindAnyObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
            var sceneSession = Object.FindAnyObjectByType<CaseSession>();
            var sceneSaves = Object.FindAnyObjectByType<CaseRuntimeSaveController>();
            var sceneSend = Object.FindAnyObjectByType<SendConfirmationController>();
            Assert.That(opening, Is.Not.Null);
            Assert.That(sceneSession, Is.Not.Null);
            Assert.That(sceneSaves, Is.Not.Null);
            Assert.That(sceneSend, Is.Not.Null);
            Assert.That(sceneSaves.IsConfigured, Is.True);
            Assert.That(sceneSaves.SavePath, Does.StartWith(Path.Combine(Application.persistentDataPath, "test-saves")));
            opening.Close();

            PrepareForOutcome(sceneSession.State, "R2");
            Assert.That(sceneSend.Open(), Is.True);
            Assert.That(sceneSend.Prepare(ResponseRoute.RefusalRelay), Is.True);
            Assert.That(sceneSend.Confirm(), Is.True);
            Assert.That(sceneSession.State.OutcomeId, Is.EqualTo("R2"));

            Assert.That(sceneSaves.Load(), Is.True);
            Assert.That(sceneSession.State.OutcomeId, Is.Null);
            Assert.That(sceneSession.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(sceneSend.Summary, Is.Null);
        }

        private static void PrepareForOutcome(CaseState state, string outcomeId)
        {
            state.AcquireSource("test:call", "T01");
            state.AcquireSource("test:private", "C01");
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
            else throw new ArgumentOutOfRangeException(nameof(outcomeId));
        }

        private void ConfirmThroughController(string outcomeId)
        {
            Assert.That(send.Open(), Is.True);
            Assert.That(send.Prepare(RouteFor(outcomeId)), Is.True);
            if (outcomeId == "R3") Assert.That(send.TogglePublicDisclosureAcknowledgement(), Is.True);
            Assert.That(send.Confirm(), Is.True);
        }

        private static ResponseRoute RouteFor(string outcomeId)
        {
            switch (outcomeId)
            {
                case "R1": return ResponseRoute.PrivateHeadset;
                case "R2": return ResponseRoute.RefusalRelay;
                case "R3": return ResponseRoute.RoomSpeaker;
                default: throw new ArgumentOutOfRangeException(nameof(outcomeId));
            }
        }

        private static void AssertDisclosureSnapshots(System.Collections.Generic.IReadOnlyList<Disclosure> actual, Disclosure[] expected)
        {
            Assert.That(actual, Has.Count.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
            {
                Assert.That(actual[index].EventId, Is.EqualTo(expected[index].EventId));
                Assert.That(actual[index].SourceId, Is.EqualTo(expected[index].SourceId));
                Assert.That(actual[index].AttributedTo, Is.EqualTo(expected[index].AttributedTo));
                Assert.That(actual[index].MaterialId, Is.EqualTo(expected[index].MaterialId));
                Assert.That(actual[index].Text, Is.EqualTo(expected[index].Text));
                Assert.That(actual[index].Recipients, Is.EqualTo(expected[index].Recipients));
            }
        }

        private sealed class FixedInput : IFirstPersonInput
        {
            public FirstPersonInputFrame Read() => new FirstPersonInputFrame(Vector2.zero, Vector2.zero);
        }
    }
}
