using System;
using System.Collections.Generic;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using UnityEngine;

namespace CommonHouse.Radio
{
    /// <summary>Modal diagram/connection comparison with an explicit, non-consent route fact.</summary>
    public sealed class RouteInspectionController : MonoBehaviour, IModalPanel, ICaseStateLoadReset
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private ModalInputCoordinator modalInput;

        private readonly RouteInspectionModel model = new RouteInspectionModel();
        private int selectedPage;
        private bool isOpen;
        private string lastActionMessage = string.Empty;

        public RouteInspectionModel Model => model;
        public bool IsOpen => isOpen;
        public int SelectedPage => selectedPage;
        public string LastActionMessage => lastActionMessage;
        public IReadOnlyList<RouteRecipientRecord> ActualRecipients => new[] { RouteInspectionModel.PrivateHeadset, RouteInspectionModel.RoomSpeaker };

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
            Tick(new RouteInspectionInputFrame(Input.GetKeyDown(KeyCode.UpArrow), Input.GetKeyDown(KeyCode.DownArrow),
                Input.GetKeyDown(KeyCode.Return), Input.GetKeyDown(KeyCode.V)));
        }

        public void Configure(CaseSession caseSession, ModalInputCoordinator coordinator)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        }

        public bool Open()
        {
            if (isOpen) return true;
            if (session == null || session.State == null)
            {
                lastActionMessage = "Case session is unavailable.";
                return false;
            }
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            isOpen = true;
            selectedPage = 0;
            lastActionMessage = string.Empty;
            return true;
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            modalInput.Release(this);
        }

        public void CloseFromModalInput() => Close();

        public void ResetAfterStateLoad()
        {
            Close();
            model.Reset();
            selectedPage = 0;
            lastActionMessage = string.Empty;
        }

        public void Tick(RouteInspectionInputFrame input)
        {
            if (!isOpen) return;
            if (input.Previous) selectedPage = 0;
            if (input.Next) selectedPage = 1;
            if (input.Read)
            {
                if (selectedPage == 0) model.ReadDiagram();
                else model.ReadConnection();
                lastActionMessage = selectedPage == 0 ? "Old diagram reviewed." : "Actual connection reviewed.";
            }
            if (input.Verify) VerifyPrivateRoute();
        }

        public RouteInspectionResult VerifyPrivateRoute()
        {
            if (!isOpen || session == null || session.State == null || !model.CanVerify)
            {
                lastActionMessage = "Review both the diagram and actual connection before verification.";
                return RouteInspectionResult.Rejected;
            }
            try
            {
                var verified = session.State.InspectPrivateRoute("route-inspection:verified");
                lastActionMessage = verified ? "Private route verified. This does not authorize contact." : "Private route was already verified. This does not authorize contact.";
                return verified ? RouteInspectionResult.Verified : RouteInspectionResult.AlreadyVerified;
            }
            catch (InvalidOperationException)
            {
                lastActionMessage = "Private routing cannot be verified after the case is resolved.";
                return RouteInspectionResult.Rejected;
            }
        }
    }
}
