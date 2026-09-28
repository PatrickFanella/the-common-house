using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Saved HDRP materials for the outpost's rooms, built from the CC0 scans (decision D-21).
    /// Mapping is world-space triplanar, so the scans keep their measured size on the
    /// stretched primitive cubes that the blockout and its tests rely on.
    /// </summary>
    public static class OutpostMaterials
    {
        public const string MaterialRoot = "Assets/CommonHouse/Art/Materials";
        public const string GeneratedRoot = "Assets/CommonHouse/Art/Generated";

        public sealed class Set
        {
            public Material Plaster;
            public Material Ceiling;
            public Material Floorboards;
            public Material KitchenTiles;
        }

        [MenuItem("Common House/Art/Build Outpost Materials")]
        public static void BuildFromMenu() => Build();

        /// <summary>Creates or updates the material assets and returns them.</summary>
        public static Set Build()
        {
            Directory.CreateDirectory(MaterialRoot);
            Directory.CreateDirectory(GeneratedRoot);
            var set = new Set
            {
                // Scan sizes are the physical dimensions Poly Haven lists for each asset, in metres.
                Plaster = Scan("Plaster", "plastered_wall_04", 3.2f, Color.white),
                Ceiling = Scan("Ceiling", "plastered_wall_04", 3.2f, new Color(0.9f, 0.9f, 0.88f)),
                Floorboards = Scan("Floorboards", "wood_floor_worn", 2f, Color.white),
                KitchenTiles = Scan("KitchenTiles", "worn_tile_floor", 2f, Color.white),
            };
            AssetDatabase.SaveAssets();
            return set;
        }

        private static Material Scan(string name, string asset, float metres, Color tint)
        {
            var prefix = ScanTextures.PolyHaven(asset);
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_diff_2k.jpg");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + "_nor_gl_2k.jpg");
            if (albedo == null || normal == null) throw new InvalidOperationException("Missing scan textures for " + asset + "; run `git lfs pull`.");
            var mask = SavedMask(asset);

            var path = MaterialRoot + "/" + name + ".mat";
            var shader = Shader.Find("HDRP/Lit");
            if (shader == null) throw new InvalidOperationException("HDRP/Lit is unavailable.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetTexture("_BaseColorMap", albedo);
            material.SetColor("_BaseColor", tint);
            material.SetTexture("_NormalMap", normal);
            material.SetFloat("_NormalScale", 1f);
            material.SetTexture("_MaskMap", mask);
            material.SetFloat("_MetallicRemapMax", 1f);
            material.SetFloat("_SmoothnessRemapMax", 1f);
            material.SetFloat("_AORemapMax", 1f);
            material.SetFloat("_UVBase", 5f); // Triplanar
            material.SetFloat("_ObjectSpaceUVMapping", 0f); // World space
            material.SetFloat("_TexWorldScale", 1f / metres);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Writes the scan's HDRP mask map as a PNG, only when its content changes.</summary>
        private static Texture2D SavedMask(string asset)
        {
            var path = GeneratedRoot + "/" + asset + "_mask_2k.png";
            var mask = ScanTextures.MaskFromArm(ScanTextures.PolyHaven(asset) + "_arm_2k.jpg");
            byte[] png;
            try
            {
                png = mask.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mask);
            }
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(png))
            {
                File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.sRGBTexture)
            {
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
