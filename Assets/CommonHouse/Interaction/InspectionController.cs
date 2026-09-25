using System;
using System.Collections.Generic;
using CommonHouse.Player;
using UnityEngine;

namespace CommonHouse.Interaction
{
    public readonly struct InspectionInputFrame
    {
        public InspectionInputFrame(bool open, bool cancel, bool acquire)
        {
            Open = open;
            Cancel = cancel;
            Acquire = acquire;
        }

        public bool Open { get; }
        public bool Cancel { get; }
        public bool Acquire { get; }
    }

    public enum InspectionAcquisitionResult
    {
        Acquired,
        AlreadyAcquired,
        Rejected,
    }

    /// <summary>Adapter seam for future case state without coupling this UI to Case 01.</summary>
    public interface IInspectionLedger
    {
        InspectionAcquisitionResult TryAcquire(string acquisitionId);
    }

    public sealed class InspectionController : MonoBehaviour, IModalPanel
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private FirstPersonMovement movement;
        [SerializeField] private ModalInputCoordinator modalInput;
        [SerializeField, Min(0.1f)] private float focusRange = 2.5f;

        private readonly LocalInspectionLedger localLedger = new LocalInspectionLedger();
        private IInspectionLedger ledger;
        private InspectionTarget currentFocus;
        private WorldInteractionTarget currentWorldFocus;
        private InspectionTarget openTarget;
        private string lastActionMessage = string.Empty;

        public InspectionTarget CurrentFocus => currentFocus;
        public WorldInteractionTarget CurrentWorldFocus => currentWorldFocus;
        public InspectionTarget OpenTarget => openTarget;
        public bool IsOpen => openTarget != null;
        public float FocusRange => focusRange;
        public string FocusedPrompt => modalInput != null && modalInput.HasOwner ? string.Empty
            : currentWorldFocus != null ? "[E] " + currentWorldFocus.Prompt
            : currentFocus == null ? string.Empty : "[E] Inspect " + currentFocus.Title;
        public string VisiblePanelText => openTarget == null ? string.Empty : openTarget.ReadableText;
        public string LastActionMessage => lastActionMessage;

        private void Awake()
        {
            if (movement == null) movement = GetComponent<FirstPersonMovement>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            if (modalInput == null) modalInput = GetComponent<ModalInputCoordinator>();
        }

        private void OnDestroy()
        {
            if (modalInput != null) modalInput.Release(this);
        }

        private void Update()
        {
            if (Application.isBatchMode || string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)) return;
            RefreshFocus();
            Tick(new InspectionInputFrame(Input.GetKeyDown(KeyCode.E), false, Input.GetKeyDown(KeyCode.Return)));
        }

        public void Configure(Camera camera, FirstPersonMovement playerMovement, ModalInputCoordinator coordinator, float range = 2.5f)
        {
            viewCamera = camera ?? throw new ArgumentNullException(nameof(camera));
            movement = playerMovement ?? throw new ArgumentNullException(nameof(playerMovement));
            modalInput = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
            focusRange = Mathf.Max(0.1f, range);
        }

        public void SetLedger(IInspectionLedger value)
        {
            ledger = value;
        }

        public void RefreshFocus()
        {
            currentWorldFocus = null;
            if (IsOpen || viewCamera == null || (modalInput != null && modalInput.HasOwner && !modalInput.IsOwner(this)))
            {
                currentFocus = null;
                return;
            }
            if (Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out var hit, focusRange,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                currentWorldFocus = hit.collider.GetComponentInParent<WorldInteractionTarget>();
                currentFocus = currentWorldFocus == null ? hit.collider.GetComponentInParent<InspectionTarget>() : null;
            }
            else
                currentFocus = null;
        }

        public void Tick(InspectionInputFrame input)
        {
            if (IsOpen)
            {
            if (input.Cancel || input.Open) Close();
                else if (input.Acquire) TryAcquireOpen();
                return;
            }
            if (input.Open) TryActivateFocused();
        }

        public bool TryActivateFocused()
        {
            RefreshFocus();
            if (currentWorldFocus != null)
            {
                var target = currentWorldFocus;
                var opened = target.Interact();
                RefreshFocus();
                return opened;
            }
            return TryOpenFocused();
        }

        public bool TryOpenFocused()
        {
            RefreshFocus();
            if (currentFocus == null)
            {
                lastActionMessage = "No readable object is in focus.";
                return false;
            }
            if (modalInput == null || !modalInput.TryOpen(this))
            {
                lastActionMessage = "Another panel is already open.";
                return false;
            }
            openTarget = currentFocus;
            currentFocus = null;
            lastActionMessage = string.Empty;
            return true;
        }

        public bool TryOpen(InspectionTarget target)
        {
            if (target == null || target != currentFocus) return false;
            return TryOpenFocused();
        }

        public void Close()
        {
            if (!IsOpen) return;
            var closed = openTarget;
            openTarget = null;
            if (modalInput != null) modalInput.Release(this);
            RefreshFocus();
            if (currentFocus != closed) currentFocus = null;
            lastActionMessage = string.Empty;
        }

        public void CloseFromModalInput()
        {
            Close();
        }

        public InspectionAcquisitionResult TryAcquireOpen()
        {
            if (openTarget == null)
            {
                lastActionMessage = "Open a readable object before recording it.";
                return InspectionAcquisitionResult.Rejected;
            }
            if (!openTarget.HasAcquisition)
            {
                lastActionMessage = "This item has no evidence to record.";
                return InspectionAcquisitionResult.Rejected;
            }
            var result = (ledger ?? localLedger).TryAcquire(openTarget.AcquisitionId);
            lastActionMessage = result == InspectionAcquisitionResult.Acquired ? "Evidence recorded."
                : result == InspectionAcquisitionResult.AlreadyAcquired ? "Evidence already recorded."
                : "Evidence could not be recorded.";
            return result;
        }

        private sealed class LocalInspectionLedger : IInspectionLedger
        {
            private readonly HashSet<string> acquired = new HashSet<string>(StringComparer.Ordinal);

            public InspectionAcquisitionResult TryAcquire(string acquisitionId)
            {
                if (string.IsNullOrWhiteSpace(acquisitionId)) return InspectionAcquisitionResult.Rejected;
                return acquired.Add(acquisitionId) ? InspectionAcquisitionResult.Acquired : InspectionAcquisitionResult.AlreadyAcquired;
            }
        }
    }
}
