using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Radio
{
    public sealed class RepairFollowupTarget : WorldInteractionTarget
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private TransmissionLog log;
        public override string Prompt => session != null && session.State != null && session.State.HasEvidence("E01")
            ? "Ask Ada about the shared repair" : "Receive the waiting call before asking Ada a follow-up";
        public void Configure(CaseSession caseSession, TransmissionLog transmissionLog) { session = caseSession; log = transmissionLog; }
        public override bool Interact()
        {
            if (session == null || session.State == null || !session.State.HasEvidence("E01") || session.State.OutcomeId != null) return false;
            if (log == null || !log.Open()) return false;
            if (log.Receive("T03") == TransmissionReceiveResult.Rejected) { log.Close(); return false; }
            return log.SelectReceivedSource("T03") && log.OpenSelectedTranscript();
        }
    }
}
