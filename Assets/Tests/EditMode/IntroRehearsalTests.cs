using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using NUnit.Framework;

namespace CommonHouse.Tests
{
    public class IntroRehearsalTests
    {
        private static IntroState Start() => new IntroState(IntroContent.Load());
        private static string Facts(IntroState state) => string.Join("|", new[] {
            string.Join(",", state.ReviewedLineIds), string.Join(",", state.AcquiredSourceIds),
            string.Join(",", state.AcknowledgedSceneIds), string.Join(",", state.Expressions.Select(item => item.Key + ":" + item.Value)),
            state.PreferredAddress ?? "blank", state.IsComplete.ToString()
        });
        private static void Read(IntroState state, string sceneId, params string[] options)
        {
            var review = state.OpenScene(sceneId);
            foreach (var option in options) review = state.SelectOption(review, option.Substring(0, 6), option);
            foreach (var line in review.Lines) TestContext.WriteLine(line.Id + " [" + line.SpeakerName + "]: " + line.Text);
            foreach (var source in review.Sources) TestContext.WriteLine(source.Id + " (" + source.Delivery + "): " + source.Description);
            state.Acknowledge("read-" + sceneId, review);
        }
        private static void Orient(IntroState state)
        {
            var summary = state.OpenOrientation();
            foreach (var line in summary.Lines) TestContext.WriteLine("Orientation [" + line.SpeakerName + "]: " + line.Text);
            state.Acknowledge("orientation", summary);
        }

        [TestCase("IN-Q03-A")]
        [TestCase("IN-Q03-B")]
        public void FullOrientationReachesBothClosingsWithoutStartingACase(string finalOption)
        {
            var baseline = new CaseState(CaseContent.LoadBaseline());
            var state = Start();
            Read(state, "IN-S00");
            Read(state, "IN-S01", "IN-Q01-D");
            Read(state, "IN-S02", "IN-Q05-A", "IN-Q04-A", "IN-Q04-B", "IN-Q04-C");
            Read(state, "IN-S03", "IN-Q06-B", "IN-Q07-A", "IN-Q07-B");
            Read(state, "IN-S04", "IN-Q08-A", "IN-Q08-B");
            Read(state, "IN-S05", "IN-Q02-A");
            Read(state, "IN-S06");
            Assert.That(state.MissingCoreAcknowledgments, Is.Empty);
            Assert.That(state.IsComplete, Is.False);
            Read(state, "IN-S07", finalOption);
            Assert.That(state.IsComplete, Is.True);
            Assert.That(state.AcknowledgedSceneIds.Count, Is.EqualTo(8));
            Assert.That(state.AcquiredSourceIds.Count, Is.EqualTo(10));
            Assert.That(state.PreferredAddress, Is.Null);
            Assert.That(baseline.Phase, Is.EqualTo(CasePhase.NotStarted));
            Assert.That(baseline.AcquiredSourceIds, Is.Empty);
            Assert.That(baseline.Disclosures, Is.Empty);
            Assert.That(baseline.OutcomeId, Is.Null);
        }

        [TestCase("IN-Q02-A")]
        [TestCase("IN-Q02-B")]
        [TestCase("IN-Q02-C")]
        public void EveryShelterResponsePreservesHospitality(string option)
        {
            var state = Start();
            Read(state, "IN-S05", option);
            Assert.That(state.ShelterAcknowledged, Is.True);
            Assert.That(state.Expressions["IN-Q02"], Is.EqualTo(option));
            Assert.That(state.PreferredAddress, Is.Null);
            Assert.That(state.IsComplete, Is.False);
        }

        [Test]
        public void PrivacyAndMinimalQuestionsFinishThroughReadableOrientation()
        {
            var state = Start();
            Read(state, "IN-S01", "IN-Q01-E");
            Read(state, "IN-S03", "IN-Q06-B");
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "IN-D01" }));
            Assert.That(state.ReviewedLineIds, Does.Not.Contain("IN-L048"));
            Assert.That(state.IsComplete, Is.False);
            Orient(state);
            Assert.That(state.AcquiredSourceIds, Is.EquivalentTo(new[] { "IN-C01", "IN-C02", "IN-C03", "IN-C04", "IN-D01", "IN-T05" }));
            Assert.That(state.MissingCoreAcknowledgments, Is.Empty);
            Read(state, "IN-S07", "IN-Q03-B");
            Assert.That(state.IsComplete, Is.True);
            Assert.That(state.PreferredAddress, Is.Null);
            Assert.That(state.Expressions.ContainsKey("IN-Q07"), Is.False);
            Assert.That(state.Expressions.ContainsKey("IN-Q08"), Is.False);
            Assert.That(state.AcquiredSourceIds.Any(id => new[] { "IN-T01", "IN-T02", "IN-T03", "IN-T04" }.Contains(id)), Is.False,
                "Completion must not retroactively acquire skipped broadcasts.");
        }

        [Test]
        public void FlexibleRoomOrderKeepsMissingCoreRecoverableAndFailedFinishAtomic()
        {
            var state = Start();
            Read(state, "IN-S05", "IN-Q02-C");
            Read(state, "IN-S06");
            var final = state.SelectOption(state.OpenScene("IN-S07"), "IN-Q03", "IN-Q03-A");
            var before = Facts(state);
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("finish", final));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.ActiveReview, Is.SameAs(final));
            Assert.That(state.MissingCoreAcknowledgments, Is.EqualTo(new[] { "warning", "network" }));
            state.Close(final);
            Orient(state);
            final = state.SelectOption(state.OpenScene("IN-S07"), "IN-Q03", "IN-Q03-A");
            Assert.That(state.Acknowledge("finish", final), Is.True, "Rejected event IDs remain available for a corrected review.");
            Assert.That(state.IsComplete, Is.True);
        }

        [Test]
        public void PauseSkipCloseAndReopenNeverFabricateLearning()
        {
            var state = Start();
            var before = Facts(state);
            var greeting = state.OpenScene("IN-S06");
            state.Pause(greeting);
            state.SkipToTranscript(greeting);
            Assert.That(state.IsPaused, Is.True);
            Assert.That(state.TranscriptOnly, Is.True);
            Assert.That(greeting.Lines.Select(line => line.Text), Does.Contain("This is the local service office."));
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("greeting", greeting));
            Assert.That(Facts(state), Is.EqualTo(before));
            state.Close(greeting);
            Assert.That(state.GreetingReviewed, Is.False);
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("greeting", greeting));
            greeting = state.OpenScene("IN-S06");
            Assert.That(state.IsPaused, Is.False);
            Assert.That(state.TranscriptOnly, Is.False);
            state.Pause(greeting);
            state.Resume(greeting);
            state.SkipToTranscript(greeting);
            Assert.That(state.Acknowledge("greeting", greeting), Is.True);
            Assert.That(state.GreetingReviewed, Is.True);
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "IN-C04", "IN-T05" }));
            Assert.That(state.IsComplete, Is.False);
        }

        [Test]
        public void OptionalAnswersAcquireOnlyACompletePresentedAccount()
        {
            var state = Start();
            var review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q07", "IN-Q07-A");
            Assert.That(review.Lines.Select(line => line.Id), Does.Contain("IN-L048").And.Not.Contain("IN-L050"));
            Assert.That(state.AcquiredSourceIds, Is.Empty);
            state.Acknowledge("first-answer", review);
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "IN-D01" }));
            Assert.Throws<InvalidOperationException>(() => state.ReplaySource("IN-C02"));
            review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q07", "IN-Q07-B");
            state.Close(review);
            Assert.That(state.AcquiredSourceIds, Does.Not.Contain("IN-C02"));
            review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q07", "IN-Q07-B");
            state.Acknowledge("second-answer", review);
            Assert.That(state.AcquiredSourceIds, Does.Contain("IN-C02"));
            Assert.That(state.ReplaySource("IN-C02").LineIds, Is.EqualTo(new[] { "IN-L048", "IN-L050" }));
        }

        [Test]
        public void UnchosenExpressionLinesStayOutOfPresentedTextAndSelectionIsPending()
        {
            var state = Start();
            var initial = state.OpenScene("IN-S01");
            Assert.That(initial.Lines.Select(line => line.Id), Is.EqualTo(new[] { "IN-L010", "IN-L011" }));
            Assert.That(initial.Choices.Single().Options.Select(option => option.Text), Does.Contain("I don't know how I got here."));
            var chosen = state.SelectOption(initial, "IN-Q01", "IN-Q01-C");
            Assert.That(chosen.Lines.Select(line => line.Id), Does.Contain("IN-L014").And.Not.Contain("IN-L012"));
            Assert.That(state.Expressions, Is.Empty);
            Assert.That(state.ReviewedLineIds, Is.Empty);
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("stale", initial));
            state.Close(chosen);
            Assert.That(state.Expressions, Is.Empty);
        }

        [Test]
        public void PreferredAddressRequiresExplicitSelectionAndAcknowledgmentAndCanRemainBlank()
        {
            var state = Start();
            var review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q06", "IN-Q06-A");
            state.Acknowledge("undecided", review);
            Assert.That(state.PreferredAddress, Is.Null);
            review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q06", "IN-Q06-A", "  Moss  ");
            Assert.That(state.PreferredAddress, Is.Null);
            state.Close(review);
            Assert.That(state.PreferredAddress, Is.Null);
            review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q06", "IN-Q06-A", "Moss");
            state.Acknowledge("address", review);
            Assert.That(state.PreferredAddress, Is.EqualTo("Moss"));
            Assert.That(IntroContent.Load().Lines.Any(line => line.Text.Contains("Moss")), Is.False);
            review = state.SelectOption(state.OpenScene("IN-S03"), "IN-Q06", "IN-Q06-B");
            state.Acknowledge("blank", review);
            Assert.That(state.PreferredAddress, Is.Null);
        }

        [TestCase(" ")]
        [TestCase("Moss\nSmith")]
        public void RejectsUnusableAddressWithoutChangingTheReview(string address)
        {
            var state = Start();
            var review = state.OpenScene("IN-S03");
            Assert.Throws<ArgumentException>(() => state.SelectOption(review, "IN-Q06", "IN-Q06-A", address));
            Assert.That(state.ActiveReview, Is.SameAs(review));
            Assert.That(state.PreferredAddress, Is.Null);
            Assert.That(state.Expressions, Is.Empty);
        }

        [Test]
        public void AcknowledgmentRetriesAndReplayPreserveFactsBeforeAndAfterCompletion()
        {
            var state = Start();
            var review = state.OpenSource("IN-T05");
            Assert.That(state.Acknowledge("recording", review), Is.True);
            var before = Facts(state);
            Assert.That(state.Acknowledge("recording", review), Is.False);
            Assert.That(state.ReplaySource("IN-T05").Delivery, Is.EqualTo("recorded"));
            Assert.That(state.ReplaySource("IN-T05").RecordedAt, Is.Null.Or.Empty);
            Assert.That(Facts(state), Is.EqualTo(before));
            var replay = state.OpenSource("IN-T05");
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("recording", replay));
            Assert.That(state.Acknowledge("replay", replay), Is.False);
            Assert.That(Facts(state), Is.EqualTo(before));
            Orient(state);
            Read(state, "IN-S07", "IN-Q03-A");
            before = Facts(state);
            var finalReplay = state.SelectOption(state.OpenScene("IN-S07"), "IN-Q03", "IN-Q03-A");
            Assert.That(state.Acknowledge("repeat-finish", finalReplay), Is.False);
            replay = state.OpenSource("IN-T05");
            Assert.That(state.Acknowledge("post-completion-replay", replay), Is.False);
            Assert.That(Facts(state), Is.EqualTo(before));
        }

        [Test]
        public void CompletionRequiresEachCoreAcknowledgmentAndAnExplicitFinalResponse()
        {
            var state = Start();
            Assert.That(state.MissingCoreAcknowledgments, Is.EqualTo(new[] { "warning", "network", "shelter", "greeting" }));
            var final = state.SelectOption(state.OpenScene("IN-S07"), "IN-Q03", "IN-Q03-B");
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("finish", final));
            state.Close(final);
            foreach (var source in new[] { "IN-C01", "IN-C03", "IN-T05" })
                state.Acknowledge("core-" + source, state.OpenSource(source));
            Assert.That(state.MissingCoreAcknowledgments, Is.EqualTo(new[] { "shelter" }));
            Read(state, "IN-S05");
            Assert.That(state.MissingCoreAcknowledgments, Is.Empty);
            final = state.OpenScene("IN-S07");
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("finish", final));
            Assert.That(state.IsComplete, Is.False);
            final = state.SelectOption(final, "IN-Q03", "IN-Q03-B");
            Assert.That(state.Acknowledge("finish", final), Is.True);
            Assert.That(state.IsComplete, Is.True);
        }

        [TestCase("warning", "IN-C01")]
        [TestCase("network", "IN-C03")]
        [TestCase("greeting", "IN-T05")]
        [TestCase("shelter", null)]
        public void EachMissingCoreIndependentlyBlocksCompletion(string missing, string omittedSource)
        {
            var state = Start();
            foreach (var source in new[] { "IN-C01", "IN-C03", "IN-T05" }.Where(id => id != omittedSource))
                state.Acknowledge("core-" + source, state.OpenSource(source));
            if (missing != "shelter") Read(state, "IN-S05");
            Assert.That(state.MissingCoreAcknowledgments, Is.EqualTo(new[] { missing }));
            var final = state.SelectOption(state.OpenScene("IN-S07"), "IN-Q03", "IN-Q03-A");
            var before = Facts(state);
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("finish", final));
            Assert.That(Facts(state), Is.EqualTo(before));
            Assert.That(state.IsComplete, Is.False);
        }

        [Test]
        public void OldAcknowledgmentRetryCannotCloseOrCommitANewPendingReview()
        {
            var state = Start();
            var old = state.OpenSource("IN-C01");
            state.Acknowledge("warning", old);
            var pending = state.OpenSource("IN-T05");
            Assert.That(state.Acknowledge("warning", old), Is.False);
            Assert.That(state.ActiveReview, Is.SameAs(pending));
            Assert.That(state.GreetingReviewed, Is.False);
            Assert.That(state.AcquiredSourceIds, Is.EqualTo(new[] { "IN-C01" }));
        }

        [Test]
        public void ReviewsAreLabelledReadonlyAndRetainMicrophoneAndUnknownOriginDescriptions()
        {
            var state = Start();
            var review = state.OpenScene("IN-S04");
            Assert.That(review.Lines.All(line => !string.IsNullOrWhiteSpace(line.SpeakerName) && !string.IsNullOrWhiteSpace(line.Text)), Is.True);
            Assert.That(review.Sources.Single(source => source.Id == "IN-T04").Description, Does.Contain("microphone"));
            Assert.Throws<NotSupportedException>(() => ((IList<IntroTranscriptLine>)review.Lines).Clear());
            Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)review.SelectedOptions).Add("IN-Q01", "IN-Q01-A"));
            state.Close(review);
            review = state.OpenOrientation();
            Assert.That(review.Sources.Single(source => source.Id == "IN-T05").Description, Does.Contain("no identifiable sender"));
            Assert.That(review.Description, Does.Contain("sender and date remain unknown"));
            Assert.That(state.AcquiredSourceIds, Is.Empty);
        }

        [Test]
        public void InvalidAndForeignActionsLeaveFactsAndActiveReviewUnchanged()
        {
            var state = Start();
            Assert.Throws<ArgumentException>(() => state.OpenScene("IN-S99"));
            Assert.Throws<ArgumentException>(() => state.OpenSource("IN-T99"));
            Assert.Throws<InvalidOperationException>(() => state.ReplaySource("IN-T05"));
            var review = state.OpenScene("IN-S03");
            var before = Facts(state);
            Assert.Throws<InvalidOperationException>(() => state.OpenOrientation());
            Assert.Throws<ArgumentException>(() => state.SelectOption(review, "IN-Q01", "IN-Q01-A"));
            Assert.Throws<ArgumentException>(() => state.SelectOption(review, "IN-Q06", "IN-Q06-Z"));
            Assert.Throws<ArgumentException>(() => state.SelectOption(review, "IN-Q06", "IN-Q06-B", "Moss"));
            Assert.Throws<ArgumentException>(() => state.Acknowledge("", review));
            var foreign = Start().OpenScene("IN-S03");
            Assert.Throws<InvalidOperationException>(() => state.Acknowledge("foreign", foreign));
            Assert.Throws<InvalidOperationException>(() => state.Close(foreign));
            state.Pause(review);
            Assert.Throws<InvalidOperationException>(() => state.SelectOption(review, "IN-Q06", "IN-Q06-B"));
            state.Resume(review);
            Assert.That(state.ActiveReview, Is.SameAs(review));
            Assert.That(Facts(state), Is.EqualTo(before));
        }
    }
}
