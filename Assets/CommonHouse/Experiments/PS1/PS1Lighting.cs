using UnityEngine;

namespace CommonHouse.Experiments.PS1
{
    /// <summary>
    /// Feeds the scene's point lights, ambient colour, fog, and vertex-snapping grid to the
    /// experimental CommonHouse/PS1 shader, which lights each vertex itself (issue #69).
    /// </summary>
    [ExecuteAlways]
    public sealed class PS1Lighting : MonoBehaviour
    {
        public const int MaxLights = 8;

        [Tooltip("Colour added to every vertex before lights.")]
        public Color ambient = new Color(0.36f, 0.34f, 0.32f);

        [Tooltip("Lumens that count as full strength for a light.")]
        public float referenceLumens = 450f;

        public Color fogColor = new Color(0.03f, 0.03f, 0.035f);
        public float fogStart = 7f;
        public float fogEnd = 26f;

        [Tooltip("Screen grid that vertex positions snap to; smaller values jitter more.")]
        public Vector2 snapResolution = new Vector2(320f, 240f);

        private static readonly int Positions = Shader.PropertyToID("_PS1LightPositions");
        private static readonly int Colors = Shader.PropertyToID("_PS1LightColors");
        private static readonly int Count = Shader.PropertyToID("_PS1LightCount");
        private static readonly int Ambient = Shader.PropertyToID("_PS1Ambient");
        private static readonly int FogColor = Shader.PropertyToID("_PS1FogColor");
        private static readonly int Fog = Shader.PropertyToID("_PS1Fog");
        private static readonly int Snap = Shader.PropertyToID("_PS1SnapResolution");

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
