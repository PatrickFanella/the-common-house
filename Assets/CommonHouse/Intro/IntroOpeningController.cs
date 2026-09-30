using System;
using System.Collections.Generic;
using System.Linq;
using CommonHouse.Content;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Intro
{
    public enum IntroActionKind { Option, Acknowledge, Pause, Resume, Transcript, Close }

    public sealed class IntroOpeningAction
    {
        public string Text { get; }
        public IntroActionKind Kind { get; }
        public string ChoiceId { get; }
        public string OptionId { get; }
        internal IntroOpeningAction(string text, IntroActionKind kind, string choiceId = null, string optionId = null)
        { Text = text; Kind = kind; ChoiceId = choiceId; OptionId = optionId; }
    }

    public sealed class IntroOpeningController : MonoBehaviour, IModalPanel
    {
        public const string ScenePath = "Assets/CommonHouse/Scenes/IntroOpening.unity";
        private static readonly string[] sceneIds = { "IN-S00", "IN-S01", "IN-S02", "IN-S03", "IN-S04", "IN-S05", "IN-S06", "IN-S07" };
        private static readonly string[] sourceIds = { "IN-C01", "IN-C02", "IN-C03", "IN-C04", "IN-D01", "IN-T01", "IN-T02", "IN-T03", "IN-T04", "IN-T05" };
        [SerializeField] private ModalInputCoordinator modalInput;
        private IntroState state;
        private int acknowledgment;
        public IntroState State => state ?? (state = new IntroState(IntroContent.Load()));
        public bool IsOpen => State.ActiveReview != null && modalInput != null && modalInput.IsOwner(this);
        public int SelectedIndex { get; private set; }
        public string LastActionMessage { get; private set; } = string.Empty;
        public string VisibleTitle => !IsOpen ? string.Empty : State.ActiveReview.Id == "IN-C01" ? "Ren's return warning"
            : State.ActiveReview.Id == "IN-C02" ? "Arrivals at the house" : State.ActiveReview.Id == "IN-D01" ? "Service-area map" : State.ActiveReview.Title;
        public string ThresholdSceneId => State.AcknowledgedSceneIds.Contains("IN-S01") &&
            (!State.Expressions.TryGetValue("IN-Q01", out var option) || option != "IN-Q01-E") ? "IN-S02" : "IN-S01";
        public IReadOnlyList<IntroOpeningAction> Actions
        {
            get
            {
                if (!IsOpen) return Array.Empty<IntroOpeningAction>();
                var result = new List<IntroOpeningAction>();
                if (!State.IsPaused)
                    foreach (var choice in State.ActiveReview.Choices)
                        foreach (var option in choice.Options)
                            result.Add(new IntroOpeningAction(option.Text, IntroActionKind.Option, choice.Id, option.Id));
                if (!State.IsPaused) result.Add(new IntroOpeningAction("Acknowledge and return", IntroActionKind.Acknowledge));
                result.Add(new IntroOpeningAction(State.IsPaused ? "Resume" : "Pause", State.IsPaused ? IntroActionKind.Resume : IntroActionKind.Pause));
                result.Add(new IntroOpeningAction("Read transcript", IntroActionKind.Transcript));
                result.Add(new IntroOpeningAction("Close for now", IntroActionKind.Close));
                return result.AsReadOnly();
            }
        }

        private void Awake() { if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>(); }
        private void OnDestroy() { if (modalInput != null) modalInput.Release(this); }
        private void Update()
        {
            if (Application.isBatchMode || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") == "test") return;
            Tick(Input.GetKeyDown(KeyCode.UpArrow), Input.GetKeyDown(KeyCode.DownArrow), Input.GetKeyDown(KeyCode.Return));
        }

        public void Configure(ModalInputCoordinator coordinator)
        {
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public bool OpenScene(string id)
        {
            if (!sceneIds.Contains(id)) { LastActionMessage = "That conversation is not available here."; return false; }
            return Open(() => State.OpenScene(id));
        }
        public bool OpenSource(string id)
        {
            if (!sourceIds.Contains(id)) { LastActionMessage = "That record is not available here."; return false; }
            return Open(() => State.OpenSource(id));
        }
        public bool OpenOrientation() => Open(() => State.OpenOrientation());
        private bool Open(Func<IntroReview> open)
        {
            if (IsOpen || modalInput == null || !modalInput.TryOpen(this))
            { LastActionMessage = "Close the current panel first."; return false; }
            try { open(); }
            catch { modalInput.Release(this); throw; }
            SelectedIndex = 0;
            LastActionMessage = State.ActiveReview.Id == "IN-S04"
                ? State.AcknowledgedSceneIds.Contains("IN-S04")
                    ? "Local replay. No outgoing transmission."
                    : "Orchard hears Ivo's arrival statement only. No player name or speech is sent."
                : State.IsComplete ? "You have a place here. The desk is available when you want it." : string.Empty;
            return true;
        }

        public void Tick(bool previous, bool next, bool submit)
        {
            if (!IsOpen) return;
            var count = Actions.Count;
            if (previous) SelectedIndex = (SelectedIndex + count - 1) % count;
            if (next) SelectedIndex = (SelectedIndex + 1) % count;
            if (submit) Activate(SelectedIndex);
        }

        // Mouse buttons and deterministic keyboard frames share this action path.
        public bool Activate(int index)
        {
            var actions = Actions;
            if (!IsOpen || index < 0 || index >= actions.Count) return false;
            var action = actions[index];
            var review = State.ActiveReview;
            switch (action.Kind)
            {
                case IntroActionKind.Option:
                    State.SelectOption(review, action.ChoiceId, action.OptionId);
                    LastActionMessage = action.OptionId == "IN-Q06-A" ? "You can decide what to put in the ledger later." : "Review the response, then acknowledge when ready.";
                    break;
                case IntroActionKind.Acknowledge:
                    if (review.Id == "IN-S07")
                    {
                        if (State.MissingCoreAcknowledgments.Count > 0)
                        {
                            LastActionMessage = "Review these before finishing: " + string.Join(", ", State.MissingCoreAcknowledgments) + ". The orientation summary is available beside reception.";
                            return false;
                        }
                        if (!review.SelectedOptions.ContainsKey("IN-Q03"))
                        {
                            LastActionMessage = "Choose whether you would like to learn the desk later. Either response keeps your place here.";
                            return false;
                        }
                    }
                    State.Acknowledge("intro-opening:" + ++acknowledgment, review);
                    modalInput.Release(this);
                    LastActionMessage = State.IsComplete ? "You have a place here. The desk can wait until you are ready." : "You can return to this conversation.";
                    break;
                case IntroActionKind.Pause: State.Pause(review); break;
                case IntroActionKind.Resume: State.Resume(review); break;
                case IntroActionKind.Transcript: State.SkipToTranscript(review); break;
                case IntroActionKind.Close: CloseFromModalInput(); break;
                default: throw new ArgumentOutOfRangeException();
            }
            SelectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, Actions.Count - 1));
            return true;
        }

        public void CloseFromModalInput()
        {
            if (!IsOpen) return;
            State.Close(State.ActiveReview);
            modalInput.Release(this);
            LastActionMessage = string.Empty;
        }
    }
}
