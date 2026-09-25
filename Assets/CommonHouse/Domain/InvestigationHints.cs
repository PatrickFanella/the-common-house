using System;
using System.Linq;

namespace CommonHouse.Domain
{
    public enum InvestigationHintStage { Lead, Comparison, Explanation }

    public sealed class InvestigationHint
    {
        internal InvestigationHint(InvestigationHintStage stage, string text)
        {
            Stage = stage;
            Text = text;
        }

        public InvestigationHintStage Stage { get; }
        public string Text { get; }
    }

    /// <summary>Pure, optional guidance derived only from recorded case state.</summary>
    public static class InvestigationHints
    {
        public static InvestigationHint Get(CaseState state, int requestCount)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.OutcomeId != null) return null;
            var stage = requestCount <= 0 ? InvestigationHintStage.Lead
                : requestCount == 1 ? InvestigationHintStage.Comparison : InvestigationHintStage.Explanation;
            if (state.IsDeferred) return Hint(stage, "Resume the investigation when you are ready.", "Recorded sources remain in the notebook while the case is deferred.", "Deferring is not a resolution and does not remove any available lead.");
            if (!Has(state, "T01")) return Hint(stage, "The waiting call at the radio desk is available when you are ready.", "When heard, keep the call as a transcript rather than treating its address as proof.", "A caller's use of a name does not confirm identity or permission.");
            if (!Has(state, "C01")) return Hint(stage, "Ren is available for a private conversation about the current call.", "Compare the caller's request with what Ren says privately, without treating either account as consent.", "Only Ren's explicit response can establish identity or authorize a route.");
            if (state.Authorization == ContactAuthorization.Unknown) return Hint(stage, "Continue the private conversation with Ren before routing any response.", "The recorded call and private conversation identify different kinds of information: a request and Ren's response.", "Do not turn E03 or E06, or any comparison, into identity confirmation or private-contact agreement.");
            if (state.Authorization == ContactAuthorization.PrivateContactAuthorized) return Hint(stage, "The private headset route is available when you choose it.", "The recorded agreement and verified route support that route together.", "The route remains private because its recipients and Ren's explicit agreement are both recorded.");
            if (!Has(state, "D01") || !Has(state, "D02")) return Hint(stage, "Ren's authorized refusal is available to relay. Optional readable documents can also be inspected in the outpost.", "Record the roster and repair note before comparing their authored marks in the notebook.", "A shared mark is a possible link only; it neither confirms identity nor changes the authorized refusal.");
            if (!Has(state, "T02")) return Hint(stage, "Ren's authorized refusal is available to relay. The archive tape is also an optional readable source.", "Keep the archive tape's caption and its relative time with its original transcript.", "The archive may add context, but it is not required for the refusal route.");
            if (!Has(state, "D03")) return Hint(stage, "Ren's authorized refusal is available to relay. Ivo can also share an optional missing log page.", "Compare the missing-page account only with sources you have recorded.", "The log gap can contextualize the breach, but it does not replace Ren's explicit decision.");
            if (!Has(state, "T03")) return Hint(stage, "You may relay Ren's authorized refusal now, or listen for Ada's optional follow-up account.", "If recorded, compare Ada's follow-up with Ren's account as separate attributed sources.", "A caller account alone does not authorize private contact.");
            if (!Has(state, "C02")) return Hint(stage, "You may relay Ren's authorized refusal now, or ask Ren about the shared repair account.", "A completed E06 requires both recorded repair accounts, not a guessed connection.", "Even E06 corroboration does not authorize contact; Ren must explicitly agree.");
            if (!state.PrivateRouteVerified) return Hint(stage, "You may relay the authorized refusal, or inspect the routing diagram and current connection.", "Compare the diagram with the current connection before treating a route as private.", "A verified private route describes recipients; it does not itself grant permission.");
            return Hint(stage, "You may relay the authorized refusal, or return to Ren for an explicit decision about private contact.", "The verified route and repair accounts are facts; Ren's decision remains separate.", "Private contact requires Ren's explicit agreement even after the route is verified.");
        }

        private static bool Has(CaseState state, string sourceId) => Array.IndexOf(state.AcquiredSourceIds.ToArray(), sourceId) >= 0;
        private static InvestigationHint Hint(InvestigationHintStage stage, string lead, string comparison, string explanation)
        {
            return new InvestigationHint(stage, stage == InvestigationHintStage.Lead ? lead
                : stage == InvestigationHintStage.Comparison ? comparison : explanation);
        }
    }
}
