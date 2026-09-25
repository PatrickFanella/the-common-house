using System;
using System.Linq;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Persistence
{
    /// <summary>
    /// Scene-level, explicitly configured development save controls. It owns no
    /// user or production save location: callers inject a store or a chosen
    /// development root, and validated loads replace the active CaseSession.
    /// </summary>
    public sealed class CaseRuntimeSaveController : MonoBehaviour, ICaseSaveCheckpoint
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private bool developmentEnvironmentEnabled;
        private CaseSaveStore store;
        private string lastStatus = string.Empty;

        public bool IsConfigured => store != null && session != null;
        public string LastStatus => lastStatus;
        public string SavePath => store == null ? string.Empty : store.SavePath;
        /// <summary>Matches the condition under which the development save HUD is rendered.</summary>
        public bool IsHudVisible
        {
            get
            {
                var modalInput = GetComponent<ModalInputCoordinator>();
                return (modalInput == null || (!modalInput.HasOwner && !modalInput.IsPaused)) &&
                    (IsConfigured || !string.IsNullOrEmpty(lastStatus));
            }
        }

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
        }

        private void Start()
        {
            if (developmentEnvironmentEnabled && store == null) ConfigureFromDevelopmentEnvironment();
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            if (Input.GetKeyDown(KeyCode.F5)) Save();
            if (Input.GetKeyDown(KeyCode.F8)) SaveCheckpoint();
            if (Input.GetKeyDown(KeyCode.F9)) Load();
        }

        private void OnGUI()
        {
            if (!IsHudVisible) return;
            CaseUi.HandleKeyboardScale();
            var wrapped = CaseUi.WrappedLabel();
            var width = Mathf.Max(120f, Mathf.Min(Screen.width - 24f, 500f));
            var controls = "[F5] Save  [F8] Pre-send checkpoint  [F9] Load  [-/=] Text size";
            var height = 20f + wrapped.CalcHeight(new GUIContent("Development save"), width - 24f)
                + wrapped.CalcHeight(new GUIContent(controls), width - 24f)
                + wrapped.CalcHeight(new GUIContent(lastStatus), width - 24f);
            var panel = new Rect(12f, Mathf.Max(12f, Screen.height - height - 12f), width, height);
            CaseUi.DrawOpaquePanel(panel);
            GUILayout.BeginArea(new Rect(panel.x + 12f, panel.y + 10f, panel.width - 24f, panel.height - 20f));
            GUILayout.Label("Development save", wrapped);
            GUILayout.Label(controls, wrapped);
            if (!string.IsNullOrEmpty(lastStatus)) GUILayout.Label(lastStatus, wrapped);
            GUILayout.EndArea();
        }

        public void Configure(CaseSession caseSession, CaseSaveStore caseStore)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            store = caseStore ?? throw new ArgumentNullException(nameof(caseStore));
            if (session.Content == null) throw new InvalidOperationException("Case session must have content before persistence is configured.");
            lastStatus = "Development save controls configured.";
        }

        /// <summary>Creates an explicit local-development store; callers choose the root.</summary>
        public void ConfigureDevelopment(CaseSession caseSession, string explicitDevelopmentRoot)
        {
            if (caseSession == null) throw new ArgumentNullException(nameof(caseSession));
            if (caseSession.Content == null) throw new InvalidOperationException("Case session must have content before persistence is configured.");
            Configure(caseSession, new CaseSaveStore(caseSession.Content, explicitDevelopmentRoot));
        }

        /// <summary>
        /// Opts this scene into an environment-provided development root. The
        /// environment must name the root explicitly; no user or production
        /// location is inferred as a fallback.
        /// </summary>
        public void EnableDevelopmentEnvironment()
        {
            developmentEnvironmentEnabled = true;
        }

        public bool Save()
        {
            if (!CanOperate()) return false;
            try
            {
                store.Save(session.State);
                lastStatus = "Development save written.";
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException || exception is System.IO.IOException)
            {
                lastStatus = "Save failed: " + exception.Message;
                return false;
            }
        }

        public bool SaveCheckpoint()
        {
            if (!Save()) return false;
            lastStatus = "Pre-send checkpoint written.";
            return true;
        }

        public bool Load()
        {
            if (!CanOperate()) return false;
            try
            {
                // CaseSaveStore finishes parsing and validation before this point;
                // a bad file therefore cannot clear the running session or panels.
                var result = store.Load();
                // Validation succeeded, so the old modal may now close. Do this
                // before replacement: a stale confirmation summary must never
                // cancel or defer the freshly loaded domain state.
                CloseActiveModalBeforeStateReplacement();
                session.ReplaceState(result.State);
                // Resetters are intentionally non-mutating. Run them after the
                // replacement so derived lists rebuild from loaded facts rather
                // than from the previous scene state.
                ResetPresentationForLoadedState();
                lastStatus = result.RecoveredFromBackup
                    ? "Loaded validated backup development save."
                    : "Loaded validated development save.";
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is InvalidOperationException || exception is System.IO.IOException)
            {
                lastStatus = "Load failed: " + exception.Message;
                return false;
            }
        }

        private bool CanOperate()
        {
            if (session == null || session.Content == null || session.State == null || store == null)
            {
                lastStatus = "Save controls require an explicit development store.";
                return false;
            }
            return true;
        }

        private void ConfigureFromDevelopmentEnvironment()
        {
            if (session == null || session.Content == null)
            {
                lastStatus = "Development save controls need an initialized case session.";
                return;
            }
            var environment = Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV");
            if (string.Equals(environment, "test", StringComparison.Ordinal))
            {
                store = new CaseSaveStore(session.Content, System.IO.Path.Combine(Application.persistentDataPath, "test-saves"));
                lastStatus = "Isolated test save controls configured.";
                return;
            }
            if (!string.Equals(environment, "development", StringComparison.Ordinal))
            {
                lastStatus = "Development save controls are disabled outside the explicit development environment.";
                return;
            }
            const string variable = "COMMON_HOUSE_SAVE_PATH";
            var root = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(root) || !System.IO.Path.IsPathRooted(root))
            {
                lastStatus = "Development save controls require absolute " + variable + ".";
                return;
            }
            try
            {
                store = new CaseSaveStore(session.Content, root);
                lastStatus = "Development save controls configured.";
            }
            catch (ArgumentException exception)
            {
                lastStatus = "Development save controls unavailable: " + exception.Message;
            }
        }

        private void CloseActiveModalBeforeStateReplacement()
        {
            // Not every modal needs a derived-state cache. Dispatch through the
            // owner’s non-mutating close contract, but only after store validation.
            var modalInput = GetComponent<ModalInputCoordinator>();
            if (modalInput != null && modalInput.HasOwner) modalInput.HandleCancel();
        }

        private void ResetPresentationForLoadedState()
        {
            foreach (var resetter in GetComponents<MonoBehaviour>().OfType<ICaseStateLoadReset>().ToArray())
                resetter.ResetAfterStateLoad();
        }
    }
}
