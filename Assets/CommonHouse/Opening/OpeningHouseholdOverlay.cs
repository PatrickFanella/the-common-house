using CommonHouse.Content;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Opening
{
    [RequireComponent(typeof(OpeningHouseholdController))]
    public sealed class OpeningHouseholdOverlay : MonoBehaviour
    {
        private OpeningHouseholdController opening;
        private Vector2 scrollPosition;

        private void Awake()
        {
            opening = GetComponent<OpeningHouseholdController>();
        }

        private void OnGUI()
        {
            if (opening == null || !opening.IsCarePanelOpen) return;
            CaseUi.HandleKeyboardScale(); var panel = CaseUi.Panel(); CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            var wrapped = CaseUi.WrappedLabel();
            GUILayout.Label("At the radio desk", CaseUi.WrappedLabel());
            GUILayout.Space(12f);
            scrollPosition = CaseUi.Scroll(scrollPosition); scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            foreach (var line in OpeningContent.CareLines)
            {
                GUILayout.Label(line.Speaker, CaseUi.WrappedLabel());
                GUILayout.Label(line.Text, wrapped);
                GUILayout.Space(10f);
            }
            GUILayout.Label(OpeningContent.CallAvailableText, wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[PgUp/PgDn] Scroll  [-/=] Text size  [Escape] Return", CaseUi.WrappedLabel());
            GUILayout.EndArea();
        }
    }
}
