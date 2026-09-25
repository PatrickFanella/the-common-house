using System;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Radio
{
    /// <summary>Reachable radio-console action for reviewing a proposed response.</summary>
    public sealed class SendConfirmationWorldTarget : WorldInteractionTarget
    {
        [SerializeField] private SendConfirmationController controller;

        public override string Prompt => "Prepare a radio response";

        public void Configure(SendConfirmationController sendConfirmation)
        {
            controller = sendConfirmation ?? throw new ArgumentNullException(nameof(sendConfirmation));
        }

        public override bool Interact() => controller != null && controller.Open();
    }
}
