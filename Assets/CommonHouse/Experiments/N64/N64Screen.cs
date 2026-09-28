using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Experiments.N64
{
    /// <summary>
    /// Experimental N64-style framebuffer: each channel is reduced to a few bits with a 4x4
    /// ordered dither, as the console's 16-bit (5-5-5-1) framebuffer did. Intended for a camera
    /// rendering at about 320x240. Registered by CommonHouse.Editor.Experiments.N64Outpost.
    /// </summary>
    [Serializable, VolumeComponentMenu("Post-processing/Custom/N64 Screen")]
    public sealed class N64Screen : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        [Tooltip("Bits per colour channel; the N64 framebuffer used 5.")]
        public ClampedIntParameter bitsPerChannel = new ClampedIntParameter(5, 2, 8);

        [Tooltip("Strength of the ordered dither applied before reducing colour depth.")]
        public ClampedFloatParameter dither = new ClampedFloatParameter(1f, 0f, 1f);

        [Tooltip("Colour saturation; below 1 greys the image slightly.")]
        public ClampedFloatParameter saturation = new ClampedFloatParameter(1f, 0f, 2f);

        public BoolParameter enabled = new BoolParameter(false);

        private Material material;

        public bool IsActive() => material != null && enabled.value;

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        private const string ShaderName = "Hidden/CommonHouse/N64Screen";

        public override void Setup()
        {
            var shader = Shader.Find(ShaderName);
            if (shader != null) material = new Material(shader);
            else Debug.LogError("Missing shader " + ShaderName);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            if (material == null) return;
            material.SetFloat("_Levels", (1 << bitsPerChannel.value) - 1);
            material.SetFloat("_Dither", dither.value);
            material.SetFloat("_Saturation", saturation.value);
            material.SetTexture("_MainTex", source);
            HDUtils.DrawFullScreen(cmd, material, destination, shaderPassId: 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(material);
        }
    }
}
