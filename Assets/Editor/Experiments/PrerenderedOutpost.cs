using System;
using System.IO;
using System.Linq;
using CommonHouse.Experiments.N64;
using CommonHouse.Outpost;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// Final Fantasy VII-IX style experiment (issue #71): the detailed, realistically lit outpost
    /// seen from fixed, composed camera angles, with low-poly residents, at 320x240 with dither.
    /// Reuses the N64 experiment's decimated resident and face photo.
    /// </summary>
    public static class PrerenderedOutpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/Prerendered";
        public const string ScenePath = Root + "/OutpostPrerendered.unity";
        public const string ProfilePath = Root + "/DitherVolumeProfile.asset";
        private const string Resident = N64Outpost.CharacterRoot + "/test-resident-01.fbx";
        private const string FaceDecal = FacePhotos.Root + "/test-resident-01_face_bilinear.mat";
        private static readonly string[] ResidentCapsules = { "IvoConversation", "RenPrivateConversation" };

        // Fixed shots, composed like a pre-rendered background: high corners and low angles.
        public static readonly (string Name, Vector3 Eye, Vector3 Target, float Fov)[] Shots =
        {
            ("radio-room", new Vector3(-5.6f, 2.75f, 3.5f), new Vector3(-10.6f, 0.7f, -2.4f), 55f),
            ("kitchen-low", new Vector3(4.4f, 0.45f, -3.5f), new Vector3(0.2f, 1.25f, 1.9f), 50f),
            ("kitchen-high", new Vector3(-4.5f, 2.8f, -3.6f), new Vector3(0.8f, 0.6f, 1.8f), 50f),
            ("archive", new Vector3(13.6f, 2.8f, -3.6f), new Vector3(9f, 0.7f, 1.6f), 55f),
            ("workshop", new Vector3(8.4f, 1.9f, -0.8f), new Vector3(11.4f, 1.1f, 2.9f), 45f),
        };

        [MenuItem("Common House/Experiments/Build Pre-rendered Outpost")]
        public static void Build()
        {
            if (!File.Exists(Resident)) throw new InvalidOperationException("Build the N64 outpost first: python3 scripts/build_n64.py");
            N64Outpost.RegisterScreen();
            Directory.CreateDirectory(Root);
            var scene = EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Resident);
            var decal = AssetDatabase.LoadAssetAtPath<Material>(FaceDecal);
            foreach (var name in ResidentCapsules)
            {
                var capsule = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(item => item.name == name);
                capsule.GetComponent<MeshRenderer>().enabled = false;
                var resident = (GameObject)PrefabUtility.InstantiatePrefab(model, capsule.parent);
                resident.name = "Low-poly resident at " + name;
                resident.transform.position = new Vector3(capsule.position.x, 0f, capsule.position.z);
                if (decal != null) FacePhotos.Attach(resident, decal, Vector3.back);
            }
            // The realistic lighting stays; a second volume adds only the 15-bit dither.
            var volume = new GameObject("Pre-rendered dither").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 5f;
            volume.sharedProfile = Profile();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save " + ScenePath);
        }

        private static VolumeProfile Profile()
        {
            AssetDatabase.DeleteAsset(ProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            var screen = profile.Add<N64Screen>(true);
            screen.enabled.value = true;
            screen.bitsPerChannel.value = 5;
            screen.dither.value = 1f;
            screen.saturation.value = 1f;
            screen.name = nameof(N64Screen);
            AssetDatabase.AddObjectToAsset(screen, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

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
            var camera = Camera.main ?? throw new InvalidOperationException("Missing camera");
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            foreach (var (name, eye, target, fov) in Shots)
            {
                camera.fieldOfView = fov;
                camera.transform.position = eye;
                camera.transform.LookAt(target);
                ExperimentCapture.Render(camera, 320, 240, Path.Combine(output, name + ".png"), FilterMode.Bilinear);
            }
        }
    }

    /// <summary>Shared low-resolution render-and-save helper for the style experiments.</summary>
    public static class ExperimentCapture
    {
        /// <summary>Renders at the given size and saves it, plus a 1280-wide copy scaled with the given filter.</summary>
        public static void Render(Camera camera, int width, int height, string path, FilterMode upscale)
        {
            var previous = RenderTexture.active;
            var frame = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { filterMode = upscale };
            var displayHeight = 1280 * height / width;
            var display = width < 1280 ? new RenderTexture(1280, displayHeight, 0, RenderTextureFormat.ARGB32) : null;
            try
            {
                frame.Create();
                camera.targetTexture = frame;
                for (var i = 0; i < 3; i++) camera.Render();
                Save(frame, path);
                if (display != null)
                {
                    display.Create();
                    Graphics.Blit(frame, display);
                    Save(display, path.Replace(".png", "-display.png"));
                }
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                frame.Release();
                UnityEngine.Object.DestroyImmediate(frame);
                if (display != null)
                {
                    display.Release();
                    UnityEngine.Object.DestroyImmediate(display);
                }
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
