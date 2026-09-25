using System;
using CommonHouse.Player;
using UnityEngine;

namespace CommonHouse.Interaction
{
    /// <summary>One owner for Escape and movement pause while a panel is modal.</summary>
    public interface IModalPanel
    {
        void CloseFromModalInput();
    }

    [RequireComponent(typeof(FirstPersonMovement))]
    public sealed class ModalInputCoordinator : MonoBehaviour
    {
        [SerializeField] private FirstPersonMovement movement;
        private IModalPanel currentOwner;

        public bool HasOwner => currentOwner != null;
        public bool IsOwner(IModalPanel panel) => ReferenceEquals(currentOwner, panel);

        private void Awake()
        {
            if (movement == null) movement = GetComponent<FirstPersonMovement>();
            if (movement == null) throw new InvalidOperationException("Modal input needs FirstPersonMovement.");
            movement.SetModalInputOwner(true);
        }

        private void OnDestroy()
        {
            if (movement != null) movement.SetModalInputOwner(false);
        }

        private void Update()
        {
            if (!Application.isBatchMode && !string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal)
                && Input.GetKeyDown(KeyCode.Escape)) HandleCancel();
        }

        public void Configure(FirstPersonMovement playerMovement)
        {
            movement = playerMovement ?? throw new ArgumentNullException(nameof(playerMovement));
            movement.SetModalInputOwner(true);
        }

        public bool TryOpen(IModalPanel panel)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));
            if (currentOwner != null) return ReferenceEquals(currentOwner, panel);
            currentOwner = panel;
            movement.SetMenuOpen(true);
            return true;
        }

        public bool Release(IModalPanel panel)
        {
            if (!ReferenceEquals(currentOwner, panel)) return false;
            currentOwner = null;
            movement.SetMenuOpen(false);
            return true;
        }

        public void HandleCancel()
        {
            if (currentOwner != null) currentOwner.CloseFromModalInput();
            else movement.SetMenuOpen(!movement.MenuOpen);
        }
    }
}
