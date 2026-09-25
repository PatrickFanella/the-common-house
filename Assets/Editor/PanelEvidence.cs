using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommonHouse.Runtime;
using CommonHouse.Domain;
using CommonHouse.Aftermath;
using CommonHouse.Hints;
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
        private static EditorWindow gameView;
        private static Type gameViewType;
        private static readonly string[] Captures = { "inspection-panel.png", "transmission-panel.png", "notebook-panel.png", "dialogue-panel.png", "opening-panel.png", "archive-panel.png", "comparison-panel.png", "routing-panel.png", "missing-page-panel.png", "roster-comparison-panel.png", "repair-account-panel.png", "consent-panel.png", "send-private-panel.png", "aftermath-private-panel.png", "send-refusal-panel.png", "aftermath-refusal-panel.png", "send-public-panel.png", "aftermath-public-panel.png", "hint-panel.png", "comparison-scrolled-panel.png" };
        private static readonly string[] GuidanceCaptures = { "welcome-controls.png", "pause-controls.png" };
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
            gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            // A utility Game view is not docked into the existing editor
            // layout, whose content dimensions vary with the host desktop.
            gameView = EditorWindow.GetWindow(gameViewType, true, "Case evidence");
            gameView.maximized = false;
            gameView.Show();
            ConfigureGameView(gameView, gameViewType);
            EditorApplication.isPlaying = true;
        }

        [Serializable]
        private sealed class CaptureSettings
        {
            public int width = 800;
            public int height = 600;
            public float scale = 1f;
            public bool guidance;
        }

        private static CaptureSettings Settings()
        {
            var path = Path.Combine(System.Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH"), "capture-settings.json");
            return File.Exists(path) ? JsonUtility.FromJson<CaptureSettings>(File.ReadAllText(path)) : new CaptureSettings();
        }

        private static void ConfigureGameView(EditorWindow window, Type gameViewType)
        {
            var settings = Settings();
            var assembly = typeof(EditorWindow).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes", true);
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
            var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType", true);
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(groupType, "Standalone") });
            var sizeType = assembly.GetType("UnityEditor.GameViewSize", true);
            var modeType = assembly.GetType("UnityEditor.GameViewSizeType", true);
            var size = Activator.CreateInstance(sizeType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, new object[] { Enum.Parse(modeType, "FixedResolution"), settings.width, settings.height, "Case evidence" }, null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            var count = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null);
            var index = count - 1;
            gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(window, index);
            // In recent Editors the public setter only updates the menu state;
            // this callback commits the fixed render target used by Screen.
            var callback = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.NonPublic);
            callback?.Invoke(window, new object[] { index, null });
            window.Repaint();
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
                ++frames;
                // Play-mode layout restoration can replace the pre-play size selection.
                if (frames == 30 && stage == 0)
                {
                    // Entering play mode reloads this Editor domain, so refresh
                    // both reflection state and the Game view after the layout
                    // has finished restoring.
                    gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                    gameView = EditorWindow.GetWindow(gameViewType, true, "Case evidence");
                    gameView.maximized = false;
                    gameView.position = new Rect(40f, 40f, 860f, 680f);
                    ConfigureGameView(gameView, gameViewType);
                }
                if (frames < 90) return;
                var settings = Settings();
                CaseUi.TextScale = settings.scale;
                if (Screen.width != settings.width || Screen.height != settings.height)
                    throw new InvalidOperationException($"Game view is {Screen.width}x{Screen.height}; expected {settings.width}x{settings.height}.");
                var captures = settings.guidance ? GuidanceCaptures : Captures;
                var path = Path.Combine(output, captures[stage]);
                if (!requested)
                {
                    var controller = UnityEngine.Object.FindFirstObjectByType<InspectionController>();
                    var radio = UnityEngine.Object.FindFirstObjectByType<TransmissionLog>();
                    if (settings.guidance)
                    {
                        var opening = UnityEngine.Object.FindFirstObjectByType<CommonHouse.Opening.OpeningHouseholdController>();
                        if (stage == 0 && !opening.IsCarePanelOpen) throw new InvalidOperationException("Welcome did not open.");
                        if (stage == 1)
                        {
                            opening.Close();
                            var modal = UnityEngine.Object.FindFirstObjectByType<ModalInputCoordinator>();
                            modal.HandleCancel();
                            if (modal.HasOwner || !modal.GetComponent<CommonHouse.Player.FirstPersonMovement>().MenuOpen)
                                throw new InvalidOperationException("Plain pause did not open.");
                        }
                    }
                    else if (stage == 0)
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
                    else if (stage == 11)
                    {
                        UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>().Close();
                        Ask("ren", "ren-private-contact");
                    }
                    else if (stage == 12 || stage == 14 || stage == 16)
                    {
                        UnityEngine.Object.FindFirstObjectByType<DialoguePresenter>().Close();
                        UnityEngine.Object.FindFirstObjectByType<AftermathHouseholdController>().Close();
                        var session = UnityEngine.Object.FindFirstObjectByType<CaseSession>();
                        if (stage != 12)
                        {
                            var state = new CaseState(session.Content);
                            state.AcquireSource("capture-call", "T01");
                            state.AcquireSource("capture-private-account", "C01");
                            state.RecordRefusalAuthorization("capture-refusal");
                            session.ReplaceState(state);
                        }
                        var send = UnityEngine.Object.FindFirstObjectByType<SendConfirmationController>();
                        var route = stage == 12 ? ResponseRoute.PrivateHeadset : stage == 14 ? ResponseRoute.RefusalRelay : ResponseRoute.RoomSpeaker;
                        if (!send.Open() || !send.Prepare(route)) throw new InvalidOperationException("Could not prepare send panel.");
                    }
                    else if (stage == 13 || stage == 15 || stage == 17)
                    {
                        var send = UnityEngine.Object.FindFirstObjectByType<SendConfirmationController>();
                        if (stage == 17) send.TogglePublicDisclosureAcknowledgement();
                        if (!send.Confirm()) throw new InvalidOperationException("Could not confirm displayed send.");
                        send.Close();
                        var aftermath = UnityEngine.Object.FindFirstObjectByType<AftermathHouseholdController>();
                        if (!aftermath.Open() || !aftermath.Acknowledge()) throw new InvalidOperationException("Could not complete matching aftermath.");
                    }
                    else if (stage == 18)
                    {
                        UnityEngine.Object.FindFirstObjectByType<AftermathHouseholdController>().Close();
                        var session = UnityEngine.Object.FindFirstObjectByType<CaseSession>();
                        session.ReplaceState(new CaseState(session.Content));
                        if (!UnityEngine.Object.FindFirstObjectByType<InvestigationHintController>().RequestHint()) throw new InvalidOperationException("Could not request optional hint.");
                    }
                    else
                    {
                        UnityEngine.Object.FindFirstObjectByType<InvestigationHintController>().CloseFromModalInput();
                        radio.Receive("T01"); radio.Receive("T02");
                        var notebook = UnityEngine.Object.FindFirstObjectByType<NotebookController>();
                        if (!notebook.TryOpen() || !notebook.Comparison.Open(notebook.Entries)) throw new InvalidOperationException("Could not compare captioned sources.");
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Left, "T01", notebook.Entries);
                        notebook.Comparison.SetSide(EvidenceComparisonSide.Right, "T02", notebook.Entries);
                        var overlay = notebook.GetComponent<NotebookOverlay>();
                        // Apply the same scroll operation as End; this is scripted evidence, not hardware input.
                        foreach (var field in new[] { "leftComparisonScrollPosition", "rightComparisonScrollPosition" })
                            typeof(NotebookOverlay).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(overlay, CaseUi.ApplyScrollKey(Vector2.zero, KeyCode.End));
                    }
                    requested = true;
                    frames = 0;
                    return;
                }
                if (frames == 90) ScreenCapture.CaptureScreenshot(path);
                if (frames < 120 || !File.Exists(path)) return;
                if (++stage < captures.Length)
                {
                    frames = 0;
                    requested = false;
                    return;
                }
                File.WriteAllText(Path.Combine(output, "panel-evidence.txt"),
                    "Actual Game view including IMGUI; explicit scripted interaction, not a human playtest.\n" +
                    SystemInfo.graphicsDeviceName + "\n" + Screen.width + "x" + Screen.height + "\nText scale: " + CaseUi.TextScale + "\n");
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
