using UnityEngine;

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
            var panel = new Rect(Screen.width * 0.14f, Screen.height * 0.12f, Screen.width * 0.72f, Screen.height * 0.76f);
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label("Routing comparison", GUI.skin.label);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label((controller.SelectedPage == 0 ? "> " : "  ") + "Old diagram", GUI.skin.label);
            GUILayout.Label("The old repair diagram labels connector A as ‘headset’ and connector B as ‘room’. The labels were not revised after the repair.", wrapped);
            GUILayout.Space(12f);
            GUILayout.Label((controller.SelectedPage == 1 ? "> " : "  ") + "Actual connection check", GUI.skin.label);
            GUILayout.Label("Connector A is the room speaker: Ivo, player, Ren. Connector B is the private headset: Ada, Ren.", wrapped);
            GUILayout.Space(12f);
            GUILayout.Label("Actual recipients", GUI.skin.label);
            foreach (var route in controller.ActualRecipients)
                GUILayout.Label(route.RouteName + ": " + string.Join(", ", route.Recipients), wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Up/Down] Choose    [Enter] Mark reviewed    [V] Verify private route    [Escape] Close", GUI.skin.label);
            if (!string.IsNullOrEmpty(controller.LastActionMessage)) GUILayout.Label(controller.LastActionMessage, wrapped);
            GUILayout.EndArea();
        }
    }
}
