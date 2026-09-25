using CommonHouse.Content;
using CommonHouse.Domain;
using NUnit.Framework;

namespace CommonHouse.Tests
{
    public class HintTests
    {
        [Test]
        public void RepeatedRequestsEscalateFromLeadToComparisonToExplanation()
        {
            var state = NewState();
            var lead = InvestigationHints.Get(state, 0);
            var comparison = InvestigationHints.Get(state, 1);
            var explanation = InvestigationHints.Get(state, 2);

            Assert.That(lead.Stage, Is.EqualTo(InvestigationHintStage.Lead));
            Assert.That(comparison.Stage, Is.EqualTo(InvestigationHintStage.Comparison));
            Assert.That(explanation.Stage, Is.EqualTo(InvestigationHintStage.Explanation));
            Assert.That(new[] { lead.Text, comparison.Text, explanation.Text }, Is.Unique);
            Assert.That(explanation.Text, Does.Contain("does not confirm identity"));
        }

        [Test]
        public void OptionalRosterComparisonDoesNotBecomeIdentityOrConsent()
        {
            var state = NewState();
            Acquire(state, "D01");
            Acquire(state, "D02");

            var hint = InvestigationHints.Get(state, 2);
            Assert.That(state.HasEvidence("E03"), Is.True);
            Assert.That(state.IdentityConfirmed, Is.False);
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            Assert.That(hint.Text, Does.Contain("does not confirm identity"));
        }

        [Test]
        public void RefusalAndDeferredStatesRetainMeaningfulActions()
        {
            var state = NewState();
            Acquire(state, "T01");
            Acquire(state, "C01");
            state.RecordRefusalAuthorization("refusal");
            Assert.That(InvestigationHints.Get(state, 0).Text, Does.Contain("authorized refusal"));

            state.Defer();
            var deferred = InvestigationHints.Get(state, 2);
            Assert.That(deferred.Text, Does.Contain("not a resolution"));
            Assert.That(deferred.Text, Does.Not.Contain("send"));
        }

        [Test]
        public void E06AndRouteFactsStillRequireExplicitAgreementAndResolvedHasNoHint()
        {
            var state = NewState();
            Acquire(state, "T01");
            Acquire(state, "C01");
            state.RecordRefusalAuthorization("refusal");
            Acquire(state, "T03");
            Acquire(state, "C02");
            Acquire(state, "D01");
            Acquire(state, "D02");
            Acquire(state, "T02");
            Acquire(state, "D03");
            state.InspectPrivateRoute("route");

            Assert.That(state.HasEvidence("E06"), Is.True);
            Assert.That(InvestigationHints.Get(state, 2).Text, Does.Contain("explicit agreement"));
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));

            state.PrepareDraft(ResponseRoute.RefusalRelay);
            var summary = state.PresentSendSummary();
            state.ConfirmSend("resolve", summary, true);
            Assert.That(InvestigationHints.Get(state, 0), Is.Null);
        }

        [Test]
        public void PrivateAgreementDoesNotOfferRefusalWhenOptionalCluesRemainUnacquired()
        {
            var state = NewState();
            Acquire(state, "T01");
            Acquire(state, "C01");
            state.RecordRefusalAuthorization("refusal");
            Acquire(state, "T03");
            Acquire(state, "C02");
            state.InspectPrivateRoute("route");
            state.RecordPrivateContactAgreement("agreement");

            var hint = InvestigationHints.Get(state, 0);
            Assert.That(state.AcquiredSourceIds, Does.Not.Contain("D01"));
            Assert.That(hint.Text, Does.Contain("private headset"));
            Assert.That(hint.Text, Does.Not.Contain("refusal"));
        }

        private static CaseState NewState() => new CaseState(CaseContent.LoadBaseline());
        private static void Acquire(CaseState state, string sourceId) => state.AcquireSource("test:" + sourceId, sourceId);
    }
}
