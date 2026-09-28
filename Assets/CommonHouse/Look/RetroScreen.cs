using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Look
{
    /// <summary>
    /// Renders the camera at a fixed number of lines, 448 like GTA 3 on the PS2 (decision D-24),
    /// through HDRP dynamic resolution, and keeps the retro shader's vertex snapping on the same
    /// grid. IMGUI panels draw after the upscale, at full resolution, so text stays sharp.
    /// Does nothing without a graphics device, as in the headless test runs.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class RetroScreen : MonoBehaviour
    {
        [Tooltip("Lines rendered before scaling up to the output.")]
        public int lines = 448;

        /// <summary>Output height to scale from when rendering into a texture, as captures do; 0 uses the screen.</summary>
        public static int OutputHeightOverride;

        private Camera view;
        private RetroLighting lighting;

        private void OnEnable()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
            view = GetComponent<Camera>();
            lighting = FindFirstObjectByType<RetroLighting>();
            DynamicResolutionHandler.SetDynamicResScaler(Percentage, DynamicResScalePolicyType.ReturnsPercentage);
        }

        private void OnDisable()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
            DynamicResolutionHandler.SetDynamicResScaler(() => 100f, DynamicResScalePolicyType.ReturnsPercentage);
        }

        private void Update()
        {
            if (lighting == null || view == null) return;
            lighting.snapResolution = new Vector2(Mathf.Round(lines * view.aspect), lines);
        }

        /// <summary>Screen percentage that renders <see cref="lines"/> lines of the current output.</summary>
        public float Percentage()
        {
            var height = OutputHeightOverride > 0 ? OutputHeightOverride : Screen.height;
            return Mathf.Clamp(100f * lines / Mathf.Max(height, 1), 10f, 100f);
        }
    }
}
