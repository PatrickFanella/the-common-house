using UnityEngine;

namespace CommonHouse.Notebook
{
    /// <summary>Temporary IMGUI reader for acquired source text and known evidence status.</summary>
    [RequireComponent(typeof(NotebookController))]
    public sealed class NotebookOverlay : MonoBehaviour
    {
        private NotebookController notebook;
        private Vector2 scrollPosition;
        private Vector2 leftComparisonScrollPosition;
        private Vector2 rightComparisonScrollPosition;

        private void Awake()
        {
            notebook = GetComponent<NotebookController>();
        }

        private void OnGUI()
        {
            if (notebook == null || !notebook.IsOpen) return;
            var panel = new Rect(Screen.width * 0.12f, Screen.height * 0.1f, Screen.width * 0.76f, Screen.height * 0.8f);
            GUI.Box(panel, "Evidence notebook");
            GUILayout.BeginArea(new Rect(panel.x + 24f, panel.y + 32f, panel.width - 48f, panel.height - 56f));
            var wrappedText = new GUIStyle(GUI.skin.label) { wordWrap = true };
            if (notebook.Comparison.IsOpen)
            {
                DrawComparison(wrappedText, (panel.width - 56f) * 0.5f);
            }
            else
            {
                DrawNotebook(wrappedText);
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(notebook.Comparison.IsOpen
                ? "[Left/Right or Up/Down] Select  [Tab] Choose side  [Enter] Set selected  [C] Close comparison  [Escape] Return to notebook"
                : "[Left/Right or Up/Down] Select evidence  [C] Compare selected evidence  [Escape] Close", wrappedText);
            GUILayout.EndArea();
        }

        private void DrawNotebook(GUIStyle wrappedText)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (notebook.Entries.Count == 0)
            {
                GUILayout.Label("No acquired evidence yet.", wrappedText);
            }
            else
            {
                var entry = notebook.SelectedEntry;
                GUILayout.Label((notebook.SelectedIndex + 1) + "/" + notebook.Entries.Count + "  " + entry.SourceId + " — " + entry.Attribution, wrappedText);
                GUILayout.Label(entry.RecordedAt, wrappedText);
                GUILayout.Space(10f);
                GUILayout.Label(entry.OriginalText, wrappedText);
                if (!string.IsNullOrWhiteSpace(entry.Caption)) GUILayout.Label(entry.Caption, wrappedText);
                foreach (var evidence in entry.Evidence)
                {
                    GUILayout.Space(10f);
                    GUILayout.Label(evidence.EvidenceId + " — " + evidence.AcquiredSources + "/" + evidence.TotalSources + " source observations acquired", wrappedText);
                    foreach (var statement in evidence.Statements)
                        GUILayout.Label(statement.Label + ": " + statement.Text, wrappedText);
                }
            }
            GUILayout.EndScrollView();
        }

        private void DrawComparison(GUIStyle wrappedText, float sideWidth)
        {
            var candidate = notebook.SelectedEntry;
            var destination = notebook.Comparison.ActiveSide;
            GUILayout.Label("Setting " + destination + " side", wrappedText);
            GUILayout.Label(candidate == null
                ? "No acquired source is selected."
                : "Selected source: " + candidate.SourceId + " — " + candidate.Attribution, wrappedText);
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            DrawComparisonSide(EvidenceComparisonSide.Left, notebook.Comparison.ResolveLeft(notebook.Entries), ref leftComparisonScrollPosition, wrappedText, sideWidth);
            GUILayout.Space(8f);
            DrawComparisonSide(EvidenceComparisonSide.Right, notebook.Comparison.ResolveRight(notebook.Entries), ref rightComparisonScrollPosition, wrappedText, sideWidth);
            GUILayout.EndHorizontal();
        }

        private void DrawComparisonSide(EvidenceComparisonSide side, NotebookEntry entry, ref Vector2 scroll, GUIStyle wrappedText, float width)
        {
            var active = notebook.Comparison.ActiveSide == side ? "> " : string.Empty;
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(width), GUILayout.ExpandHeight(true));
            GUILayout.Label(active + side + " side", wrappedText);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            if (entry == null)
            {
                GUILayout.Label("Select an acquired source with Enter.", wrappedText);
            }
            else
            {
                GUILayout.Label(entry.SourceId + " — " + entry.Attribution, wrappedText);
                GUILayout.Label(entry.RecordedAt, wrappedText);
                GUILayout.Space(8f);
                GUILayout.Label(entry.OriginalText, wrappedText);
                if (!string.IsNullOrWhiteSpace(entry.Caption)) GUILayout.Label(entry.Caption, wrappedText);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
