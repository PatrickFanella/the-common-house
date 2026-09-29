using UnityEngine;

namespace CommonHouse.Look
{
    /// <summary>
    /// Feeds the scene's point lights, ambient colour, fog, and vertex-snapping grid to the
    /// CommonHouse/Retro shader, which lights each vertex itself (decision D-24).
    /// </summary>
    [ExecuteAlways]
    public sealed class RetroLighting : MonoBehaviour
    {
        public const int MaxLights = 8;

        [Tooltip("Colour added to every vertex before lights.")]
        public Color ambient = new Color(0.4f, 0.38f, 0.36f);

        [Tooltip("Lumens that count as full strength for a light.")]
        public float referenceLumens = 1300f;

        public Color fogColor = new Color(0.16f, 0.14f, 0.12f);
        public float fogStart = 5f;
        public float fogEnd = 22f;

        [Tooltip("Screen grid that vertex positions snap to; smaller values jitter more.")]
        public Vector2 snapResolution = new Vector2(640f, 448f);

        private static readonly int Positions = Shader.PropertyToID("_RetroLightPositions");
        private static readonly int Colors = Shader.PropertyToID("_RetroLightColors");
        private static readonly int Count = Shader.PropertyToID("_RetroLightCount");
        private static readonly int Ambient = Shader.PropertyToID("_RetroAmbient");
        private static readonly int FogColor = Shader.PropertyToID("_RetroFogColor");
        private static readonly int Fog = Shader.PropertyToID("_RetroFog");
        private static readonly int Snap = Shader.PropertyToID("_RetroSnapResolution");

        private void OnEnable() => Apply();
        private void Update() => Apply();

        /// <summary>Sets the shader globals from the enabled point and spot lights.</summary>
        public void Apply()
        {
            var positions = new Vector4[MaxLights];
            var colors = new Vector4[MaxLights];
            var count = 0;
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (count == MaxLights || !light.isActiveAndEnabled || light.type == LightType.Directional) continue;
                var p = light.transform.position;
                positions[count] = new Vector4(p.x, p.y, p.z, light.range);
                var strength = light.intensity / Mathf.Max(referenceLumens, 1f);
                colors[count] = light.color.linear * strength;
                count++;
            }
            Shader.SetGlobalVectorArray(Positions, positions);
            Shader.SetGlobalVectorArray(Colors, colors);
            Shader.SetGlobalInt(Count, count);
            Shader.SetGlobalColor(Ambient, ambient.linear);
            Shader.SetGlobalColor(FogColor, fogColor.linear);
            Shader.SetGlobalVector(Fog, new Vector4(fogStart, fogEnd, 0f, 0f));
            Shader.SetGlobalVector(Snap, snapResolution);
        }
    }
}
