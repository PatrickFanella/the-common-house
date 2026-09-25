using System.Collections;
using System.Linq;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Notebook;
using CommonHouse.Opening;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class CallerCorroborationTests
    {
        [UnityTest]
        public IEnumerator BothSceneConsultationOrdersRemainAttributedAndNeverGrantConsent()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout");
            yield return null;
            Object.FindFirstObjectByType<OpeningHouseholdController>().Close();
            var session = Object.FindFirstObjectByType<CaseSession>();
            var radio = Object.FindFirstObjectByType<TransmissionLog>();
            var dialogue = Object.FindFirstObjectByType<DialoguePresenter>();
            var caller = Object.FindFirstObjectByType<RepairFollowupTarget>();
            var ren = Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsSortMode.None).Single(target => target.ResidentId == "ren");
            var notebook = Object.FindFirstObjectByType<NotebookController>();
            Assert.That(caller.Interact(), Is.False, "Follow-up requires the initial call.");
            for (var order = 0; order < 2; order++)
            {
                session.ReplaceState(new CaseState(session.Content));
                Assert.That(radio.Receive("T01"), Is.EqualTo(TransmissionReceiveResult.Received));
                if (order == 0) ReadAda(caller, radio); else AskRen(ren, dialogue);
                Assert.That(session.State.HasEvidence("E06"), Is.False);
                notebook.RefreshEntries();
                var partial = notebook.Entries.Single(entry => entry.SourceId == (order == 0 ? "T03" : "C02"));
                Assert.That(partial.Evidence.Single().Statements.Select(statement => statement.Label), Is.EqualTo(new[] { "Attributed claim" }));
                if (order == 0) AskRen(ren, dialogue); else ReadAda(caller, radio);
                Assert.That(session.State.HasEvidence("E06"), Is.True);
                Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
                Assert.That(session.State.IdentityConfirmed, Is.False);
                var count = session.State.Disclosures.Count;
                ReadAda(caller, radio);
                AskRen(ren, dialogue);
                Assert.That(session.State.Disclosures.Count, Is.EqualTo(count));
                Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T01", "T03", "C02" }));
            }
        }

        private static void ReadAda(RepairFollowupTarget target, TransmissionLog radio)
        {
            Activate(target);
            Assert.That(radio.IsShowingTranscript, Is.True);
            Assert.That(radio.SelectedRecord.SourceId, Is.EqualTo("T03"));
            radio.Close();
        }

        private static void AskRen(ResidentDialogueTarget target, DialoguePresenter dialogue)
        {
            Activate(target);
            for (var i = 0; i < dialogue.EligibleQuestions.Count && dialogue.EligibleQuestions[dialogue.SelectedIndex].Id != "ren-repair-account"; i++)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            Assert.That(dialogue.EligibleQuestions[dialogue.SelectedIndex].Id, Is.EqualTo("ren-repair-account"));
            Assert.That(dialogue.SelectCurrent(), Is.True);
            Assert.That(dialogue.CurrentLine.SpeakerId, Is.EqualTo("ren"));
            dialogue.Close();
        }

        private static void Activate(WorldInteractionTarget target)
        {
            var inspection = Object.FindFirstObjectByType<InspectionController>();
            var player = inspection.transform;
            var capsule = player.GetComponent<CharacterController>();
            var direction = target.transform.position.z < 0 ? -1f : 1f;
            capsule.enabled = false;
            player.position = target.transform.position + new Vector3(0, -1.4f, -direction * 1.9f);
            player.rotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
            capsule.enabled = true;
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.TryActivateFocused(), Is.True);
        }
    }
}
