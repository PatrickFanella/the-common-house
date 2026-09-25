using System;
using System.Linq;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Runtime
{
    /// <summary>
    /// Bridges explicit readable-object acquisition to the scene's shared case
    /// session. The inspection controller remains responsible for the player
    /// opening a panel and pressing Enter.
    /// </summary>
    public sealed class CaseInspectionBinding : MonoBehaviour
    {
        [SerializeField] private CaseSession session;
        [SerializeField] private InspectionController inspection;

        private void Awake()
        {
            if (session == null) session = GetComponent<CaseSession>();
            if (inspection == null) inspection = GetComponent<InspectionController>();
        }

        private void Start()
        {
            Bind();
        }

        public void Configure(CaseSession caseSession, InspectionController controller)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
            inspection = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Bind()
        {
            if (session == null || inspection == null) return;
            inspection.SetLedger(new CaseInspectionLedger(session));
        }
    }

    /// <summary>Case-state adapter for document evidence found through inspection.</summary>
    public sealed class CaseInspectionLedger : IInspectionLedger
    {
        private readonly CaseSession session;

        public CaseInspectionLedger(CaseSession caseSession)
        {
            session = caseSession ?? throw new ArgumentNullException(nameof(caseSession));
        }

        public InspectionAcquisitionResult TryAcquire(string acquisitionId)
        {
            if (string.IsNullOrWhiteSpace(acquisitionId) || session.Content == null || session.State == null)
                return InspectionAcquisitionResult.Rejected;

            var source = session.Content.Sources.SingleOrDefault(item => item.Id == acquisitionId);
            if (source == null || source.Kind != "document") return InspectionAcquisitionResult.Rejected;
            if (session.State.AcquiredSourceIds.Contains(source.Id)) return InspectionAcquisitionResult.AlreadyAcquired;

            try
            {
                return session.State.AcquireSource("inspection:" + source.Id, source.Id)
                    ? InspectionAcquisitionResult.Acquired
                    : InspectionAcquisitionResult.AlreadyAcquired;
            }
            catch (InvalidOperationException)
            {
                return InspectionAcquisitionResult.Rejected;
            }
        }
    }
}
