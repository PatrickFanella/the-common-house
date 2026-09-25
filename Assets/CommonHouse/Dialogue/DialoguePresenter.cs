using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Dialogue
{
    public readonly struct DialogueInputFrame
    {
        public DialogueInputFrame(bool previous, bool next, bool submit)
        {
            Previous = previous;
            Next = next;
            Submit = submit;
        }

        public bool Previous { get; }
        public bool Next { get; }
        public bool Submit { get; }
    }

    public sealed class DialogueLine
    {
        public DialogueLine(string speakerId, string text)
        {
            SpeakerId = speakerId;
            Text = text;
        }

        public string SpeakerId { get; }
        public string Text { get; }
    }

    /// <summary>State-driven dialogue selection; the overlay only renders this presenter.</summary>
    public sealed class DialoguePresenter : MonoBehaviour, IModalPanel, ICaseStateLoadReset
    {
        [SerializeField] private ModalInputCoordinator modalInput;
        [SerializeField] private CaseSession session;
        private CaseState state;
        private IReadOnlyList<DialogueNode> nodes = DemoDialogueNodes.All;
        private DialogueLine currentLine;
        private int selectedIndex;

        public bool IsOpen { get; private set; }
        public DialogueLine CurrentLine => currentLine;
        public int SelectedIndex => selectedIndex;
        private CaseState ActiveState => session != null ? session.State : state;
        public IReadOnlyList<DialogueNode> EligibleQuestions => nodes.Where(node => node.Eligibility(ActiveState)).ToArray();

        private void Awake()
        {
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
            if (session == null) session = GetComponent<CaseSession>();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            Tick(new DialogueInputFrame(Input.GetKeyDown(KeyCode.UpArrow), Input.GetKeyDown(KeyCode.DownArrow), Input.GetKeyDown(KeyCode.Return)));
        }

        public void Configure(CaseState caseState, ModalInputCoordinator coordinator, IEnumerable<DialogueNode> authoredNodes = null)
        {
            session = null;
            state = caseState ?? throw new ArgumentNullException(nameof(caseState));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            nodes = (authoredNodes ?? Case01DialogueNodes.All).ToArray();
            if (nodes.Count == 0) throw new ArgumentException("Dialogue needs at least one authored node.", nameof(authoredNodes));
            selectedIndex = 0;
            currentLine = null;
        }

        /// <summary>Uses the session's current state, including a later validated load replacement.</summary>
        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator, IEnumerable<DialogueNode> authoredNodes = null)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            state = null;
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            nodes = (authoredNodes ?? DemoDialogueNodes.All).ToArray();
            if (nodes.Count == 0) throw new ArgumentException("Dialogue needs at least one authored node.", nameof(authoredNodes));
            selectedIndex = 0;
            currentLine = null;
        }

        public bool Open()
        {
            if (ActiveState == null || modalInput == null || !modalInput.TryOpen(this)) return false;
            IsOpen = true;
            currentLine = null;
            ClampSelection();
            return true;
        }

        /// <summary>Opens a resident-scoped authored set through a world interaction.</summary>
        public bool Open(IEnumerable<DialogueNode> authoredNodes)
        {
            if (authoredNodes == null) throw new ArgumentNullException(nameof(authoredNodes));
            var selectedNodes = authoredNodes.ToArray();
            if (selectedNodes.Length == 0) return false;
            if (IsOpen) Close();
            nodes = selectedNodes;
            selectedIndex = 0;
            currentLine = null;
            return Open();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            modalInput.Release(this);
        }

        public void CloseFromModalInput()
        {
            Close();
        }

        public void ResetAfterStateLoad()
        {
            Close();
            currentLine = null;
            selectedIndex = 0;
        }

        public void Tick(DialogueInputFrame input)
        {
            if (!IsOpen) return;
            var questions = EligibleQuestions;
            if (questions.Count == 0) return;
            if (input.Previous) selectedIndex = (selectedIndex + questions.Count - 1) % questions.Count;
            if (input.Next) selectedIndex = (selectedIndex + 1) % questions.Count;
            if (input.Submit) SelectCurrent();
        }

        public bool SelectCurrent()
        {
            if (!IsOpen) return false;
            var questions = EligibleQuestions;
            if (questions.Count == 0) return false;
            ClampSelection();
            var node = questions[selectedIndex];
            foreach (var effect in node.Effects) Apply(node.Id, effect);
            var response = string.IsNullOrEmpty(node.ResponseSourceId) ? node.Response : ActiveState.ReplaySource(node.ResponseSourceId).Text;
            currentLine = new DialogueLine(node.SpeakerId, response);
            ClampSelection();
            return true;
        }

        private void ClampSelection()
        {
            var count = EligibleQuestions.Count;
            selectedIndex = count == 0 ? 0 : Mathf.Clamp(selectedIndex, 0, count - 1);
        }

        private void Apply(string nodeId, DialogueEffectKind effect)
        {
            switch (effect)
            {
                case DialogueEffectKind.None:
                    return;
                case DialogueEffectKind.AcquirePrivateAccount:
                    ActiveState.AcquireSource("dialogue:" + nodeId + ":acquire", "C01");
                    return;
                case DialogueEffectKind.AcquireMissingPage:
                    ActiveState.AcquireSource("dialogue:" + nodeId + ":acquire", "D03");
                    return;
                case DialogueEffectKind.AcquireRenRepair:
                    ActiveState.AcquireSource("dialogue:" + nodeId + ":acquire", "C02");
                    return;
                case DialogueEffectKind.AuthorizeRefusal:
                    ActiveState.RecordRefusalAuthorization("dialogue:" + nodeId + ":refusal");
                    return;
                case DialogueEffectKind.ProtectPrivateContact:
                    ActiveState.RecordPromise("dialogue:" + nodeId + ":promise", CasePromise.ProtectPrivateContact);
                    return;
                case DialogueEffectKind.AuthorizePrivateContact:
                    ActiveState.RecordPrivateContactAgreement("dialogue:" + nodeId + ":agreement");
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect));
            }
        }
    }
}
