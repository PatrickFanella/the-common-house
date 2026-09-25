using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Tests
{
    public class ProjectSmokeTests
    {
        [Test]
        public void BootstrapSceneImportsWithCameraLightAndBuildEntry()
        {
            const string path = "Assets/CommonHouse/Scenes/Bootstrap.unity";
            Assert.That(EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == path));
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Assert.That(scene.IsValid());
            Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).Count(), Is.EqualTo(1));
            Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>()).Any(light => light.type == LightType.Directional));
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.Null, "The foundation uses the built-in renderer.");
        }

        [Test]
        public void RunnerUsesDisposableStateAndExplicitTestMode()
        {
            Assert.That(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), Is.EqualTo("test"));
            Assert.That(Application.persistentDataPath.Replace('\\', '/'), Does.Contain("/CommonHouseVerification/run-"));
            Assert.That(System.IO.Directory.Exists(Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH")), Is.True);
        }
    }
}
