using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;

namespace CommonHouse.Domain
{
    public enum CasePhase { NotStarted, Investigating, Resolved, AftermathCompleted }
    public enum CasePromise { ProtectPrivateContact }

    public sealed class Disclosure
    {
        public string EventId { get; }
        public string SourceId { get; }
        public string AttributedTo { get; }
        public IReadOnlyList<string> Recipients { get; }
        internal Disclosure(string eventId, ContentSource source, IEnumerable<string> recipients)
        {
            EventId = eventId;
            SourceId = source.Id;
            AttributedTo = source.SpeakerId;
            Recipients = Array.AsReadOnly(recipients.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        }
    }

    // In-memory facts only: no MonoBehaviour, IO, clock, randomness, or authored-asset mutation.
    public sealed class CaseState
    {
        private readonly CaseContent content;
        private readonly Dictionary<string, ContentSource> sources;
        private readonly HashSet<string> people;
        private readonly HashSet<string> acquired = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<CasePromise> promises = new HashSet<CasePromise>();
        private readonly Dictionary<string, string> events = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<Disclosure> disclosures = new List<Disclosure>();

        public CasePhase Phase { get; private set; }
        public string OutcomeId { get; private set; }
        public bool IdentityConfirmed => HasEvidence("E04");
        public IReadOnlyList<string> AcquiredSourceIds => Array.AsReadOnly(acquired.OrderBy(id => id, StringComparer.Ordinal).ToArray());
        public IReadOnlyList<string> DiscoveredEvidenceIds => Array.AsReadOnly(content.Evidence.Where(item => item.SourceIds.All(acquired.Contains)).Select(item => item.Id).OrderBy(id => id, StringComparer.Ordinal).ToArray());
        public IReadOnlyList<CasePromise> Promises => Array.AsReadOnly(promises.OrderBy(item => item).ToArray());
        public IReadOnlyList<Disclosure> Disclosures => disclosures.AsReadOnly();

        public CaseState(CaseContent content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            sources = content.Sources.ToDictionary(item => item.Id, StringComparer.Ordinal);
            people = new HashSet<string>(content.Residents.Concat(content.Callers).Select(item => item.Id), StringComparer.Ordinal) { "player" };
        }

        public bool HasEvidence(string evidenceId)
        {
            var record = content.Evidence.SingleOrDefault(item => item.Id == evidenceId);
            if (record == null) throw new ArgumentException("Unknown evidence ID.", nameof(evidenceId));
            return record.SourceIds.All(acquired.Contains);
        }

        public IReadOnlyList<string> MissingSources(string evidenceId)
        {
            var record = content.Evidence.SingleOrDefault(item => item.Id == evidenceId);
            if (record == null) throw new ArgumentException("Unknown evidence ID.", nameof(evidenceId));
            return Array.AsReadOnly(record.SourceIds.Where(id => !acquired.Contains(id)).ToArray());
        }

        private ContentSource Source(string sourceId)
        {
            if (sourceId == null || !sources.TryGetValue(sourceId, out var source))
                throw new ArgumentException("Unknown source ID.", nameof(sourceId));
            return source;
        }

        // Length prefixes avoid ambiguity even if a caller supplies separators in an event ID.
        private static string Payload(params string[] values) => string.Concat(values.Select(value => value.Length + ":" + value));

        private bool Apply(string eventId, string payload, Func<bool> action)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("An event ID is required.", nameof(eventId));
            if (events.TryGetValue(eventId, out var previous))
            {
                if (previous != payload) throw new InvalidOperationException("Event ID reused with a different payload.");
                return false;
            }
            // Validation in action must precede mutation; rejected events are not consumed.
            var changed = action();
            events.Add(eventId, payload);
            return changed;
        }

        public bool AcquireSource(string eventId, string sourceId)
        {
            var source = Source(sourceId);
            return Apply(eventId, Payload("acquire", sourceId), () =>
            {
                if (acquired.Contains(sourceId)) return false;
                if (OutcomeId != null) throw new InvalidOperationException("New investigation facts require an unresolved case.");
                acquired.Add(sourceId);
                // A spoken account discloses its attributed text to the player once.
                // Reading a document is acquisition, not an inferred conversation.
                if (source.Kind != "document") disclosures.Add(new Disclosure(eventId, source, new[] { "player" }));
                Phase = CasePhase.Investigating;
                return true;
            });
        }

        public ContentSource ReplaySource(string sourceId)
        {
            var source = Source(sourceId);
            if (!acquired.Contains(sourceId)) throw new InvalidOperationException("Source has not been acquired.");
            return source;
        }

        public bool RecordPromise(string eventId, CasePromise promise)
        {
            if (!Enum.IsDefined(typeof(CasePromise), promise)) throw new ArgumentException("Unknown promise.", nameof(promise));
            return Apply(eventId, Payload("promise", promise.ToString()), () =>
            {
                if (promises.Contains(promise)) return false;
                if (OutcomeId != null) throw new InvalidOperationException("Cannot add a promise after resolution.");
                promises.Add(promise);
                return true;
            });
        }

        // Storage seam for issue #8 routing rules. Kept internal so presentation cannot
        // bypass future eligibility/consent checks. No public send operation exists yet.
        internal bool CommitResolution(string eventId, string outcomeId, string[] disclosedSourceIds, string[] recipientIds)
        {
            if (!content.Outcomes.Any(item => item.Id == outcomeId)) throw new ArgumentException("Unknown outcome.", nameof(outcomeId));
            if (disclosedSourceIds == null || disclosedSourceIds.Length == 0 || disclosedSourceIds.Distinct().Count() != disclosedSourceIds.Length)
                throw new ArgumentException("Unique disclosure sources are required.", nameof(disclosedSourceIds));
            var selected = disclosedSourceIds.Select(Source).OrderBy(item => item.Id, StringComparer.Ordinal).ToArray();
            if (recipientIds == null || recipientIds.Length == 0 || recipientIds.Any(id => id == null || !people.Contains(id)) || recipientIds.Distinct().Count() != recipientIds.Length)
                throw new ArgumentException("Known unique recipients are required.", nameof(recipientIds));
            var recipients = recipientIds.OrderBy(id => id, StringComparer.Ordinal).ToArray();
            var payload = Payload("resolve", outcomeId, Payload(selected.Select(item => item.Id).ToArray()), Payload(recipients));
            return Apply(eventId, payload, () =>
            {
                if (OutcomeId != null) throw new InvalidOperationException("Case already resolved.");
                if (selected.Any(item => !acquired.Contains(item.Id))) throw new InvalidOperationException("Cannot disclose an unacquired source.");
                var additions = selected.Select(item => new Disclosure(eventId, item, recipients)).ToArray();
                disclosures.AddRange(additions);
                OutcomeId = outcomeId;
                Phase = CasePhase.Resolved;
                return true;
            });
        }

        public bool CompleteAftermath(string eventId)
        {
            return Apply(eventId, Payload("aftermath"), () =>
            {
                if (OutcomeId == null) throw new InvalidOperationException("No resolved outcome has an aftermath.");
                if (Phase == CasePhase.AftermathCompleted) return false;
                Phase = CasePhase.AftermathCompleted;
                return true;
            });
        }
    }
}
