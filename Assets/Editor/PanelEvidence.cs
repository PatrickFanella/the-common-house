using System;
using System.IO;
using System.Linq;
using CommonHouse.Runtime;
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
        private static readonly string[] Captures = { "inspection-panel.png", "transmission-panel.png", "notebook-panel.png", "dialogue-panel.png", "opening-panel.png", "archive-panel.png", "comparison-panel.png", "routing-panel.png", "missing-page-panel.png", "roster-comparison-panel.png", "repair-account-panel.png", "consent-panel.png" };
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

        private static void Ask(string resident, string nodeId)
        {
            var target = UnityEngine.Object.FindObjectsByType<ResidentDialogueTarget>(FindObjectsSortMode.None).Single(value => value.ResidentId == resident);
            if (!target.Interact()) throw new InvalidOperationException("Could not open resident conversation.");
            var dialogue = UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>();
            for (var i = 0; i < dialogue.EligibleQuestions.Count && dialogue.EligibleQuestions[dialogue.SelectedIndex].Id != nodeId; i++)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            if (dialogue.EligibleQuestions[dialogue.SelectedIndex].Id != nodeId || !dialogue.SelectCurrent()) throw new InvalidOperationException("Missing eligible question: " + nodeId);
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
                        UnityEngine.Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>().Close();
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
                    else if (stage == 3)
                    {
                        UnityEngine.Object.FindFirstObjectByType<NotebookController>().Close();
                        var dialogue = UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>();
                        if (!dialogue.Open(DialogueCatalog.ForResident("ren")) || !dialogue.SelectCurrent()) throw new InvalidOperationException("Could not open private dialogue.");
                    }
                    else if (stage == 4)
                    {
                        UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>().Close();
                        if (!UnityEngine.Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>().OpenCare()) throw new InvalidOperationException("Could not reopen care.");
                    }
                    else if (stage == 5)
                    {
                        UnityEngine.Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>().Close();
                        if (!UnityEngine.Object.FindFirstObjectByType<ArchiveTapeTarget>().Interact()) throw new InvalidOperationException("Could not read archive tape.");
                    }
                    else if (stage == 6)
                    {
                        radio.Close();
                        var notebook = UnityEngine.Object.FindFirstObjectByType<NotebookController>();
                        if (!notebook.TryOpen() || !notebook.Comparison.Open(notebook.Entries)) throw new InvalidOperationException("Could not compare sources.");
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "T01", notebook.Entries);
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "T02", notebook.Entries);
                    }
                    else if (stage == 7)
                    {
                        UnityEngine.Object.FindFirstObjectByType<NotebookController>().Close();
                        var route = UnityEngine.Object.FindFirstObjectByType<RouteInspectionController>();
                        if (!route.Open()) throw new InvalidOperationException("Could not open route inspection.");
                        route.Tick(new RouteInspectionInputFrame(false, false, true, false));
                        route.Tick(new RouteInspectionInputFrame(false, true, true, false));
                        if (route.VerifyPrivateRoute() != RouteInspectionResult.Verified) throw new InvalidOperationException("Could not verify route.");
                    }
                    else if (stage == 8)
                    {
                        UnityEngine.Object.FindFirstObjectByType<RouteInspectionController>().Close();
                        Ask("ivo", "ivo-log-gap-request");
                    }
                    else if (stage == 9)
                    {
                        UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>().Close();
                        var ledger = new CaseInspectionLedger(UnityEngine.Object.FindFirstObjectByType<CaseSession>());
                        ledger.TryAcquire("D01"); ledger.TryAcquire("D02");
                        var notebook = UnityEngine.Object.FindFirstObjectByType<NotebookController>();
                        if (!notebook.TryOpen() || !notebook.Comparison.Open(notebook.Entries)) throw new InvalidOperationException("Could not compare documents.");
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "D01", notebook.Entries);
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "D02", notebook.Entries);
                    }
                    else if (stage == 10)
                    {
                        UnityEngine.Object.FindFirstObjectByType<NotebookController>().Close();
                        if (!UnityEngine.Object.FindFirstObjectByType<RepairFollowupTarget>().Interact()) throw new InvalidOperationException("Could not obtain Ada account.");
                        radio.Close();
                        Ask("ren", "ren-repair-account");
                    }
                    else
                    {
                        UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>().Close();
                        Ask("ren", "ren-private-contact");
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
