using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using NUnit.Framework;
using UnityEngine;

namespace CommonHouse.Tests
{
    public class CaseContentTests
    {
        private static string Json => Resources.Load<TextAsset>("Case01").text;

        [Test]
        public void LoadsExactlyTheBaselineWithStableCastTransmissionAndOutcomeIds()
        {
            var content = CaseContent.LoadBaseline();
            Assert.That(content.Id, Is.EqualTo("case-01"));
            Assert.That(content.Evidence.Select(item => item.Id), Is.EquivalentTo(new[] { "E01", "E02", "E03", "E04", "E05", "E06" }));
            Assert.That(content.Outcomes.Select(item => item.Id), Is.EquivalentTo(new[] { "R1", "R2", "R3" }));
            Assert.That(content.Residents.Select(item => item.Id), Is.EquivalentTo(new[] { "ren", "ivo" }));
            Assert.That(content.Callers.Single().Id, Is.EqualTo("ada"));
            Assert.That(content.Sources.Where(item => item.Kind == "transmission").Select(item => item.Id), Is.EquivalentTo(new[] { "T01", "T02", "T03" }));
        }

        [Test]
        public void CompositeCluesRetainIndependentSourcesAndKnowledgeCategories()
        {
            var content = CaseContent.LoadBaseline();
            var hypothesis = content.Evidence.Single(item => item.Id == "E03");
            Assert.That(hypothesis.SourceIds, Is.EquivalentTo(new[] { "D01", "D02" }));
            Assert.That(hypothesis.Statements.Select(item => item.Kind), Is.EquivalentTo(new[] { "observation", "hypothesis" }));
            Assert.That(content.Evidence.Single(item => item.Id == "E04").Statements.Single().AttributedTo, Is.EqualTo("ren"));
            var repair = content.Evidence.Single(item => item.Id == "E06");
            Assert.That(repair.SourceIds, Is.EquivalentTo(new[] { "T03", "C02" }));
            Assert.That(repair.Statements.Where(item => item.Kind == "attributed-claim").Select(item => item.AttributedTo), Is.EquivalentTo(new[] { "ada", "ren" }));
            Assert.That(repair.Statements.Last().Kind, Is.EqualTo("corroboration"));
            Assert.That(repair.Statements.Last().Text, Does.Contain("no consent"));
        }

        [Test]
        public void AllSourcesHaveReadableTextAttributionAndRecoveryAndTapeHasCaption()
        {
            var content = CaseContent.LoadBaseline();
            foreach (var source in content.Sources)
            {
                Assert.That(source.Text, Is.Not.Empty);
                Assert.That(content.Recoveries.Any(item => item.Id == source.RecoveryId));
                if (source.Kind != "document") Assert.That(source.SpeakerId, Is.Not.Empty);
            }
            Assert.That(content.Sources.Single(item => item.Id == "T02").Caption, Does.Contain("bell"));
            Assert.That(content.Recoveries.Single(item => item.Id == "private-question").Text, Does.Contain("E03 is not required"));
        }

        [TestCase("\"id\": \"E06\"", "\"id\": \"E05\"")]
        [TestCase("\"id\": \"E06\"", "\"id\": \"E07\"")]
        [TestCase("\"id\": \"R3\"", "\"id\": \"R2\"")]
        [TestCase("\"id\": \"R3\"", "\"id\": \"R4\"")]
        [TestCase("\"id\": \"T03\"", "\"id\": \"T01\"")]
        [TestCase("\"id\": \"T03\"", "\"id\": \"missing-source\"")]
        [TestCase("\"id\": \"replay-call\"", "\"id\": \"missing-recovery\"")]
        [TestCase("\"speakerId\": \"ada\"", "\"speakerId\": \"unknown\"")]
        [TestCase("\"attributedTo\": \"ada\"", "\"attributedTo\": \"ivo\"")]
        [TestCase("\"id\": \"roster\"", "\"id\": \"replay-call\"")]
        [TestCase("\"sourceIds\":", "\"removedSources\":")]
        [TestCase("\"statements\":", "\"removedStatements\":")]
        [TestCase("\"D01\",\n        \"D02\"", "\"D01\",\n        \"D01\"")]
        [TestCase("\"T03\",\n        \"C02\"", "\"T03\"")]
        [TestCase("\"kind\": \"hypothesis\"", "\"kind\": \"truth\"")]
        [TestCase("[Kitchen bell sounds on the recording.]", "")]
        [TestCase("Ada: I am Ada. Please tell Mara that the road is clear. I can remain available for a later response.", "")]
        public void RejectsBrokenAuthoring(string before, string after)
        {
            Assert.That(Json, Does.Contain(before), "The mutation must affect the real asset.");
            Assert.Throws<ArgumentException>(() => CaseContent.FromJson(Json.Replace(before, after)));
        }

        [TestCase("")]
        [TestCase("{}")]
        [TestCase("null")]
        public void RejectsMissingContent(string json)
        {
            Assert.Throws<ArgumentException>(() => CaseContent.FromJson(json));
        }

        [Test]
        public void ContentExposesNoWritablePropertiesOrMutableCollections()
        {
            var content = CaseContent.LoadBaseline();
            Assert.Throws<NotSupportedException>(() => ((IList<EvidenceContent>)content.Evidence).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<string>)content.Evidence[0].SourceIds).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<ContentStatement>)content.Evidence[0].Statements).Clear());
            foreach (var type in new[] { typeof(CaseContent), typeof(EvidenceContent), typeof(ContentSource), typeof(ContentStatement), typeof(NamedContent) })
            {
                Assert.That(type.GetFields(), Is.Empty);
                Assert.That(type.GetProperties().Any(property => property.CanWrite), Is.False);
            }
            Assert.That(CaseContent.LoadBaseline().Evidence.Count, Is.EqualTo(6));
        }
    }
}
