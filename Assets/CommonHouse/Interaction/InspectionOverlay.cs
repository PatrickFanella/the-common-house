using UnityEngine;

namespace CommonHouse.Interaction
{
    /// <summary>Built-in IMGUI presentation for the temporary readable-text panel.</summary>
    [RequireComponent(typeof(InspectionController))]
    public sealed class InspectionOverlay : MonoBehaviour
    {
        private InspectionController controller;
        private Vector2 scrollPosition;

        private void Awake()
        {
            controller = GetComponent<InspectionController>();
        }

        private void OnGUI()
        {
            if (controller == null) return;
            if (!controller.IsOpen)
            {
                if (!string.IsNullOrEmpty(controller.FocusedPrompt))
                    GUI.Label(new Rect(Screen.width / 2f - 120f, Screen.height / 2f + 22f, 240f, 28f), controller.FocusedPrompt);
                return;
            }

            var panel = new Rect(Screen.width * 0.16f, Screen.height * 0.13f, Screen.width * 0.68f, Screen.height * 0.74f);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label(controller.OpenTarget.Title, GUI.skin.label);
            GUILayout.Space(14f);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.Label(controller.VisiblePanelText, new GUIStyle(GUI.skin.label) { wordWrap = true });
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label(controller.OpenTarget.HasAcquisition ? "[Enter] Record evidence    [Escape] Close" : "[Escape] Close", GUI.skin.label);
            if (!string.IsNullOrEmpty(controller.LastActionMessage)) GUILayout.Label(controller.LastActionMessage, GUI.skin.label);
            GUILayout.EndArea();
        }
    }
}
