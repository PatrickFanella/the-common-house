using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Prepares characters built by scripts/characters.py (decision D-23): a humanoid avatar from
    /// MPFB's game-engine rig, and HDRP materials built from each part's MakeHuman textures.
    /// </summary>
    public static class CharacterAssets
    {
        public const string Root = "Assets/CommonHouse/Art/Characters";
        private const string SkinProfile = "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/SkinDiffusionProfile.asset";

        [Serializable]
        private sealed class MaterialEntry
        {
            public string name;
            public string kind;
            public string diffuse;
            public string normal;
        }

        [Serializable]
        private sealed class Description
        {
            public string id;
            public List<MaterialEntry> materials;
        }

        /// <summary>IDs of the characters present under <see cref="Root"/>.</summary>
        public static IEnumerable<string> Ids()
        {
            return Directory.Exists(Root)
                ? Directory.GetDirectories(Root).Select(Path.GetFileName).Where(id => File.Exists(FbxPath(id))).OrderBy(id => id)
                : Enumerable.Empty<string>();
        }

        public static string FbxPath(string id) => Root + "/" + id + "/" + id + ".fbx";

        [MenuItem("Common House/Art/Prepare Characters")]
        public static void PrepareAll()
        {
            foreach (var id in Ids())
            {
                var model = Prepare(id);
                var avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath(id)).OfType<Avatar>().FirstOrDefault();
                var importer = (ModelImporter)AssetImporter.GetAtPath(FbxPath(id));
                var embedded = string.Join(" | ", AssetDatabase.LoadAllAssetsAtPath(FbxPath(id)).OfType<Material>().Select(m => m.name));
                var remaps = string.Join(" | ", importer.GetExternalObjectMap().Select(pair => pair.Key.name + "->" + (pair.Value ? pair.Value.name : "null")));
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                var animator = instance.GetComponent<Animator>();
                var head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
                var used = string.Join(" | ", instance.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Select(m => m ? m.name : "null"));
                Debug.Log($"[CharacterAssets] {id}: embedded [{embedded}] remaps [{remaps}] used [{used}] head y {(head ? head.position.y : -1f):F2} scale {instance.transform.lossyScale}");
                UnityEngine.Object.DestroyImmediate(instance);
                Debug.Log($"[CharacterAssets] {id}: avatar {(avatar != null && avatar.isValid && avatar.isHuman ? "valid humanoid" : "INVALID")}, renderers {model.GetComponentsInChildren<Renderer>().Length}");
            }
            AssetDatabase.SaveAssets();
        }

        /// <summary>Configures the model importer and materials, and returns the model prefab.</summary>
        public static GameObject Prepare(string id)
        {
            var fbx = FbxPath(id);
            var importer = (ModelImporter)AssetImporter.GetAtPath(fbx) ?? throw new InvalidOperationException("Missing character " + fbx);
            var changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                changed = true;
            }
            if (importer.importAnimation || importer.importCameras || importer.importLights)
            {
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                changed = true;
            }
            if (!importer.importBlendShapes)
            {
                importer.importBlendShapes = true;
                changed = true;
            }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                changed = true;
            }
            if (changed) importer.SaveAndReimport();

            // Build every material first: texture reimports reload the model importer and would
            // discard remaps added before them.
            var materials = Entries(id).Select(entry => (entry.name, material: PartMaterial(id, entry))).ToList();
            AssetDatabase.SaveAssets();
            importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
            changed = false;
            foreach (var (name, material) in materials)
            {
                var source = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (importer.GetExternalObjectMap().TryGetValue(source, out var mapped) && mapped == material) continue;
                importer.AddRemap(source, material);
                changed = true;
            }
            if (changed) importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        }

        /// <summary>Reads the material table that tools/characters/mpfb_build.py wrote beside the FBX.</summary>
        private static List<MaterialEntry> Entries(string id)
        {
            var path = Root + "/" + id + "/" + id + ".character.json";
            var description = JsonUtility.FromJson<Description>(File.ReadAllText(path));
            if (description?.materials == null || description.materials.Count == 0)
                throw new InvalidOperationException(path + " lists no materials");
            return description.materials;
        }

        private static Material PartMaterial(string id, MaterialEntry entry)
        {
            var folder = Root + "/" + id + "/Materials";
            Directory.CreateDirectory(folder);
            var name = entry.name.Replace("Human.", "");
            var path = folder + "/" + name + ".mat";
            var shader = Shader.Find("HDRP/Lit") ?? throw new InvalidOperationException("HDRP/Lit is unavailable.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            // Hair, brows, and lashes are cards with alpha; the eye's cornea shell shares the eye
            // texture, whose alpha reveals the iris.
            var cutout = entry.kind == "hair" || entry.kind == "eyebrows" || entry.kind == "eyelashes" || entry.kind == "eyes";
            material.SetTexture("_BaseColorMap", entry.diffuse == null ? null : Texture(entry.diffuse, cutout, false));
            material.SetTexture("_NormalMap", entry.normal == null ? null : Texture(entry.normal, false, true));
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", Smoothness(entry.kind));
            HDMaterial.SetAlphaClipping(material, cutout);
            if (cutout) HDMaterial.SetAlphaCutoff(material, entry.kind == "eyes" ? 0.5f : entry.kind == "hair" ? 0.35f : 0.25f);
            material.SetFloat("_DoubleSidedEnable", cutout && entry.kind != "eyes" ? 1f : 0f);
            if (entry.kind == "basemesh")
            {
                // Subsurface scattering with HDRP's skin profile softens the scanned skin texture.
                material.SetFloat("_MaterialID", 0f);
                material.SetFloat("_SubsurfaceMask", 1f);
                var profile = AssetDatabase.LoadAssetAtPath<DiffusionProfileSettings>(SkinProfile);
                if (profile != null) HDMaterial.SetDiffusionProfile(material, profile);
            }
            else
            {
                material.SetFloat("_MaterialID", 1f); // Standard
            }
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static float Smoothness(string kind)
        {
            switch (kind)
            {
                case "basemesh": return 0.45f;
                case "eyes": return 0.9f;
                case "teeth": return 0.6f;
                case "tongue": return 0.55f;
                case "hair": return 0.35f;
                default: return 0.25f;
            }
        }

        private static Texture2D Texture(string relative, bool alpha, bool normal)
        {
            var path = Root + "/Textures/" + relative;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path) ?? throw new InvalidOperationException("Missing texture " + path);
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (importer.textureType != type || importer.alphaIsTransparency != alpha)
            {
                importer.textureType = type;
                importer.alphaIsTransparency = alpha;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
