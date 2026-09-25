using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Hints
{
    [RequireComponent(typeof(InvestigationHintController))]
    public sealed class InvestigationHintOverlay : MonoBehaviour
    {
        private InvestigationHintController controller;
        private Vector2 scroll;
        private void Awake() => controller = GetComponent<InvestigationHintController>();
        private void OnGUI()
        {
            if (controller == null || !controller.IsOpen || controller.CurrentHint == null) return;
            CaseUi.HandleKeyboardScale(); var panel = CaseUi.Panel(); CaseUi.DrawOpaquePanel(panel, "Optional investigation hint");
            var wrapped = CaseUi.WrappedLabel();
            GUILayout.BeginArea(new Rect(panel.x + 24f, panel.y + 38f, panel.width - 48f, panel.height - 72f));
            scroll = CaseUi.Scroll(scroll); scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label(controller.CurrentHint.Text, wrapped);
            GUILayout.EndScrollView();
            GUILayout.Label("[H] More detail  [PgUp/PgDn] Scroll  [-/=] Text size  [Escape] Close", wrapped);
            GUILayout.EndArea();
        }
    }
}
