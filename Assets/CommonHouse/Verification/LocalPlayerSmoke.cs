using System;
using System.Collections;
using System.IO;
using System.Linq;
using CommonHouse.Aftermath;
using CommonHouse.Dialogue;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Opening;
using CommonHouse.Persistence;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Verification
{
    /// <summary>Explicit, disposable local-player proof path. It is inert outside test mode and its exact command flag.</summary>
    public sealed class LocalPlayerSmoke : MonoBehaviour
    {
        private const string SmokeArgument = "--common-house-smoke=r2";
        private string output;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartWhenExplicitlyRequested()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            if (!Environment.GetCommandLineArgs().Contains(SmokeArgument)) return;
            new GameObject("Common House local player smoke").AddComponent<LocalPlayerSmoke>();
        }
#endif

        private void Start()
        {
            output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrWhiteSpace(output))
            {
                Debug.LogError("Local player smoke requires COMMON_HOUSE_TEST_DATA_PATH.");
                Application.Quit(2);
                return;
            }
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            var smoke = RunSmoke();
            while (true)
            {
                bool moved;
                object current = null;
                try
                {
                    moved = smoke.MoveNext();
                    if (moved) current = smoke.Current;
                }
                catch (Exception error)
                {
                    Debug.LogError("Local player smoke failed: " + error);
                    WriteReceipt(false, error.Message, string.Empty, 0, string.Empty);
                    Application.Quit(1);
                    yield break;
                }
                if (!moved) yield break;
                yield return current;
            }
        }

        private IEnumerator RunSmoke()
        {
            yield return null;
            var inspection = Require<InspectionController>();
            var opening = Require<OpeningHouseholdController>();
            var dialogue = Require<DialoguePresenter>();
            var send = Require<SendConfirmationController>();
            var saves = Require<CaseRuntimeSaveController>();
            var session = Require<CaseSession>();
            var aftermath = Require<AftermathHouseholdController>();
            if (!saves.IsConfigured) throw new InvalidOperationException("Disposable development save controls were not configured.");

            opening.Close();
            Activate(inspection, FindTarget<OpeningWorldTarget>(target => target.Prompt.Contains("Receive the waiting call")));
            RequireState(session, "T01");
            Require<TransmissionLog>().Close();

            Activate(inspection, FindTarget<ResidentDialogueTarget>(target => target.ResidentId == "ren"));
            SelectDialogue(dialogue, "ren-current-call");
            dialogue.Close();
            RequireState(session, "C01");
            if (session.State.Authorization != ContactAuthorization.RefusalAuthorized)
                throw new InvalidOperationException("Ren's explicit refusal authorization was not recorded.");

            Activate(inspection, Require<SendConfirmationWorldTarget>());
            SendR2(send);
            if (!send.Confirm() || session.State.OutcomeId != "R2")
                throw new InvalidOperationException("The initial R2 send did not complete.");
            if (!saves.HandleLoadKey(KeyCode.L) || session.State.OutcomeId != null || session.State.DraftRoute != ResponseRoute.RefusalRelay)
                throw new InvalidOperationException("Pre-send reload did not restore the unresolved refusal draft.");

            Activate(inspection, Require<SendConfirmationWorldTarget>());
            SendR2(send);
            if (!send.Confirm() || session.State.OutcomeId != "R2")
                throw new InvalidOperationException("The restored R2 send did not complete.");
            send.Close();

            Activate(inspection, Require<AftermathWorldTarget>());
            if (!aftermath.Acknowledge() || session.State.Phase != CasePhase.AftermathCompleted)
                throw new InvalidOperationException("The refusal aftermath did not complete.");
            if (!saves.Save()) throw new InvalidOperationException("Post-send save failed.");
            if (!saves.HandleLoadKey(KeyCode.L) || session.State.OutcomeId != "R2" || session.State.Phase != CasePhase.AftermathCompleted)
                throw new InvalidOperationException("Post-send reload did not restore R2 aftermath.");
            var outgoing = session.State.Disclosures.Count(disclosure => disclosure.EventId == "radio-send:R2");
            if (outgoing != 1) throw new InvalidOperationException("Expected exactly one outgoing R2 disclosure, found " + outgoing + ".");

            Activate(inspection, Require<AftermathWorldTarget>());
            var screenshot = Path.Combine(output, "smoke", "r2-aftermath.png");
            Directory.CreateDirectory(Path.GetDirectoryName(screenshot));
            ScreenCapture.CaptureScreenshot(screenshot);
            yield return new WaitForEndOfFrame();
            for (var frame = 0; frame < 8 && !File.Exists(screenshot); frame++) yield return null;
            if (!File.Exists(screenshot) || new FileInfo(screenshot).Length == 0)
                throw new InvalidOperationException("Player screenshot was not written.");
            WriteReceipt(true, "R2 player smoke passed.", screenshot, outgoing, saves.SavePath);
            Application.Quit(0);
        }

        private static void SendR2(SendConfirmationController send)
        {
            if (!send.Prepare(ResponseRoute.RefusalRelay) || send.Summary == null || !send.Summary.CanSend)
                throw new InvalidOperationException("The refusal response could not be prepared for review.");
        }

        private static void RequireState(CaseSession session, string source)
        {
            if (session.State.OutcomeId != null || !session.State.AcquiredSourceIds.Contains(source))
                throw new InvalidOperationException("Expected scene interaction to record " + source + ".");
        }

        private static void SelectDialogue(DialoguePresenter dialogue, string nodeId)
        {
            for (var count = 0; count < dialogue.EligibleQuestions.Count && dialogue.EligibleQuestions[dialogue.SelectedIndex].Id != nodeId; count++)
                dialogue.Tick(new DialogueInputFrame(false, true, false));
            if (dialogue.EligibleQuestions.Count == 0 || dialogue.EligibleQuestions[dialogue.SelectedIndex].Id != nodeId || !dialogue.SelectCurrent())
                throw new InvalidOperationException("Required dialogue node was unavailable: " + nodeId);
        }

        private static void Activate(InspectionController inspection, Component target)
        {
            var collider = target.GetComponent<Collider>();
            var camera = Camera.main;
            if (collider == null || camera == null) throw new InvalidOperationException("Scene interaction target is not focusable.");
            var center = collider.bounds.center;
            camera.transform.position = center - Vector3.forward * 0.9f;
            camera.transform.LookAt(center);
            Physics.SyncTransforms();
            inspection.RefreshFocus();
            if (inspection.CurrentWorldFocus != target || !inspection.TryActivateFocused())
                throw new InvalidOperationException("Could not activate focused scene target: " + target.name);
        }

        private static T Require<T>() where T : Component
        {
            var component = FindAnyObjectByType<T>();
            if (component == null) throw new InvalidOperationException("Missing scene component: " + typeof(T).Name);
            return component;
        }

        private static T FindTarget<T>(Func<T, bool> predicate) where T : Component
        {
            var target = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault(predicate);
            if (target == null) throw new InvalidOperationException("Missing required scene target: " + typeof(T).Name);
            return target;
        }

        private void WriteReceipt(bool passed, string message, string screenshot, int outgoing, string savePath)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(output, "smoke"));
                File.WriteAllText(Path.Combine(output, "smoke", "smoke-result.json"),
                    "{\n  \"status\": \"" + (passed ? "passed" : "failed") + "\",\n  \"route\": \"R2\",\n  \"message\": \"" + Escape(message) + "\",\n  \"screenshot\": \"" + Escape(screenshot) + "\",\n  \"outgoing_disclosures\": " + outgoing + ",\n  \"persistent_data_path\": \"" + Escape(Application.persistentDataPath) + "\",\n  \"save_path\": \"" + Escape(savePath) + "\",\n  \"screen_width\": " + Screen.width + ",\n  \"screen_height\": " + Screen.height + "\n}\n");
            }
            catch (Exception receiptError)
            {
                Debug.LogError("Could not write player smoke receipt: " + receiptError.Message);
            }
        }

        private static string Escape(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }
}
