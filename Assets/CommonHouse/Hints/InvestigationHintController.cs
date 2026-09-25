using System;
using CommonHouse.Domain;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Hints
{
    public sealed class InvestigationHintController : MonoBehaviour, IModalPanel
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;
        private int requestCount;
        private bool isOpen;
        private string stateKey;

        public bool IsOpen => isOpen;
        public InvestigationHint CurrentHint
        {
            get
            {
                ResetForStateChange();
                return session == null || session.State == null ? null : InvestigationHints.Get(session.State, requestCount);
            }
        }

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            if (Input.GetKeyDown(KeyCode.H)) RequestHint();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public bool RequestHint()
        {
            if (session == null || session.State == null || InvestigationHints.Get(session.State, 0) == null) return false;
            if (!isOpen)
            {
                if (modalInput == null || !modalInput.TryOpen(this)) return false;
                isOpen = true;
                requestCount = 0;
                stateKey = CurrentStateKey();
            }
            else
            {
                var changed = stateKey != CurrentStateKey();
                ResetForStateChange();
                if (!changed) requestCount++;
            }
            return true;
        }

        public void CloseFromModalInput()
        {
            if (!isOpen) return;
            isOpen = false;
            requestCount = 0;
            stateKey = null;
            modalInput?.Release(this);
        }

        private string CurrentStateKey()
        {
            if (session == null || session.State == null) return string.Empty;
            var state = session.State;
            return string.Join("|", state.AcquiredSourceIds) + ":" + state.Authorization + ":" + state.PrivateRouteVerified + ":" + state.IsDeferred + ":" + state.OutcomeId;
        }

        private void ResetForStateChange()
        {
            if (!isOpen) return;
            var current = CurrentStateKey();
            if (stateKey != current)
            {
                requestCount = 0;
                stateKey = current;
            }
        }
    }
}
