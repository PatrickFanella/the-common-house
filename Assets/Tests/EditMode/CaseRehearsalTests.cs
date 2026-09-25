using System;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using NUnit.Framework;

namespace CommonHouse.Tests
{
    public class CaseRehearsalTests
    {
        private static CaseState Start()
        {
            var state = new CaseState(CaseContent.LoadBaseline());
            TestContext.WriteLine("Opening household care is an authored scene prerequisite, not simulated here.");
            Read(state, "T01");
            return state;
        }

        private static string Facts(CaseState state) => string.Join(" | ", new[] {
            state.Phase.ToString(), state.Authorization.ToString(), state.AuthorizationEventId ?? "no authorization event",
            state.OutcomeId ?? "no outcome", state.DraftRoute?.ToString() ?? "no draft", state.IsDeferred.ToString(),
            string.Join(",", state.AcquiredSourceIds), string.Join(",", state.DiscoveredEvidenceIds),
            string.Join(";", state.Disclosures.Select(d => d.EventId + ":" + d.MaterialId + ":" + d.Text + ":" + string.Join(",", d.Recipients)))
        });

        private static void Read(CaseState state, string source)
        {
            state.AcquireSource("read-" + source, source);
            var before = Facts(state);
            var text = state.ReplaySource(source);
            Assert.That(text.Text, Is.Not.Empty);
            Assert.That(state.AcquireSource("read-" + source, source), Is.False);
            Assert.That(state.AcquireSource("revisit-" + source, source), Is.False);
            Assert.That(Facts(state), Is.EqualTo(before), "Text replay and revisiting must preserve facts.");
            TestContext.WriteLine("Read/replay " + source + " [" + text.SpeakerId + "]: " + text.Text);
            if (!string.IsNullOrEmpty(text.Caption)) TestContext.WriteLine(text.Caption);
            TestContext.WriteLine("Complete evidence: " + string.Join(", ", state.DiscoveredEvidenceIds));
        }

        private static void AskRen(CaseState state)
        {
            Read(state, "C01");
            Assert.That(state.HasEvidence("E04"), Is.True);
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.Unknown));
            state.RecordRefusalAuthorization("ren-initial-response");
            TestContext.WriteLine("Ren explicitly authorizes refusal; this is separate from acquiring E04.");
        }

        private static void Archive(CaseState state)
        {
            Read(state, "T02");
            Read(state, "D01");
            Assert.That(state.HasEvidence("E03"), Is.False);
            Read(state, "D02");
        }

        private static void ReadyRoute(CaseState state, ResponseRoute route, bool renAccountFirst)
        {
            if (route != ResponseRoute.PrivateHeadset) return;
            Read(state, renAccountFirst ? "C02" : "T03");
            Assert.That(state.HasEvidence("E06"), Is.False);
            Read(state, renAccountFirst ? "T03" : "C02");
            state.InspectPrivateRoute("inspect-connection");
            Assert.That(state.Authorization, Is.EqualTo(ContactAuthorization.RefusalAuthorized));
            TestContext.WriteLine("Both repair accounts complete E06. Inspect diagram and actual private connection; no new consent yet.");
            state.RecordPrivateContactAgreement("ren-later-agreement");
            TestContext.WriteLine("Ren explicitly agrees to private contact in the later conversation.");
        }

        private static void Finish(CaseState state, ResponseRoute route)
        {
            var known = state.AcquiredSourceIds.ToArray();
            state.PrepareDraft(route);
            var summary = state.PresentSendSummary();
            Assert.That(summary.CanSend, Is.True, summary.BlockingReason);
            TestContext.WriteLine("Present " + summary.OutcomeId + ": " + summary.Text);
            TestContext.WriteLine("Source " + summary.SourceId + "; recipients: " + string.Join(", ", summary.Recipients));
            if (summary.Warning != null) TestContext.WriteLine("Acknowledge warning: " + summary.Warning);
            Assert.That(state.ConfirmSend("confirm", summary, true, route == ResponseRoute.RoomSpeaker), Is.True);
            var sent = state.Disclosures.Single(d => d.EventId == "confirm");
            var expectedOutcome = route == ResponseRoute.PrivateHeadset ? "R1" : route == ResponseRoute.RefusalRelay ? "R2" : "R3";
            Assert.That(state.OutcomeId, Is.EqualTo(expectedOutcome));
            Assert.That(state.Phase, Is.EqualTo(CasePhase.Resolved));
            var expectedRecipients = route == ResponseRoute.PrivateHeadset ? new[] { "ren", "ada" } :
                route == ResponseRoute.RefusalRelay ? new[] { "ada" } : new[] { "ren", "ivo", "player" };
            Assert.That(sent.Recipients, Is.EquivalentTo(expectedRecipients));
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(known), "Sending cannot expose private message contents.");
            if (route == ResponseRoute.RefusalRelay)
            {
                Assert.That(sent.Text, Is.EqualTo("Ren declines immediate contact."));
                Assert.That(sent.MaterialId, Is.EqualTo("authorized-refusal"));
            }
            if (route == ResponseRoute.PrivateHeadset)
                Assert.That(sent.MaterialId, Is.EqualTo("private-connection"));
            var resolved = Facts(state);
            Assert.That(state.ConfirmSend("confirm", summary, true, true), Is.False);
            Assert.That(Facts(state), Is.EqualTo(resolved));
            Assert.That(state.CompleteAftermath("aftermath"), Is.True);
            var final = Facts(state);
            Assert.That(state.CompleteAftermath("aftermath"), Is.False);
            Assert.That(state.CompleteAftermath("revisit-aftermath"), Is.False);
            Assert.That(state.ConfirmSend("confirm", summary, true, true), Is.False);
            foreach (var source in state.AcquiredSourceIds) state.ReplaySource(source);
            Assert.That(Facts(state), Is.EqualTo(final));
            Assert.That(state.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            TestContext.WriteLine("Final facts: " + final);
            TestContext.WriteLine("Aftermath phase recorded once. The matching household scene is not implemented by this rehearsal.");
        }

        [TestCase(ResponseRoute.PrivateHeadset, true)]
        [TestCase(ResponseRoute.PrivateHeadset, false)]
        [TestCase(ResponseRoute.RefusalRelay, true)]
        [TestCase(ResponseRoute.RefusalRelay, false)]
        [TestCase(ResponseRoute.RoomSpeaker, true)]
        [TestCase(ResponseRoute.RoomSpeaker, false)]
        public void EachOutcomeSurvivesBothDiscoveryOrdersAndReplay(ResponseRoute route, bool archiveFirst)
        {
            TestContext.WriteLine(archiveFirst ? "Archive-first route" : "Conversation-first route");
            var state = Start();
            if (archiveFirst)
            {
                Archive(state);
                Assert.That(state.IdentityConfirmed, Is.False, "E03 remains a hypothesis.");
                AskRen(state);
            }
            else
            {
                Assert.That(state.MissingSources("E04"), Is.EqualTo(new[] { "C01" }));
                AskRen(state);
                Archive(state);
            }
            Read(state, "D03");
            ReadyRoute(state, route, !archiveFirst);
            Finish(state, route);
        }

        [TestCase(ResponseRoute.PrivateHeadset)]
        [TestCase(ResponseRoute.RefusalRelay)]
        [TestCase(ResponseRoute.RoomSpeaker)]
        public void SkipOptionalContentCancelDeferAndResumeStillReachEveryOutcome(ResponseRoute route)
        {
            var state = Start();
            state.PrepareDraft(ResponseRoute.RefusalRelay);
            var blocked = state.PresentSendSummary();
            Assert.That(blocked.CanSend, Is.False);
            state.CancelConfirmation();
            state.Defer();
            TestContext.WriteLine("Skip E02/E03/E05. Cancel pre-identity draft and defer; private Ren question remains available.");
            Assert.That(state.MissingSources("E04"), Is.EqualTo(new[] { "C01" }));
            state.Resume();
            AskRen(state);
            var summary = state.PresentSendSummary();
            Assert.That(summary.CanSend, Is.True, "Authorized refusal must remain a completion route.");
            state.CancelConfirmation();
            state.Defer();
            Assert.That(state.OutcomeId, Is.Null);
            Assert.That(state.DraftRoute, Is.EqualTo(ResponseRoute.RefusalRelay));
            Assert.That(state.SendBlockingReason(ResponseRoute.RefusalRelay), Does.Contain("Resume"));
            Assert.That(state.MissingSources("E06"), Is.EquivalentTo(new[] { "T03", "C02" }));
            state.Resume();
            TestContext.WriteLine("Resume with draft, refusal authorization, and both follow-up leads intact.");
            ReadyRoute(state, route, false);
            Finish(state, route);
            Assert.That(state.HasEvidence("E02"), Is.False);
            Assert.That(state.HasEvidence("E03"), Is.False);
            Assert.That(state.HasEvidence("E05"), Is.False);
            if (route == ResponseRoute.RefusalRelay)
            {
                Assert.That(state.HasEvidence("E06"), Is.False);
                Assert.That(state.OutcomeId, Is.EqualTo("R2"));
                Assert.That(state.Phase, Is.EqualTo(CasePhase.AftermathCompleted));
            }
        }

        [TestCase("T03", "C02")]
        [TestCase("C02", "T03")]
        public void PartialCorroborationKeepsRefusalAndTheMissingAccountRecoverable(string first, string second)
        {
            var state = Start();
            AskRen(state);
            Read(state, first);
            state.InspectPrivateRoute("inspect");
            Assert.Throws<InvalidOperationException>(() => state.RecordPrivateContactAgreement("agreement"));
            Assert.That(state.MissingSources("E06"), Is.EqualTo(new[] { second }));
            Assert.That(state.SendBlockingReason(ResponseRoute.RefusalRelay), Is.Null);
            TestContext.WriteLine("Only one repair account: private contact blocked; refusal and the other account remain available.");
            state.Defer();
            state.Resume();
            Read(state, second);
            Assert.That(state.RecordPrivateContactAgreement("agreement"), Is.True);
            Finish(state, ResponseRoute.PrivateHeadset);
        }
    }
}
