using System;
using System.IO;
using UnityEngine;

namespace CommonHouse.Editor
{
    /// <summary>Helpers for the CC0 scans under Assets/CommonHouse/Art/Sourced/ (decision D-21).</summary>
    internal static class ScanTextures
    {
        public const string PolyHavenRoot = "Assets/CommonHouse/Art/Sourced/PolyHaven/";

        /// <summary>Asset path prefix for a Poly Haven scan, e.g. ".../wood_floor_worn/wood_floor_worn".</summary>
        public static string PolyHaven(string asset) => PolyHavenRoot + asset + "/" + asset;

        /// <summary>
        /// Repacks a Poly Haven ARM map (occlusion, roughness, metal) into an HDRP mask map
        /// (metallic, occlusion, detail mask, smoothness). Reads the file directly, so the
        /// result does not depend on the ARM map's import settings.
        /// </summary>
        public static Texture2D MaskFromArm(string armPath)
        {
            var arm = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                if (!arm.LoadImage(File.ReadAllBytes(armPath))) throw new InvalidOperationException("Cannot decode " + armPath);
                var pixels = arm.GetPixels32();
                for (var i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    pixels[i] = new Color32(p.b, p.r, 0, (byte)(255 - p.g));
                }
                var mask = new Texture2D(arm.width, arm.height, TextureFormat.RGBA32, true, true)
                {
                    name = Path.GetFileNameWithoutExtension(armPath).Replace("_arm", "_mask"),
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 8,
                };
                mask.SetPixels32(pixels);
                mask.Apply(true);
                return mask;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(arm);
            }
        }
    }
}
