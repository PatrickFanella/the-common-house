using UnityEngine;

namespace CommonHouse.Notebook
{
    /// <summary>Temporary IMGUI reader for acquired source text and known evidence status.</summary>
    [RequireComponent(typeof(NotebookController))]
    public sealed class NotebookOverlay : MonoBehaviour
    {
        private NotebookController notebook;
        private Vector2 scrollPosition;

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
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (notebook.Entries.Count == 0)
            {
                GUILayout.Label("No acquired evidence yet.", wrappedText);
            }
            else
            {
                var entry = notebook.SelectedEntry;
                GUILayout.Label((notebook.SelectedIndex + 1) + "/" + notebook.Entries.Count + "  " + entry.SourceId + " — " + entry.Attribution, wrappedText);
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
            GUILayout.FlexibleSpace();
            GUILayout.Label("[Left/Right or Up/Down] Select evidence    [Escape] Close", wrappedText);
            GUILayout.EndArea();
        }
    }
}
