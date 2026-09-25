using System;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Radio
{
    public readonly struct SendConfirmationInputFrame
    {
        public SendConfirmationInputFrame(bool previous, bool next, bool submit, bool acknowledgePublic, bool defer, bool resume)
        {
            Previous = previous;
            Next = next;
            Submit = submit;
            AcknowledgePublic = acknowledgePublic;
            Defer = defer;
            Resume = resume;
        }

        public bool Previous { get; }
        public bool Next { get; }
        public bool Submit { get; }
        public bool AcknowledgePublic { get; }
        public bool Defer { get; }
        public bool Resume { get; }
    }

    /// <summary>Modal presentation of a domain-owned response draft and its required confirmation.</summary>
    public sealed class SendConfirmationController : MonoBehaviour, IModalPanel
    {
        private static readonly ResponseRoute[] Routes = { ResponseRoute.PrivateHeadset, ResponseRoute.RefusalRelay, ResponseRoute.RoomSpeaker };

        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;
        private int selectedRouteIndex;
        private bool isOpen;
        private bool publicDisclosureAcknowledged;
        private SendSummary summary;
        private string lastActionMessage = string.Empty;

        public bool IsOpen => isOpen;
        public ResponseRoute SelectedRoute => Routes[selectedRouteIndex];
        public SendSummary Summary => summary;
        public bool IsReviewing => summary != null;
        public bool PublicDisclosureAcknowledged => publicDisclosureAcknowledged;
        public string LastActionMessage => lastActionMessage;

        private CaseState State => session == null ? null : session.State;

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
            Tick(new SendConfirmationInputFrame(Input.GetKeyDown(KeyCode.UpArrow), Input.GetKeyDown(KeyCode.DownArrow),
                Input.GetKeyDown(KeyCode.Return), Input.GetKeyDown(KeyCode.A), Input.GetKeyDown(KeyCode.D), Input.GetKeyDown(KeyCode.R)));
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public bool Open()
        {
            if (isOpen) return true;
            if (State == null || State.OutcomeId != null)
            {
                lastActionMessage = "The case is unavailable for a new response.";
                return false;
            }
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            isOpen = true;
            summary = null;
            publicDisclosureAcknowledged = false;
            lastActionMessage = string.Empty;
            return true;
        }

        public void Close()
        {
            if (!isOpen) return;
            if (summary != null && State != null && State.OutcomeId == null) CancelReview();
            isOpen = false;
            modalInput.Release(this);
        }

        public void CloseFromModalInput() => Close();

        public void Tick(SendConfirmationInputFrame input)
        {
            if (!isOpen) return;
            if (input.Defer) { Defer(); return; }
            if (input.Resume) { ResumeDraft(); return; }
            if (!IsReviewing)
            {
                if (input.Previous) SelectOffset(-1);
                if (input.Next) SelectOffset(1);
                if (input.Submit) Prepare(SelectedRoute);
                return;
            }
            if (input.AcknowledgePublic)
            {
                TogglePublicDisclosureAcknowledgement();
                return;
            }
            if (input.Submit) Confirm();
        }

        public void SelectOffset(int offset)
        {
            if (!isOpen || IsReviewing || offset == 0) return;
            selectedRouteIndex = (selectedRouteIndex + offset) % Routes.Length;
            if (selectedRouteIndex < 0) selectedRouteIndex += Routes.Length;
        }

        public bool Prepare(ResponseRoute route)
        {
            if (!isOpen || State == null) return false;
            try
            {
                State.PrepareDraft(route);
                summary = State.PresentSendSummary();
                selectedRouteIndex = Array.IndexOf(Routes, route);
                publicDisclosureAcknowledged = false;
                lastActionMessage = summary.CanSend ? "Review the response before sending." : summary.BlockingReason;
                return true;
            }
            catch (InvalidOperationException exception)
            {
                lastActionMessage = exception.Message;
                return false;
            }
        }

        public bool CancelReview()
        {
            if (summary == null) return false;
            try
            {
                State.CancelConfirmation();
                summary = null;
                publicDisclosureAcknowledged = false;
                lastActionMessage = "Confirmation cancelled. The draft remains available.";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                lastActionMessage = exception.Message;
                return false;
            }
        }

        public bool Defer()
        {
            if (!isOpen || State == null) return false;
            try
            {
                State.Defer();
                summary = null;
                publicDisclosureAcknowledged = false;
                lastActionMessage = "Response deferred. The draft and leads remain available.";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                lastActionMessage = exception.Message;
                return false;
            }
        }

        public bool ResumeDraft()
        {
            if (!isOpen || State == null) return false;
            try
            {
                State.Resume();
                if (!State.DraftRoute.HasValue)
                {
                    summary = null;
                    publicDisclosureAcknowledged = false;
                    lastActionMessage = "No response draft was saved. Choose a route to prepare one.";
                    return true;
                }
                return Prepare(State.DraftRoute.Value);
            }
            catch (InvalidOperationException exception)
            {
                lastActionMessage = exception.Message;
                return false;
            }
        }

        public bool TogglePublicDisclosureAcknowledgement()
        {
            if (summary == null || !summary.IsUnauthorizedDisclosure) return false;
            publicDisclosureAcknowledged = !publicDisclosureAcknowledged;
            lastActionMessage = publicDisclosureAcknowledged ? "Unauthorized room disclosure acknowledged." : "Unauthorized room disclosure acknowledgement removed.";
            return true;
        }

        public bool Confirm()
        {
            if (summary == null || State == null)
            {
                lastActionMessage = "Prepare and review a response before sending.";
                return false;
            }
            if (!summary.CanSend)
            {
                lastActionMessage = summary.BlockingReason;
                return false;
            }
            if (summary.IsUnauthorizedDisclosure && !publicDisclosureAcknowledged)
            {
                lastActionMessage = "Acknowledge the unauthorized room disclosure before sending.";
                return false;
            }
            try
            {
                var sent = State.ConfirmSend("radio-send:" + summary.OutcomeId, summary, true, publicDisclosureAcknowledged);
                lastActionMessage = sent ? "Response sent." : "This response was already sent.";
                return sent;
            }
            catch (InvalidOperationException exception)
            {
                lastActionMessage = exception.Message;
                return false;
            }
        }
    }
}
