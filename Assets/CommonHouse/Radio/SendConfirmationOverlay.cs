using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Radio
{
    [RequireComponent(typeof(SendConfirmationController))]
    public sealed class SendConfirmationOverlay : MonoBehaviour
    {
        private SendConfirmationController controller;
        private Vector2 scroll;

        private void Awake()
        {
            controller = GetComponent<SendConfirmationController>();
        }

        private void OnGUI()
        {
            if (controller == null || !controller.IsOpen) return;
            CaseUi.HandleKeyboardScale();
            var panel = CaseUi.Panel();
            var wrapped = CaseUi.WrappedLabel();
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label("Response confirmation", CaseUi.WrappedLabel());
            scroll = CaseUi.Scroll(scroll); scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            if (!controller.IsReviewing)
            {
                DrawRoute("Private headset", controller.SelectedRoute == CommonHouse.Domain.ResponseRoute.PrivateHeadset, wrapped);
                DrawRoute("Authorized refusal relay", controller.SelectedRoute == CommonHouse.Domain.ResponseRoute.RefusalRelay, wrapped);
                DrawRoute("Room speaker disclosure", controller.SelectedRoute == CommonHouse.Domain.ResponseRoute.RoomSpeaker, wrapped);
            }
            else
            {
                var summary = controller.Summary;
                GUILayout.Label("Route: " + RouteLabel(summary.OutcomeId), wrapped);
                GUILayout.Label("Source: " + summary.SourceId, wrapped);
                GUILayout.Label("Selected material: " + summary.MaterialId, wrapped);
                GUILayout.Label("Message: " + summary.Text, wrapped);
                GUILayout.Label("Recipients: " + string.Join(", ", summary.Recipients), wrapped);
                if (!summary.CanSend) GUILayout.Label("Blocked: " + summary.BlockingReason, wrapped);
                if (summary.IsUnauthorizedDisclosure)
                {
                    GUILayout.Label("Warning: " + summary.Warning, wrapped);
                    GUILayout.Label("Acknowledged: " + (controller.PublicDisclosureAcknowledged ? "yes" : "no"), wrapped);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            var controls = controller.IsReviewing
                ? controller.Summary.IsUnauthorizedDisclosure
                    ? "[Enter] Confirm    [A] Acknowledge public disclosure    [D] Defer    [Escape] Cancel"
                    : "[Enter] Confirm    [D] Defer    [Escape] Cancel"
                : "[Up/Down] Choose route    [Enter] Review    [D] Defer    [R] Resume draft    [Escape] Close";
            GUILayout.Label(controls + "  [PgUp/PgDn] Scroll  [-/=] Text size", wrapped);
            if (!string.IsNullOrEmpty(controller.LastActionMessage)) GUILayout.Label(controller.LastActionMessage, wrapped);
            GUILayout.EndArea();
        }

        private static void DrawRoute(string label, bool selected, GUIStyle wrapped)
        {
            GUILayout.Label((selected ? "> " : "  ") + label, wrapped);
        }

        private static string RouteLabel(string outcomeId)
        {
            switch (outcomeId)
            {
                case "R1": return "Private headset";
                case "R2": return "Authorized refusal relay";
                case "R3": return "Room speaker disclosure";
                default: return outcomeId;
            }
        }
    }
}
