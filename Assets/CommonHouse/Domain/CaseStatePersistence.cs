using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHouse.Domain
{
    [Serializable]
    public sealed class PersistedCaseEvent
    {
        public string Id;
        public string Payload;
    }

    [Serializable]
    public sealed class PersistedDisclosure
    {
        public string EventId;
        public string SourceId;
        public string AttributedTo;
        public string MaterialId;
        public string Text;
        public string[] Recipients;
    }

    // Public field DTOs keep Unity's JSON serializer out of the domain state itself.
    // They are copied on capture and validated before any state is restored.
    [Serializable]
    public sealed class CaseStateSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion;
        public string CaseId;
        public string Phase;
        public string OutcomeId;
        public string[] AcquiredSourceIds;
        public string[] Promises;
        public PersistedCaseEvent[] Events;
        public PersistedDisclosure[] Disclosures;
        public string Authorization;
        public string AuthorizationEventId;
        public bool PrivateRouteVerified;
        public bool HasDraftRoute;
        public string DraftRoute;
        public bool IsDeferred;
    }

    public sealed partial class CaseState
    {
        public CaseStateSnapshot CaptureSnapshot()
        {
            return new CaseStateSnapshot
            {
                SchemaVersion = CaseStateSnapshot.CurrentSchemaVersion,
                CaseId = content.Id,
                Phase = Phase.ToString(),
                OutcomeId = OutcomeId,
                AcquiredSourceIds = acquired.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                Promises = promises.OrderBy(item => item).Select(item => item.ToString()).ToArray(),
                Events = events.OrderBy(item => item.Key, StringComparer.Ordinal)
                    .Select(item => new PersistedCaseEvent { Id = item.Key, Payload = item.Value }).ToArray(),
                Disclosures = disclosures.Select(item => new PersistedDisclosure
                {
                    EventId = item.EventId,
                    SourceId = item.SourceId,
                    AttributedTo = item.AttributedTo,
                    MaterialId = item.MaterialId,
                    Text = item.Text,
                    Recipients = item.Recipients.ToArray(),
                }).ToArray(),
                Authorization = Authorization.ToString(),
                AuthorizationEventId = AuthorizationEventId,
                PrivateRouteVerified = PrivateRouteVerified,
                HasDraftRoute = DraftRoute.HasValue,
                DraftRoute = DraftRoute.HasValue ? DraftRoute.Value.ToString() : null,
                IsDeferred = IsDeferred,
            };
        }

        public static CaseState RestoreSnapshot(CommonHouse.Content.CaseContent content, CaseStateSnapshot snapshot)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.SchemaVersion != CaseStateSnapshot.CurrentSchemaVersion)
                throw new InvalidOperationException("Save schema version " + snapshot.SchemaVersion + " is incompatible; expected " + CaseStateSnapshot.CurrentSchemaVersion + ".");
            if (snapshot.CaseId != content.Id)
                throw new InvalidOperationException("Save case ID does not match the loaded case content.");
            if (!Enum.TryParse(snapshot.Phase, out CasePhase phase) || !Enum.IsDefined(typeof(CasePhase), phase))
                throw new InvalidOperationException("Save has an invalid case phase.");
            if (!Enum.TryParse(snapshot.Authorization, out ContactAuthorization authorization) || !Enum.IsDefined(typeof(ContactAuthorization), authorization))
                throw new InvalidOperationException("Save has an invalid contact authorization.");
            snapshot.OutcomeId = NormalizeOptional(snapshot.OutcomeId);
            snapshot.AuthorizationEventId = NormalizeOptional(snapshot.AuthorizationEventId);
            snapshot.DraftRoute = NormalizeOptional(snapshot.DraftRoute);
            if (snapshot.HasDraftRoute != (snapshot.DraftRoute != null) ||
                (snapshot.HasDraftRoute && (!Enum.TryParse(snapshot.DraftRoute, out ResponseRoute ignoredRoute) || !Enum.IsDefined(typeof(ResponseRoute), ignoredRoute))))
                throw new InvalidOperationException("Save has an invalid draft route.");

            var state = new CaseState(content);
            var sourceIds = RequireUnique(snapshot.AcquiredSourceIds, "acquired source");
            foreach (var sourceId in sourceIds) state.Source(sourceId);

            var restoredPromises = new HashSet<CasePromise>();
            foreach (var promiseName in RequireUnique(snapshot.Promises, "promise"))
            {
                if (!Enum.TryParse(promiseName, out CasePromise promise) || !Enum.IsDefined(typeof(CasePromise), promise))
                    throw new InvalidOperationException("Save has an unknown promise.");
                restoredPromises.Add(promise);
            }

            var restoredEvents = new Dictionary<string, string>(StringComparer.Ordinal);
            if (snapshot.Events == null) throw new InvalidOperationException("Save is missing its event ledger.");
            foreach (var item in snapshot.Events)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || item.Payload == null || !restoredEvents.TryAdd(item.Id, item.Payload))
                    throw new InvalidOperationException("Save has an invalid or duplicate event ledger entry.");
            }

            var restoredDisclosures = new List<Disclosure>();
            if (snapshot.Disclosures == null) throw new InvalidOperationException("Save is missing disclosure records.");
            foreach (var item in snapshot.Disclosures)
                restoredDisclosures.Add(state.RestoreDisclosure(item, restoredEvents));

            state.ValidateSnapshotConsistency(phase, snapshot.OutcomeId, sourceIds, restoredPromises,
                restoredEvents, restoredDisclosures, authorization, snapshot.AuthorizationEventId,
                snapshot.PrivateRouteVerified, snapshot.HasDraftRoute, snapshot.DraftRoute, snapshot.IsDeferred);

            state.acquired.UnionWith(sourceIds);
            state.promises.UnionWith(restoredPromises);
            foreach (var item in restoredEvents) state.events.Add(item.Key, item.Value);
            state.disclosures.AddRange(restoredDisclosures);
            state.Phase = phase;
            state.OutcomeId = snapshot.OutcomeId;
            state.Authorization = authorization;
            state.AuthorizationEventId = snapshot.AuthorizationEventId;
            state.PrivateRouteVerified = snapshot.PrivateRouteVerified;
            state.DraftRoute = snapshot.HasDraftRoute ? (ResponseRoute)Enum.Parse(typeof(ResponseRoute), snapshot.DraftRoute) : (ResponseRoute?)null;
            state.IsDeferred = snapshot.IsDeferred;
            // A confirmation object is UI-session-specific and must be presented again after loading.
            state.presentedSummary = null;
            return state;
        }

        private static HashSet<string> RequireUnique(string[] values, string label)
        {
            if (values == null) throw new InvalidOperationException("Save is missing " + label + " records.");
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
                if (string.IsNullOrWhiteSpace(value) || !result.Add(value))
                    throw new InvalidOperationException("Save has an invalid or duplicate " + label + " record.");
            return result;
        }

        private static string NormalizeOptional(string value) => string.IsNullOrEmpty(value) ? null : value;

        private Disclosure RestoreDisclosure(PersistedDisclosure item, IReadOnlyDictionary<string, string> restoredEvents)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.EventId) || !restoredEvents.ContainsKey(item.EventId) ||
                string.IsNullOrWhiteSpace(item.SourceId) || string.IsNullOrWhiteSpace(item.MaterialId) || item.Text == null)
                throw new InvalidOperationException("Save has an invalid disclosure record.");
            var source = Source(item.SourceId);
            if (item.AttributedTo != source.SpeakerId)
                throw new InvalidOperationException("Save disclosure attribution does not match its source.");
            var recipients = RequireUnique(item.Recipients, "disclosure recipient");
            if (recipients.Any(id => !people.Contains(id)))
                throw new InvalidOperationException("Save disclosure has an unknown recipient.");
            return new Disclosure(item.EventId, source, recipients, item.MaterialId, item.Text);
        }

        private void ValidateSnapshotConsistency(CasePhase phase, string outcomeId, HashSet<string> sourceIds,
            HashSet<CasePromise> restoredPromises, IReadOnlyDictionary<string, string> restoredEvents,
            IReadOnlyList<Disclosure> restoredDisclosures, ContactAuthorization authorization, string authorizationEventId,
            bool privateRouteVerified, bool hasDraftRoute, string draftRoute, bool isDeferred)
        {
            if (outcomeId != null && !content.Outcomes.Any(item => item.Id == outcomeId))
                throw new InvalidOperationException("Save has an unknown outcome.");
            if ((phase == CasePhase.NotStarted && (sourceIds.Count != 0 || outcomeId != null)) ||
                (phase == CasePhase.Investigating && (sourceIds.Count == 0 || outcomeId != null)) ||
                ((phase == CasePhase.Resolved || phase == CasePhase.AftermathCompleted) && outcomeId == null))
                throw new InvalidOperationException("Save phase does not match its evidence and outcome.");
            if (outcomeId != null)
            {
                var expectedRoute = outcomeId == "R1" ? ResponseRoute.PrivateHeadset :
                    outcomeId == "R2" ? ResponseRoute.RefusalRelay : ResponseRoute.RoomSpeaker;
                if (!hasDraftRoute || draftRoute != expectedRoute.ToString() || isDeferred)
                    throw new InvalidOperationException("Save resolution does not retain its completed route state.");
            }
            if (authorization == ContactAuthorization.Unknown)
            {
                if (authorizationEventId != null) throw new InvalidOperationException("Unknown authorization cannot have an event.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(authorizationEventId) || !restoredEvents.TryGetValue(authorizationEventId, out var authorizationPayload))
                    throw new InvalidOperationException("Save authorization event is missing from the event ledger.");
                if (authorization == ContactAuthorization.RefusalAuthorized)
                {
                    if (!HasAll(sourceIds, "T01", "C01") || authorizationPayload != Payload("authorize-refusal"))
                        throw new InvalidOperationException("Save refusal authorization is inconsistent with the case facts.");
                }
                else if (authorization == ContactAuthorization.PrivateContactAuthorized)
                {
                    if (!HasAll(sourceIds, "T01", "C01", "T03", "C02") || !privateRouteVerified || authorizationPayload != Payload("authorize-private-contact") ||
                        !restoredEvents.Values.Contains(Payload("authorize-refusal")))
                        throw new InvalidOperationException("Save private-contact authorization is inconsistent with the case facts.");
                }
            }
            if (privateRouteVerified && !restoredEvents.Values.Contains(Payload("inspect-private-route")))
                throw new InvalidOperationException("Save private-route inspection is missing from the event ledger.");

            foreach (var sourceId in sourceIds)
                if (!restoredEvents.Values.Contains(Payload("acquire", sourceId)))
                    throw new InvalidOperationException("Save acquired source is missing from the event ledger.");
            var expectedAcquisitionDisclosures = sourceIds.Select(Source).Where(item => item.Kind != "document").ToArray();
            foreach (var source in expectedAcquisitionDisclosures)
            {
                if (!restoredDisclosures.Any(item => item.SourceId == source.Id && item.MaterialId == "source" && item.Text == source.Text &&
                    item.Recipients.SequenceEqual(new[] { "player" }) && restoredEvents.TryGetValue(item.EventId, out var payload) && payload == Payload("acquire", source.Id)))
                    throw new InvalidOperationException("Save is missing an acquired spoken-source disclosure.");
            }
            foreach (var disclosure in restoredDisclosures)
            {
                if (!sourceIds.Contains(disclosure.SourceId))
                    throw new InvalidOperationException("Save disclosure references an unacquired source.");
                if (disclosure.MaterialId == "source")
                {
                    var source = Source(disclosure.SourceId);
                    if (source.Kind == "document" || disclosure.Text != source.Text || !disclosure.Recipients.SequenceEqual(new[] { "player" }) ||
                        !restoredEvents.TryGetValue(disclosure.EventId, out var payload) || payload != Payload("acquire", source.Id))
                        throw new InvalidOperationException("Save has a forged source disclosure.");
                }
            }
            var disclosureKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var disclosure in restoredDisclosures)
                if (!disclosureKeys.Add(Payload(disclosure.EventId, disclosure.SourceId, disclosure.AttributedTo ?? "", disclosure.MaterialId,
                    disclosure.Text, Payload(disclosure.Recipients.ToArray()))))
                    throw new InvalidOperationException("Save has a duplicate disclosure record.");

            if (outcomeId == null)
            {
                if (restoredDisclosures.Any(item => item.MaterialId != "source"))
                    throw new InvalidOperationException("An unresolved save cannot contain an outgoing disclosure.");
            }
            else
            {
                var outgoing = restoredDisclosures.Where(item => item.MaterialId != "source").ToArray();
                if (outgoing.Length != 1) throw new InvalidOperationException("A resolved save requires exactly one outgoing disclosure.");
                ValidateOutcomeDisclosure(outcomeId, outgoing[0], restoredEvents, authorization, sourceIds);
                if (phase == CasePhase.AftermathCompleted && !restoredEvents.Values.Contains(Payload("aftermath")))
                    throw new InvalidOperationException("Completed aftermath is missing from the event ledger.");
            }

            foreach (var promise in restoredPromises)
                if (!restoredEvents.Values.Contains(Payload("promise", promise.ToString())))
                    throw new InvalidOperationException("Save promise is missing from the event ledger.");
            foreach (var item in restoredEvents)
                if (!IsRecognizedEventPayload(item.Value, sourceIds, restoredPromises, outcomeId, restoredDisclosures, authorization, authorizationEventId, privateRouteVerified, phase))
                    throw new InvalidOperationException("Save event ledger contains an unrecognized or inconsistent action.");
        }

        private void ValidateOutcomeDisclosure(string outcomeId, Disclosure disclosure, IReadOnlyDictionary<string, string> restoredEvents,
            ContactAuthorization authorization, HashSet<string> sourceIds)
        {
            var matches = (outcomeId == "R1" && authorization == ContactAuthorization.PrivateContactAuthorized &&
                           disclosure.SourceId == "T01" && disclosure.MaterialId == "private-connection" &&
                           disclosure.Text == "Establish private contact; the conversation contents remain private." &&
                           disclosure.Recipients.SequenceEqual(new[] { "ada", "ren" }) && HasAll(sourceIds, "T01", "C01", "T03", "C02")) ||
                          (outcomeId == "R2" && authorization == ContactAuthorization.RefusalAuthorized &&
                           disclosure.SourceId == "C01" && disclosure.MaterialId == "authorized-refusal" &&
                           disclosure.Text == "Ren declines immediate contact." && disclosure.Recipients.SequenceEqual(new[] { "ada" }) && HasAll(sourceIds, "T01", "C01")) ||
                          (outcomeId == "R3" && disclosure.SourceId == "T01" && disclosure.MaterialId == "public-request" &&
                           disclosure.Text == "Ada requests Mara: the road is clear." && disclosure.Recipients.SequenceEqual(new[] { "ivo", "player", "ren" }) && HasAll(sourceIds, "T01", "C01"));
            if (!matches) throw new InvalidOperationException("Save outcome disclosure is inconsistent with the selected route.");
            var payload = Payload("resolve", outcomeId, Payload(disclosure.SourceId), Payload(disclosure.Recipients.ToArray()), disclosure.MaterialId, disclosure.Text);
            if (!restoredEvents.TryGetValue(disclosure.EventId, out var actualPayload) || actualPayload != payload)
                throw new InvalidOperationException("Save outcome disclosure does not match its event ledger entry.");
        }

        private bool IsRecognizedEventPayload(string payload, HashSet<string> sourceIds, HashSet<CasePromise> restoredPromises,
            string outcomeId, IReadOnlyList<Disclosure> restoredDisclosures, ContactAuthorization authorization,
            string authorizationEventId, bool privateRouteVerified, CasePhase phase)
        {
            if (sourceIds.Any(id => payload == Payload("acquire", id))) return true;
            if (restoredPromises.Any(item => payload == Payload("promise", item.ToString()))) return true;
            if (payload == Payload("authorize-refusal")) return authorization != ContactAuthorization.Unknown;
            if (payload == Payload("authorize-private-contact")) return authorization == ContactAuthorization.PrivateContactAuthorized;
            if (payload == Payload("inspect-private-route")) return privateRouteVerified;
            if (payload == Payload("aftermath")) return phase == CasePhase.AftermathCompleted;
            return outcomeId != null && restoredDisclosures.Any(item => payload == Payload("resolve", outcomeId,
                Payload(item.SourceId), Payload(item.Recipients.ToArray()), item.MaterialId, item.Text));
        }

        private static bool HasAll(ISet<string> values, params string[] required) => required.All(values.Contains);
    }
}
