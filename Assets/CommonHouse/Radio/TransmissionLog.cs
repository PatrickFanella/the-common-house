using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Radio
{
    public enum TransmissionReceiveResult { Received, AlreadyReceived, Rejected }

    public readonly struct TransmissionLogInputFrame
    {
        public TransmissionLogInputFrame(bool toggle, bool previous, bool next, bool confirm)
        {
            Toggle = toggle;
            Previous = previous;
            Next = next;
            Confirm = confirm;
        }

        public bool Toggle { get; }
        public bool Previous { get; }
        public bool Next { get; }
        public bool Confirm { get; }
    }

    public sealed class TransmissionLogRecord
    {
        public string SourceId { get; }
        public string SpeakerId { get; }
        public string SpeakerLabel { get; }
        public string Transcript { get; }
        public string SoundCaption { get; }
        public string AssertedContactIdentity { get; }
        public bool IsAssertedContactIdentityConfirmed { get; }

        internal TransmissionLogRecord(ContentSource source, string speakerLabel, string assertedIdentity, bool isIdentityConfirmed)
        {
            SourceId = source.Id;
            SpeakerId = source.SpeakerId;
            SpeakerLabel = speakerLabel;
            Transcript = source.Text;
            SoundCaption = source.Caption ?? string.Empty;
            AssertedContactIdentity = assertedIdentity ?? string.Empty;
            IsAssertedContactIdentityConfirmed = isIdentityConfirmed;
        }
    }

    /// <summary>
    /// A review-only log of acquired radio transmissions. Receiving is explicit;
    /// opening, selecting, and replaying entries never changes case state.
    /// </summary>
    public sealed class TransmissionLog : MonoBehaviour, IModalPanel
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;

        private readonly List<TransmissionLogRecord> records = new List<TransmissionLogRecord>();
        private int selectedIndex;
        private bool isOpen;
        private bool isShowingTranscript;
        private string lastActionMessage = string.Empty;

        public IReadOnlyList<TransmissionLogRecord> Records => records.AsReadOnly();
        public int SelectedIndex => selectedIndex;
        public bool IsOpen => isOpen;
        public bool IsShowingTranscript => isShowingTranscript;
        public string LastActionMessage => lastActionMessage;
        public TransmissionLogRecord SelectedRecord => records.Count == 0 ? null : records[selectedIndex];

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
            RefreshRecords();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            Tick(new TransmissionLogInputFrame(Input.GetKeyDown(KeyCode.R), Input.GetKeyDown(KeyCode.UpArrow),
                Input.GetKeyDown(KeyCode.DownArrow), Input.GetKeyDown(KeyCode.Return)));
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            RefreshRecords();
        }

        // This is an explicit world interaction. It is deliberately not called by opening the log.
        public TransmissionReceiveResult Receive(string sourceId)
        {
            if (session == null || session.Content == null || session.State == null)
            {
                lastActionMessage = "Case session is unavailable.";
                return TransmissionReceiveResult.Rejected;
            }
            var source = session.Content.Sources.SingleOrDefault(item => item.Id == sourceId);
            if (source == null || source.Kind != "transmission")
            {
                lastActionMessage = "Only a received radio transmission can be logged.";
                return TransmissionReceiveResult.Rejected;
            }
            try
            {
                var received = session.State.AcquireSource("radio-receive-" + sourceId, sourceId);
                RefreshRecords();
                lastActionMessage = received ? "Transmission received and available for review." : "Transmission is already available for review.";
                return received ? TransmissionReceiveResult.Received : TransmissionReceiveResult.AlreadyReceived;
            }
            catch (InvalidOperationException)
            {
                lastActionMessage = "This transmission cannot be received in the current case state.";
                return TransmissionReceiveResult.Rejected;
            }
        }

        public bool Open()
        {
            RefreshRecords();
            if (isOpen) return true;
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            isOpen = true;
            isShowingTranscript = false;
            lastActionMessage = records.Count == 0 ? "No transmissions have been received." : string.Empty;
            return true;
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            isShowingTranscript = false;
            if (modalInput != null) modalInput.Release(this);
        }

        public void CloseFromModalInput()
        {
            if (isShowingTranscript)
            {
                isShowingTranscript = false;
                return;
            }
            Close();
        }

        public void Tick(TransmissionLogInputFrame input)
        {
            if (input.Toggle)
            {
                if (isOpen) Close();
                else Open();
                return;
            }
            if (!isOpen || isShowingTranscript) return;
            if (input.Previous) SelectOffset(-1);
            if (input.Next) SelectOffset(1);
            if (input.Confirm) OpenSelectedTranscript();
        }

        public void SelectOffset(int offset)
        {
            RefreshRecords();
            if (!isOpen || records.Count == 0 || offset == 0) return;
            selectedIndex = (selectedIndex + offset) % records.Count;
            if (selectedIndex < 0) selectedIndex += records.Count;
        }

        public bool SelectReceivedSource(string sourceId)
        {
            RefreshRecords();
            var index = records.FindIndex(record => record.SourceId == sourceId);
            if (!isOpen || index < 0) return false;
            selectedIndex = index;
            return true;
        }

        public bool OpenSelectedTranscript()
        {
            RefreshRecords();
            if (!isOpen || records.Count == 0)
            {
                lastActionMessage = "Select a received transmission first.";
                return false;
            }
            // This query enforces that the entry remains a real acquired source, with no effects.
            session.State.ReplaySource(SelectedRecord.SourceId);
            isShowingTranscript = true;
            lastActionMessage = string.Empty;
            return true;
        }

        public void RefreshRecords()
        {
            var previousId = records.Count == 0 ? null : records[Mathf.Clamp(selectedIndex, 0, records.Count - 1)].SourceId;
            records.Clear();
            if (session == null || session.Content == null || session.State == null) return;
            foreach (var sourceId in session.State.AcquiredSourceIds)
            {
                var source = session.Content.Sources.Single(item => item.Id == sourceId);
                if (source.Kind != "transmission") continue;
                records.Add(new TransmissionLogRecord(source, DisplayName(source.SpeakerId), AssertedIdentity(source), session.State.IdentityConfirmed));
            }
            records.Sort((left, right) => string.CompareOrdinal(left.SourceId, right.SourceId));
            selectedIndex = Math.Max(0, records.FindIndex(item => item.SourceId == previousId));
            if (records.Count == 0) selectedIndex = 0;
        }

        private string DisplayName(string speakerId)
        {
            var person = session.Content.Callers.Concat(session.Content.Residents).SingleOrDefault(item => item.Id == speakerId);
            return person == null ? speakerId : person.Text;
        }

        // The baseline's first call asks for Mara. This is a caller assertion, not E04 identity confirmation.
        private static string AssertedIdentity(ContentSource source) => source.Id == "T01" ? "Mara" : string.Empty;
    }
}
