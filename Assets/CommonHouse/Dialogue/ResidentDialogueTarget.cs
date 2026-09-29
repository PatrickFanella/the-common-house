using System;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Dialogue
{
    /// <summary>Opens only the authored dialogue for the resident at this reachable location.</summary>
    public sealed class ResidentDialogueTarget : WorldInteractionTarget
    {
        [SerializeField] private DialoguePresenter presenter;
        [SerializeField] private string residentId;

        public string ResidentId => residentId;
        public override string Prompt => string.Equals(residentId, "ren", StringComparison.Ordinal)
            ? "Speak privately with Ren"
            : string.Equals(residentId, "ivo", StringComparison.Ordinal)
                ? "Speak with Ivo"
                : string.Empty;

        public void Configure(DialoguePresenter dialoguePresenter, string resident)
        {
            presenter = dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
            residentId = resident ?? throw new ArgumentNullException(nameof(resident));
            DialogueCatalog.ForResident(residentId);
        }

        public override bool Interact()
        {
            return presenter != null && !string.IsNullOrEmpty(residentId) && presenter.OpenForResident(residentId);
        }
    }
}
