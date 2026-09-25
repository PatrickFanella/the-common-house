using System;
using System.IO;
using System.Linq;
using CommonHouse.Outpost;
using CommonHouse.Player;
using CommonHouse.Interaction;
using CommonHouse.Runtime;
using CommonHouse.Radio;
using CommonHouse.Notebook;
using CommonHouse.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonHouse.Editor
{
    /// <summary>Creates the temporary-lighting, primitive-only Case 01 outpost scene.</summary>
    public static class OutpostSceneGenerator
    {
        private const float WallHeight = 3f;
        private const float WallThickness = 0.25f;
        private const float DoorHalfWidth = 1.3f;

        [MenuItem("Common House/Generate Outpost Blockout")]
        public static void Generate()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutpostLayout.ScenePath) ?? "Assets");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("OutpostBlockout");
            var layout = root.AddComponent<OutpostLayout>();

            CreateRoom(root.transform, OutpostLayout.RadioRoom, -9.5f, 9f);
            CreateRoom(root.transform, OutpostLayout.Kitchen, 0f, 10f);
            CreateRoom(root.transform, OutpostLayout.ArchiveWorkshop, 9.5f, 9f);
            CreateOuterWalls(root.transform);
            CreateDoorwayWall(root.transform, -5f);
            CreateDoorwayWall(root.transform, 5f);

            AddAnchor(root.transform, OutpostLayout.RadioRoom, "radio-console", new Vector3(-11.5f, 0.5f, -2.5f));
            AddAnchor(root.transform, OutpostLayout.RadioRoom, "radio-receiver", new Vector3(-8f, 0.5f, -2.5f));
            AddAnchor(root.transform, OutpostLayout.Kitchen, "kitchen-table", new Vector3(0f, 0.5f, 1.5f));
            AddAnchor(root.transform, OutpostLayout.ArchiveWorkshop, "archive-roster", new Vector3(8f, 0.5f, -2.5f));
            AddAnchor(root.transform, OutpostLayout.ArchiveWorkshop, "workshop-bench", new Vector3(11.5f, 0.5f, 2.5f));
            AddAnchor(root.transform, OutpostLayout.ArchiveWorkshop, "routing-switch", new Vector3(7.5f, 0.5f, 2.5f));
            CreateSampleInspection(root.transform);

            CreateLighting(root.transform);
            CreateCamera(root.transform);
            if (!EditorSceneManager.SaveScene(scene, OutpostLayout.ScenePath))
                throw new InvalidOperationException("Could not save the outpost blockout scene.");
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(entry => entry.path != OutpostLayout.ScenePath)
                .Append(new EditorBuildSettingsScene(OutpostLayout.ScenePath, true))
                .ToArray();
            AssetDatabase.SaveAssets();
        }

        private static void CreateRoom(Transform root, string roomId, float centerX, float width)
        {
            var room = new GameObject(roomId).transform;
            room.SetParent(root, false);
            CreatePrimitive(room, "Floor", PrimitiveType.Cube, new Vector3(centerX, -0.15f, 0f), new Vector3(width, 0.3f, 8f), true);
            CreatePrimitive(room, "Ceiling", PrimitiveType.Cube, new Vector3(centerX, WallHeight + 0.15f, 0f), new Vector3(width, 0.3f, 8f), false);
        }

        private static void CreateOuterWalls(Transform root)
        {
            CreatePrimitive(root, "NorthWall", PrimitiveType.Cube, new Vector3(0f, WallHeight / 2f, 4f), new Vector3(28f, WallHeight, WallThickness), true);
            CreatePrimitive(root, "SouthWall", PrimitiveType.Cube, new Vector3(0f, WallHeight / 2f, -4f), new Vector3(28f, WallHeight, WallThickness), true);
            CreatePrimitive(root, "WestWall", PrimitiveType.Cube, new Vector3(-14f, WallHeight / 2f, 0f), new Vector3(WallThickness, WallHeight, 8f), true);
            CreatePrimitive(root, "EastWall", PrimitiveType.Cube, new Vector3(14f, WallHeight / 2f, 0f), new Vector3(WallThickness, WallHeight, 8f), true);
        }

        private static void CreateDoorwayWall(Transform root, float x)
        {
            CreatePrimitive(root, "Partition-" + x + "-north", PrimitiveType.Cube, new Vector3(x, WallHeight / 2f, 2.65f), new Vector3(WallThickness, WallHeight, 2.7f), true);
            CreatePrimitive(root, "Partition-" + x + "-south", PrimitiveType.Cube, new Vector3(x, WallHeight / 2f, -2.65f), new Vector3(WallThickness, WallHeight, 2.7f), true);
            CreatePrimitive(root, "DoorLintel-" + x, PrimitiveType.Cube, new Vector3(x, 2.75f, 0f), new Vector3(WallThickness, 0.5f, DoorHalfWidth * 2f), true);
        }

        private static void AddAnchor(Transform root, string roomId, string anchorId, Vector3 position)
        {
            var room = root.Find(roomId);
            if (room == null) throw new InvalidOperationException("Missing generated room " + roomId);
            var marker = CreatePrimitive(room, anchorId, PrimitiveType.Cylinder, position, new Vector3(0.45f, 1f, 0.45f), false);
            marker.AddComponent<OutpostAnchor>().Configure(anchorId, roomId);
        }

        private static void CreateSampleInspection(Transform root)
        {
            var notice = CreatePrimitive(root, "ShiftNotice", PrimitiveType.Cube, new Vector3(-10f, 1.4f, 2.1f),
                new Vector3(1.2f, 0.8f, 0.08f), true);
            notice.AddComponent<InspectionTarget>().Configure(
                "sample-shift-notice",
                "Shift notice",
                "Sample inspection text. Keep the radio log readable for whoever takes the next shift.",
                "sample-shift-notice");
        }

        private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, bool collider)
        {
            var item = GameObject.CreatePrimitive(type);
            item.name = name;
            item.transform.SetParent(parent);
            item.transform.position = position;
            item.transform.localScale = scale;
            if (!collider)
            {
                var itemCollider = item.GetComponent<Collider>();
                if (itemCollider != null) UnityEngine.Object.DestroyImmediate(itemCollider);
            }
            return item;
        }

        private static void CreateLighting(Transform root)
        {
            var light = new GameObject("Temporary Directional Light").AddComponent<Light>();
            light.transform.SetParent(root);
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.25f);
        }

        private static void CreateCamera(Transform root)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(root);
            player.transform.position = new Vector3(-10f, 0.05f, 0f);
            var capsule = player.AddComponent<CharacterController>();
            capsule.height = 1.8f;
            capsule.radius = 0.3f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            var movement = player.AddComponent<FirstPersonMovement>();
            var modalInput = player.AddComponent<ModalInputCoordinator>();
            var camera = new GameObject("PlayerCamera").AddComponent<Camera>();
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camera.tag = "MainCamera";
            movement.SetViewTransform(camera.transform);
            player.AddComponent<InspectionController>().Configure(camera, movement, modalInput);
            player.AddComponent<InspectionOverlay>();
            var session = player.AddComponent<CaseSession>();
            player.AddComponent<TransmissionLog>().Configure(session, modalInput);
            player.AddComponent<TransmissionLogOverlay>();
            player.AddComponent<NotebookController>().Configure(session, modalInput);
            player.AddComponent<NotebookOverlay>();
            player.AddComponent<DialoguePresenter>().Configure(session, modalInput);
            player.AddComponent<DialogueOverlay>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
        }
    }
}
