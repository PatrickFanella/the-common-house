using System;
using System.IO;
using System.Linq;
using CommonHouse.Intro;
using CommonHouse.Interaction;
using CommonHouse.Look;
using CommonHouse.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    public static class IntroOpeningSceneGenerator
    {
        [MenuItem("Common House/Generate Arrival Opening")]
        public static void Generate()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Arrival refuge — temporary threshold staging").transform;
            var plaster = Material("Plaster");
            var floor = Material("Floorboards");
            var wood = Material("dining_table");
            Box(root, "Floor", new Vector3(0f, -.15f, 2.5f), new Vector3(8f, .3f, 11f), floor);
            Box(root, "West wall", new Vector3(-4f, 1.5f, 2.5f), new Vector3(.25f, 3f, 11f), plaster);
            Box(root, "East wall", new Vector3(4f, 1.5f, 2.5f), new Vector3(.25f, 3f, 11f), plaster);
            Box(root, "Threshold boundary", new Vector3(0f, 1.5f, -3f), new Vector3(8f, 3f, .25f), plaster);
            Box(root, "Rear wall", new Vector3(0f, 1.5f, 8f), new Vector3(8f, 3f, .25f), plaster);
            Box(root, "Ceiling", new Vector3(0f, 3.15f, 2.5f), new Vector3(8f, .3f, 11f), plaster);
            Box(root, "Doorway west", new Vector3(-2.5f, 1.5f, 1.5f), new Vector3(3f, 3f, .25f), plaster);
            Box(root, "Doorway east", new Vector3(2.5f, 1.5f, 1.5f), new Vector3(3f, 3f, .25f), plaster);
            Box(root, "Reception counter", new Vector3(2.7f, .4f, 4f), new Vector3(1.8f, .8f, .8f), wood);
            Box(root, "Welcome table", new Vector3(-2.2f, .4f, 5.5f), new Vector3(2f, .8f, 1f), wood);
            Box(root, "Temporary bedding", new Vector3(-2.4f, .3f, 7f), new Vector3(1.3f, .6f, 1.5f), wood);
            Box(root, "Radio desk", new Vector3(0f, .4f, 7.3f), new Vector3(1.6f, .8f, .8f), wood);

            var player = new GameObject("Arrival player");
            player.transform.SetParent(root, false);
            player.transform.position = new Vector3(0f, .05f, -2f);
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f; capsule.radius = .3f; capsule.center = Vector3.up * .9f;
            var movement = player.AddComponent<FirstPersonMovement>();
            var modal = player.AddComponent<ModalInputCoordinator>();
            var camera = new GameObject("Arrival camera").AddComponent<Camera>();
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = Vector3.up * 1.6f;
            camera.transform.localRotation = Quaternion.Euler(7f, 0f, 0f);
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.04f, .05f, .08f);
            camera.gameObject.AddComponent<HDAdditionalCameraData>().allowDynamicResolution = true;
            camera.gameObject.AddComponent<RetroScreen>();
            movement.SetViewTransform(camera.transform);
            var inspection = player.AddComponent<InspectionController>();
            inspection.Configure(camera, movement, modal);
            var opening = player.AddComponent<IntroOpeningController>();
            opening.Configure(modal);
            player.AddComponent<IntroOpeningOverlay>();

            Target(root, opening, "Entrance notice", new Vector3(0f, 1.4f, 0f), IntroTargetKind.Scene, "IN-S00", "Look at the entrance", plaster);
            Target(root, opening, "Ren at threshold", new Vector3(-1.5f, 1.2f, .4f), IntroTargetKind.Threshold, "IN-S01", "Speak with Ren", wood);
            Target(root, opening, "Reception ledger", new Vector3(2.7f, 1.1f, 3.5f), IntroTargetKind.Scene, "IN-S03", "Check in with Ivo", wood);
            Target(root, opening, "Return warning", new Vector3(1.5f, 1.5f, 5f), IntroTargetKind.Source, "IN-C01", "Ask Ren to repeat the return warning", plaster);
            Target(root, opening, "Service area map", new Vector3(3.7f, 1.5f, 5.8f), IntroTargetKind.Source, "IN-D01", "Read the service-area map", plaster);
            Target(root, opening, "Food and bedding", new Vector3(-2.2f, 1.1f, 5f), IntroTargetKind.Scene, "IN-S05", "Join the welcome at the table", wood);
            Target(root, opening, "Radio programs", new Vector3(0f, 1.1f, 6.8f), IntroTargetKind.Scene, "IN-S04", "Review the radio programs", wood);
            Target(root, opening, "Office greeting", new Vector3(2f, 1.4f, 7.2f), IntroTargetKind.Scene, "IN-S06", "Review the recorded office greeting", plaster);
            Target(root, opening, "Orientation summary", new Vector3(.8f, 1.4f, 3.2f), IntroTargetKind.Orientation, "orientation", "Review the house orientation", plaster);
            Target(root, opening, "Tomorrow's shift", new Vector3(-.6f, 1.2f, 4.3f), IntroTargetKind.Scene, "IN-S07", "Ask Ren about tomorrow", wood);

            foreach (var z in new[] { 0f, 4f, 7f })
            {
                var lamp = new GameObject("Temporary refuge lamp").AddComponent<Light>();
                lamp.transform.SetParent(root, false);
                lamp.transform.position = new Vector3(0f, 2.5f, z);
                lamp.type = LightType.Point;
                lamp.gameObject.AddComponent<HDAdditionalLightData>();
                lamp.lightUnit = LightUnit.Lumen; lamp.intensity = 1100f; lamp.range = 8f;
                lamp.color = new Color(1f, .9f, .78f); lamp.shadows = LightShadows.None;
            }
            root.gameObject.AddComponent<RetroLighting>().Apply();
            var volume = new GameObject("Arrival retro volume").AddComponent<Volume>();
            volume.transform.SetParent(root, false);
            volume.isGlobal = true;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(RetroLook.ProfilePath)
                ?? throw new InvalidOperationException("Missing existing retro volume profile.");
            if (!EditorSceneManager.SaveScene(scene, IntroOpeningController.ScenePath)) throw new InvalidOperationException("Could not save arrival scene.");
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(item => item.path != IntroOpeningController.ScenePath)
                .Append(new EditorBuildSettingsScene(IntroOpeningController.ScenePath, true)).ToArray();
            AssetDatabase.SaveAssets();
        }

        // Export only the new scene and its build-list entry from a disposable Editor invocation.
        public static void GenerateAndExport()
        {
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrWhiteSpace(output)) throw new InvalidOperationException("Explicit verification output is required.");
            Generate();
            Directory.CreateDirectory(output);
            foreach (var path in new[] { IntroOpeningController.ScenePath, IntroOpeningController.ScenePath + ".meta", "ProjectSettings/EditorBuildSettings.asset" })
                File.Copy(path, Path.Combine(output, Path.GetFileName(path)), true);
        }
        private static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>(RetroLook.MaterialRoot + "/" + name + ".mat")
            ?? throw new InvalidOperationException("Missing existing material " + name);
        private static GameObject Box(Transform root, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(root, false); go.transform.position = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        private static void Target(Transform root, IntroOpeningController controller, string name, Vector3 position,
            IntroTargetKind kind, string id, string prompt, Material material)
        {
            Box(root, name, position, new Vector3(.7f, .5f, .15f), material)
                .AddComponent<IntroOpeningTarget>().Configure(controller, kind, id, prompt);
        }
    }
}
