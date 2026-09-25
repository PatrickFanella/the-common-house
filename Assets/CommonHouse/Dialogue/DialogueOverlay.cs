using UnityEngine;
using CommonHouse.Interaction;

namespace CommonHouse.Dialogue
{
    [RequireComponent(typeof(DialoguePresenter))]
    public sealed class DialogueOverlay : MonoBehaviour
    {
        private DialoguePresenter presenter;
        private Vector2 scroll;
        private int previousSelection = -1;

        private void Awake()
        {
            presenter = GetComponent<DialoguePresenter>();
        }

        private void OnGUI()
        {
            if (presenter == null || !presenter.IsOpen) return;
            CaseUi.HandleKeyboardScale();
            var wrapped = CaseUi.WrappedLabel();
            var panel = CaseUi.Panel();
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            scroll = CaseUi.Scroll(scroll); scroll = GUILayout.BeginScrollView(scroll);
            if (presenter.CurrentLine != null)
            {
                GUILayout.Label("Speaker: " + presenter.CurrentLine.SpeakerId, CaseUi.WrappedLabel());
                GUILayout.Label(presenter.CurrentLine.Text, wrapped);
                GUILayout.Space(16f);
            }
            var questions = presenter.EligibleQuestions;
            for (var index = 0; index < questions.Count; index++)
            {
                GUILayout.Label((index == presenter.SelectedIndex ? "> " : "  ") + questions[index].Question, wrapped);
                if (Event.current.type == EventType.Repaint && index == presenter.SelectedIndex && previousSelection >= 0 && previousSelection != index)
                    scroll.y = GUILayoutUtility.GetLastRect().yMin;
            }
            if (Event.current.type == EventType.Repaint) previousSelection = presenter.SelectedIndex;
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Up/Down] Choose  [PgUp/PgDn] Scroll  [-/=] Text size  [Enter] Ask  [Escape] Close", CaseUi.WrappedLabel());
            GUILayout.EndArea();
        }
    }
}
