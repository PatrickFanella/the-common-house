using UnityEngine;

namespace CommonHouse.Hints
{
    [RequireComponent(typeof(InvestigationHintController))]
    public sealed class InvestigationHintOverlay : MonoBehaviour
    {
        private InvestigationHintController controller;
        private void Awake() => controller = GetComponent<InvestigationHintController>();
        private void OnGUI()
        {
            if (controller == null || !controller.IsOpen || controller.CurrentHint == null) return;
            var panel = new Rect(Screen.width * .18f, Screen.height * .34f, Screen.width * .64f, Screen.height * .26f);
            GUI.Box(panel, "Optional investigation hint");
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUI.Label(new Rect(panel.x + 24f, panel.y + 34f, panel.width - 48f, panel.height - 72f), controller.CurrentHint.Text, wrapped);
            GUI.Label(new Rect(panel.x + 24f, panel.y + panel.height - 30f, panel.width - 48f, 22f), "[H] More detail   [Escape] Close", wrapped);
        }
    }
}
