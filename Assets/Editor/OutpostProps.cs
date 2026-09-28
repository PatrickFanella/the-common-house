using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Prepares CC0 Poly Haven models (decision D-21) for the outpost: one HDRP/Lit material per
    /// model part, built from that part's diffuse, normal, and ARM maps, and remapped in the FBX
    /// importer so every instance uses it. Models carry no colliders.
    /// </summary>
    public static class OutpostProps
    {
        public const string MaterialRoot = OutpostMaterials.MaterialRoot + "/Props";
        // Matches the file name after the asset ID, e.g. "_glass_nor_gl_1k.jpg" or "_diff_2k.jpg".
        private static readonly Regex MapFile = new Regex(@"^_((?<part>[a-z0-9]+)_)?(?<map>diff|nor_gl|arm)_(?<res>\d+k)\.jpg$");

        /// <summary>The Poly Haven models placed in the outpost.</summary>
        public static readonly string[] Models =
        {
            "metal_office_desk", "vintage_radio_transceiver", "portable_cassette_player", "clipboard", "pot_enamel_01",
            "metal_stool_01", "hanging_industrial_lamp", "mounted_fluorescent_lights", "dining_table",
            "painted_wooden_chair_01", "painted_wooden_chair_02", "vintage_electric_kettle", "painted_wooden_cabinet",
            "wall_clock", "wooden_bookshelf_worn", "standing_chalkboard_01", "binder_notebook", "cardboard_box_01",
        };

        /// <summary>Prepares every model and logs its material mapping and size, for review after import changes.</summary>
        [MenuItem("Common House/Art/Prepare Outpost Props")]
        public static void PrepareAll()
        {
            foreach (var asset in Models)
            {
                var prefab = Model(asset);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>();
                    var bounds = renderers[0].bounds;
                    foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                    var materials = string.Join(", ", renderers.SelectMany(renderer => renderer.sharedMaterials).Select(material => material == null ? "none" : material.name).Distinct());
                    Debug.Log($"[OutpostProps] {asset}: size {bounds.size.ToString("F3")} min {bounds.min.ToString("F3")} materials {materials}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Places a model so the bottom centre of its bounds is at <paramref name="bottomCentre"/>, with
        /// its front facing <paramref name="facing"/> (a horizontal direction).
        /// </summary>
        public static GameObject Place(Transform parent, string asset, Vector3 bottomCentre, Vector3 facing)
        {
            var holder = new GameObject("Prop: " + asset).transform;
            holder.SetParent(parent, false);
            holder.position = bottomCentre;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(Model(asset), holder);
            // Keep the model root's own rotation: some FBX roots carry the axis conversion.
            instance.transform.localPosition = Vector3.zero;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            instance.transform.localPosition = new Vector3(bottomCentre.x - bounds.center.x, bottomCentre.y - bounds.min.y, bottomCentre.z - bounds.center.z);
            // Poly Haven models face -Z after axis conversion.
            holder.rotation = Quaternion.LookRotation(-new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);
            FillMissingMaterials(asset, renderers);
            return holder.gameObject;
        }

        /// <summary>Gives submeshes that have no FBX material the model's main material.</summary>
        private static void FillMissingMaterials(string asset, IEnumerable<Renderer> renderers)
        {
            var main = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + asset + ".mat");
            if (main == null) return;
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                var changed = false;
                for (var i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && AssetDatabase.GetAssetPath(materials[i]).StartsWith(MaterialRoot, StringComparison.Ordinal)) continue;
                    materials[i] = main;
                    changed = true;
                }
                if (changed) renderer.sharedMaterials = materials;
            }
        }

        /// <summary>Imports the model with project materials and returns its model prefab.</summary>
        public static GameObject Model(string asset)
        {
            var folder = ScanTextures.PolyHavenRoot + asset;
            var fbx = Directory.GetFiles(folder, "*.fbx").Select(path => path.Replace('\\', '/')).SingleOrDefault()
                      ?? throw new InvalidOperationException("No FBX for " + asset + "; run `git lfs pull`.");
            var importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
            var changed = Configure(importer);
            var parts = Parts(folder, asset);
            foreach (var source in AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().Select(material => material.name).Distinct().ToList())
            {
                var part = Match(source, asset, parts.Keys) ?? throw new InvalidOperationException(
                    $"{asset}: no texture set for FBX material '{source}' (parts: {string.Join(", ", parts.Keys.Select(key => key == "" ? "main" : key))}).");
                var material = PartMaterial(asset, part, parts[part]);
                var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), source);
                if (!importer.GetExternalObjectMap().TryGetValue(id, out var mapped) || mapped != material)
                {
                    importer.AddRemap(id, material);
                    changed = true;
                }
            }
            if (changed) importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        }

        private static bool Configure(ModelImporter importer)
        {
            var changed = false;
            void Set<T>(Func<T> get, Action<T> set, T value)
            {
                if (EqualityComparer<T>.Default.Equals(get(), value)) return;
                set(value);
                changed = true;
            }
            Set(() => importer.importAnimation, value => importer.importAnimation = value, false);
            Set(() => importer.importCameras, value => importer.importCameras = value, false);
            Set(() => importer.importLights, value => importer.importLights = value, false);
            Set(() => importer.addCollider, value => importer.addCollider = value, false);
            Set(() => importer.bakeAxisConversion, value => importer.bakeAxisConversion = value, true);
            Set(() => importer.materialImportMode, value => importer.materialImportMode = value, ModelImporterMaterialImportMode.ImportViaMaterialDescription);
            if (changed) importer.SaveAndReimport();
            return changed;
        }

        /// <summary>Texture sets by part name ("" for the main part), from the downloaded file names.</summary>
        private static Dictionary<string, Dictionary<string, string>> Parts(string folder, string asset)
        {
            var parts = new Dictionary<string, Dictionary<string, string>>();
            foreach (var path in Directory.GetFiles(folder, "*.jpg").Select(path => path.Replace('\\', '/')))
            {
                var name = Path.GetFileName(path);
                if (!name.StartsWith(asset + "_", StringComparison.Ordinal)) continue;
                var match = MapFile.Match(name.Substring(asset.Length));
                if (!match.Success) continue;
                var part = match.Groups["part"].Success ? match.Groups["part"].Value : "";
                if (!parts.TryGetValue(part, out var maps)) parts[part] = maps = new Dictionary<string, string>();
                maps[match.Groups["map"].Value] = path;
            }
            if (parts.Count == 0) throw new InvalidOperationException("No texture maps for " + asset);
            return parts;
        }

        /// <summary>
        /// Longest part name contained in the FBX material name after the asset ID (which may itself
        /// contain a part name, as "standing_chalkboard_01" contains "board"), else the main part,
        /// else the only part.
        /// </summary>
        private static string Match(string source, string asset, IEnumerable<string> parts)
        {
            var list = parts.ToList();
            var lower = source.ToLowerInvariant();
            if (lower.StartsWith(asset, StringComparison.Ordinal)) lower = lower.Substring(asset.Length);
            var named = list.Where(part => part != "" && lower.Contains(part)).OrderByDescending(part => part.Length).FirstOrDefault();
            if (named != null) return named;
            if (list.Contains("")) return "";
            return list.Count == 1 ? list[0] : null;
        }

        private static Material PartMaterial(string asset, string part, Dictionary<string, string> maps)
        {
            var name = part == "" ? asset : asset + "_" + part;
            var path = MaterialRoot + "/" + name + ".mat";
            Directory.CreateDirectory(MaterialRoot);
            var shader = Shader.Find("HDRP/Lit") ?? throw new InvalidOperationException("HDRP/Lit is unavailable.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            var glass = !maps.ContainsKey("diff");
            material.SetTexture("_BaseColorMap", glass ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(maps["diff"]));
            material.SetColor("_BaseColor", glass ? new Color(0.85f, 0.9f, 0.9f, 0.18f) : Color.white);
            material.SetTexture("_NormalMap", maps.TryGetValue("nor_gl", out var normal) ? NormalMap(normal) : null);
            material.SetFloat("_NormalScale", 1f);
            if (maps.TryGetValue("arm", out var arm))
            {
                var resolution = Regex.Match(Path.GetFileName(arm), @"_(\d+k)\.jpg$").Groups[1].Value;
                material.SetTexture("_MaskMap", ScanTextures.SavedMask(arm, OutpostMaterials.GeneratedRoot + "/" + name + "_mask_" + resolution + ".png"));
                material.SetFloat("_MetallicRemapMax", 1f);
                material.SetFloat("_SmoothnessRemapMax", 1f);
                material.SetFloat("_AORemapMax", 1f);
            }
            else
            {
                material.SetTexture("_MaskMap", null);
            }
            HDMaterial.SetSurfaceType(material, glass);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D NormalMap(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
