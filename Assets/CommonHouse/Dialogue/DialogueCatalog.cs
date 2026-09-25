using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHouse.Dialogue
{
    /// <summary>Builds a resident's available authored conversation at the moment of interaction.</summary>
    public static class DialogueCatalog
    {
        public static IReadOnlyList<DialogueNode> ForResident(string residentId)
        {
            switch (residentId)
            {
                case "ren":
                    return Case01DialogueNodes.All.Concat(RepairDialogueNodes.All).ToArray();
                case "ivo":
                    return IvoDialogueNodes.All.ToArray();
                default:
                    throw new ArgumentException("Unknown resident ID.", nameof(residentId));
            }
        }
    }
}
