using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonHouse.Content
{
    // Authored data only. Acquisition, consent and resolution belong to player state.
    [Serializable]
    public sealed class ContentSource
    {
        [SerializeField] private string id, kind, speakerId, text, caption, recoveryId, recordedAt;
        public string Id => id;
        public string Kind => kind;
        public string SpeakerId => speakerId;
        public string Text => text;
        public string Caption => caption;
        public string RecoveryId => recoveryId;
        // Optional authored metadata. An absent value is intentionally distinct
        // from an invented timestamp in the notebook presentation.
        public string RecordedAt => recordedAt;
    }

    [Serializable]
    public sealed class ContentStatement
    {
        [SerializeField] private string kind, text, attributedTo;
        [SerializeField] private string[] sourceIds;
        public string Kind => kind;
        public string Text => text;
        public string AttributedTo => attributedTo;
        public IReadOnlyList<string> SourceIds => Array.AsReadOnly(sourceIds ?? Array.Empty<string>());
    }

    [Serializable]
    public sealed class EvidenceContent
    {
        [SerializeField] private string id;
        [SerializeField] private string[] sourceIds;
        [SerializeField] private ContentStatement[] statements;
        public string Id => id;
        // Every source is required for a complete record; partial collection is player state.
        public IReadOnlyList<string> SourceIds => Array.AsReadOnly(sourceIds ?? Array.Empty<string>());
        public IReadOnlyList<ContentStatement> Statements => Array.AsReadOnly(statements ?? Array.Empty<ContentStatement>());
    }

    [Serializable]
    public sealed class NamedContent
    {
        [SerializeField] private string id, text;
        public string Id => id;
        public string Text => text;
    }

    [Serializable]
    public sealed class CaseContent
    {
        [SerializeField] private string id;
        [SerializeField] private NamedContent[] residents, callers, recoveries, outcomes;
        [SerializeField] private ContentSource[] sources;
        [SerializeField] private EvidenceContent[] evidence;
        public string Id => id;
        public IReadOnlyList<NamedContent> Residents => Array.AsReadOnly(residents);
        public IReadOnlyList<NamedContent> Callers => Array.AsReadOnly(callers);
        public IReadOnlyList<NamedContent> Recoveries => Array.AsReadOnly(recoveries);
        public IReadOnlyList<NamedContent> Outcomes => Array.AsReadOnly(outcomes);
        public IReadOnlyList<ContentSource> Sources => Array.AsReadOnly(sources);
        public IReadOnlyList<EvidenceContent> Evidence => Array.AsReadOnly(evidence);

        public static CaseContent LoadBaseline()
        {
            var asset = Resources.Load<TextAsset>("Case01");
            if (asset == null) throw new InvalidOperationException("Missing Case01 text asset.");
            return FromJson(asset.text);
        }

        public static CaseContent FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Empty case content.");
            var content = JsonUtility.FromJson<CaseContent>(json);
            if (content == null) throw new ArgumentException("Missing case content.");
            content.Validate();
            return content;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new ArgumentException(message);
        }

        private static HashSet<string> Ids<T>(T[] records, Func<T, string> key, string label) where T : class
        {
            Require(records != null && records.Length > 0, "Missing " + label);
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in records)
                Require(record != null && !string.IsNullOrWhiteSpace(key(record)) && ids.Add(key(record)), "Invalid or duplicate " + label + " ID");
            return ids;
        }

        private static HashSet<string> NamedIds(NamedContent[] records, string label)
        {
            var ids = Ids(records, item => item.Id, label);
            Require(records.All(item => !string.IsNullOrWhiteSpace(item.Text)), "Missing " + label + " text");
            return ids;
        }

        private void Validate()
        {
            Require(id == "case-01", "Expected case-01.");
            var residentIds = NamedIds(residents, "resident");
            var callerIds = NamedIds(callers, "caller");
            Require(residentIds.SetEquals(new[] { "ren", "ivo" }) && callerIds.SetEquals(new[] { "ada" }), "Unexpected baseline cast.");
            var people = new HashSet<string>(residentIds, StringComparer.Ordinal);
            people.UnionWith(callerIds);
            var recoveryIds = NamedIds(recoveries, "recovery");
            var outcomeIds = NamedIds(outcomes, "outcome");
            Require(outcomeIds.SetEquals(new[] { "R1", "R2", "R3" }), "Expected exactly R1-R3.");
            var sourceIds = Ids(sources, item => item.Id, "source/transmission");
            foreach (var source in sources)
            {
                Require(new[] { "transmission", "conversation", "document" }.Contains(source.Kind), "Unknown source kind.");
                Require(!string.IsNullOrWhiteSpace(source.Text), "Missing transcript or document text: " + source.Id);
                Require(source.RecoveryId != null && recoveryIds.Contains(source.RecoveryId), "Dangling recovery: " + source.Id);
                if (source.Kind != "document")
                    Require(source.SpeakerId != null && people.Contains(source.SpeakerId), "Missing or unknown speaker: " + source.Id);
                else
                    Require(string.IsNullOrEmpty(source.SpeakerId) || people.Contains(source.SpeakerId), "Unknown document attribution.");
            }
            var evidenceIds = Ids(evidence, item => item.Id, "evidence");
            Require(evidenceIds.SetEquals(new[] { "E01", "E02", "E03", "E04", "E05", "E06" }), "Expected exactly E01-E06.");
            foreach (var record in evidence)
            {
                Require(record.SourceIds.Count > 0 && record.SourceIds.Distinct().Count() == record.SourceIds.Count, "Missing or duplicate evidence sources.");
                Require(record.SourceIds.All(source => source != null && sourceIds.Contains(source)), "Dangling evidence source.");
                Require(record.Statements.Count > 0, "Missing evidence statements.");
                foreach (var statement in record.Statements)
                {
                    Require(statement != null, "Null statement.");
                    Require(new[] { "observation", "attributed-claim", "hypothesis", "corroboration" }.Contains(statement.Kind), "Unknown statement kind.");
                    Require(!string.IsNullOrWhiteSpace(statement.Text), "Missing statement text.");
                    Require(statement.SourceIds.Count > 0 && statement.SourceIds.Distinct().Count() == statement.SourceIds.Count && statement.SourceIds.All(source => record.SourceIds.Contains(source)), "Dangling or duplicate statement source.");
                    Require(string.IsNullOrEmpty(statement.AttributedTo) || people.Contains(statement.AttributedTo), "Unknown statement attribution.");
                    if (statement.Kind == "attributed-claim")
                        Require(!string.IsNullOrEmpty(statement.AttributedTo) && statement.SourceIds.Any(source => sources.Single(item => item.Id == source).SpeakerId == statement.AttributedTo), "Claim requires its source speaker.");
                }
            }
            // These composite/accessibility requirements are specific to the six-record baseline.
            Require(new HashSet<string>(evidence.Single(item => item.Id == "E03").SourceIds).SetEquals(new[] { "D01", "D02" }), "E03 needs both documents.");
            Require(new HashSet<string>(evidence.Single(item => item.Id == "E06").SourceIds).SetEquals(new[] { "T03", "C02" }), "E06 needs both accounts.");
            var tape = sources.SingleOrDefault(item => item.Id == "T02");
            Require(tape != null && !string.IsNullOrWhiteSpace(tape.Caption), "E02 requires the bell caption.");
            Require(evidence.Single(item => item.Id == "E02").SourceIds.Contains("T02"), "E02 must reference the captioned tape.");
        }
    }
}
