using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Notebook
{
    public readonly struct NotebookInputFrame
    {
        public NotebookInputFrame(bool open, bool next, bool previous)
        {
            Open = open;
            Next = next;
            Previous = previous;
        }

        public bool Open { get; }
        public bool Next { get; }
        public bool Previous { get; }
    }

    public sealed class NotebookStatement
    {
        internal NotebookStatement(string label, string text, string attributedTo)
        {
            Label = label;
            Text = text;
            AttributedTo = attributedTo;
        }

        public string Label { get; }
        public string Text { get; }
        public string AttributedTo { get; }
    }

    public sealed class NotebookEvidenceReference
    {
        internal NotebookEvidenceReference(string evidenceId, int acquiredSources, int totalSources, IReadOnlyList<NotebookStatement> statements)
        {
            EvidenceId = evidenceId;
            AcquiredSources = acquiredSources;
            TotalSources = totalSources;
            Statements = statements;
        }

        public string EvidenceId { get; }
        public int AcquiredSources { get; }
        public int TotalSources { get; }
        public bool IsComplete => AcquiredSources == TotalSources;
        public IReadOnlyList<NotebookStatement> Statements { get; }
    }

    public sealed class NotebookEntry
    {
        internal NotebookEntry(ContentSource source, string attribution, IReadOnlyList<NotebookEvidenceReference> evidence)
        {
            SourceId = source.Id;
            SourceKind = source.Kind;
            Attribution = attribution;
            OriginalText = source.Text;
            Caption = source.Caption;
            Evidence = evidence;
        }

        public string SourceId { get; }
        public string SourceKind { get; }
        public string Attribution { get; }
        public string OriginalText { get; }
        public string Caption { get; }
        public IReadOnlyList<NotebookEvidenceReference> Evidence { get; }
    }

    /// <summary>
    /// Read-only notebook presentation. It derives every visible row from the
    /// shared session's acquired source IDs; opening and reading do not alter
    /// case state or infer facts that have not been acquired.
    /// </summary>
    public sealed class NotebookController : MonoBehaviour, IModalPanel
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;

        private readonly List<NotebookEntry> entries = new List<NotebookEntry>();
        private bool isOpen;
        private int selectedIndex;

        public IReadOnlyList<NotebookEntry> Entries => entries.AsReadOnly();
        public bool IsOpen => isOpen;
        public int SelectedIndex => selectedIndex;
        public NotebookEntry SelectedEntry => entries.Count == 0 ? null : entries[selectedIndex];

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            Tick(new NotebookInputFrame(
                Input.GetKeyDown(KeyCode.N),
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow),
                Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow)));
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            RefreshEntries();
        }

        public void Tick(NotebookInputFrame input)
        {
            if (!isOpen)
            {
                if (input.Open) TryOpen();
                return;
            }
            if (!modalInput.IsOwner(this))
            {
                isOpen = false;
                return;
            }
            if (input.Next) SelectNext();
            if (input.Previous) SelectPrevious();
        }

        public bool TryOpen()
        {
            if (session == null || modalInput == null) return false;
            if (!modalInput.TryOpen(this)) return false;
            RefreshEntries();
            isOpen = true;
            return true;
        }

        public void CloseFromModalInput()
        {
            Close();
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            modalInput?.Release(this);
        }

        public void RefreshEntries()
        {
            entries.Clear();
            if (session == null || session.Content == null || session.State == null)
            {
                selectedIndex = 0;
                return;
            }
            var acquired = new HashSet<string>(session.State.AcquiredSourceIds, StringComparer.Ordinal);
            foreach (var source in session.Content.Sources.Where(item => acquired.Contains(item.Id)))
                entries.Add(CreateEntry(session.Content, source, acquired));
            selectedIndex = entries.Count == 0 ? 0 : Mathf.Clamp(selectedIndex, 0, entries.Count - 1);
        }

        public void SelectNext()
        {
            if (entries.Count > 0) selectedIndex = (selectedIndex + 1) % entries.Count;
        }

        public void SelectPrevious()
        {
            if (entries.Count > 0) selectedIndex = (selectedIndex - 1 + entries.Count) % entries.Count;
        }

        private static NotebookEntry CreateEntry(CaseContent content, ContentSource source, ISet<string> acquired)
        {
            var evidence = content.Evidence
                .Where(record => record.SourceIds.Contains(source.Id))
                .Select(record => CreateEvidenceReference(record, acquired))
                .ToArray();
            return new NotebookEntry(source, DescribeAttribution(content, source), Array.AsReadOnly(evidence));
        }

        private static NotebookEvidenceReference CreateEvidenceReference(EvidenceContent record, ISet<string> acquired)
        {
            var knownSourceCount = record.SourceIds.Count(acquired.Contains);
            var statements = record.Statements
                .Where(statement => statement.SourceIds.All(acquired.Contains))
                .Select(statement => new NotebookStatement(StatementLabel(statement.Kind), statement.Text, statement.AttributedTo))
                .ToArray();
            return new NotebookEvidenceReference(record.Id, knownSourceCount, record.SourceIds.Count, Array.AsReadOnly(statements));
        }

        private static string DescribeAttribution(CaseContent content, ContentSource source)
        {
            if (string.IsNullOrEmpty(source.SpeakerId)) return "Original " + source.Kind;
            var person = content.Residents.Concat(content.Callers).SingleOrDefault(item => item.Id == source.SpeakerId);
            return person == null ? "Original " + source.Kind : "Original " + source.Kind + " attributed to " + person.Text;
        }

        private static string StatementLabel(string kind)
        {
            switch (kind)
            {
                case "observation": return "Observation";
                case "attributed-claim": return "Attributed claim";
                case "hypothesis": return "Hypothesis";
                case "corroboration": return "Corroboration";
                default: throw new InvalidOperationException("Unknown authored statement kind.");
            }
        }
    }
}
