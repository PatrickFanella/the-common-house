using System;
using System.IO;
using System.Linq;
using CommonHouse.Outpost;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// Isometric RPG view experiment (issue #72), after Disco Elysium and Fallout: the realistic
    /// outpost from an orthographic camera looking north-west from above, with the ceilings and the
    /// walls nearest the camera hidden, and the detailed test resident at the resident positions.
    /// </summary>
    public static class IsometricOutpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/Isometric";
        public const string ScenePath = Root + "/OutpostIsometric.unity";
        private const string StandIn = "test-resident-01";
        private static readonly string[] ResidentCapsules = { "IvoConversation", "RenPrivateConversation" };
        // Walls between the camera and the rooms; the cutaway hides them.
        private static readonly string[] CutawayWalls = { "SouthWall", "EastWall" };

        public static readonly (string Name, Vector3 Target, float Size)[] Frames =
        {
            ("overview", new Vector3(0f, 0f, 0f), 10.5f),
            ("radio-room", new Vector3(-9.5f, 0.5f, 0f), 4.6f),
            ("kitchen", new Vector3(0f, 0.5f, 0.5f), 4.6f),
            ("archive", new Vector3(9.5f, 0.5f, 0f), 4.6f),
        };

        [MenuItem("Common House/Experiments/Build Isometric Outpost")]
        public static void Build()
        {
            Directory.CreateDirectory(Root);
            var model = CharacterAssets.Prepare(StandIn);
            var scene = EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var name in ResidentCapsules)
            {
                var capsule = transforms.Single(item => item.name == name);
                capsule.GetComponent<MeshRenderer>().enabled = false;
                var resident = (GameObject)PrefabUtility.InstantiatePrefab(model, capsule.parent);
                resident.name = "Resident at " + name;
                resident.transform.position = new Vector3(capsule.position.x, 0f, capsule.position.z);
                // MPFB characters face +Z; turn them towards the camera side of the room.
                resident.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
            foreach (var renderer in transforms.Where(item => item.name == "Ceiling" || CutawayWalls.Contains(item.name)).Select(item => item.GetComponent<MeshRenderer>()))
                renderer.enabled = false;
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save " + ScenePath);
        }

        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene(ScenePath);
            var camera = Camera.main ?? throw new InvalidOperationException("Missing camera");
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            // Looking north-west from 40 degrees above, as isometric RPGs frame a room.
            var rotation = Quaternion.Euler(40f, -45f, 0f);
            foreach (var (name, target, size) in Frames)
            {
                camera.orthographicSize = size;
                camera.transform.rotation = rotation;
                camera.transform.position = target - rotation * Vector3.forward * 60f;
                ExperimentCapture.Render(camera, 1600, 1200, Path.Combine(output, name + ".png"), FilterMode.Bilinear);
            }
        }
    }
}
