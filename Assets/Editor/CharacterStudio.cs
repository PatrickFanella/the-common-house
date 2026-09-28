using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Renders each generated character in a neutral room corner for review: full body from the
    /// front, a three-quarter view, and the face. Builds its own unsaved scene.
    /// </summary>
    public static class CharacterStudio
    {
        private const int Width = 1200;
        private const int Height = 1500;

        // Explicit graphical evidence operation in a disposable project.
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required for rendered characters.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildRoom();
            var camera = new GameObject("Studio camera").AddComponent<Camera>();
            camera.nearClipPlane = 0.02f;
            camera.farClipPlane = 30f;
            var hdCamera = camera.gameObject.AddComponent<HDAdditionalCameraData>();
            hdCamera.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            hdCamera.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.High;

            var inventory = new StringBuilder();
            var ids = CharacterAssets.Ids().ToList();
            if (ids.Count == 0) throw new InvalidOperationException("No characters under " + CharacterAssets.Root);
            foreach (var id in ids)
            {
                var model = CharacterAssets.Prepare(id);
                var character = (GameObject)PrefabUtility.InstantiatePrefab(model);
                character.transform.position = Vector3.zero;
                var animator = character.GetComponent<Animator>();
                var avatar = animator != null ? animator.avatar : null;
                var human = avatar != null && avatar.isValid && avatar.isHuman;
                var renderers = character.GetComponentsInChildren<Renderer>();
                var head = human ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
                if (head == null) throw new InvalidOperationException(id + " has no humanoid head bone");
                // Skinned-mesh bounds are conservative, so frame and measure from the head bone.
                var face = head.position + Vector3.up * 0.03f;
                var middle = face.y * 0.5f + 0.05f;
                inventory.AppendLine($"{id}: humanoid {human}, head bone at {head.position.y:F2} m, renderers {renderers.Length}, materials {string.Join(", ", renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name).Distinct())}");

                Shoot(camera, 30f, new Vector3(0f, middle, 4.2f), new Vector3(0f, middle, 0f), Path.Combine(output, id + "-front.jpg"));
                Shoot(camera, 30f, new Vector3(2.5f, middle + 0.2f, 3.3f), new Vector3(0f, middle, 0f), Path.Combine(output, id + "-three-quarter.jpg"));
                Shoot(camera, 22f, face + new Vector3(0.25f, 0.02f, 0.75f), face, Path.Combine(output, id + "-face.jpg"));
                UnityEngine.Object.DestroyImmediate(character);
            }
            File.WriteAllText(Path.Combine(output, "character-inventory.txt"), inventory.ToString());
            File.WriteAllText(Path.Combine(output, "graphics-device.txt"), SystemInfo.graphicsDeviceName + "\n" + SystemInfo.graphicsDeviceVersion + "\n");
        }

        /// <summary>A plaster corner on floorboards, with key, fill, and rim lights and fixed exposure.</summary>
        private static void BuildRoom()
        {
            var materials = OutpostMaterials.Build();
            void Box(string name, Vector3 position, Vector3 scale, Material material)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = name;
                box.transform.position = position;
                box.transform.localScale = scale;
                box.GetComponent<MeshRenderer>().sharedMaterial = material;
            }
            Box("Floor", new Vector3(0f, -0.05f, 0f), new Vector3(8f, 0.1f, 8f), materials.Floorboards);
            Box("Back wall", new Vector3(0f, 1.5f, -1.6f), new Vector3(8f, 3f, 0.1f), materials.Plaster);
            Box("Side wall", new Vector3(-2.4f, 1.5f, 0f), new Vector3(0.1f, 3f, 8f), materials.Plaster);
            Light("Key", new Vector3(1.6f, 2.4f, 2.2f), 1800f, new Color(1f, 0.9f, 0.8f), true);
            Light("Fill", new Vector3(-1.8f, 1.6f, 2.4f), 700f, new Color(0.85f, 0.9f, 1f), false);
            Light("Rim", new Vector3(-0.6f, 2.2f, -1.2f), 600f, new Color(1f, 0.95f, 0.9f), false);

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var exposure = profile.Add<Exposure>(true);
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.value = 6f;
            profile.Add<MotionBlur>(true).intensity.value = 0f;
            profile.Add<Bloom>(true).intensity.value = 0.05f;
            var environment = profile.Add<VisualEnvironment>(true);
            environment.skyType.value = (int)SkyType.Gradient;
            environment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
            var sky = profile.Add<GradientSky>(true);
            sky.top.value = sky.middle.value = sky.bottom.value = new Color(0.25f, 0.24f, 0.23f) * 1.2f * Mathf.Pow(2f, 6f) * 0.05f;
            var volume = new GameObject("Studio volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;
        }

        private static void Light(string name, Vector3 position, float lumens, Color color, bool shadows)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            go.AddComponent<HDAdditionalLightData>();
            light.lightUnit = LightUnit.Lumen;
            light.intensity = lumens;
            light.color = color;
            light.range = 12f;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        }

        private static void Shoot(Camera camera, float fov, Vector3 eye, Vector3 target, string path)
        {
            camera.fieldOfView = fov;
            camera.transform.position = eye;
            camera.transform.LookAt(target);
            var previous = RenderTexture.active;
            var render = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                render.Create();
                camera.targetTexture = render;
                // HDRP applies exposure and sky lighting a frame late.
                for (var frame = 0; frame < 3; frame++) camera.Render();
                RenderTexture.active = render;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToJPG(92));
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                render.Release();
                UnityEngine.Object.DestroyImmediate(render);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
    }
}
