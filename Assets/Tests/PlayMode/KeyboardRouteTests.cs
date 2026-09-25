using System;
using System.Collections;
using System.IO;
using System.Linq;
using CommonHouse.Aftermath;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Notebook;
using CommonHouse.Opening;
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
    public class KeyboardRouteTests
    {
        [Test]
        public void ArrowKeyLookBindingMovesTheViewWithoutPointerInputAndCancelsOpposites()
        {
            Assert.That(KeyboardLookBindings.Resolve(true, false, false, false), Is.EqualTo(new Vector2(-1f, 0f)));
            Assert.That(KeyboardLookBindings.Resolve(false, true, true, false), Is.EqualTo(new Vector2(1f, 1f)));
            Assert.That(KeyboardLookBindings.Resolve(true, true, true, true), Is.EqualTo(Vector2.zero));
            Assert.That(KeyboardMovementBindings.Resolve(false, false, false, false), Is.EqualTo(Vector2.zero));
            Assert.That(KeyboardMovementBindings.Resolve(true, false, true, false), Is.EqualTo(new Vector2(-1f, 1f)));

            var player = new GameObject("Keyboard Look Player");
            var camera = new GameObject("Keyboard Look Camera").AddComponent<Camera>();
            try
            {
                player.AddComponent<CharacterController>();
                var movement = player.AddComponent<FirstPersonMovement>();
                camera.transform.SetParent(player.transform, false);
                movement.SetViewTransform(camera.transform);
                for (var frame = 0; frame < 30; frame++)
                    movement.Tick(new FirstPersonInputFrame(KeyboardMovementBindings.Resolve(false, false, false, false),
                        KeyboardLookBindings.Resolve(false, true, false, false, 1f / 30f)), 1f / 30f);

                var yawAtThirtyFrames = player.transform.eulerAngles.y;
                Assert.That(yawAtThirtyFrames, Is.EqualTo(120f).Within(0.01f));
                player.transform.rotation = Quaternion.identity;
                for (var frame = 0; frame < 120; frame++)
                    movement.Tick(new FirstPersonInputFrame(KeyboardMovementBindings.Resolve(false, false, false, false),
                        KeyboardLookBindings.Resolve(false, true, false, false, 1f / 120f)), 1f / 120f);
                Assert.That(player.transform.eulerAngles.y, Is.EqualTo(yawAtThirtyFrames).Within(0.01f),
                    "One second of keyboard look must turn the same amount at 30 and 120 frames.");
            }
            finally
            {
                Object.DestroyImmediate(player);
            }
        }

        [UnityTest]
        public IEnumerator KeyboardFramesCompleteTheRefusalRouteWithFocusRestoreAndDisposableReload()
        {
            var saveRoot = Path.Combine(Path.GetTempPath(), "common-house-keyboard-route-" + Guid.NewGuid().ToString("N"));
            try
            {
                yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
                yield return null;

                var opening = Object.FindAnyObjectByType<OpeningHouseholdController>();
                var inspection = Object.FindAnyObjectByType<InspectionController>();
                var modal = Object.FindAnyObjectByType<ModalInputCoordinator>();
                var session = Object.FindAnyObjectByType<CaseSession>();
                var movement = inspection.GetComponent<FirstPersonMovement>();
                var radio = Object.FindAnyObjectByType<TransmissionLog>();
                var notebook = Object.FindAnyObjectByType<NotebookController>();
                var dialogue = Object.FindAnyObjectByType<DialoguePresenter>();
                var routing = Object.FindAnyObjectByType<RouteInspectionController>();
                var send = Object.FindAnyObjectByType<SendConfirmationController>();
                var saves = Object.FindAnyObjectByType<CaseRuntimeSaveController>();
                var aftermath = Object.FindAnyObjectByType<AftermathHouseholdController>();
                Assert.That(opening, Is.Not.Null);
                Assert.That(inspection, Is.Not.Null);
                Assert.That(modal, Is.Not.Null);
                Assert.That(session, Is.Not.Null);
                Assert.That(movement, Is.Not.Null);
                Assert.That(radio, Is.Not.Null);
                Assert.That(notebook, Is.Not.Null);
                Assert.That(dialogue, Is.Not.Null);
                Assert.That(routing, Is.Not.Null);
                Assert.That(send, Is.Not.Null);
                Assert.That(saves, Is.Not.Null);
                Assert.That(aftermath, Is.Not.Null);

                opening.Close();
                Assert.That(modal.HasOwner, Is.False);

                var openingTargets = Object.FindObjectsByType<OpeningWorldTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                var call = openingTargets.Single(target => target.Prompt.Contains("waiting call"));
                Activate(inspection, call);
                Assert.That(radio.IsShowingTranscript, Is.True);
                Assert.That(session.State.AcquiredSourceIds, Does.Contain("T01"));
                modal.HandleCancel();
                Assert.That(radio.IsShowingTranscript, Is.False);
                modal.HandleCancel();
                Assert.That(radio.IsOpen, Is.False);
                Assert.That(modal.HasOwner, Is.False);

                var archiveTape = Object.FindAnyObjectByType<ArchiveTapeTarget>();
                Activate(inspection, archiveTape);
                Assert.That(radio.IsShowingTranscript, Is.True);
                Assert.That(radio.SelectedRecord.SourceId, Is.EqualTo("T02"));
                modal.HandleCancel();
                modal.HandleCancel();

                var ren = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Single(target => target.ResidentId == "ren");
                Activate(inspection, ren);
                SelectDialogue(dialogue, "ren-current-call");
                Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
                modal.HandleCancel();
                Assert.That(dialogue.IsOpen, Is.False);

                var documents = Object.FindObjectsByType<InspectionTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                InspectAndAcquire(inspection, documents.Single(target => target.AcquisitionId == "D01"));
                InspectAndAcquire(inspection, documents.Single(target => target.AcquisitionId == "D02"));
                notebook.RefreshEntries();
                Assert.That(notebook.TryOpen(), Is.True);
                var identityBeforeComparison = session.State.IdentityConfirmed;
                var authorizationBeforeComparison = session.State.Authorization;
                notebook.Tick(new NotebookInputFrame(false, false, false, comparisonToggle: true));
                SelectNotebookEntry(notebook, "D01");
                notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSetSelected: true));
                notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSideToggle: true));
                SelectNotebookEntry(notebook, "D02");
                notebook.Tick(new NotebookInputFrame(false, false, false, comparisonSetSelected: true));
                Assert.That(notebook.Comparison.LeftSourceId, Is.EqualTo("D01"));
                Assert.That(notebook.Comparison.RightSourceId, Is.EqualTo("D02"));
                Assert.That(session.State.IdentityConfirmed, Is.EqualTo(identityBeforeComparison));
                Assert.That(session.State.Authorization, Is.EqualTo(authorizationBeforeComparison));
                modal.HandleCancel();
                Assert.That(notebook.Comparison.IsOpen, Is.False);
                modal.HandleCancel();
                Assert.That(notebook.IsOpen, Is.False);

                var routeTarget = Object.FindAnyObjectByType<RouteInspectionWorldTarget>();
                Activate(inspection, routeTarget);
                routing.Tick(new RouteInspectionInputFrame(false, false, true, false));
                routing.Tick(new RouteInspectionInputFrame(false, true, false, false));
                routing.Tick(new RouteInspectionInputFrame(false, false, true, false));
                Assert.That(routing.VerifyPrivateRoute(), Is.EqualTo(RouteInspectionResult.Verified));
                Assert.That(session.State.PrivateRouteVerified, Is.True);
                modal.HandleCancel();

                saves.Configure(session, new CaseSaveStore(session.Content, saveRoot));
                send.ConfigureCheckpoint(saves);
                var console = Object.FindAnyObjectByType<SendConfirmationWorldTarget>();
                Activate(inspection, console);
                send.Tick(new SendConfirmationInputFrame(false, true, false, false, false, false));
                send.Tick(new SendConfirmationInputFrame(false, false, true, false, false, false));
                Assert.That(send.Summary.OutcomeId, Is.EqualTo("R2"));
                modal.HandleCancel();
                Assert.That(session.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));

                Activate(inspection, console);
                send.Tick(new SendConfirmationInputFrame(false, false, false, false, true, false));
                Assert.That(session.State.IsDeferred, Is.True);
                send.Tick(new SendConfirmationInputFrame(false, false, false, false, false, true));
                Assert.That(session.State.IsDeferred, Is.False);
                Assert.That(send.Summary.OutcomeId, Is.EqualTo("R2"));
                Assert.That(send.Confirm(), Is.True);
                Assert.That(session.State.OutcomeId, Is.EqualTo("R2"));
                Assert.That(saves.Load(), Is.True);
                Assert.That(session.State.OutcomeId, Is.Null, "The pre-send checkpoint restores the unsent draft.");
                Assert.That(session.State.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));

                Activate(inspection, console);
                send.Tick(new SendConfirmationInputFrame(false, false, true, false, false, false));
                Assert.That(send.Summary.OutcomeId, Is.EqualTo("R2"));
                Assert.That(send.Confirm(), Is.True);
                Assert.That(session.State.OutcomeId, Is.EqualTo("R2"));
                modal.HandleCancel();
                Assert.That(modal.HasOwner, Is.False);
                var aftermathTarget = Object.FindAnyObjectByType<AftermathWorldTarget>();
                ActivateFromClearCamera(inspection, aftermathTarget);
                aftermath.Tick(new AftermathInputFrame(true));
                Assert.That(session.State.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
                modal.HandleCancel();

                modal.HandleCancel();
                Assert.That(movement.MenuOpen, Is.True);
                modal.HandleCancel();
                Assert.That(movement.MenuOpen, Is.False);
            }
            finally
            {
                if (Directory.Exists(saveRoot)) Directory.Delete(saveRoot, true);
            }
        }

        private static void Activate(InspectionController inspection, WorldInteractionTarget target)
        {
            Activate(inspection, target, 0.9f);
        }

        private static void Activate(InspectionController inspection, WorldInteractionTarget target, float distance)
        {
            Focus(inspection, target.transform.position, distance);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.TryActivateFocused(), Is.True);
        }

        private static void InspectAndAcquire(InspectionController inspection, InspectionTarget target)
        {
            Focus(inspection, target.transform.position, 0.25f);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentFocus, Is.SameAs(target));
            Assert.That(inspection.TryOpenFocused(), Is.True);
            Assert.That(inspection.TryAcquireOpen(), Is.EqualTo(InspectionAcquisitionResult.Acquired));
            inspection.Close();
        }

        // The kitchen aftermath terminal is near blockout furniture. Place the
        // scene camera in its clear interaction lane so this remains a raycast
        // focus test, rather than depending on unrelated collider placement.
        private static void ActivateFromClearCamera(InspectionController inspection, WorldInteractionTarget target)
        {
            var camera = inspection.GetComponentInChildren<Camera>();
            camera.transform.position = target.transform.position + new Vector3(0f, 0f, -0.25f);
            camera.transform.LookAt(target.transform.position);
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.TryActivateFocused(), Is.True);
        }

        private static void Focus(InspectionController inspection, Vector3 targetPosition, float distance)
        {
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            capsule.enabled = false;
            inspection.transform.position = targetPosition + new Vector3(0f, -1.4f, -distance);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.LookAt(targetPosition);
            capsule.enabled = true;
            Physics.SyncTransforms();
        }

        private static void SelectDialogue(DialoguePresenter dialogue, string nodeId)
        {
            var index = dialogue.EligibleQuestions.ToList().FindIndex(node => node.Id == nodeId);
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            while (dialogue.SelectedIndex != index)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(dialogue.SelectCurrent(), Is.True);
        }

        private static void SelectNotebookEntry(NotebookController notebook, string sourceId)
        {
            for (var attempts = 0; attempts < notebook.Entries.Count && notebook.SelectedEntry.SourceId != sourceId; attempts++)
                notebook.Tick(new NotebookInputFrame(false, true, false));
            Assert.That(notebook.SelectedEntry.SourceId, Is.EqualTo(sourceId));
        }
    }
}
