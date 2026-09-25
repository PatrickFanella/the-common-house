using System;
using System.Collections.Generic;
using CommonHouse.Domain;

namespace CommonHouse.Dialogue
{
    /// <summary>One provisional private account; corroboration remains a derived domain fact.</summary>
    public static class RepairDialogueNodes
    {
        public static readonly IReadOnlyList<DialogueNode> All = Array.AsReadOnly(new[]
        {
            new DialogueNode("ren-repair-account", "ren", "Ask privately about the shared repair.", "",
                new[] { DialogueEffectKind.AcquireRenRepair },
                state => state.OutcomeId == null && state.HasEvidence("E01"), "C02")
        });
    }
}
