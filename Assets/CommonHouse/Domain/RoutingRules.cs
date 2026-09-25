using System;
using System.Collections.Generic;

namespace CommonHouse.Domain
{
    public enum ContactAuthorization { Unknown, RefusalAuthorized, PrivateContactAuthorized }
    public enum ResponseRoute { PrivateHeadset, RefusalRelay, RoomSpeaker }

    public sealed class SendSummary
    {
        public ResponseRoute Route { get; }
        public string OutcomeId { get; }
        public string SourceId { get; }
        public string MaterialId { get; }
        public string Text { get; }
        public IReadOnlyList<string> Recipients { get; }
        public string BlockingReason { get; }
        public bool CanSend => BlockingReason == null;
        public bool IsUnauthorizedDisclosure => Route == ResponseRoute.RoomSpeaker;
        public string Warning => IsUnauthorizedDisclosure ? "This room-speaker disclosure is not authorized." : null;
        internal string AuthorizationEventId { get; }
        internal SendSummary(ResponseRoute route, string outcome, string source, string material,
            string text, string[] recipients, string reason, string authorizationEventId)
        {
            Route = route;
            OutcomeId = outcome;
            SourceId = source;
            MaterialId = material;
            Text = text;
            Recipients = Array.AsReadOnly((string[])recipients.Clone());
            BlockingReason = reason;
            AuthorizationEventId = authorizationEventId;
        }
    }

    public sealed partial class CaseState
    {
        public ContactAuthorization Authorization { get; private set; }
        public string AuthorizationEventId { get; private set; }
        public bool PrivateRouteVerified { get; private set; }
        public ResponseRoute? DraftRoute { get; private set; }
        public bool IsDeferred { get; private set; }
        private SendSummary presentedSummary;

        private void RequireUnresolved()
        {
            if (OutcomeId != null) throw new InvalidOperationException("Case already resolved.");
        }

        // Called by completion of the authored private conversation, separately from
        // notebook acquisition/replay. No evidence read silently records permission.
        public bool RecordRefusalAuthorization(string eventId)
        {
            return Apply(eventId, Payload("authorize-refusal"), () =>
            {
                if (Authorization != ContactAuthorization.Unknown) return false;
                RequireUnresolved();
                if (!HasEvidence("E01") || !HasEvidence("E04"))
                    throw new InvalidOperationException("The initial private conversation is required.");
                Authorization = ContactAuthorization.RefusalAuthorized;
                AuthorizationEventId = eventId;
                return true;
            });
        }

        public bool InspectPrivateRoute(string eventId)
        {
            return Apply(eventId, Payload("inspect-private-route"), () =>
            {
                if (PrivateRouteVerified) return false;
                RequireUnresolved();
                PrivateRouteVerified = true;
                return true;
            });
        }

        public bool RecordPrivateContactAgreement(string eventId)
        {
            return Apply(eventId, Payload("authorize-private-contact"), () =>
            {
                if (Authorization == ContactAuthorization.PrivateContactAuthorized) return false;
                RequireUnresolved();
                if (Authorization != ContactAuthorization.RefusalAuthorized || !HasEvidence("E04") || !HasEvidence("E06") || !PrivateRouteVerified)
                    throw new InvalidOperationException("Private agreement requires the initial conversation, both repair accounts, and verified private routing.");
                Authorization = ContactAuthorization.PrivateContactAuthorized;
                AuthorizationEventId = eventId;
                return true;
            });
        }

        public void PrepareDraft(ResponseRoute route)
        {
            if (!Enum.IsDefined(typeof(ResponseRoute), route)) throw new ArgumentException("Unknown route.", nameof(route));
            RequireUnresolved();
            DraftRoute = route;
            presentedSummary = null;
            IsDeferred = false;
        }

        public void CancelConfirmation()
        {
            RequireUnresolved();
            presentedSummary = null;
        }

        public void Defer()
        {
            RequireUnresolved();
            IsDeferred = true;
            presentedSummary = null;
        }

        public void Resume()
        {
            RequireUnresolved();
            IsDeferred = false;
        }

        public string SendBlockingReason(ResponseRoute route)
        {
            if (!Enum.IsDefined(typeof(ResponseRoute), route)) throw new ArgumentException("Unknown route.", nameof(route));
            if (OutcomeId != null) return "Case already resolved.";
            if (IsDeferred) return "Resume the investigation before sending.";
            if (!HasEvidence("E01") || !HasEvidence("E04")) return "Recipient unresolved: ask privately using the current call.";
            switch (route)
            {
                case ResponseRoute.RefusalRelay:
                    return Authorization == ContactAuthorization.RefusalAuthorized ? null : "A current authorized refusal is required.";
                case ResponseRoute.PrivateHeadset:
                    if (!HasEvidence("E06")) return "Both shared-repair accounts are required.";
                    if (!PrivateRouteVerified) return "Inspect the diagram and actual private connection.";
                    return Authorization == ContactAuthorization.PrivateContactAuthorized ? null : "Ren must explicitly agree to private contact.";
                default:
                    // This remains unauthorized even after private agreement.
                    return null;
            }
        }

        // The presentation layer must display this exact summary before requesting confirmation.
        // A domain test cannot prove that a future UI actually rendered it.
        public SendSummary PresentSendSummary()
        {
            RequireUnresolved();
            if (!DraftRoute.HasValue) throw new InvalidOperationException("Prepare a draft first.");
            var route = DraftRoute.Value;
            var reason = SendBlockingReason(route);
            switch (route)
            {
                case ResponseRoute.PrivateHeadset:
                    presentedSummary = new SendSummary(route, "R1", "T01", "private-connection",
                        "Establish private contact; the conversation contents remain private.",
                        IdentityConfirmed ? new[] { "ren", "ada" } : new[] { "unresolved", "ada" }, reason, AuthorizationEventId);
                    break;
                case ResponseRoute.RefusalRelay:
                    presentedSummary = new SendSummary(route, "R2", "C01", "authorized-refusal",
                        IdentityConfirmed ? "Ren declines immediate contact." : "Refusal draft pending the private conversation.",
                        new[] { "ada" }, reason, AuthorizationEventId);
                    break;
                default:
                    presentedSummary = new SendSummary(route, "R3", "T01", "public-request",
                        "Ada requests Mara: the road is clear.", new[] { "ren", "ivo", "player" }, reason, AuthorizationEventId);
                    break;
            }
            return presentedSummary;
        }

        public bool ConfirmSend(string eventId, SendSummary summary, bool confirmed, bool acknowledgeUnauthorizedDisclosure = false)
        {
            if (summary == null) throw new ArgumentNullException(nameof(summary));
            if (!confirmed) throw new InvalidOperationException("Explicit confirmation is required.");
            if (!summary.CanSend) throw new InvalidOperationException(summary.BlockingReason);
            if (summary.IsUnauthorizedDisclosure && !acknowledgeUnauthorizedDisclosure)
                throw new InvalidOperationException("Acknowledge the unauthorized room disclosure.");
            // Resolve uses the shared event ledger: an exact retry remains harmless after
            // resolution. A different send cannot replace the outcome or its disclosures.
            var changed = CommitResolution(eventId, summary.OutcomeId, new[] { summary.SourceId },
                new List<string>(summary.Recipients).ToArray(), summary.MaterialId, summary.Text, () =>
                {
                    if (!ReferenceEquals(summary, presentedSummary) || DraftRoute != summary.Route || summary.AuthorizationEventId != AuthorizationEventId)
                        throw new InvalidOperationException("Summary is stale or belongs to another session; present it again.");
                    if (!summary.CanSend) throw new InvalidOperationException(summary.BlockingReason);
                    var reason = SendBlockingReason(summary.Route);
                    if (reason != null) throw new InvalidOperationException(reason);
                });
            return changed;
        }
    }
}
