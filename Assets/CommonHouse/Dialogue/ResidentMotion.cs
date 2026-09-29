using System;
using System.Linq;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Dialogue
{
    public enum ResidentMotionPhase { Standing, StartingWalk, Walking, StoppingWalk, SittingDown, Seated, StandingUp }

    /// <summary>A short authored route within one room; animation never changes case state.</summary>
    [RequireComponent(typeof(Animator))]
    public sealed class ResidentMotion : MonoBehaviour
    {
        [SerializeField] private DialoguePresenter dialogue;
        [SerializeField] private ModalInputCoordinator modal;
        [SerializeField] private Transform interactionTarget;
        [SerializeField] private string residentId;
        [SerializeField] private Vector3 standingPosition;
        [SerializeField] private Vector3 seatPosition;
        [SerializeField] private float standingSeconds = 20f;
        [SerializeField] private float seatedSeconds = 24f;
        [SerializeField] private float walkSpeed = 0.65f;
        private Animator animator;
        private Quaternion restingRotation;
        private float elapsed;
        private bool returning;
        private string playing;
        private bool wasTalking;

        public ResidentMotionPhase Phase { get; private set; }
        public string ResidentId => residentId;
        public Vector3 StandingPosition => standingPosition;
        public Vector3 SeatPosition => seatPosition;
        public bool IsTalking { get; private set; }
        public string PlayingClip => playing;

        public void Configure(DialoguePresenter presenter, ModalInputCoordinator coordinator, Transform target,
            string id, Vector3 seat, float idleSeconds)
        {
            dialogue = presenter;
            modal = coordinator;
            interactionTarget = target;
            residentId = id;
            standingPosition = transform.position;
            seatPosition = seat;
            standingSeconds = idleSeconds;
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            restingRotation = transform.rotation;
            animator.applyRootMotion = false;
            // These two rigs must keep their pose current when their routine moves them out of view.
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            playing = "ResidentIdle";
        }

        private void Update()
        {
            if (!Application.isBatchMode && Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                Tick(Time.deltaTime);
        }

        /// <summary>Explicit clock permits disposable scene tests without waiting through idle holds.</summary>
        public void Tick(float seconds)
        {
            if (seconds < 0f || float.IsNaN(seconds) || float.IsInfinity(seconds))
                throw new ArgumentOutOfRangeException(nameof(seconds));
            var speaking = dialogue != null && dialogue.IsOpen && dialogue.ActiveResidentId == residentId
                && dialogue.CurrentLine != null && dialogue.CurrentLine.SpeakerId == residentId;
            var transition = Phase == ResidentMotionPhase.SittingDown || Phase == ResidentMotionPhase.StandingUp;
            IsTalking = speaking && !transition;
            var held = modal != null && (modal.HasOwner || modal.IsPaused);
            animator.speed = held && !IsTalking ? 0f : 1f;
            if (IsTalking)
            {
                Play(Phase == ResidentMotionPhase.Seated ? "sitting-talking" : "talking");
                wasTalking = true;
            }
            else
            {
                // Resume the interrupted one-shot at its previous time rather than restarting it.
                Play(ClipForPhase(), wasTalking ? elapsed / Mathf.Max(Length(ClipForPhase()), 0.01f) : 0f);
                wasTalking = false;
            }
            if (!held)
            {
                elapsed += seconds;
                switch (Phase)
                {
                    case ResidentMotionPhase.Standing:
                        if (elapsed >= standingSeconds) { returning = false; Enter(ResidentMotionPhase.StartingWalk); }
                        break;
                    case ResidentMotionPhase.StartingWalk:
                        FaceDestination();
                        if (elapsed >= Length("start-walking")) Enter(ResidentMotionPhase.Walking);
                        break;
                    case ResidentMotionPhase.Walking:
                        FaceDestination();
                        var destination = returning ? standingPosition : seatPosition;
                        var next = Vector3.MoveTowards(transform.position, destination, walkSpeed * seconds);
                        // The route is authored clear of furniture. Yield to the player rather than pushing through them.
                        if (modal == null || Vector3.Distance(next, modal.transform.position) > 0.75f)
                            transform.position = next;
                        if (Vector3.Distance(transform.position, destination) < 0.001f) Enter(ResidentMotionPhase.StoppingWalk);
                        break;
                    case ResidentMotionPhase.StoppingWalk:
                        if (elapsed >= Length("stop-walking"))
                        {
                            transform.rotation = restingRotation;
                            Enter(returning ? ResidentMotionPhase.Standing : ResidentMotionPhase.SittingDown);
                        }
                        break;
                    case ResidentMotionPhase.SittingDown:
                        if (elapsed >= Length("stand-to-sit")) Enter(ResidentMotionPhase.Seated);
                        break;
                    case ResidentMotionPhase.Seated:
                        if (elapsed >= seatedSeconds) Enter(ResidentMotionPhase.StandingUp);
                        break;
                    case ResidentMotionPhase.StandingUp:
                        if (elapsed >= Length("sit-to-stand")) { returning = true; Enter(ResidentMotionPhase.StartingWalk); }
                        break;
                }
            }
            // The collider and E target follow the visible body, including the seated height.
            if (interactionTarget != null)
                interactionTarget.position = transform.position + Vector3.up * (Phase == ResidentMotionPhase.Seated ? 0.85f : 1.4f);
        }

        private void FaceDestination()
        {
            var direction = (returning ? standingPosition : seatPosition) - transform.position;
            if (direction.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private void Enter(ResidentMotionPhase phase)
        {
            Phase = phase;
            elapsed = 0f;
            Play(ClipForPhase());
        }

        private string ClipForPhase()
        {
            switch (Phase)
            {
                case ResidentMotionPhase.StartingWalk: return "start-walking";
                case ResidentMotionPhase.Walking: return "walking";
                case ResidentMotionPhase.StoppingWalk: return "stop-walking";
                case ResidentMotionPhase.SittingDown: return "stand-to-sit";
                case ResidentMotionPhase.Seated: return "sitting-idle";
                case ResidentMotionPhase.StandingUp: return "sit-to-stand";
                default: return "ResidentIdle";
            }
        }

        private float Length(string clip)
        {
            var animation = animator.runtimeAnimatorController.animationClips.FirstOrDefault(item => item.name == clip);
            return animation != null ? animation.length : 1f;
        }

        private void Play(string state, float normalizedTime = 0f)
        {
            if (playing == state) return;
            playing = state;
            animator.CrossFadeInFixedTime(state, 0.15f, 0, normalizedTime * Length(state));
        }

        private void OnDisable()
        {
            if (animator != null) animator.speed = 1f;
            IsTalking = false;
        }
    }
}
