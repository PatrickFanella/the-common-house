using System;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using NUnit.Framework;

namespace CommonHouse.Tests
{
    public class RoutingRulesTests
    {
        private static CaseState NewState() => new CaseState(CaseContent.LoadBaseline());
        private static void Acquire(CaseState state, params string[] ids)
        {
            foreach (var id in ids) state.AcquireSource("acquire-" + id, id);
        }
        private static CaseState RefusalReady()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            state.RecordRefusalAuthorization("ren-refuses");
            return state;
        }
        private static CaseState PrivateReady()
        {
            var state = RefusalReady();
            Acquire(state, "T03", "C02");
            state.InspectPrivateRoute("inspect");
            state.RecordPrivateContactAgreement("ren-agrees");
            return state;
        }
        private static SendSummary Draft(CaseState state, ResponseRoute route)
        {
            state.PrepareDraft(route);
            return state.PresentSendSummary();
        }

        [TestCase(ResponseRoute.PrivateHeadset)]
        [TestCase(ResponseRoute.RefusalRelay)]
        [TestCase(ResponseRoute.RoomSpeaker)]
        public void DraftBeforeIdentityIsRetainedButSendingIsBlocked(ResponseRoute route)
        {
            var state = NewState();
            Acquire(state, "T01", "D01", "D02");
            var summary = Draft(state, route);
            Assert.That(summary.CanSend, Is.False);
            Assert.That(summary.BlockingReason, Does.Contain("unresolved"));
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true, true));
            Assert.That(state.DraftRoute, Is.EqualTo(route));
            Assert.That(state.OutcomeId, Is.Null);
            Assert.That(state.IdentityConfirmed, Is.False);
        }

        [Test]
        public void AllEvidenceAndRouteInspectionStillGrantNoAuthorization()
        {
            var state = NewState();
            Acquire(state, "T01", "T02", "D01", "D02", "C01", "D03", "T03", "C02");
            state.InspectPrivateRoute("inspect");
            foreach (var id in state.AcquiredSourceIds) state.ReplaySource(id);
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(state.AuthorizationEventId, Is.Null);
            Assert.That(Draft(state, ResponseRoute.RefusalRelay).CanSend, Is.False);
            Assert.That(Draft(state, ResponseRoute.PrivateHeadset).CanSend, Is.False);
            Assert.Throws<InvalidOperationException>(() => state.RecordPrivateContactAgreement("premature"));
            Assert.That(state.Disclosures.All(item => item.Recipients.SequenceEqual(new[] { "player" })), Is.True);
        }

        [Test]
        public void RefusalNeedsTheActualPrivateConversationAndSeparateAuthorizationEvent()
        {
            var state = NewState();
            Acquire(state, "T01");
            Assert.Throws<InvalidOperationException>(() => state.RecordRefusalAuthorization("refusal"));
            Acquire(state, "C01");
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(state.RecordRefusalAuthorization("refusal"), Is.True);
            Assert.That(state.RecordRefusalAuthorization("refusal"), Is.False);
            Assert.That(state.RecordRefusalAuthorization("repeated-question"), Is.False);
            Assert.That(state.AuthorizationEventId, Is.EqualTo("refusal"));
        }

        [TestCase("ada")]
        [TestCase("ren")]
        [TestCase("route")]
        public void PrivateAgreementFailsUntilBothAccountsAndRouteInspectionExist(string missing)
        {
            var state = RefusalReady();
            if (missing != "ada") Acquire(state, "T03");
            if (missing != "ren") Acquire(state, "C02");
            if (missing != "route") state.InspectPrivateRoute("inspect");
            Assert.Throws<InvalidOperationException>(() => state.RecordPrivateContactAgreement("agreement"));
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
            Assert.That(Draft(state, ResponseRoute.PrivateHeadset).CanSend, Is.False);
            Assert.That(Draft(state, ResponseRoute.RefusalRelay).CanSend, Is.True);
            if (missing == "ada") Acquire(state, "T03");
            if (missing == "ren") Acquire(state, "C02");
            if (missing == "route") state.InspectPrivateRoute("inspect");
            Assert.That(state.RecordPrivateContactAgreement("agreement"), Is.True, "Failed events must not consume the ID.");
        }

        [Test]
        public void CorroborationAndPromisesDoNotSubstituteForLaterAgreement()
        {
            var state = RefusalReady();
            Acquire(state, "T03", "C02");
            state.InspectPrivateRoute("inspect");
            state.RecordPromise("promise", CasePromise.ProtectPrivateContact);
            Assert.That(Draft(state, ResponseRoute.PrivateHeadset).CanSend, Is.False);
            Assert.That(state.RecordPrivateContactAgreement("agreement"), Is.True);
            Assert.That(Draft(state, ResponseRoute.PrivateHeadset).CanSend, Is.True);
            Assert.That(state.OutcomeId, Is.Null);
        }

        [Test]
        public void LaterAgreementCannotReuseTheInitialRefusalEventOrBeResetByReplay()
        {
            var state = RefusalReady();
            Acquire(state, "T03", "C02");
            state.InspectPrivateRoute("inspect");
            Assert.Throws<InvalidOperationException>(() => state.RecordPrivateContactAgreement("ren-refuses"));
            state.RecordPrivateContactAgreement("later-agreement");
            Assert.That(state.RecordRefusalAuthorization("repeat-initial-question"), Is.False);
            Assert.That(state.RecordPrivateContactAgreement("repeat-agreement"), Is.False);
            state.ReplaySource("C01");
            state.AcquireSource("reacquire", "C01");
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.PrivateContactAuthorized));
            Assert.That(state.AuthorizationEventId, Is.EqualTo("later-agreement"));
        }

        [Test]
        public void RefusalCompletesWithOnlyEssentialEvidenceAndDisclosesOnlyTheRefusal()
        {
            var state = RefusalReady();
            var summary = Draft(state, ResponseRoute.RefusalRelay);
            Assert.That(summary.Recipients, Is.EqualTo(new[] { "ada" }));
            Assert.That(summary.MaterialId, Is.EqualTo("authorized-refusal"));
            Assert.That(state.ConfirmSend("send", summary, true), Is.True);
            Assert.That(state.OutcomeId, Is.EqualTo("R2"));
            Assert.That(state.Phase, Is.EqualTo(CasePhase.Resolved));
            var sent = state.Disclosures.Single(item => item.EventId == "send");
            Assert.That(sent.SourceId, Is.EqualTo("C01"));
            Assert.That(sent.AttributedTo, Is.EqualTo("ren"));
            Assert.That(sent.Text, Is.EqualTo("Ren declines immediate contact."));
            Assert.That(sent.Text, Does.Not.Contain("Mara"));
            Assert.That(sent.Text, Does.Not.Contain("location"));
            Assert.That(state.DiscoveredEvidenceIds, Is.EquivalentTo(new[] { "E01", "E04" }));
            Assert.That(state.CompleteAftermath("aftermath"), Is.True);
        }

        [Test]
        public void PrivateContactRecordsOnlyTheConnectionWithNoHouseholdListenersOrPrivateDialogue()
        {
            var state = PrivateReady();
            var known = state.AcquiredSourceIds.ToArray();
            var summary = Draft(state, ResponseRoute.PrivateHeadset);
            Assert.That(summary.Recipients, Is.EquivalentTo(new[] { "ren", "ada" }));
            Assert.That(state.ConfirmSend("send", summary, true), Is.True);
            Assert.That(state.OutcomeId, Is.EqualTo("R1"));
            var sent = state.Disclosures.Single(item => item.EventId == "send");
            Assert.That(sent.MaterialId, Is.EqualTo("private-connection"));
            Assert.That(sent.Text, Does.Contain("contents remain private"));
            Assert.That(sent.Recipients, Does.Not.Contain("player"));
            Assert.That(sent.Recipients, Does.Not.Contain("ivo"));
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(known));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PublicDisclosureAlwaysRequiresTheWarningAcknowledgment(bool hasPrivateAgreement)
        {
            var state = hasPrivateAgreement ? PrivateReady() : RefusalReady();
            var summary = Draft(state, ResponseRoute.RoomSpeaker);
            Assert.That(summary.CanSend, Is.True);
            Assert.That(summary.IsUnauthorizedDisclosure, Is.True);
            Assert.That(summary.Warning, Does.Contain("not authorized"));
            Assert.That(summary.Recipients, Is.EquivalentTo(new[] { "ren", "ivo", "player" }));
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true));
            Assert.That(state.OutcomeId, Is.Null);
            Assert.That(state.ConfirmSend("send", summary, true, true), Is.True);
            Assert.That(state.OutcomeId, Is.EqualTo("R3"));
            Assert.That(state.Disclosures.Last().Recipients, Is.EquivalentTo(summary.Recipients));
        }

        [Test]
        public void PublicDisclosureRequiresKnowledgeButDoesNotRequireOrCreateConsent()
        {
            var state = NewState();
            Acquire(state, "T01", "C01");
            var summary = Draft(state, ResponseRoute.RoomSpeaker);
            Assert.That(state.ConfirmSend("send", summary, true, true), Is.True);
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(state.PrivateRouteVerified, Is.False);
        }

        [Test]
        public void ChangedAuthorizationRejectsAnOldRefusalDraftAtSendTime()
        {
            var state = RefusalReady();
            var summary = Draft(state, ResponseRoute.RefusalRelay);
            Acquire(state, "T03", "C02");
            state.InspectPrivateRoute("inspect");
            state.RecordPrivateContactAgreement("agreement");
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true));
            Assert.That(state.PresentSendSummary().CanSend, Is.False);
            Assert.That(state.OutcomeId, Is.Null);
            Assert.That(state.Disclosures.All(item => item.EventId != "send"), Is.True);
        }

        [Test]
        public void CancelDeferAndResumeRetainDraftAndRecoverableSourcesWithoutSending()
        {
            var state = RefusalReady();
            var summary = Draft(state, ResponseRoute.RefusalRelay);
            var missing = state.MissingSources("E06").ToArray();
            state.CancelConfirmation();
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true));
            summary = state.PresentSendSummary();
            state.Defer();
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true));
            Assert.That(state.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(state.MissingSources("E06"), Is.EqualTo(missing));
            Assert.That(state.OutcomeId, Is.Null);
            Acquire(state, "T03");
            state.Resume();
            Assert.That(state.ConfirmSend("send", state.PresentSendSummary(), true), Is.True);
            Assert.That(state.OutcomeId, Is.EqualTo("R2"));
        }

        [Test]
        public void ConfirmationIsRequiredAndSummaryCannotBeBorrowedOrReusedAfterRouteChange()
        {
            var state = RefusalReady();
            var summary = Draft(state, ResponseRoute.RefusalRelay);
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, false));
            var another = RefusalReady();
            Draft(another, ResponseRoute.RefusalRelay);
            Assert.Throws<InvalidOperationException>(() => another.ConfirmSend("send", summary, true));
            state.PrepareDraft(ResponseRoute.RoomSpeaker);
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("send", summary, true));
            Assert.That(state.OutcomeId, Is.Null);
        }

        [TestCase(ResponseRoute.RefusalRelay)]
        [TestCase(ResponseRoute.PrivateHeadset)]
        [TestCase(ResponseRoute.RoomSpeaker)]
        public void DuplicateSendAndAftermathCannotApplyEffectsAgain(ResponseRoute route)
        {
            var state = route == ResponseRoute.PrivateHeadset ? PrivateReady() : RefusalReady();
            var summary = Draft(state, route);
            Assert.That(state.ConfirmSend("send", summary, true, true), Is.True);
            var count = state.Disclosures.Count;
            Assert.That(state.ConfirmSend("send", summary, true, true), Is.False);
            Assert.Throws<InvalidOperationException>(() => state.ConfirmSend("second-send", summary, true, true));
            Assert.That(state.Disclosures.Count, Is.EqualTo(count));
            Assert.That(state.CompleteAftermath("aftermath"), Is.True);
            Assert.That(state.CompleteAftermath("revisit"), Is.False);
            Assert.That(state.ConfirmSend("send", summary, true, true), Is.False);
            Assert.That(state.Disclosures.Count, Is.EqualTo(count));
        }
    }
}
