using System;

namespace CommonHouse.Aftermath
{
    /// <summary>Provisional household consequences for the three already-resolved routes.</summary>
    public sealed class AftermathEntry
    {
        public AftermathEntry(string outcomeId, string heading, string text)
        {
            OutcomeId = outcomeId ?? throw new ArgumentNullException(nameof(outcomeId));
            Heading = heading ?? throw new ArgumentNullException(nameof(heading));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string OutcomeId { get; }
        public string Heading { get; }
        public string Text { get; }
    }

    public static class AftermathContent
    {
        private static readonly AftermathEntry PrivateContact = new AftermathEntry(
            "R1", "A routine repair",
            "Ren invites you to help with the next routine repair. The private call remains Ren's to share, if they choose.");
        private static readonly AftermathEntry RefusalRelayed = new AftermathEntry(
            "R2", "The refusal stands",
            "Ren thanks you for relaying the refusal. The choice was complete, and ordinary household life remains available: food, shifts, and company without pressure.");
        private static readonly AftermathEntry PublicDisclosure = new AftermathEntry(
            "R3", "Distance and acknowledgment",
            "Ren asks for distance. Ivo acknowledges that the public disclosure repeated an earlier breach; saying so does not erase what was disclosed.");

        public static AftermathEntry ForOutcome(string outcomeId)
        {
            switch (outcomeId)
            {
                case "R1": return PrivateContact;
                case "R2": return RefusalRelayed;
                case "R3": return PublicDisclosure;
                default: return null;
            }
        }
    }
}
