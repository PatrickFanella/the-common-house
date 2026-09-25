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
                {
                    var style = CaseUi.WrappedLabel();
                    style.alignment = TextAnchor.MiddleCenter;
                    var width = Mathf.Min(600f, Screen.width - 40f);
                    var height = style.CalcHeight(new GUIContent(controller.FocusedPrompt), width - 16f) + 12f;
                    var prompt = new Rect((Screen.width - width) * .5f, Screen.height * .5f + 22f, width, height);
                    CaseUi.DrawOpaquePanel(prompt);
                    GUI.Label(prompt, controller.FocusedPrompt, style);
                }
                return;
            }

            CaseUi.HandleKeyboardScale();
            var panel = CaseUi.Panel();
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label(controller.OpenTarget.Title, CaseUi.WrappedLabel());
            GUILayout.Space(14f);
            scrollPosition = CaseUi.Scroll(scrollPosition); scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.Label(controller.VisiblePanelText, CaseUi.WrappedLabel());
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label((controller.OpenTarget.HasAcquisition ? "[Enter] Record evidence    [Escape] Close" : "[Escape] Close") + "  [PgUp/PgDn] Scroll  [-/=] Text size", CaseUi.WrappedLabel());
            if (!string.IsNullOrEmpty(controller.LastActionMessage)) GUILayout.Label(controller.LastActionMessage, CaseUi.WrappedLabel());
            GUILayout.EndArea();
        }
    }
}
