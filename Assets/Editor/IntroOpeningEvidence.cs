using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommonHouse.Intro;
using CommonHouse.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CommonHouse.Editor
{
    [InitializeOnLoad]
    public static class IntroOpeningEvidence
    {
        private const string Key = "CommonHouse.IntroOpeningEvidence.Active";
        private static readonly string[] captures = { "arrival-world.png", "arrival-choice.png", "return-warning.png", "warning-actions.png", "welcome.png", "welcome-actions.png", "arrival-pause.png",
            "radio-programs.png", "radio-actions.png", "office-greeting.png", "greeting-actions.png", "orientation.png", "orientation-actions.png", "closing-choice.png", "closing-actions.png", "arrival-complete.png" };
        private static int stage, frames;
        private static bool requested;
        [Serializable] private sealed class Settings { public float scale = 1f; }
        static IntroOpeningEvidence() { EditorApplication.update += Update; }
        public static void Capture()
        {
            if (Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test") throw new InvalidOperationException("Disposable test context is required.");
            EditorSceneManager.OpenScene(IntroOpeningController.ScenePath);
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView", true);
            var window = EditorWindow.GetWindow(type, true, "Arrival evidence");
            window.maximized = false;
            window.Show();
            // Use the repository's already-qualified fixed Game-view setup.
            typeof(PanelEvidence).GetMethod("ConfigureGameView", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { window, type });
            SessionState.SetBool(Key, true);
            EditorApplication.isPlaying = true;
        }
        private static void Open(string targetName)
        {
            var inspection = UnityEngine.Object.FindFirstObjectByType<InspectionController>();
            var target = UnityEngine.Object.FindObjectsByType<IntroOpeningTarget>(FindObjectsSortMode.None).Single(item => item.name == targetName);
            var capsule = inspection.GetComponent<CharacterController>();
            var camera = inspection.GetComponentInChildren<Camera>();
            capsule.enabled = false;
            inspection.transform.position = new Vector3(target.transform.position.x, .05f, target.transform.position.z - 1.5f);
            inspection.transform.rotation = Quaternion.identity;
            camera.transform.LookAt(target.transform.position);
            capsule.enabled = true;
            Physics.SyncTransforms();
            if (!inspection.TryActivateFocused()) throw new InvalidOperationException("Unreachable capture target " + targetName);
        }
        private static void Update()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            if (++frames < 90) return;
            try
            {
                var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
                var path = Path.Combine(output, captures[stage]);
                var opening = UnityEngine.Object.FindFirstObjectByType<IntroOpeningController>();
                if (opening == null) return;
                if (!requested)
                {
                    var settings = JsonUtility.FromJson<Settings>(File.ReadAllText(Path.Combine(output, "capture-settings.json")));
                    CaseUi.TextScale = settings.scale;
                    if (Screen.width != 800 || Screen.height != 600) throw new InvalidOperationException("Expected an 800x600 Game view.");
                    if (stage == 0) UnityEngine.Object.FindFirstObjectByType<InspectionController>().RefreshFocus();
                    else if (stage == 1)
                    {
                        Open("Ren at threshold");
                        var index = opening.Actions.Select((action, i) => new { action, i }).Single(item => item.action.OptionId == "IN-Q01-C").i;
                        opening.Activate(index);
                    }
                    else if (stage == 2) { opening.CloseFromModalInput(); Open("Return warning"); }
                    else if (stage == 3 || stage == 5 || stage == 8 || stage == 10 || stage == 12 || stage == 14)
                    {
                        var overlay = opening.GetComponent<IntroOpeningOverlay>();
                        typeof(IntroOpeningOverlay).GetField("scroll", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(overlay, CaseUi.ApplyScrollKey(Vector2.zero, KeyCode.End));
                    }
                    else if (stage == 4) { opening.CloseFromModalInput(); Open("Food and bedding"); }
                    else if (stage == 6) { opening.CloseFromModalInput(); opening.GetComponent<ModalInputCoordinator>().HandleCancel(); }
                    else if (stage == 7) { opening.GetComponent<ModalInputCoordinator>().HandleCancel(); Open("Radio programs"); }
                    else if (stage == 9) { opening.CloseFromModalInput(); Open("Office greeting"); }
                    else if (stage == 11) { opening.CloseFromModalInput(); Open("Orientation summary"); }
                    else if (stage == 13)
                    {
                        var acknowledge = opening.Actions.Select((action, i) => new { action, i }).Single(item => item.action.Kind == IntroActionKind.Acknowledge).i;
                        if (!opening.Activate(acknowledge)) throw new InvalidOperationException("Orientation acknowledgment failed.");
                        Open("Tomorrow's shift");
                        var choice = opening.Actions.Select((action, i) => new { action, i }).Single(item => item.action.OptionId == "IN-Q03-B").i;
                        opening.Activate(choice);
                    }
                    else if (stage == 15)
                    {
                        var acknowledge = opening.Actions.Select((action, i) => new { action, i }).Single(item => item.action.Kind == IntroActionKind.Acknowledge).i;
                        if (!opening.Activate(acknowledge) || !opening.State.IsComplete) throw new InvalidOperationException("Intro did not complete.");
                    }
                    requested = true;
                    frames = 0;
                    return;
                }
                if (frames == 90) ScreenCapture.CaptureScreenshot(path);
                if (frames < 120 || !File.Exists(path)) return;
                if (++stage < captures.Length) { frames = 0; requested = false; return; }
                File.WriteAllText(Path.Combine(output, "intro-evidence.txt"), "Actual Game view; scripted focused interactions, not a human playtest.\n800x600; text scale " + CaseUi.TextScale + "\n" + SystemInfo.graphicsDeviceName + "\n");
                SessionState.SetBool(Key, false);
                EditorApplication.Exit(0);
            }
            catch (Exception error) { Debug.LogException(error); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
        }
    }
}
