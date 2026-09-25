using UnityEngine;

namespace CommonHouse.Dialogue
{
    [RequireComponent(typeof(DialoguePresenter))]
    public sealed class DialogueOverlay : MonoBehaviour
    {
        private DialoguePresenter presenter;
        private Vector2 scroll;

        private void Awake()
        {
            presenter = GetComponent<DialoguePresenter>();
        }

        private void OnGUI()
        {
            if (presenter == null || !presenter.IsOpen) return;
            var wrapped = new GUIStyle(GUI.skin.label) { wordWrap = true };
            var panel = new Rect(Screen.width * 0.14f, Screen.height * 0.12f, Screen.width * 0.72f, Screen.height * 0.76f);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            scroll = GUILayout.BeginScrollView(scroll);
            if (presenter.CurrentLine != null)
            {
                GUILayout.Label("Speaker: " + presenter.CurrentLine.SpeakerId, GUI.skin.label);
                GUILayout.Label(presenter.CurrentLine.Text, wrapped);
                GUILayout.Space(16f);
            }
            var questions = presenter.EligibleQuestions;
            for (var index = 0; index < questions.Count; index++)
                GUILayout.Label((index == presenter.SelectedIndex ? "> " : "  ") + questions[index].Question, wrapped);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Up/Down] Choose    [Enter] Ask    [Escape] Close", GUI.skin.label);
            GUILayout.EndArea();
        }
    }
}
