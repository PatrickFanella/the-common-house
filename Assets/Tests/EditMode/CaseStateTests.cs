using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using NUnit.Framework;

namespace CommonHouse.Tests
{
    public class CaseStateTests
    {
        private static CaseState NewState() => new CaseState(CaseContent.LoadBaseline());
        private static void Acquire(CaseState state, params string[] sources)
        {
            foreach (var source in sources) state.AcquireSource("acquire-" + source, source);
        }
        private static string Facts(CaseState state) => string.Join("|", new[] {
            state.Phase.ToString(), state.OutcomeId ?? "none",
            string.Join(",", state.AcquiredSourceIds), string.Join(",", state.DiscoveredEvidenceIds),
            string.Join(",", state.Promises),
            string.Join(";", state.Disclosures.Select(item => item.EventId + ":" + item.SourceId + ":" + item.AttributedTo + ":" + string.Join(",", item.Recipients)))
        });

        [Test]
        public void NewSessionsHaveIndependentEmptyStateAndShareOnlyImmutableContent()
        {
            var content = CaseContent.LoadBaseline();
            var first = new CaseState(content);
            var second = new CaseState(content);
            Acquire(first, "T01");
            Assert.That(second.Phase, Is.EqualTo(CasePhase.NotStarted));
            Assert.That(second.DiscoveredEvidenceIds, Is.Empty);
            Assert.That(second.Disclosures, Is.Empty);
            Assert.That(second.Promises, Is.Empty);
            Assert.That(second.OutcomeId, Is.Null);
            Assert.That(content.Evidence.Count, Is.EqualTo(6));
        }

        [Test]
        public void CompositeEvidenceRequiresEachSourceWithoutInferringIdentity()
        {
            var state = NewState();
            Acquire(state, "T01", "D01", "T03");
            Assert.That(state.HasEvidence("E03"), Is.False);
            Assert.That(state.HasEvidence("E06"), Is.False);
            Assert.That(state.MissingSources("E03"), Is.EqualTo(new[] { "D02" }));
            Assert.That(state.MissingSources("E06"), Is.EqualTo(new[] { "C02" }));
            Acquire(state, "D02");
            Assert.That(state.HasEvidence("E03"), Is.True);
            Assert.That(state.IdentityConfirmed, Is.False);
            Acquire(state, "C02", "C01");
            Assert.That(state.HasEvidence("E06"), Is.True);
            Assert.That(state.IdentityConfirmed, Is.True);
            Assert.That(state.Promises, Is.Empty);
            Assert.That(state.OutcomeId, Is.Null);
        }

        [Test]
        public void ArchiveFirstAndConversationFirstReachTheSameEvidenceFacts()
        {
            var archive = NewState();
            var conversation = NewState();
            Acquire(archive, "T01", "T02", "D01", "D02", "C01", "D03", "T03", "C02");
            Acquire(conversation, "T01", "C01", "C02", "T03", "D03", "D02", "D01", "T02");
            Assert.That(archive.DiscoveredEvidenceIds, Is.EqualTo(conversation.DiscoveredEvidenceIds));
            Assert.That(archive.DiscoveredEvidenceIds.Count, Is.EqualTo(6));
            Assert.That(archive.AcquiredSourceIds, Is.EqualTo(conversation.AcquiredSourceIds));
            Assert.That(archive.Disclosures.Select(item => item.SourceId), Is.EquivalentTo(conversation.Disclosures.Select(item => item.SourceId)));
            Assert.That(archive.Phase, Is.EqualTo(CasePhase.Investigating));
            Assert.That(conversation.OutcomeId, Is.Null);
        }

        [Test]
        public void RepeatAcquisitionAndReplayDoNotDuplicateOrStrengthenFacts()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            var before = Facts(state);
            Assert.That(state.AcquireSource("acquire-T01", "T01"), Is.False);
            Assert.That(state.AcquireSource("second-receipt", "T01"), Is.False);
            Assert.That(state.ReplaySource("T01").SpeakerId, Is.EqualTo("ada"));
            state.ReplaySource("C01");
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.Disclosures.Count, Is.EqualTo(2));
            Assert.That(state.Disclosures.Single(item => item.SourceId == "C01").AttributedTo, Is.EqualTo("ren"));
            Assert.That(state.Disclosures.All(item => item.Recipients.SequenceEqual(new[] { "player" })), Is.True);
        }

        [Test]
        public void EventIdCollisionCannotApplyAnotherAction()
        {
            var state = NewState();
            state.AcquireSource("event", "T01");
            var before = Facts(state);
            Assert.Throws<InvalidOperationException>(() => state.AcquireSource("event", "C01"));
            Assert.Throws<InvalidOperationException>(() => state.RecordPromise("event", CasePromise.ProtectPrivateContact));
            Assert.That(Facts(state), Is.EqualTo(before));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("unknown")]
        public void UnknownSourcesAreRejectedWithoutConsumingEvent(string source)
        {
            var state = NewState();
            var before = Facts(state);
            Assert.Throws<ArgumentException>(() => state.AcquireSource("event", source));
            Assert.Throws<ArgumentException>(() => state.ReplaySource(source));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.AcquireSource("event", "T01"), Is.True);
        }

        [Test]
        public void UnknownEvidenceAndUnacquiredReplayDoNotRevealContent()
        {
            var state = NewState();
            Assert.Throws<ArgumentException>(() => state.HasEvidence("E07"));
            Assert.Throws<ArgumentException>(() => state.MissingSources("E07"));
            Assert.Throws<InvalidOperationException>(() => state.ReplaySource("C01"));
            Assert.That(state.AcquiredSourceIds, Is.Empty);
        }

        [Test]
        public void PromiseIsSeparateFromEvidenceDisclosureAndOutcomeAndRecordedOnce()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            Assert.That(state.RecordPromise("promise", CasePromise.ProtectPrivateContact), Is.True);
            var before = Facts(state);
            Assert.That(state.RecordPromise("promise", CasePromise.ProtectPrivateContact), Is.False);
            Assert.That(state.RecordPromise("repeated-promise", CasePromise.ProtectPrivateContact), Is.False);
            Assert.Throws<ArgumentException>(() => state.RecordPromise("invalid", (CasePromise)99));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.OutcomeId, Is.Null);
        }

        [Test]
        public void ResolutionAndDisclosureCommitTogetherAndAftermathAppliesOnce()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            Assert.That(state.CommitResolution("send", "R3", new[] { "T01" }, new[] { "ren", "ivo" }), Is.True);
            Assert.That(state.Phase, Is.EqualTo(CasePhase.Resolved));
            Assert.That(state.OutcomeId, Is.EqualTo("R3"));
            var sent = state.Disclosures.Single(item => item.EventId == "send");
            Assert.That(sent.SourceId, Is.EqualTo("T01"));
            Assert.That(sent.AttributedTo, Is.EqualTo("ada"));
            Assert.That(sent.Recipients, Is.EquivalentTo(new[] { "ren", "ivo" }));
            var before = Facts(state);
            Assert.That(state.CommitResolution("send", "R3", new[] { "T01" }, new[] { "ivo", "ren" }), Is.False);
            Assert.Throws<InvalidOperationException>(() => state.CommitResolution("second-send", "R2", new[] { "C01" }, new[] { "ada" }));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.CompleteAftermath("aftermath"), Is.True);
            Assert.That(state.CompleteAftermath("aftermath"), Is.False);
            Assert.That(state.CompleteAftermath("revisit"), Is.False);
            Assert.That(state.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            Assert.That(state.Disclosures.Count, Is.EqualTo(3));
            Assert.That(state.ReplaySource("T01").Id, Is.EqualTo("T01"));
        }

        [TestCase("R9", "T01", "ren", typeof(ArgumentException))]
        [TestCase("R3", "missing", "ren", typeof(ArgumentException))]
        [TestCase("R3", "T01", "unknown", typeof(ArgumentException))]
        [TestCase("R3", "T02", "ren", typeof(InvalidOperationException))]
        public void InvalidResolutionLeavesAllFactsAndEventIdIntact(string outcome, string source, string recipient, Type exception)
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            var before = Facts(state);
            Assert.Throws(exception, () => state.CommitResolution("send", outcome, new[] { source }, new[] { recipient }));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.CommitResolution("send", "R3", new[] { "T01" }, new[] { "ren", "ivo" }), Is.True);
        }

        [Test]
        public void EmptyRecipientsAndPartialInvalidBatchesCannotPartiallyCommit()
        {
            var state = NewState();
            Acquire(state, "T01");
            var before = Facts(state);
            Assert.Throws<ArgumentException>(() => state.CommitResolution("send", "R3", new[] { "T01", "bad" }, new[] { "ren" }));
            Assert.Throws<ArgumentException>(() => state.CommitResolution("send", "R3", new[] { "T01" }, Array.Empty<string>()));
            Assert.Throws<ArgumentException>(() => state.CommitResolution("send", "R3", new[] { "T01" }, new[] { "ren", "bad" }));
            Assert.Throws<ArgumentException>(() => state.CommitResolution("send", "R3", new[] { "T01", "T01" }, new[] { "ren" }));
            Assert.That(Facts(state), Is.EqualTo(before));
        }

        [Test]
        public void PrematureAftermathAndMissingEventIdsDoNotChangeState()
        {
            var state = NewState();
            Assert.Throws<InvalidOperationException>(() => state.CompleteAftermath("later"));
            Assert.Throws<ArgumentException>(() => state.AcquireSource(" ", "T01"));
            Assert.That(state.Phase, Is.EqualTo(CasePhase.NotStarted));
            Assert.That(state.Disclosures, Is.Empty);
        }

        [Test]
        public void PublicCollectionsAndCallerArraysCannotMutateRecordedFacts()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            var recipients = new[] { "ren", "ivo" };
            var sources = new[] { "T01" };
            state.CommitResolution("send", "R3", sources, recipients);
            var before = Facts(state);
            recipients[0] = "ada";
            sources[0] = "C01";
            Assert.Throws<NotSupportedException>(() => ((IList<string>)state.AcquiredSourceIds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<Disclosure>)state.Disclosures).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)state.Disclosures.Last().Recipients).Clear());
            Assert.That(Facts(state), Is.EqualTo(before));
        }
    }
}
