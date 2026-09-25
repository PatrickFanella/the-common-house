using CommonHouse.Content;
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
            var panel = new Rect(Screen.width * 0.14f, Screen.height * 0.12f, Screen.width * 0.72f, Screen.height * 0.76f);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.Label("At the radio desk", GUI.skin.label);
            GUILayout.Space(12f);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            foreach (var line in OpeningContent.CareLines)
            {
                GUILayout.Label(line.Speaker, GUI.skin.label);
                GUILayout.Label(line.Text, wrapped);
                GUILayout.Space(10f);
            }
            GUILayout.Label(OpeningContent.CallAvailableText, wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Escape] Return to the room", GUI.skin.label);
            GUILayout.EndArea();
        }
    }
}
