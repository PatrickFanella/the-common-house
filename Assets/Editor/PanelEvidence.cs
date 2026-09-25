using System;
using System.IO;
using CommonHouse.Interaction;
using CommonHouse.Outpost;
using CommonHouse.Radio;
using CommonHouse.Notebook;
using CommonHouse.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CommonHouse.Editor
{
    // Explicit isolated graphical run; captures the rendered Game view including IMGUI.
    [InitializeOnLoad]
    public static class PanelEvidence
    {
        private const string ActiveKey = "CommonHouse.PanelEvidence.Active";
        private static int frames;
        private static bool requested;
        private static int stage;
        private static readonly string[] Captures = { "inspection-panel.png", "transmission-panel.png", "notebook-panel.png", "dialogue-panel.png" };
        static PanelEvidence()
        {
            EditorApplication.update += Update;
        }

        public static void Capture()
        {
            if (System.Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Panel evidence requires disposable test state.");
            EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            SessionState.SetBool(ActiveKey, true);
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(gameViewType);
            window.Show();
            window.Focus();
            window.maximized = true;
            EditorApplication.isPlaying = true;
        }

        private static void Update()
        {
            if (!SessionState.GetBool(ActiveKey, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            try
            {
                var output = System.Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
                if (++frames < 90) return;
                var path = Path.Combine(output, Captures[stage]);
                if (!requested)
                {
                    var controller = UnityEngine.Object.FindFirstObjectByType<InspectionController>();
                    var radio = UnityEngine.Object.FindFirstObjectByType<TransmissionLog>();
                    if (stage == 0)
                    {
                        controller.RefreshFocus();
                        if (!controller.TryOpenFocused()) throw new InvalidOperationException("Scene notice was not reachable.");
                    }
                    else if (stage == 1)
                    {
                        controller.Close();
                        if (radio.Receive("T01") != TransmissionReceiveResult.Received) throw new InvalidOperationException("Could not seed isolated received call.");
                        if (!radio.Open() || !radio.OpenSelectedTranscript()) throw new InvalidOperationException("Could not open received transcript.");
                    }
                    else if (stage == 2)
                    {
                        radio.Close();
                        if (!UnityEngine.Object.FindFirstObjectByType<NotebookController>().TryOpen()) throw new InvalidOperationException("Could not open notebook.");
                    }
                    else
                    {
                        UnityEngine.Object.FindFirstObjectByType<NotebookController>().Close();
                        var dialogue = UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>();
                        if (!dialogue.Open() || !dialogue.SelectCurrent()) throw new InvalidOperationException("Could not open dialogue.");
                    }
                    requested = true;
                    frames = 0;
                    return;
                }
                if (frames == 90) ScreenCapture.CaptureScreenshot(path);
                if (frames < 120 || !File.Exists(path)) return;
                if (++stage < Captures.Length)
                {
                    frames = 0;
                    requested = false;
                    return;
                }
                File.WriteAllText(Path.Combine(output, "panel-evidence.txt"),
                    "Actual Game view including IMGUI; explicit scripted interaction, not a human playtest.\n" +
                    SystemInfo.graphicsDeviceName + "\n" + Screen.width + "x" + Screen.height + "\n");
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(1);
            }
        }
    }
}
