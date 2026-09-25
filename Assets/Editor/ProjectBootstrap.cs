using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CommonHouse.Editor
{
    public static class ProjectBootstrap
    {
        public const string ScenePath = "Assets/CommonHouse/Scenes/Bootstrap.unity";

        // Explicit one-time setup only. Ordinary import/test commands do not run this.
        public static void Configure()
        {
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("Bootstrap scene exists; preserving current project settings.");
            PlayerSettings.companyName = "CommonHouseDevelopment";
            PlayerSettings.productName = "The Common House POC";
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode3D;
            Directory.CreateDirectory("Assets/CommonHouse/Scenes");
            AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save the generated bootstrap scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }
    }
}
