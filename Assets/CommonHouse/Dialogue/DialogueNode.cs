using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Domain;

namespace CommonHouse.Dialogue
{
    public enum DialogueEffectKind
    {
        None,
        AcquirePrivateAccount,
        AcquireMissingPage,
        AcquireRenRepair,
        AuthorizeRefusal,
        ProtectPrivateContact,
        AuthorizePrivateContact,
    }

    public sealed class DialogueNode
    {
        public DialogueNode(string id, string speakerId, string question, string response, IEnumerable<DialogueEffectKind> effects,
            Func<CaseState, bool> eligibility, string responseSourceId = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            SpeakerId = speakerId ?? throw new ArgumentNullException(nameof(speakerId));
            Question = question ?? throw new ArgumentNullException(nameof(question));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Effects = Array.AsReadOnly((effects ?? Array.Empty<DialogueEffectKind>()).ToArray());
            Eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
            ResponseSourceId = responseSourceId;
        }

        public string Id { get; }
        public string SpeakerId { get; }
        public string Question { get; }
        public string Response { get; }
        public IReadOnlyList<DialogueEffectKind> Effects { get; }
        public Func<CaseState, bool> Eligibility { get; }
        public string ResponseSourceId { get; }
    }

    /// <summary>Small provisional Case 01 dialogue data, separate from panel code.</summary>
    public static class Case01DialogueNodes
    {
        public static readonly IReadOnlyList<DialogueNode> All = Array.AsReadOnly(new[]
        {
            new DialogueNode(
                "ren-current-call", "ren", "Ask Ren privately about the current call.",
                "",
                new[] { DialogueEffectKind.AcquirePrivateAccount, DialogueEffectKind.AuthorizeRefusal },
                state => state.OutcomeId == null && state.AcquiredSourceIds.Contains("T01") && !state.AcquiredSourceIds.Contains("C01"),
                "C01"),
            new DialogueNode(
                "ren-protect-private-contact", "ren", "Promise to protect a private conversation.",
                "Thank you. A promise is not the same thing as permission, but it matters.",
                new[] { DialogueEffectKind.ProtectPrivateContact },
                state => state.OutcomeId == null && state.IdentityConfirmed && !state.Promises.Contains(CasePromise.ProtectPrivateContact)),
            new DialogueNode(
                "ren-private-contact", "ren", "Ask whether Ren wants private contact now.",
                "With the repair account confirmed and the private line checked, I agree to speak privately.",
                new[] { DialogueEffectKind.AuthorizePrivateContact },
                state => state.OutcomeId == null && state.Authorization == ContactAuthorization.RefusalAuthorized && state.HasEvidence("E06") && state.PrivateRouteVerified),
            new DialogueNode(
                "ren-leave-open", "ren", "Leave the matter open for now.",
                "The call can wait. The leads remain available when you are ready.",
                new[] { DialogueEffectKind.None },
                state => state.OutcomeId == null),
        });
    }

    /// <summary>Non-case-specific scene demo. It deliberately has no case effects.</summary>
    public static class DemoDialogueNodes
    {
        public static readonly IReadOnlyList<DialogueNode> All = Array.AsReadOnly(new[]
        {
            new DialogueNode("outpost-check-in", "ivo", "Ask about the next shift.",
                "The outpost log stays here. Take your time with it.", new[] { DialogueEffectKind.None }, state => state.OutcomeId == null),
            new DialogueNode("outpost-leave", "ivo", "End the conversation.",
                "The conversation can wait.", new[] { DialogueEffectKind.None }, state => state.OutcomeId == null),
        });
    }
}
