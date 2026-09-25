using System;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Aftermath
{
    public readonly struct AftermathInputFrame
    {
        public AftermathInputFrame(bool acknowledge)
        {
            Acknowledge = acknowledge;
        }

        public bool Acknowledge { get; }
    }

    /// <summary>
    /// A resolved-case household check-in. Presentation is read-only until the
    /// player explicitly acknowledges it; that acknowledgment completes the
    /// domain aftermath once and changes no disclosure or relationship facts.
    /// </summary>
    public sealed class AftermathHouseholdController : MonoBehaviour, IModalPanel, ICaseStateLoadReset
    {
        private const string AftermathEventId = "aftermath:household-check-in";

        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;
        private bool isOpen;
        private string lastActionMessage = string.Empty;

        public bool IsOpen => isOpen;
        public string LastActionMessage => lastActionMessage;
        public bool CanOpen => ActiveEntry != null;
        public bool IsAcknowledged => session != null && session.State != null && session.State.Phase == CasePhase.AftermathCompleted;
        public AftermathEntry ActiveEntry => session == null || session.State == null ? null : AftermathContent.ForOutcome(session.State.OutcomeId);

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
            Tick(new AftermathInputFrame(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)));
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public bool Open()
        {
            if (isOpen) return true;
            if (!CanOpen)
            {
                lastActionMessage = "Resolve the current call before checking in with the household.";
                return false;
            }
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            isOpen = true;
            lastActionMessage = string.Empty;
            return true;
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            modalInput?.Release(this);
        }

        public void CloseFromModalInput() => Close();

        public void ResetAfterStateLoad()
        {
            Close();
            lastActionMessage = string.Empty;
        }

        public bool Acknowledge()
        {
            if (!isOpen || session == null || session.State == null || ActiveEntry == null)
            {
                lastActionMessage = "Open a resolved household check-in first.";
                return false;
            }
            try
            {
                var completed = session.State.CompleteAftermath(AftermathEventId);
                lastActionMessage = completed ? "Household check-in acknowledged." : "This household check-in was already acknowledged.";
                return completed;
            }
            catch (InvalidOperationException)
            {
                lastActionMessage = "This aftermath is no longer available.";
                return false;
            }
        }

        public void Tick(AftermathInputFrame input)
        {
            if (isOpen && input.Acknowledge) Acknowledge();
        }
    }
}
