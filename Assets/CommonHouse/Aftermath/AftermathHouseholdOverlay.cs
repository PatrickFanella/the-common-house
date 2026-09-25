using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Aftermath
{
    [RequireComponent(typeof(AftermathHouseholdController))]
    public sealed class AftermathHouseholdOverlay : MonoBehaviour
    {
        private AftermathHouseholdController aftermath;
        private Vector2 scrollPosition;

        private void Awake()
        {
            aftermath = GetComponent<AftermathHouseholdController>();
        }

        private void OnGUI()
        {
            if (aftermath == null || !aftermath.IsOpen || aftermath.ActiveEntry == null) return;
            CaseUi.HandleKeyboardScale(); var panel = CaseUi.Panel(); CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            var wrapped = CaseUi.WrappedLabel();
            GUILayout.Label(aftermath.ActiveEntry.Heading, CaseUi.WrappedLabel());
            GUILayout.Space(12f);
            scrollPosition = CaseUi.Scroll(scrollPosition); scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.Label(aftermath.ActiveEntry.Text, wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label(aftermath.IsAcknowledged
                ? "This check-in is complete. [Escape] Return to the kitchen"
                : "[Enter] Acknowledge check-in    [Escape] Return to the kitchen", wrapped);
            if (!string.IsNullOrEmpty(aftermath.LastActionMessage)) GUILayout.Label(aftermath.LastActionMessage, wrapped);
            GUILayout.EndArea();
        }
    }
}
