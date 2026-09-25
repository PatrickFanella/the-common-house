using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonHouse.Notebook
{
    public enum EvidenceComparisonSide
    {
        Left,
        Right
    }

    /// <summary>
    /// Presentation state for a player-selected, two-source comparison. It
    /// retains source IDs only and resolves them from the notebook's acquired
    /// entries, so a comparison cannot reveal or mutate case state.
    /// </summary>
    public sealed class EvidenceComparison
    {
        public bool IsOpen { get; private set; }
        public EvidenceComparisonSide ActiveSide { get; private set; } = EvidenceComparisonSide.Left;
        public string LeftSourceId { get; private set; }
        public string RightSourceId { get; private set; }

        public bool Open(IReadOnlyList<NotebookEntry> entries)
        {
            if (entries == null || entries.Count == 0) return false;
            IsOpen = true;
            ActiveSide = EvidenceComparisonSide.Left;
            RemoveUnavailableSelections(entries);
            return true;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void ToggleActiveSide()
        {
            ActiveSide = ActiveSide == EvidenceComparisonSide.Left
                ? EvidenceComparisonSide.Right
                : EvidenceComparisonSide.Left;
        }

        public bool SetActiveSide(string sourceId, IReadOnlyList<NotebookEntry> entries)
        {
            return SetSide(ActiveSide, sourceId, entries);
        }

        public bool SetSide(EvidenceComparisonSide side, string sourceId, IReadOnlyList<NotebookEntry> entries)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || Find(entries, sourceId) == null) return false;
            if (side == EvidenceComparisonSide.Left) LeftSourceId = sourceId;
            else RightSourceId = sourceId;
            return true;
        }

        public NotebookEntry ResolveLeft(IReadOnlyList<NotebookEntry> entries)
        {
            return Find(entries, LeftSourceId);
        }

        public NotebookEntry ResolveRight(IReadOnlyList<NotebookEntry> entries)
        {
            return Find(entries, RightSourceId);
        }

        public void RemoveUnavailableSelections(IReadOnlyList<NotebookEntry> entries)
        {
            if (Find(entries, LeftSourceId) == null) LeftSourceId = null;
            if (Find(entries, RightSourceId) == null) RightSourceId = null;
        }

        private static NotebookEntry Find(IReadOnlyList<NotebookEntry> entries, string sourceId)
        {
            return entries == null || string.IsNullOrWhiteSpace(sourceId)
                ? null
                : entries.FirstOrDefault(entry => entry.SourceId == sourceId);
        }
    }
}
