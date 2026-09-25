using System;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Radio
{
    /// <summary>Connects the archive routing switch to the readable route comparison.</summary>
    public sealed class RouteInspectionWorldTarget : WorldInteractionTarget
    {
        [SerializeField] private RouteInspectionController routeInspection;

        public override string Prompt => "Inspect routing switch";

        public void Configure(RouteInspectionController controller)
        {
            routeInspection = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public override bool Interact() => routeInspection != null && routeInspection.Open();
    }
}
