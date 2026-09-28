using System;
using System.IO;
using System.Linq;
using UnityEditor;
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

        /// <summary>
        /// Saves the HDRP mask map for an ARM map as a linear PNG, rewriting it only when its content
        /// changes, and returns the imported texture.
        /// </summary>
        public static Texture2D SavedMask(string armPath, string maskPath)
        {
            var mask = MaskFromArm(armPath);
            byte[] png;
            try
            {
                png = mask.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mask);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(maskPath) ?? "Assets");
            if (!File.Exists(maskPath) || !File.ReadAllBytes(maskPath).SequenceEqual(png))
            {
                File.WriteAllBytes(maskPath, png);
                AssetDatabase.ImportAsset(maskPath);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(maskPath);
            if (importer.sRGBTexture)
            {
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
        }
    }
}
