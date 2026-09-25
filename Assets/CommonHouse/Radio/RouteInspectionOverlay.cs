using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Radio
{
    [RequireComponent(typeof(RouteInspectionController))]
    public sealed class RouteInspectionOverlay : MonoBehaviour
    {
        private RouteInspectionController controller;
        private Vector2 scroll;

        private void Awake()
        {
            controller = GetComponent<RouteInspectionController>();
        }

        private void OnGUI()
        {
            if (controller == null || !controller.IsOpen) return;
            CaseUi.HandleKeyboardScale(); var panel = CaseUi.Panel(); var wrapped = CaseUi.WrappedLabel(); CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label("Routing comparison", CaseUi.WrappedLabel());
            scroll = CaseUi.Scroll(scroll); scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label((controller.SelectedPage == 0 ? "> " : "  ") + "Old diagram", CaseUi.WrappedLabel());
            GUILayout.Label("The old repair diagram labels connector A as ‘headset’ and connector B as ‘room’. The labels were not revised after the repair.", wrapped);
            GUILayout.Space(12f);
            GUILayout.Label((controller.SelectedPage == 1 ? "> " : "  ") + "Actual connection check", CaseUi.WrappedLabel());
            GUILayout.Label("Connector A is the room speaker: Ivo, player, Ren. Connector B is the private headset: Ada, Ren.", wrapped);
            GUILayout.Space(12f);
            GUILayout.Label("Actual recipients", CaseUi.WrappedLabel());
            foreach (var route in controller.ActualRecipients)
                GUILayout.Label(route.RouteName + ": " + string.Join(", ", route.Recipients), wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Up/Down] Choose  [PgUp/PgDn] Scroll  [-/=] Text size  [Enter] Review  [V] Verify  [Escape] Close", CaseUi.WrappedLabel());
            if (!string.IsNullOrEmpty(controller.LastActionMessage)) GUILayout.Label(controller.LastActionMessage, wrapped);
            GUILayout.EndArea();
        }
    }
}
