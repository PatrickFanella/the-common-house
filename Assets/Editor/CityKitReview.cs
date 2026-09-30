using System.IO;
using CommonHouse.Look;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    public static class CityKitReview
    {
        public const string ScenePath = "Assets/CommonHouse/Scenes/CityKitReview.unity";

        [MenuItem("Common House/City Kit/Generate Assets And Review Scene")]
        public static void BuildInEditor()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            CityAssetKit.Generate();
        }

        public static void Generate()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("City kit street study").transform;
            foreach (var x in new[] { -12f, -4f, 4f, 12f })
            {
                Place(root, x == 4f ? "road-crossing" : "road-straight", new Vector3(x, 0f, 0f), 90f);
                Place(root, "pavement", new Vector3(x, 0f, 6f), 90f);
                Place(root, "pavement", new Vector3(x, 0f, -6f), -90f);
            }
            Place(root, "alley", new Vector3(4f, 0f, 12f));
            Building(root, -12f, 3, "brick-window", "storefront");
            Building(root, -2f, 4, "office-window", "doorway");
            Building(root, 10f, 2, "concrete-window", "shutter");
            Place(root, "fire-escape", new Vector3(-12f, 3.15f, 7.7f));
            foreach (var x in new[] { -14f, 0f, 14f }) Place(root, "streetlamp", new Vector3(x, 0.15f, 4.65f));
            Place(root, "bus-shelter", new Vector3(-5f, 0.15f, -5.7f), 180f);
            Place(root, "bench", new Vector3(7.5f, 0.15f, -5.7f), 180f);
            Place(root, "litter-bin", new Vector3(-2.6f, 0.15f, -5.7f));
            Place(root, "utility-cabinet", new Vector3(15f, 0.15f, 7f));
            Place(root, "notice-board", new Vector3(1f, 0.15f, -6.6f), 180f);
            foreach (var x in new[] { 2f, 6f }) Place(root, "bollard", new Vector3(x, 0.15f, 4.6f));
            foreach (var x in new[] { -15f, 15f }) Place(root, "utility-pole", new Vector3(x, 0.15f, 7f), 90f);
            for (var wire = 0; wire < 3; wire++)
            {
                var cable = OfficeAssetKit.Box(root, "Overhead street wire", new Vector3(0f, 5.93f, 7f), new Vector3(30f, 0.025f, 0.025f), CityAssetKit.Material("metal"));
                cable.transform.position += Vector3.forward * (wire - 1) * 0.7f;
            }
            var lighting = new GameObject("City review lighting").AddComponent<RetroLighting>();
            lighting.ambient = new Color(0.87f, 0.86f, 0.82f);
            lighting.fogColor = new Color(0.50f, 0.58f, 0.65f);
            lighting.fogStart = 24f; lighting.fogEnd = 90f; lighting.Apply();
            var volume = new GameObject("City review volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(RetroLook.ProfilePath);
            var camera = new GameObject("City review camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(24f, 10f, -23f);
            camera.transform.LookAt(new Vector3(0f, 3.5f, 7f));
            camera.nearClipPlane = 0.05f; camera.farClipPlane = 120f;
            camera.fieldOfView = 55f;
            var hd = camera.gameObject.AddComponent<HDAdditionalCameraData>();
            hd.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            hd.backgroundColorHDR = new Color(0.50f, 0.58f, 0.65f);
            hd.allowDynamicResolution = true;
            camera.gameObject.AddComponent<RetroScreen>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void Building(Transform parent, float x, int floors, string upper, string ground)
        {
            var root = OfficeAssetKit.Group(parent, "Provisional building " + x, new Vector3(x, 0.15f, 8f));
            OfficeAssetKit.Box(root, "Closed building mass", new Vector3(0f, floors * 1.5f, 4f), new Vector3(8f, floors * 3f, 8f), CityAssetKit.Material(upper == "brick-window" ? "brick" : "concrete"), true);
            for (var floor = 0; floor < floors; floor++)
            foreach (var bay in new[] { -2f, 2f })
                Place(root, floor == 0 ? ground : upper, new Vector3(bay, floor * 3f, -0.15f));
            for (var depth = 0; depth < 2; depth++)
            foreach (var bay in new[] { -2f, 2f })
                Place(root, "roof-cap", new Vector3(bay, floors * 3f, 2f + depth * 4f));
            // A visible side uses the same 4m bays rotated onto the building's east wall.
            for (var floor = 0; floor < floors; floor++)
            foreach (var z in new[] { 2f, 6f }) Place(root, upper, new Vector3(4.15f, floor * 3f, z), -90f);
        }

        private static void Place(Transform parent, string id, Vector3 position, float yaw = 0f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CityAssetKit.Root + "/Prefabs/" + id + ".prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
