using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonHouse.Experiments.N64;
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
    /// Experimental PlayStation variant of the outpost (issue #69), built from the N64 scene so it
    /// reuses the decimated models and 64 px textures. Every surface uses the CommonHouse/PS1
    /// shader (affine textures, snapped vertices, vertex lighting and fog, no filtering), the frame
    /// is dithered to 15-bit colour, and captures are scaled up without filtering.
    /// </summary>
    public static class PS1Outpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/PS1";
        public const string ScenePath = Root + "/OutpostPS1.unity";
        public const string MaterialRoot = Root + "/Materials";
        public const string ProfilePath = Root + "/PS1VolumeProfile.asset";
        private const string ShaderName = "CommonHouse/PS1";

        [MenuItem("Common House/Experiments/Build PS1 Outpost")]
        public static void Build()
        {
            if (!File.Exists(N64Outpost.ScenePath))
                throw new InvalidOperationException("Build the N64 outpost first: python3 scripts/build_n64.py");
            N64Outpost.RegisterScreen();
            Directory.CreateDirectory(MaterialRoot);
            var shader = Shader.Find(ShaderName) ?? throw new InvalidOperationException("Missing shader " + ShaderName);
            var scene = EditorSceneManager.OpenScene(N64Outpost.ScenePath);

            // HDRP decals do not reach a custom unlit shader, so faces become flat cards instead,
            // as several PlayStation games drew them.
            foreach (var decal in UnityEngine.Object.FindObjectsByType<DecalProjector>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var photo = decal.material != null ? decal.material.GetTexture("_BaseColorMap") as Texture2D : null;
                if (photo != null) FaceCard(decal.transform, photo, shader);
                UnityEngine.Object.DestroyImmediate(decal.gameObject);
            }

            var variants = new Dictionary<Material, Material>();
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => Variant(material, shader, variants)).ToArray();

            var lighting = new GameObject("PS1 lighting").AddComponent<PS1Lighting>();
            lighting.Apply();
            var volume = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).Single(item => item.isGlobal);
            volume.sharedProfile = Profile();
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
        }

        private static Material Variant(Material source, Shader shader, Dictionary<Material, Material> variants)
        {
            if (source == null) return null;
            if (source.shader == shader) return source;
            if (variants.TryGetValue(source, out var existing)) return existing;
            var path = MaterialRoot + "/" + Safe(source.name) + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = source.name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            var texture = source.HasProperty("_BaseColorMap") ? source.GetTexture("_BaseColorMap") : null;
            material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_MainTex", source.HasProperty("_BaseColorMap") ? source.GetTextureScale("_BaseColorMap") : Vector2.one);
            var color = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : Color.white;
            material.SetColor("_Color", color);
            var clipped = source.HasProperty("_AlphaCutoffEnable") && source.GetFloat("_AlphaCutoffEnable") > 0.5f;
            var transparent = source.HasProperty("_SurfaceType") && source.GetFloat("_SurfaceType") > 0.5f;
            // No blending on this console-style surface: cut-out parts stay cut out, and faint
            // glass disappears.
            material.SetFloat("_Cutoff", clipped ? (source.HasProperty("_AlphaCutoff") ? source.GetFloat("_AlphaCutoff") : 0.5f) : transparent ? 0.5f : 0f);
            var worldMapped = source.HasProperty("_UVBase") && source.GetFloat("_UVBase") > 4.5f;
            material.SetFloat("_WorldUV", worldMapped ? 1f : 0f);
            if (worldMapped) material.SetFloat("_WorldUVScale", source.GetFloat("_TexWorldScale"));
            var doubleSided = source.HasProperty("_DoubleSidedEnable") && source.GetFloat("_DoubleSidedEnable") > 0.5f;
            material.SetFloat("_Cull", doubleSided ? (float)CullMode.Off : (float)CullMode.Back);
            EditorUtility.SetDirty(material);
            variants[source] = material;
            return material;
        }

        /// <summary>A quad showing the face photo, where the decal was, facing out of the face.</summary>
        private static void FaceCard(Transform decal, Texture2D photo, Shader shader)
        {
            var path = MaterialRoot + "/" + Safe(photo.name) + "_card.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = photo.name + " card" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_MainTex", photo);
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_Cull", (float)CullMode.Off);
            EditorUtility.SetDirty(material);

            var card = GameObject.CreatePrimitive(PrimitiveType.Quad);
            UnityEngine.Object.DestroyImmediate(card.GetComponent<Collider>());
            card.name = "Face card";
            card.transform.SetParent(decal.parent, true);
            // The decal sat in front of the face, projecting back into it along its +Z.
            var faceForward = -decal.forward;
            card.transform.position = decal.position - faceForward * 0.015f;
            card.transform.rotation = Quaternion.LookRotation(-faceForward, Vector3.up);
            card.transform.localScale = new Vector3(FacePhotos.FaceSize.x, FacePhotos.FaceSize.y, 1f);
            card.GetComponent<MeshRenderer>().sharedMaterial = material;
            card.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
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
            profile.Add<ScreenSpaceAmbientOcclusion>(true).intensity.value = 0f;
            profile.Add<ContactShadows>(true).enable.value = false;
            // The PS1 shader fogs each vertex itself.
            profile.Add<Fog>(true).enabled.value = false;
            var screen = profile.Add<N64Screen>(true);
            screen.enabled.value = true;
            screen.bitsPerChannel.value = 5;
            screen.dither.value = 1f;
            screen.saturation.value = 1.05f;
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

        /// <summary>Renders the PS1 scene from the same views as the N64 comparison.</summary>
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            N64Outpost.RegisterScreen();
            EditorSceneManager.OpenScene(ScenePath);
            UnityEngine.Object.FindFirstObjectByType<PS1Lighting>().Apply();
            var camera = Camera.main ?? throw new InvalidOperationException("Missing camera in " + ScenePath);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            foreach (var (name, eye, target) in N64Outpost.Views())
            {
                camera.fieldOfView = 60f;
                camera.transform.position = eye;
                camera.transform.LookAt(target);
                Render(camera, Path.Combine(output, name + "-ps1.png"));
            }
        }

        private static void Render(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var frame = new RenderTexture(N64Outpost.ScreenWidth, N64Outpost.ScreenHeight, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            var display = new RenderTexture(1280, 960, 0, RenderTextureFormat.ARGB32);
            try
            {
                frame.Create();
                display.Create();
                camera.targetTexture = frame;
                for (var i = 0; i < 3; i++) camera.Render();
                Save(frame, path);
                // PlayStation output is shown here with sharp pixels, scaled without filtering.
                Graphics.Blit(frame, display);
                Save(display, path.Replace(".png", "-display.png"));
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                frame.Release();
                display.Release();
                UnityEngine.Object.DestroyImmediate(frame);
                UnityEngine.Object.DestroyImmediate(display);
            }
        }

        private static void Save(RenderTexture texture, string path)
        {
            RenderTexture.active = texture;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
