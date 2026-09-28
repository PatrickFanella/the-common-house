using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonHouse.Experiments.PS1;
using CommonHouse.Outpost;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// 1990s TV-anime experiment (issue #73): the realistic outpost and detailed test resident drawn
    /// with the CommonHouse/Anime shader (flat, posterised colour, two-tone light, ink outlines).
    /// </summary>
    public static class AnimeOutpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/Anime";
        public const string ScenePath = Root + "/OutpostAnime.unity";
        public const string MaterialRoot = Root + "/Materials";
        public const string ProfilePath = Root + "/AnimeVolumeProfile.asset";
        private const string ShaderName = "CommonHouse/Anime";
        private const string StandIn = "test-resident-01";
        private static readonly string[] ResidentCapsules = { "IvoConversation", "RenPrivateConversation" };

        [MenuItem("Common House/Experiments/Build Anime Outpost")]
        public static void Build()
        {
            Directory.CreateDirectory(MaterialRoot);
            var shader = Shader.Find(ShaderName) ?? throw new InvalidOperationException("Missing shader " + ShaderName);
            var model = CharacterAssets.Prepare(StandIn);
            var scene = EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var name in ResidentCapsules)
            {
                var capsule = transforms.Single(item => item.name == name);
                capsule.GetComponent<MeshRenderer>().enabled = false;
                var resident = (GameObject)PrefabUtility.InstantiatePrefab(model, capsule.parent);
                resident.name = "Resident at " + name;
                // MPFB characters face +Z; the review views look at them from -Z.
                resident.transform.SetPositionAndRotation(new Vector3(capsule.position.x, 0f, capsule.position.z), Quaternion.Euler(0f, 180f, 0f));
            }
            var variants = new Dictionary<Material, Material>();
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => Variant(material, shader, variants)).ToArray();
            }
            var lighting = new GameObject("Anime lighting").AddComponent<PS1Lighting>();
            lighting.ambient = new Color(0.42f, 0.4f, 0.42f);
            lighting.referenceLumens = 700f;
            lighting.Apply();
            var volume = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).Single(item => item.isGlobal);
            volume.sharedProfile = Profile();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save " + ScenePath);
        }

        private static Material Variant(Material source, Shader shader, Dictionary<Material, Material> variants)
        {
            if (source == null) return null;
            if (variants.TryGetValue(source, out var existing)) return existing;
            var path = MaterialRoot + "/" + Safe(source.name) + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = source.name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            var name = source.name.ToLowerInvariant();
            var texture = source.HasProperty("_BaseColorMap") ? source.GetTexture("_BaseColorMap") : null;
            material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_MainTex", source.HasProperty("_BaseColorMap") ? source.GetTextureScale("_BaseColorMap") : Vector2.one);
            material.SetColor("_Color", source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : Color.white);
            var clipped = source.HasProperty("_AlphaCutoffEnable") && source.GetFloat("_AlphaCutoffEnable") > 0.5f;
            var transparent = source.HasProperty("_SurfaceType") && source.GetFloat("_SurfaceType") > 0.5f;
            material.SetFloat("_Cutoff", clipped ? source.GetFloat("_AlphaCutoff") : transparent ? 0.5f : 0f);
            var worldMapped = source.HasProperty("_UVBase") && source.GetFloat("_UVBase") > 4.5f;
            material.SetFloat("_WorldUV", worldMapped ? 1f : 0f);
            if (worldMapped) material.SetFloat("_WorldUVScale", source.GetFloat("_TexWorldScale"));
            var doubleSided = source.HasProperty("_DoubleSidedEnable") && source.GetFloat("_DoubleSidedEnable") > 0.5f;
            material.SetFloat("_Cull", doubleSided ? (float)CullMode.Off : (float)CullMode.Back);
            // Detail to keep: eyes, brows, and lashes must still read; hair a little; the rest is flat.
            var fine = name.Contains("eye") || name.Contains("brow") || name.Contains("lash") || name.Contains("high-poly");
            // Walls, floors, and ceilings take the texture's average colour (its smallest mip), so
            // only the two light tones shade them, as on a painted background. Posterising a
            // stained scan instead makes blotches.
            var character = fine || name.Contains("short") || name.Contains("hair") || name == "body" || name.Contains("suit") || name.Contains("shoes");
            material.SetFloat("_Flatten", fine ? 1f : name.Contains("short") || name.Contains("hair") ? 3f : name == "body" ? 4f : worldMapped ? 12f : 5f);
            material.SetFloat("_Levels", character ? 6f : worldMapped ? 12f : 8f);
            material.SetFloat("_Saturation", character ? 1.25f : 1.1f);
            // Cut-out cards and glass get no ink line; everything else does.
            material.SetFloat("_OutlineWidth", clipped || transparent ? 0f : 0.0025f);
            // Skin shadows warm, as cel painters shade faces; other shadows cool.
            material.SetColor("_ShadowTint", name == "body" ? new Color(0.86f, 0.64f, 0.62f) : new Color(0.55f, 0.55f, 0.72f));
            EditorUtility.SetDirty(material);
            variants[source] = material;
            return material;
        }

        private static VolumeProfile Profile()
        {
            AssetDatabase.DeleteAsset(ProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            profile.Add<Exposure>(true).mode.value = ExposureMode.Fixed;
            profile.Add<MotionBlur>(true).intensity.value = 0f;
            profile.Add<Bloom>(true).intensity.value = 0f;
            profile.Add<Tonemapping>(true).mode.value = TonemappingMode.None;
            profile.Add<Fog>(true).enabled.value = false;
            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static string Safe(string name) => new string(name.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());

        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene(ScenePath);
            UnityEngine.Object.FindFirstObjectByType<PS1Lighting>().Apply();
            var camera = Camera.main ?? throw new InvalidOperationException("Missing camera");
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            foreach (var (name, eye, target) in N64Outpost.Views())
            {
                camera.fieldOfView = 60f;
                camera.transform.position = eye;
                camera.transform.LookAt(target);
                ExperimentCapture.Render(camera, 1280, 960, Path.Combine(output, name + ".png"), FilterMode.Bilinear);
            }
        }
    }
}
