using System;
using System.IO;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class SaveStateTests
    {
        private string root;

        [SetUp]
        public void CreateDisposableRoot()
        {
            root = Path.Combine(Path.GetTempPath(), "common-house-save-tests-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void RemoveDisposableRoot()
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }

        private static CaseState NewState() => new CaseState(CaseContent.LoadBaseline());
        private static void Acquire(CaseState state, params string[] sourceIds)
        {
            foreach (var sourceId in sourceIds) state.AcquireSource("acquire-" + sourceId, sourceId);
        }
        private static CaseState PrivateReady()
        {
            var state = NewState();
            Acquire(state, "T01", "C01", "T03", "C02");
            state.RecordRefusalAuthorization("ren-refuses");
            state.InspectPrivateRoute("inspect");
            state.RecordPrivateContactAgreement("ren-agrees");
            return state;
        }
        private CaseSaveStore Store() => new CaseSaveStore(CaseContent.LoadBaseline(), root);

        [Test]
        public void RoundTripPreservesCaseFactsLedgerAndCompletedAftermath()
        {
            var state = PrivateReady();
            state.RecordPromise("promise", CasePromise.ProtectPrivateContact);
            state.PrepareDraft(ResponseRoute.PrivateHeadset);
            var summary = state.PresentSendSummary();
            Assert.That(state.ConfirmSend("send", summary, true), Is.True);
            Assert.That(state.CompleteAftermath("aftermath"), Is.True);
            var store = new CaseSaveStore(CaseContent.LoadBaseline(), root);
            store.Save(state);

            var result = store.Load();
            var loaded = result.State;
            Assert.That(result.RecoveredFromBackup, Is.False);
            Assert.That(loaded.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            Assert.That(loaded.OutcomeId, Is.EqualTo("R1"));
            Assert.That(loaded.AcquiredSourceIds, Is.EqualTo(state.AcquiredSourceIds));
            Assert.That(loaded.Promises, Is.EqualTo(state.Promises));
            Assert.That(loaded.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
            Assert.That(loaded.AuthorizationEventId, Is.EqualTo("ren-agrees"));
            Assert.That(loaded.PrivateRouteVerified, Is.True);
            Assert.That(loaded.DraftRoute, Is.EqualTo(ResponseRoute.PrivateHeadset));
            Assert.That(loaded.IsDeferred, Is.False);
            Assert.That(loaded.Disclosures.Select(item => item.EventId + ":" + item.SourceId + ":" + item.MaterialId + ":" + item.Text + ":" + string.Join(",", item.Recipients)),
                Is.EqualTo(state.Disclosures.Select(item => item.EventId + ":" + item.SourceId + ":" + item.MaterialId + ":" + item.Text + ":" + string.Join(",", item.Recipients))));
            Assert.That(loaded.ConfirmSend("send", summary, true), Is.False, "The restored event ledger must make an exact send retry inert.");
            Assert.That(loaded.AcquireSource("acquire-T01", "T01"), Is.False, "The restored event ledger must make an exact retry inert.");
            Assert.Throws<InvalidOperationException>(() => loaded.AcquireSource("acquire-T01", "C01"));
        }

        [Test]
        public void LoadedDraftRequiresAFreshSummaryAndPreservesDefer()
        {
            var state = PrivateReady();
            state.PrepareDraft(ResponseRoute.PrivateHeadset);
            var beforeSave = state.PresentSendSummary();
            state.Defer();
            var store = new CaseSaveStore(CaseContent.LoadBaseline(), root);
            store.Save(state);

            var loaded = store.Load().State;
            Assert.That(loaded.DraftRoute, Is.EqualTo(ResponseRoute.PrivateHeadset));
            Assert.That(loaded.IsDeferred, Is.True);
            Assert.Throws<InvalidOperationException>(() => loaded.ConfirmSend("send", beforeSave, true));
            loaded.Resume();
            Assert.That(loaded.ConfirmSend("send", loaded.PresentSendSummary(), true), Is.True);
        }

        [Test]
        public void MalformedAndInvalidSavesAreRejectedWithoutResettingTheOriginalFile()
        {
            var store = new CaseSaveStore(CaseContent.LoadBaseline(), root);
            var state = NewState();
            Acquire(state, "T01");
            store.Save(state);

            File.WriteAllText(store.SavePath, "{\"SchemaVersion\":", System.Text.Encoding.UTF8);
            var malformed = File.ReadAllText(store.SavePath);
            var malformedError = Assert.Throws<InvalidOperationException>(() => store.Load());
            Assert.That(malformedError.Message, Does.Contain("Files were preserved"));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(malformed));

            var forged = state.CaptureSnapshot();
            forged.AcquiredSourceIds = new[] { "unknown-source" };
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(forged));
            var invalid = File.ReadAllText(store.SavePath);
            var invalidError = Assert.Throws<InvalidOperationException>(() => store.Load());
            Assert.That(invalidError.Message, Does.Contain("Unknown source ID"));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(invalid));

            var incompatible = state.CaptureSnapshot();
            incompatible.SchemaVersion = CaseStateSnapshot.CurrentSchemaVersion + 1;
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(incompatible));
            var versioned = File.ReadAllText(store.SavePath);
            var versionError = Assert.Throws<InvalidOperationException>(() => store.Load());
            Assert.That(versionError.Message, Does.Contain("incompatible"));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(versioned));
        }

        [Test]
        public void InterruptedPrimaryWriteRecoversTheRetainedBackupWithoutReplacingTheBrokenFile()
        {
            var store = new CaseSaveStore(CaseContent.LoadBaseline(), root);
            var state = NewState();
            Acquire(state, "T01");
            store.Save(state);
            Acquire(state, "C01");
            state.RecordRefusalAuthorization("ren-refuses");
            store.Save(state);
            Assert.That(File.Exists(store.BackupPath), Is.True, "A subsequent atomic save retains its previous valid primary as backup.");

            File.WriteAllText(store.SavePath, "{interrupted", System.Text.Encoding.UTF8);
            var interrupted = File.ReadAllText(store.SavePath);
            var result = store.Load();
            Assert.That(result.RecoveredFromBackup, Is.True);
            Assert.That(result.SourcePath, Is.EqualTo(store.BackupPath));
            Assert.That(result.State.AcquiredSourceIds, Is.EqualTo(new[] { "T01" }));
            Assert.That(result.State.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(interrupted), "Recovery must not silently overwrite evidence of the interrupted write.");
            Assert.Throws<InvalidOperationException>(() => store.Save(result.State), "A recovered backup must not authorize replacement of the failed primary.");
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(interrupted));
            Assert.That(File.Exists(store.BackupPath), Is.True);
        }

        [Test]
        public void ForgedRecipientsAndRouteMaterialAreRejectedWithTheOriginalPreserved()
        {
            var state = PrivateReady();
            state.PrepareDraft(ResponseRoute.PrivateHeadset);
            state.ConfirmSend("send", state.PresentSendSummary(), true);
            var forged = state.CaptureSnapshot();
            forged.Disclosures.Single(item => item.MaterialId == "private-connection").Recipients = new[] { "ada", "ivo" };
            var store = new CaseSaveStore(CaseContent.LoadBaseline(), root);
            Directory.CreateDirectory(root);
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(forged));
            var original = File.ReadAllText(store.SavePath);

            var error = Assert.Throws<InvalidOperationException>(() => store.Load());
            Assert.That(error.Message, Does.Contain("outcome disclosure"));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(original));
        }

        [TestCase("R2")]
        [TestCase("R3")]
        public void RoundTripPreservesRefusalAndPublicOutcomeRecipients(string outcome)
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            state.RecordRefusalAuthorization("ren-refuses");
            var route = outcome == "R2" ? ResponseRoute.RefusalRelay : ResponseRoute.RoomSpeaker;
            state.PrepareDraft(route);
            var summary = state.PresentSendSummary();
            state.ConfirmSend("send", summary, true, outcome == "R3");
            var store = Store();
            store.Save(state);

            var loaded = store.Load().State;
            Assert.That(loaded.OutcomeId, Is.EqualTo(outcome));
            Assert.That(loaded.Disclosures.Last().MaterialId, Is.EqualTo(summary.MaterialId));
            Assert.That(loaded.Disclosures.Last().Recipients, Is.EquivalentTo(summary.Recipients));
            Assert.That(loaded.ConfirmSend("send", summary, true, true), Is.False);
        }

        [Test]
        public void DeferredStateWithoutADraftRoundTripsBecauseTheDomainPermitsIt()
        {
            var state = NewState();
            state.Defer();
            var store = Store();
            store.Save(state);

            var loaded = store.Load().State;
            Assert.That(loaded.DraftRoute, Is.Null);
            Assert.That(loaded.IsDeferred, Is.True);
            Assert.That(loaded.Phase, Is.EqualTo(CasePhase.NotStarted));
        }

        [Test]
        public void UndefinedNumericEnumsAndDuplicateDisclosuresAreRejected()
        {
            var state = NewState();
            Acquire(state, "T01");
            var store = Store();
            Directory.CreateDirectory(root);
            var numeric = state.CaptureSnapshot();
            numeric.Phase = "99";
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(numeric));
            var numericBytes = File.ReadAllText(store.SavePath);
            Assert.That(Assert.Throws<InvalidOperationException>(() => store.Load()).Message, Does.Contain("invalid case phase"));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(numericBytes));

            var duplicate = state.CaptureSnapshot();
            duplicate.Disclosures = duplicate.Disclosures.Concat(new[]
            {
                new PersistedDisclosure
                {
                    EventId = duplicate.Disclosures[0].EventId,
                    SourceId = duplicate.Disclosures[0].SourceId,
                    AttributedTo = duplicate.Disclosures[0].AttributedTo,
                    MaterialId = duplicate.Disclosures[0].MaterialId,
                    Text = duplicate.Disclosures[0].Text,
                    Recipients = duplicate.Disclosures[0].Recipients.ToArray(),
                },
            }).ToArray();
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(duplicate));
            Assert.That(Assert.Throws<InvalidOperationException>(() => store.Load()).Message, Does.Contain("duplicate disclosure"));
        }

        [Test]
        public void MissingDocumentLedgerEntryAndOrphanedTemporaryFileDoNotSilentlyChangeThePrimary()
        {
            var state = NewState();
            Acquire(state, "D01");
            var store = Store();
            store.Save(state);
            var missingLedger = state.CaptureSnapshot();
            missingLedger.Events = missingLedger.Events.Where(item => item.Payload != "7:acquire3:D01").ToArray();
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(missingLedger));
            Assert.That(Assert.Throws<InvalidOperationException>(() => store.Load()).Message, Does.Contain("missing from the event ledger"));

            File.Delete(store.SavePath);
            store = Store();
            store.Save(state);
            File.WriteAllText(store.SavePath + ".tmp", "{truncated", System.Text.Encoding.UTF8);
            var primary = File.ReadAllText(store.SavePath);
            var loaded = store.Load();
            Assert.That(loaded.RecoveredFromBackup, Is.False);
            Assert.That(loaded.State.AcquiredSourceIds, Is.EqualTo(new[] { "D01" }));
            Assert.That(File.ReadAllText(store.SavePath), Is.EqualTo(primary));
            Assert.That(File.Exists(store.SavePath + ".tmp"), Is.True);
        }

        [TestCase(false, "PrivateHeadset", false)]
        [TestCase(true, "RoomSpeaker", false)]
        [TestCase(true, "PrivateHeadset", true)]
        public void ResolvedSnapshotsMustRetainTheirOwnRouteAndCannotRemainDeferred(bool hasDraft, string route, bool deferred)
        {
            var state = PrivateReady();
            state.PrepareDraft(ResponseRoute.PrivateHeadset);
            state.ConfirmSend("send", state.PresentSendSummary(), true);
            var forged = state.CaptureSnapshot();
            forged.HasDraftRoute = hasDraft;
            forged.DraftRoute = hasDraft ? route : null;
            forged.IsDeferred = deferred;
            var store = Store();
            Directory.CreateDirectory(root);
            File.WriteAllText(store.SavePath, JsonUtility.ToJson(forged));

            Assert.That(Assert.Throws<InvalidOperationException>(() => store.Load()).Message, Does.Contain("completed route state"));
        }
    }
}
