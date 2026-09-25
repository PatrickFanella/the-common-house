using System;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Aftermath
{
    /// <summary>Reachable household check-in with no pre-resolution action.</summary>
    public sealed class AftermathWorldTarget : WorldInteractionTarget
    {
        [SerializeField] private AftermathHouseholdController aftermath;

        public override string Prompt
        {
            get
            {
                if (aftermath == null || !aftermath.CanOpen) return string.Empty;
                return aftermath.IsAcknowledged ? "Revisit the household check-in" : "Check in with the household";
            }
        }

        public void Configure(AftermathHouseholdController controller)
        {
            aftermath = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public override bool Interact() => aftermath != null && aftermath.Open();
    }
}
