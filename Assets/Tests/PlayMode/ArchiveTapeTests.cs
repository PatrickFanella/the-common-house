using System.Collections;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace CommonHouse.Tests
{
    public class ArchiveTapeTests
    {
        [UnityTest]
        public IEnumerator LabelledShelfSupportsTranscriptOnlyReplayAcrossRoomVisits()
        {
            yield return SceneManager.LoadSceneAsync("OutpostBlockout");
            yield return null; // Allow the initial welcome Start callback before dismissing it.
            Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>().Close();
            var target = Object.FindFirstObjectByType<ArchiveTapeTarget>();
            var inspection = Object.FindFirstObjectByType<InspectionController>();
            var session = Object.FindFirstObjectByType<CaseSession>();
            var log = Object.FindFirstObjectByType<TransmissionLog>();
            var player = inspection.transform;
            var capsule = player.GetComponent<CharacterController>();
            capsule.enabled = false;
            player.position = target.transform.position + new Vector3(0, -1.4f, 1.9f);
            player.rotation = Quaternion.Euler(0, 180, 0);
            capsule.enabled = true;
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            Assert.That(inspection.CurrentWorldFocus, Is.SameAs(target));
            Assert.That(inspection.FocusedPrompt, Does.Contain("dated before"));
            inspection.Tick(new InspectionInputFrame(true, false, false));
            Assert.That(log.IsShowingTranscript, Is.True);
            Assert.That(log.SelectedRecord.SourceId, Is.EqualTo("T02"));
            Assert.That(log.SelectedRecord.Transcript, Does.Contain("Ada"));
            Assert.That(log.SelectedRecord.SoundCaption, Does.Contain("Kitchen bell"));
            Assert.That(session.State.HasEvidence("E02"), Is.True);
            Assert.That(session.State.IdentityConfirmed, Is.False);
            Assert.That(session.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            var disclosures = session.State.Disclosures.Count;
            log.Close();
            capsule.enabled = false;
            player.position = new Vector3(0, 0.05f, 0);
            capsule.enabled = true;
            yield return null;
            Assert.That(log.Open(), Is.True);
            Assert.That(log.SelectReceivedSource("T02"), Is.True);
            Assert.That(log.OpenSelectedTranscript(), Is.True);
            log.Close();
            Assert.That(target.Interact(), Is.True);
            Assert.That(session.State.Disclosures.Count, Is.EqualTo(disclosures));
            Assert.That(session.State.AcquiredSourceIds, Is.EquivalentTo(new[] { "T02" }));
            log.Close();
        }
    }
}
