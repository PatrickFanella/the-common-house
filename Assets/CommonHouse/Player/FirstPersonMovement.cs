using System;
using UnityEngine;

namespace CommonHouse.Player
{
    /// <summary>
    /// Small legacy-input first-person controller for the outpost blockout.
    /// Tests replace native input before their first frame, so they do not read
    /// from or change the host pointer.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonMovement : MonoBehaviour
    {
        private const float GroundedVelocity = -2f;
        private const float Gravity = -20f;

        [SerializeField, Min(0.01f)] private float movementSpeed = 3.5f;
        [Tooltip("Degrees per legacy mouse axis. Configure this in the Inspector until an options menu exists.")]
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 2f;
        [SerializeField] private Transform viewTransform;
        [SerializeField] private bool headBobEnabled;
        [SerializeField] private bool cameraShakeEnabled;

        private CharacterController characterController;
        private IFirstPersonInput inputSource;
        private bool readsNativeInput = true;
        private bool menuOpen;
        private bool modalInputOwner;
        private float pitch;
        private float verticalVelocity;

        public float MovementSpeed
        {
            get => movementSpeed;
            set => movementSpeed = Mathf.Max(0.01f, value);
        }

        public float MouseSensitivity
        {
            get => mouseSensitivity;
            set => mouseSensitivity = Mathf.Max(0.01f, value);
        }

        public bool HeadBobEnabled => headBobEnabled;
        public bool CameraShakeEnabled => cameraShakeEnabled;
        public bool MenuOpen => menuOpen;
        public bool HasModalInputOwner => modalInputOwner;
        public bool PointerCaptured => Cursor.lockState == CursorLockMode.Locked && Cursor.visible == false;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputSource = new LegacyFirstPersonInput();
            readsNativeInput = !Application.isBatchMode &&
                !string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal);
            if (viewTransform == null) viewTransform = transform;
        }

        private void Update()
        {
            if (!readsNativeInput) return;
            if (!modalInputOwner && Input.GetKeyDown(KeyCode.Escape)) SetMenuOpen(!menuOpen);
            if (!menuOpen) CapturePointer();
            Tick(inputSource.Read(), Time.deltaTime);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && readsNativeInput) SetMenuOpen(true);
        }

        private void OnDisable()
        {
            if (readsNativeInput) ReleasePointer();
        }

        private void OnDestroy()
        {
            if (readsNativeInput) ReleasePointer();
        }

        /// <summary>Integrates one deterministic input frame for local tests.</summary>
        public void Tick(FirstPersonInputFrame frame, float deltaTime)
        {
            if (menuOpen || deltaTime <= 0f) return;

            var moveInput = Vector2.ClampMagnitude(frame.Move, 1f);
            var localMove = new Vector3(moveInput.x, 0f, moveInput.y);
            var worldMove = transform.TransformDirection(localMove) * movementSpeed;

            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = GroundedVelocity;
            verticalVelocity += Gravity * deltaTime;
            worldMove.y = verticalVelocity;
            characterController.Move(worldMove * deltaTime);

            transform.Rotate(Vector3.up, frame.Look.x * mouseSensitivity, Space.World);
            pitch = Mathf.Clamp(pitch - frame.Look.y * mouseSensitivity, -85f, 85f);
            viewTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// Replaces legacy hardware input. Supplying a test source guarantees
        /// automatic tests leave the operating-system pointer untouched.
        /// </summary>
        public void SetInputSource(IFirstPersonInput source)
        {
            if (readsNativeInput) ReleasePointer();
            inputSource = source ?? throw new ArgumentNullException(nameof(source));
            readsNativeInput = false;
        }

        public void RestoreNativeInput()
        {
            inputSource = new LegacyFirstPersonInput();
            readsNativeInput = true;
        }

        public void SetViewTransform(Transform value)
        {
            viewTransform = value != null ? value : transform;
            pitch = NormalizePitch(viewTransform.localEulerAngles.x);
        }

        /// <summary>Called by menu or pause presentation before it takes focus.</summary>
        public void SetMenuOpen(bool value)
        {
            menuOpen = value;
            if (menuOpen && readsNativeInput) ReleasePointer();
            else if (readsNativeInput) CapturePointer();
        }

        /// <summary>
        /// Gives one modal presentation component ownership of Escape. The
        /// movement controller still supplies the pause and pointer behavior.
        /// </summary>
        public void SetModalInputOwner(bool value)
        {
            modalInputOwner = value;
        }

        private static float NormalizePitch(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static void CapturePointer()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void ReleasePointer()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public readonly struct FirstPersonInputFrame
    {
        public FirstPersonInputFrame(Vector2 move, Vector2 look)
        {
            Move = move;
            Look = look;
        }

        public Vector2 Move { get; }
        public Vector2 Look { get; }
    }

    public interface IFirstPersonInput
    {
        FirstPersonInputFrame Read();
    }

    internal sealed class LegacyFirstPersonInput : IFirstPersonInput
    {
        public FirstPersonInputFrame Read()
        {
            return new FirstPersonInputFrame(
                new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        }
    }
}
