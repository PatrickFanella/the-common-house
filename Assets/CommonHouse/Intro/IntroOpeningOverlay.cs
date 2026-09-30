using CommonHouse.Domain;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Intro
{
    [RequireComponent(typeof(IntroOpeningController))]
    public sealed class IntroOpeningOverlay : MonoBehaviour
    {
        private IntroOpeningController opening;
        private Vector2 scroll;
        private IntroReview previousReview;
        private int previousSelection = -1;
        public const string Guidance = "[W/A/S/D] Move  [Mouse or arrows] Look  [E] Interact\nFood and bedding are available. Take your time; you can return to conversations.";
        public const string CompletionGuidance = "You have a place here. The desk can wait until you are ready.";
        public const string PauseGuidance = "Paused\n[W/A/S/D] Move  [Mouse or arrows] Look  [E] Interact\n[Up/Down] Choose a panel action  [Enter] Select\n[PgUp/PgDn] Scroll  [-/=] Text size\n[Escape] Close a conversation or resume.";
        public bool IsPauseGuidanceVisible => opening != null && !opening.IsOpen && (GetComponent<ModalInputCoordinator>()?.IsPaused ?? false);
        public string VisiblePauseGuidanceText => IsPauseGuidanceVisible ? PauseGuidance : string.Empty;
        private void Awake() { opening = GetComponent<IntroOpeningController>(); }
        private void OnGUI()
        {
            if (opening == null) return;
            if (!opening.IsOpen)
            {
                var modal = GetComponent<ModalInputCoordinator>();
                if (modal != null && modal.HasOwner) return;
                if (IsPauseGuidanceVisible)
                {
                    CaseUi.HandleKeyboardScale();
                    var pausePanel = CaseUi.Panel();
                    CaseUi.DrawOpaquePanel(pausePanel);
                    GUILayout.BeginArea(new Rect(pausePanel.x + 28f, pausePanel.y + 24f, pausePanel.width - 56f, pausePanel.height - 48f));
                    scroll = CaseUi.Scroll(scroll);
                    scroll = GUILayout.BeginScrollView(scroll);
                    GUILayout.Label(VisiblePauseGuidanceText, CaseUi.WrappedLabel());
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    return;
                }
                var style = CaseUi.WrappedLabel();
                var width = Screen.width - 40f;
                var height = style.CalcHeight(new GUIContent(Guidance), width - 16f) + 12f;
                var area = new Rect(20f, Screen.height - height - 20f, width, height);
                CaseUi.DrawOpaquePanel(area);
                GUI.Label(new Rect(area.x + 8f, area.y + 6f, area.width - 16f, area.height - 12f), Guidance, style);
                if (opening.State.IsComplete)
                {
                    var complete = new Rect(20f, 20f, width, style.CalcHeight(new GUIContent(CompletionGuidance), width - 16f) + 12f);
                    CaseUi.DrawOpaquePanel(complete);
                    GUI.Label(new Rect(complete.x + 8f, complete.y + 6f, complete.width - 16f, complete.height - 12f), CompletionGuidance, style);
                }
                var inspection = GetComponent<InspectionController>();
                if (inspection != null && !string.IsNullOrEmpty(inspection.FocusedPrompt))
                {
                    var promptWidth = Mathf.Min(600f, Screen.width - 40f);
                    var promptHeight = style.CalcHeight(new GUIContent(inspection.FocusedPrompt), promptWidth - 16f) + 12f;
                    var prompt = new Rect((Screen.width - promptWidth) * .5f, Screen.height * .5f + 22f, promptWidth, promptHeight);
                    CaseUi.DrawOpaquePanel(prompt);
                    GUI.Label(new Rect(prompt.x + 8f, prompt.y + 6f, prompt.width - 16f, prompt.height - 12f), inspection.FocusedPrompt, style);
                }
                return;
            }
            var review = opening.State.ActiveReview;
            if (!ReferenceEquals(previousReview, review)) { scroll = Vector2.zero; previousReview = review; previousSelection = -1; }
            CaseUi.HandleKeyboardScale();
            var panel = CaseUi.Panel();
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, panel.height - 48f));
            GUILayout.Label(opening.VisibleTitle + (opening.State.IsPaused ? " — paused" : opening.State.TranscriptOnly ? " — transcript" : ""), CaseUi.WrappedLabel());
            scroll = CaseUi.Scroll(scroll);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            GUILayout.Label(review.Description, CaseUi.WrappedLabel());
            GUILayout.Space(12f);
            foreach (var line in review.Lines)
            {
                GUILayout.Label(line.SpeakerName + ": " + line.Text, CaseUi.WrappedLabel());
                GUILayout.Space(8f);
            }
            foreach (var source in review.Sources)
                if (source.Description != review.Description) GUILayout.Label(source.Description, CaseUi.WrappedLabel());
            GUILayout.Space(12f);
            var actions = opening.Actions;
            var button = new GUIStyle(GUI.skin.button) { wordWrap = true, fontSize = Mathf.RoundToInt(16f * CaseUi.TextScale), alignment = TextAnchor.MiddleLeft };
            for (var index = 0; index < actions.Count; index++)
            {
                if (GUILayout.Button((index == opening.SelectedIndex ? "> " : "  ") + actions[index].Text, button))
                    opening.Activate(index);
                if (Event.current.type == EventType.Repaint && index == opening.SelectedIndex && previousSelection >= 0 && previousSelection != index)
                    scroll.y = GUILayoutUtility.GetLastRect().yMin;
            }
            if (Event.current.type == EventType.Repaint) previousSelection = opening.SelectedIndex;
            GUILayout.EndScrollView();
            if (!string.IsNullOrEmpty(opening.LastActionMessage)) GUILayout.Label(opening.LastActionMessage, CaseUi.WrappedLabel());
            GUILayout.Label("[Up/Down] Choose  [Enter] Select  [Escape] Close\n[PgUp/PgDn] Scroll  [-/=] Text size", CaseUi.WrappedLabel());
            GUILayout.EndArea();
        }
    }
}
