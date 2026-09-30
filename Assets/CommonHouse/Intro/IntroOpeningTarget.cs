using System;
using CommonHouse.Interaction;
using UnityEngine;

namespace CommonHouse.Intro
{
    public enum IntroTargetKind { Scene, Source, Threshold, Orientation }
    public sealed class IntroOpeningTarget : WorldInteractionTarget
    {
        [SerializeField] private IntroOpeningController opening;
        [SerializeField] private IntroTargetKind kind;
        [SerializeField] private string contentId, prompt;
        public string ContentId => kind == IntroTargetKind.Threshold && opening != null ? opening.ThresholdSceneId : contentId;
        public override string Prompt => prompt ?? string.Empty;
        public void Configure(IntroOpeningController controller, IntroTargetKind targetKind, string id, string text)
        {
            opening = controller ?? throw new ArgumentNullException(nameof(controller));
            kind = targetKind;
            contentId = id;
            prompt = text;
        }
        public override bool Interact() => opening != null &&
            (kind == IntroTargetKind.Orientation ? opening.OpenOrientation() :
                kind == IntroTargetKind.Source ? opening.OpenSource(ContentId) : opening.OpenScene(ContentId));
    }
}
