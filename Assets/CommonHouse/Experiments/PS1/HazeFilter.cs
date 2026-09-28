using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Experiments.PS1
{
    /// <summary>
    /// PS2-era colour filter after GTA 3 (issue #77): blends each pixel towards a soft blur of the
    /// frame where the blur is brighter, which gives bright areas a glow; then tints and lifts the
    /// image, which washes it out. GTA 3's "trails" also blended in earlier frames; that part needs
    /// frame history and is not implemented.
    /// </summary>
    [Serializable, VolumeComponentMenu("Post-processing/Custom/Haze Filter")]
    public sealed class HazeFilter : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public BoolParameter enabled = new BoolParameter(false);

        [Tooltip("How far each pixel moves towards the brighter of itself and the blur.")]
        public ClampedFloatParameter haze = new ClampedFloatParameter(0.45f, 0f, 1f);

        [Tooltip("Blur radius in pixels of the frame being filtered.")]
        public ClampedFloatParameter radius = new ClampedFloatParameter(2.5f, 0f, 8f);

        [Tooltip("Colour multiplied into the image.")]
        public ColorParameter tint = new ColorParameter(new Color(1f, 0.96f, 0.88f));

        [Tooltip("Amount added to every channel, raising the blacks.")]
        public ClampedFloatParameter lift = new ClampedFloatParameter(0.03f, 0f, 0.2f);

        public ClampedFloatParameter saturation = new ClampedFloatParameter(0.85f, 0f, 2f);

        private Material material;

        public bool IsActive() => material != null && enabled.value;

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        private const string ShaderName = "Hidden/CommonHouse/HazeFilter";

        public override void Setup()
        {
            var shader = Shader.Find(ShaderName);
            if (shader != null) material = new Material(shader);
            else Debug.LogError("Missing shader " + ShaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (material == null) return;
            material.SetFloat("_Haze", haze.value);
            material.SetFloat("_Radius", radius.value);
            material.SetColor("_Tint", tint.value);
            material.SetFloat("_Lift", lift.value);
            material.SetFloat("_Saturation", saturation.value);
            material.SetTexture("_MainTex", source);
            HDUtils.DrawFullScreen(cmd, material, destination, shaderPassId: 0);
        }

        public override void Cleanup() => CoreUtils.Destroy(material);
    }
}
