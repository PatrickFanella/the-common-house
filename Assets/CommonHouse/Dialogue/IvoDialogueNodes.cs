using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Domain;

namespace CommonHouse.Dialogue
{
    /// <summary>
    /// Provisional Ivo dialogue for the visible archive-log gap. The canonical
    /// page remains a document: asking Ivo supplies it without turning access
    /// into trespass or a new conversational fact.
    /// </summary>
    public static class IvoDialogueNodes
    {
        public static readonly IReadOnlyList<DialogueNode> All = Array.AsReadOnly(new[]
        {
            new DialogueNode(
                "ivo-log-gap-request", "ivo", "Ask Ivo about the missing log page.", "",
                new[] { DialogueEffectKind.AcquireMissingPage },
                state => state.OutcomeId == null && !state.AcquiredSourceIds.Contains("D03"),
                "D03"),
            new DialogueNode(
                "ivo-log-gap-decline", "ivo", "Leave the missing page alone for now.",
                "That is your call. The gap will still be here if you want to ask about it later.",
                new[] { DialogueEffectKind.None },
                state => state.OutcomeId == null && !state.AcquiredSourceIds.Contains("D03")),
            new DialogueNode(
                "ivo-log-gap-replay", "ivo", "Review the page Ivo supplied.", "",
                new[] { DialogueEffectKind.None },
                state => state.OutcomeId == null && state.AcquiredSourceIds.Contains("D03"),
                "D03"),
            new DialogueNode(
                "ivo-log-gap-leave", "ivo", "End the conversation.",
                "The page and the question can wait.", new[] { DialogueEffectKind.None }, state => state.OutcomeId == null),
        });
    }
}
