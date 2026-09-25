using System;
using CommonHouse.Content;
using CommonHouse.Interaction;
using CommonHouse.Radio;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Opening
{
    /// <summary>
    /// Presents the provisional household welcome. Offering care is an opening
    /// condition, not a reward, timer, evidence item, or prerequisite that can
    /// be consumed by leaving the panel.
    /// </summary>
    public sealed class OpeningHouseholdController : MonoBehaviour, IModalPanel
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private TransmissionLog transmissionLog;
        [SerializeField] private ModalInputCoordinator modalInput;

        private bool careOffered;
        private bool carePanelOpen;
        private string lastActionMessage = string.Empty;

        public bool IsCareOffered => careOffered;
        public bool IsCarePanelOpen => carePanelOpen;
        public bool IsFirstCallAvailable => careOffered;
        public string LastActionMessage => lastActionMessage;

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
            if (transmissionLog == null) transmissionLog = GetComponent<TransmissionLog>();
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
            OfferCare();
        }

        private void Start()
        {
            // The welcome is presented before the call, but Escape can dismiss it
            // without turning the waiting call into a completion gate.
            OpenCare();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        /// <summary>Starts with the care offer already made, so interruption never removes the waiting call.</summary>
        public void OfferCare()
        {
            careOffered = true;
        }

        public void Configure(CaseSession caseSession, TransmissionLog log, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            transmissionLog = log ?? throw new ArgumentNullException(nameof(log));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            OfferCare();
        }

        public bool OpenCare()
        {
            OfferCare();
            if (carePanelOpen) return true;
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            carePanelOpen = true;
            lastActionMessage = string.Empty;
            return true;
        }

        public void Close()
        {
            if (!carePanelOpen) return;
            carePanelOpen = false;
            if (modalInput != null) modalInput.Release(this);
        }

        public void CloseFromModalInput()
        {
            Close();
        }

        /// <summary>Receives the waiting first call only from its dedicated world target.</summary>
        public TransmissionReceiveResult ReceiveFirstCall()
        {
            if (!careOffered || session == null || transmissionLog == null)
            {
                lastActionMessage = "The radio desk is not ready.";
                return TransmissionReceiveResult.Rejected;
            }

            // Reserve visible modal presentation before mutating case state. A
            // blocked world action must not receive evidence behind another panel.
            if (modalInput == null || (modalInput.HasOwner && !modalInput.IsOwner(transmissionLog)) || !transmissionLog.Open())
            {
                lastActionMessage = "Another panel is already open.";
                return TransmissionReceiveResult.Rejected;
            }
            var result = transmissionLog.Receive("T01");
            lastActionMessage = transmissionLog.LastActionMessage;
            if (result == TransmissionReceiveResult.Rejected) return result;
            if (!transmissionLog.SelectReceivedSource("T01") || !transmissionLog.OpenSelectedTranscript())
            {
                lastActionMessage = "The received call could not be opened for review.";
                return TransmissionReceiveResult.Rejected;
            }
            return result;
        }
    }
}
