using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Radio
{
    /// <summary>Explicit playback at the labelled shelf; transcript is the complete clue.</summary>
    public sealed class ArchiveTapeTarget : WorldInteractionTarget
    {
        [SerializeField] private TransmissionLog log;
        public override string Prompt => "Read archive tape — dated before the current call";
        public void Configure(TransmissionLog transmissionLog) { log = transmissionLog; }

        public override bool Interact()
        {
            if (log == null || !log.Open()) return false;
            if (log.Receive("T02") == TransmissionReceiveResult.Rejected)
            {
                log.Close();
                return false;
            }
            return log.SelectReceivedSource("T02") && log.OpenSelectedTranscript();
        }
    }
}
