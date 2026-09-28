using System;
using System.IO;
using System.Linq;
using CommonHouse.Experiments.N64;
using CommonHouse.Experiments.PS1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// The chosen direction (D-24, issue #77): the PS1 look with a PS2-era treatment after GTA 3.
    /// Builds from the PS1 scene: 640x448 frames with vertex snapping to match, heavier brownish
    /// fog, the haze filter, and a lighter dither. The first-person camera is unchanged.
    /// </summary>
    public static class Gta3Outpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/Gta3";
        public const string ScenePath = Root + "/OutpostGta3.unity";
        public const string ProfilePath = Root + "/Gta3VolumeProfile.asset";
        public const int Width = 640;
        public const int Height = 448;

        [MenuItem("Common House/Experiments/Build GTA 3-style Outpost")]
        public static void Build()
        {
            if (!File.Exists(PS1Outpost.ScenePath))
                throw new InvalidOperationException("Build the PS1 outpost first: python3 scripts/build_ps1.py");
            RegisterEffects();
            Directory.CreateDirectory(Root);
            var scene = EditorSceneManager.OpenScene(PS1Outpost.ScenePath);
            var lighting = UnityEngine.Object.FindFirstObjectByType<PS1Lighting>();
            lighting.snapResolution = new Vector2(Width, Height);
            lighting.fogStart = 5f;
            lighting.fogEnd = 22f;
            lighting.fogColor = new Color(0.16f, 0.14f, 0.12f);
            lighting.ambient = new Color(0.4f, 0.38f, 0.36f);
            lighting.Apply();
            var volume = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).Single(item => item.isGlobal);
            volume.sharedProfile = Profile();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save " + ScenePath);
        }

        /// <summary>Registers the haze filter ahead of the dither, so the dither is not blurred.</summary>
        public static void RegisterEffects()
        {
            N64Outpost.RegisterScreen();
            var settings = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>();
            var haze = typeof(HazeFilter).AssemblyQualifiedName;
            var serialized = new SerializedObject(settings);
            var property = serialized.GetIterator();
            while (property.Next(true))
            {
                if (!property.isArray || property.propertyType == SerializedPropertyType.String) continue;
                if (!property.propertyPath.EndsWith("m_AfterPostProcessCustomPostProcesses.m_CustomPostProcessTypesAsString", StringComparison.Ordinal)) continue;
                var present = false;
                for (var i = 0; i < property.arraySize; i++) present |= property.GetArrayElementAtIndex(i).stringValue == haze;
                if (present) continue;
                property.InsertArrayElementAtIndex(0);
                property.GetArrayElementAtIndex(0).stringValue = haze;
            }
            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
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
            profile.Add<Fog>(true).enabled.value = false;
            var haze = profile.Add<HazeFilter>(true);
            haze.enabled.value = true;
            var screen = profile.Add<N64Screen>(true);
            screen.enabled.value = true;
            // The PS2 framebuffer was usually 32-bit; keep a lighter trace of the PS1 dither.
            screen.bitsPerChannel.value = 6;
            screen.dither.value = 0.6f;
            screen.saturation.value = 1f;
            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
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
            RegisterEffects();
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
                // PS2 video output was soft, so scale up with bilinear filtering.
                ExperimentCapture.Render(camera, Width, Height, Path.Combine(output, name + ".png"), FilterMode.Bilinear);
            }
        }
    }
}
