using System;
using System.Collections;
using System.IO;
using System.Linq;
using CommonHouse.Aftermath;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Opening;
using CommonHouse.Persistence;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    /// <summary>
    /// Integrated scene qualification. Helpers use the same interaction ray and
    /// controller actions as the scene, while avoiding host input injection.
    /// </summary>
    public class CaseRouteTests
    {
        [UnityTest] public IEnumerator R1ArchiveFirst() { yield return RunRoute(ResponseRoute.PrivateHeadset, true); }
        [UnityTest] public IEnumerator R1ConversationFirst() { yield return RunRoute(ResponseRoute.PrivateHeadset, false); }
        [UnityTest] public IEnumerator R2ArchiveFirst() { yield return RunRoute(ResponseRoute.RefusalRelay, true); }
        [UnityTest] public IEnumerator R2ConversationFirst() { yield return RunRoute(ResponseRoute.RefusalRelay, false); }
        [UnityTest] public IEnumerator R3ArchiveFirst() { yield return RunRoute(ResponseRoute.RoomSpeaker, true); }
        [UnityTest] public IEnumerator R3ConversationFirst() { yield return RunRoute(ResponseRoute.RoomSpeaker, false); }

        private static IEnumerator RunRoute(ResponseRoute route, bool archiveFirst)
        {
            var saveRoot = Path.Combine(Path.GetTempPath(), "common-house-case-route-" + Guid.NewGuid().ToString("N"));
            var previousVolume = AudioListener.volume;
            try
            {
                AudioListener.volume = 0f;
                yield return SceneManager.LoadSceneAsync("OutpostBlockout", LoadSceneMode.Single);
                yield return null;

                var opening = Object.FindAnyObjectByType<OpeningHouseholdController>();
                var inspection = Object.FindAnyObjectByType<InspectionController>();
                var modal = Object.FindAnyObjectByType<ModalInputCoordinator>();
                var session = Object.FindAnyObjectByType<CaseSession>();
                var log = Object.FindAnyObjectByType<TransmissionLog>();
                var dialogue = Object.FindAnyObjectByType<DialoguePresenter>();
                var routing = Object.FindAnyObjectByType<RouteInspectionController>();
                var send = Object.FindAnyObjectByType<SendConfirmationController>();
                var saves = Object.FindAnyObjectByType<CaseRuntimeSaveController>();
                var aftermath = Object.FindAnyObjectByType<AftermathHouseholdController>();
                Assert.That(opening, Is.Not.Null);
                Assert.That(inspection, Is.Not.Null);
                Assert.That(modal, Is.Not.Null);
                Assert.That(session, Is.Not.Null);
                Assert.That(log, Is.Not.Null);
                Assert.That(dialogue, Is.Not.Null);
                Assert.That(routing, Is.Not.Null);
                Assert.That(send, Is.Not.Null);
                Assert.That(saves, Is.Not.Null);
                Assert.That(aftermath, Is.Not.Null);
                Assert.That(AudioListener.volume, Is.EqualTo(0f), "This route must remain valid with audio muted.");
                opening.Close();

                var firstCall = Object.FindObjectsByType<OpeningWorldTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Single(target => target.Prompt.Contains("waiting call"));
                var archive = Object.FindAnyObjectByType<ArchiveTapeTarget>();
                var ren = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .Single(target => target.ResidentId == "ren");

                Activate(inspection, firstCall);
                Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T01"));
                CloseTranscriptAndLog(modal, log);

                if (archiveFirst) yield return AcquireAndReplayArchive(inspection, archive, modal, log, session);
                yield return SpeakWithRen(inspection, ren, modal, dialogue, "ren-current-call");
                Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
                if (!archiveFirst) yield return AcquireAndReplayArchive(inspection, archive, modal, log, session);

                Assert.That(session.State.HasEvidence("E02"), Is.True);
                Assert.That(log.Records.Single(record => record.SourceId == "T02").SoundCaption,
                    Is.EqualTo("[Kitchen bell sounds on the recording.]"),
                    "The consequential sound is available as a caption while audio is muted.");

                if (route == ResponseRoute.PrivateHeadset)
                    yield return PreparePrivateConsent(inspection, ren, modal, dialogue, routing, log, session);
                else
                    Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized),
                        "Archive context and optional evidence do not create private-contact consent.");

                Assert.That(session.State.AcquiredSourceIds, Does.Not.Contain("D01"));
                Assert.That(session.State.AcquiredSourceIds, Does.Not.Contain("D02"));
                Assert.That(session.State.AcquiredSourceIds, Does.Not.Contain("D03"));
                if (route != ResponseRoute.PrivateHeadset)
                {
                    Assert.That(session.State.AcquiredSourceIds, Does.Not.Contain("T03"));
                    Assert.That(session.State.AcquiredSourceIds, Does.Not.Contain("C02"));
                }

                saves.Configure(session, new CaseSaveStore(session.Content, saveRoot));
                send.ConfigureCheckpoint(saves);
                var console = Object.FindAnyObjectByType<SendConfirmationWorldTarget>();
                Activate(inspection, console);
                PrepareViaFrames(send, route);
                Assert.That(send.Summary.Route, Is.EqualTo(route));
                Assert.That(send.Defer(), Is.True);
                Assert.That(session.State.IsDeferred, Is.True);
                Assert.That(send.ResumeDraft(), Is.True);
                Assert.That(session.State.IsDeferred, Is.False);
                Assert.That(send.Summary.Route, Is.EqualTo(route));
                Assert.That(saves.SaveCheckpoint(), Is.True);
                Assert.That(saves.Load(), Is.True, "Reload immediately before sending restores the reviewed draft.");
                Assert.That(session.State.OutcomeId, Is.Null);
                Assert.That(session.State.DraftRoute, Is.EqualTo(route));

                Activate(inspection, console);
                PrepareViaFrames(send, route);
                if (route == ResponseRoute.RoomSpeaker)
                {
                    Assert.That(send.PublicDisclosureAcknowledged, Is.False);
                    Assert.That(send.Confirm(), Is.False, "Public disclosure requires its distinct acknowledgment.");
                    Assert.That(session.State.OutcomeId, Is.Null);
                    Assert.That(send.TogglePublicDisclosureAcknowledgement(), Is.True);
                }
                Assert.That(send.Confirm(), Is.True);
                Assert.That(session.State.OutcomeId, Is.EqualTo(OutcomeId(route)));
                AssertOutgoingDisclosure(session.State, route);
                Assert.That(send.Confirm(), Is.False, "Repeated confirmation must not duplicate a completed disclosure.");
                Assert.That(session.State.Disclosures.Count(item => item.MaterialId != "source"), Is.EqualTo(1));

                Assert.That(saves.Save(), Is.True);
                Assert.That(saves.Load(), Is.True, "Reload immediately after sending preserves the completed resolution.");
                Assert.That(session.State.OutcomeId, Is.EqualTo(OutcomeId(route)));
                Assert.That(session.State.Disclosures.Count(item => item.MaterialId != "source"), Is.EqualTo(1));
                AssertOutgoingDisclosure(session.State, route);

                ActivateFromClearCamera(inspection, Object.FindAnyObjectByType<AftermathWorldTarget>());
                aftermath.Tick(new AftermathInputFrame(true));
                Assert.That(session.State.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
                Assert.That(aftermath.Acknowledge(), Is.False, "One aftermath acknowledgement completes the case exactly once.");
            }
            finally
            {
                AudioListener.volume = previousVolume;
                if (Directory.Exists(saveRoot)) Directory.Delete(saveRoot, true);
            }
        }

        private static IEnumerator AcquireAndReplayArchive(InspectionController inspection, ArchiveTapeTarget archive,
            ModalInputCoordinator modal, TransmissionLog log, CaseSession session)
        {
            Activate(inspection, archive);
            Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T02"));
            Assert.That(log.SelectedRecord.Transcript, Does.Contain("Ada is addressed by name"));
            var sourceCount = session.State.AcquiredSourceIds.Count;
            var disclosureCount = session.State.Disclosures.Count;
            CloseTranscriptAndLog(modal, log);
            Activate(inspection, archive);
            CloseTranscriptAndLog(modal, log);
            Assert.That(session.State.AcquiredSourceIds.Count, Is.EqualTo(sourceCount), "Replay must not acquire a source twice.");
            Assert.That(session.State.Disclosures.Count, Is.EqualTo(disclosureCount), "Replay must not duplicate an attributed disclosure.");
            yield break;
        }

        private static IEnumerator PreparePrivateConsent(InspectionController inspection, ResidentDialogueTarget ren,
            ModalInputCoordinator modal, DialoguePresenter dialogue, RouteInspectionController routing,
            TransmissionLog log, CaseSession session)
        {
            var followup = Object.FindAnyObjectByType<RepairFollowupTarget>();
            Activate(inspection, followup);
            Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T03"));
            CloseTranscriptAndLog(modal, log);
            yield return SpeakWithRen(inspection, ren, modal, dialogue, "ren-repair-account");
            Assert.That(session.State.HasEvidence("E06"), Is.True);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized),
                "Corroboration is evidence, not consent.");

            Activate(inspection, Object.FindAnyObjectByType<RouteInspectionWorldTarget>());
            routing.Tick(new RouteInspectionInputFrame(false, false, true, false));
            routing.Tick(new RouteInspectionInputFrame(false, true, false, false));
            routing.Tick(new RouteInspectionInputFrame(false, false, true, false));
            Assert.That(routing.VerifyPrivateRoute(), Is.EqualTo(RouteInspectionResult.Verified));
            modal.HandleCancel();
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized),
                "A verified private endpoint is not contact permission.");

            yield return SpeakWithRen(inspection, ren, modal, dialogue, "ren-private-contact");
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
        }

        private static IEnumerator SpeakWithRen(InspectionController inspection, ResidentDialogueTarget ren,
            ModalInputCoordinator modal, DialoguePresenter dialogue, string nodeId)
        {
            Activate(inspection, ren);
            var index = dialogue.EligibleQuestions.ToList().FindIndex(node => node.Id == nodeId);
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "Expected authored Ren question: " + nodeId);
            while (dialogue.SelectedIndex != index)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(dialogue.SelectCurrent(), Is.True);
            modal.HandleCancel();
            Assert.That(dialogue.IsOpen, Is.False);
            yield break;
        }

        private static void PrepareViaFrames(SendConfirmationController send, ResponseRoute route)
        {
            for (var attempts = 0; attempts < 3 && send.SelectedRoute != route; attempts++)
                send.Tick(new SendConfirmationInputFrame(false, true, false, false, false, false));
            Assert.That(send.SelectedRoute, Is.EqualTo(route));
            send.Tick(new SendConfirmationInputFrame(false, false, true, false, false, false));
            Assert.That(send.IsReviewing, Is.True);
        }

        private static void AssertOutgoingDisclosure(CaseState state, ResponseRoute route)
        {
            var outgoing = state.Disclosures.Where(item => item.MaterialId != "source").ToArray();
            Assert.That(outgoing, Has.Length.EqualTo(1));
            var disclosure = outgoing[0];
            switch (route)
            {
                case ResponseRoute.PrivateHeadset:
                    Assert.That(disclosure.MaterialId, Is.EqualTo("private-connection"));
                    Assert.That(disclosure.Recipients, Is.EqualTo(new[] { "ada", "ren" }));
                    break;
                case ResponseRoute.RefusalRelay:
                    Assert.That(disclosure.MaterialId, Is.EqualTo("authorized-refusal"));
                    Assert.That(disclosure.Recipients, Is.EqualTo(new[] { "ada" }));
                    break;
                case ResponseRoute.RoomSpeaker:
                    Assert.That(disclosure.MaterialId, Is.EqualTo("public-request"));
                    Assert.That(disclosure.Recipients, Is.EqualTo(new[] { "ivo", "player", "ren" }));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(route));
            }
        }

        private static string OutcomeId(ResponseRoute route)
        {
            switch (route)
            {
                case ResponseRoute.PrivateHeadset: return "R1";
                case ResponseRoute.RefusalRelay: return "R2";
                case ResponseRoute.RoomSpeaker: return "R3";
                default: throw new ArgumentOutOfRangeException(nameof(route));
            }
        }

        private static void CloseTranscriptAndLog(ModalInputCoordinator modal, TransmissionLog log)
        {
            Assert.That(log.IsShowingTranscript, Is.True);
            modal.HandleCancel();
            Assert.That(log.IsShowingTranscript, Is.False);
            modal.HandleCancel();
            Assert.That(log.IsOpen, Is.False);
        }

        private static void Activate(InspectionController inspection, WorldInteractionTarget target)
        {
            Focus(inspection, target.transform.position, 0.9f);
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.TryActivateFocused(), Is.True);
        }

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
    }
}
