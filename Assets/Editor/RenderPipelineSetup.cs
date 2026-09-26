using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>Explicit one-time HDRP configuration (decision D-20). Import and test commands do not run this.</summary>
    public static class RenderPipelineSetup
    {
        public const string AssetPath = "Assets/CommonHouse/Rendering/HDRenderPipelineAsset.asset";

        public static void ConfigureHdrp()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath) ?? "Assets");
            var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }
            GraphicsSettings.defaultRenderPipeline = asset;
            for (var level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.renderPipeline = null;
            }
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64, new[] { GraphicsDeviceType.Vulkan });
            AssetDatabase.SaveAssets();
            if (GraphicsSettings.defaultRenderPipeline != asset)
                throw new InvalidOperationException("HDRP asset was not assigned.");
        }
    }
}
