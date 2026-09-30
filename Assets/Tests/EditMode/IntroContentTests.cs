using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class IntroContentTests
    {
        private static string Json => Resources.Load<TextAsset>("Intro").text;

        [Test]
        public void LoadsTheCompleteIntroIndependentlyOfTheBaseline()
        {
            var intro = IntroContent.Load();
            Assert.That(intro.Id, Is.EqualTo("intro"));
            Assert.That(intro.Version, Is.EqualTo(1));
            Assert.That(intro.Scenes.Select(scene => scene.Id), Is.EqualTo(Enumerable.Range(0, 8).Select(number => "IN-S" + number.ToString("D2"))));
            Assert.That(intro.Lines.Count, Is.EqualTo(66));
            Assert.That(intro.Sources.Count, Is.EqualTo(10));
            Assert.That(intro.Choices.Count, Is.EqualTo(8));
            var baseline = CaseContent.LoadBaseline();
            Assert.That(baseline.Id, Is.EqualTo("case-01"));
            Assert.That(baseline.Evidence.Count, Is.EqualTo(6));
            Assert.That(baseline.Outcomes.Select(outcome => outcome.Id), Is.EquivalentTo(new[] { "R1", "R2", "R3" }));
        }

        [Test]
        public void SourcesProvideAttributionAndDescriptionsWithoutMediaAssets()
        {
            var intro = IntroContent.Load();
            foreach (var source in intro.Sources)
            {
                Assert.That(source.Description, Is.Not.Empty);
                if (source.Kind == "document") continue;
                Assert.That(source.LineIds, Is.Not.Empty);
                foreach (var lineId in source.LineIds)
                {
                    var line = intro.Lines.Single(item => item.Id == lineId);
                    Assert.That(line.Text, Is.Not.Empty);
                    Assert.That(intro.Speakers.Any(item => item.Id == line.SpeakerId), Is.True);
                }
            }
            var greeting = intro.Sources.Single(source => source.Id == "IN-T05");
            Assert.That(greeting.Delivery, Is.EqualTo("recorded"));
            Assert.That(greeting.RecordedAt, Is.Null.Or.Empty, "An unknown timestamp stays unknown.");
            Assert.That(greeting.Description, Does.Contain("no identifiable sender"));
            Assert.That(intro.Lines.Single(line => line.Id == "IN-L101").Text, Is.EqualTo("This is the local service office."));
            Assert.That(intro.Sources.Single(source => source.Id == "IN-T04").Description, Does.Contain("microphone"));
        }

        [Test]
        public void ExpressionAlternativesRetainShelterAndDoNotAssignABiography()
        {
            var intro = IntroContent.Load();
            var arrival = intro.Choices.Single(choice => choice.Id == "IN-Q01");
            Assert.That(arrival.Options.Select(option => option.Text), Does.Contain("Can I come inside?"));
            Assert.That(arrival.Options.Single(option => option.Id == "IN-Q01-E").NextSceneId, Is.EqualTo("IN-S01"));
            Assert.That(intro.Choices.Single(choice => choice.Id == "IN-Q06").Options.Select(option => option.Text), Does.Contain("Leave it blank for now."));
            Assert.That(intro.Choices.Single(choice => choice.Id == "IN-Q02").Options.All(option => option.NextSceneId == "IN-S06"), Is.True);
            Assert.That(intro.Choices.Single(choice => choice.Id == "IN-Q03").Options.All(option => option.ResponseLineIds.SequenceEqual(new[] { "IN-L113" })), Is.True);
        }

        [TestCase("\"id\": \"IN-L001\"", "\"id\": \"IN-L002\"")]
        [TestCase("\"id\": \"IN-L001\"", "\"id\": \"bad-line\"")]
        [TestCase("\"id\": \"IN-T05\"", "\"id\": \"IN-T04\"")]
        [TestCase("\"id\": \"IN-Q08\"", "\"id\": \"IN-Q07\"")]
        [TestCase("\"id\": \"IN-Q01-A\"", "\"id\": \"IN-Q01-B\"")]
        [TestCase("\"id\": \"IN-S07\"", "\"id\": \"IN-S08\"")]
        [TestCase("\"speakerId\": \"ren\"", "\"speakerId\": \"unknown\"")]
        [TestCase("\"name\": \"Ren\"", "\"name\": \"\"")]
        [TestCase("\"text\": \"There's someone by the steps.\"", "\"text\": \"\"")]
        [TestCase("\"responseLineIds\":", "\"missingResponseLineIds\":")]
        [TestCase("\"responseLineIds\": [\n            \"IN-L012\"", "\"responseLineIds\": [\n            \"IN-L999\"")]
        [TestCase("\"sourceIds\": [\n        \"IN-C01\"", "\"sourceIds\": [\n        \"IN-C99\"")]
        [TestCase("\"choiceIds\": [\n        \"IN-Q01\"", "\"choiceIds\": [\n        \"IN-Q99\"")]
        [TestCase("\"lineIds\":", "\"missingLineIds\":")]
        [TestCase("\"choiceIds\":", "\"missingChoiceIds\":")]
        [TestCase("\"sourceIds\":", "\"missingSourceIds\":")]
        [TestCase("\"nextSceneId\": \"IN-S02\"", "\"nextSceneId\": \"IN-S99\"")]
        [TestCase("\"delivery\": \"recorded\"", "\"delivery\": \"live\"")]
        [TestCase("\"delivery\": \"account\"", "\"delivery\": \"recorded\"")]
        [TestCase("\"kind\": \"document\"", "\"kind\": \"unknown\"")]
        [TestCase("\"description\":", "\"missingDescription\":")]
        [TestCase("\"version\": 1", "\"version\": 2")]
        public void RejectsInvalidAuthoredContent(string before, string after)
        {
            Assert.That(Json, Does.Contain(before), "Mutation must affect the actual resource.");
            Assert.Throws<ArgumentException>(() => IntroContent.FromJson(Json.Replace(before, after)));
        }

        [TestCase("A fixed telephone greeting says the local service office is closed. It is stored playback, with no identifiable sender, verified date, or return connection.")]
        [TestCase("An old telecommunications service-area map carries handwritten station names and question marks. Earlier distance measurements are crossed out; it gives no dependable travel coordinates.")]
        public void RejectsMissingAccessibleEvidenceEvenWhenDialogueRemains(string description)
        {
            Assert.That(Json, Does.Contain(description));
            Assert.Throws<ArgumentException>(() => IntroContent.FromJson(Json.Replace(description, "")));
        }

        [TestCase("")]
        [TestCase("{}")]
        [TestCase("null")]
        public void RejectsMissingContent(string json)
        {
            Assert.Throws<ArgumentException>(() => IntroContent.FromJson(json));
        }

        [Test]
        public void ContentHasNoWritablePropertiesOrPlayerState()
        {
            var intro = IntroContent.Load();
            Assert.Throws<NotSupportedException>(() => ((IList<IntroLine>)intro.Lines).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)intro.Scenes[0].LineIds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<IntroOption>)intro.Choices[0].Options).Clear());
            foreach (var type in new[] { typeof(IntroContent), typeof(IntroLine), typeof(IntroSource), typeof(IntroScene), typeof(IntroChoice), typeof(IntroOption), typeof(IntroSpeaker) })
            {
                Assert.That(type.GetFields(), Is.Empty);
                Assert.That(type.GetProperties().Any(property => property.CanWrite), Is.False);
            }
            Assert.That(typeof(IntroContent).GetMethods().Any(method => method.Name.Contains("Acquire") || method.Name.Contains("Save") || method.Name.Contains("Confirm")), Is.False);
            Assert.That(IntroContent.Load().Lines.Count, Is.EqualTo(66));
        }
    }
}
