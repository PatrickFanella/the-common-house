using System;
using CommonHouse.Interaction;
using CommonHouse.Radio;
using UnityEngine;

namespace CommonHouse.Opening
{
    public enum OpeningWorldAction { Care, ReceiveFirstCall }

    /// <summary>A spatial opening action; it never listens for global keyboard input itself.</summary>
    public sealed class OpeningWorldTarget : WorldInteractionTarget
    {
        [SerializeField] private OpeningHouseholdController opening;
        [SerializeField] private OpeningWorldAction action;

        public override string Prompt
        {
            get
            {
                if (opening == null) return string.Empty;
                switch (action)
                {
                    case OpeningWorldAction.Care:
                        return "Check in at the radio desk";
                    case OpeningWorldAction.ReceiveFirstCall:
                        return opening.IsFirstCallAvailable ? "Receive the waiting call" : "The radio desk is not ready.";
                    default:
                        return string.Empty;
                }
            }
        }

        public void Configure(OpeningHouseholdController controller, OpeningWorldAction worldAction)
        {
            opening = controller ?? throw new ArgumentNullException(nameof(controller));
            action = worldAction;
        }

        public override bool Interact()
        {
            if (opening == null) return false;
            switch (action)
            {
                case OpeningWorldAction.Care:
                    return opening.OpenCare();
                case OpeningWorldAction.ReceiveFirstCall:
                    var result = opening.ReceiveFirstCall();
                    return result == TransmissionReceiveResult.Received || result == TransmissionReceiveResult.AlreadyReceived;
                default:
                    return false;
            }
        }
    }
}
