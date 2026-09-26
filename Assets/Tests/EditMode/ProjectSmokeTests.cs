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
        public void BootstrapSceneImportsWithCameraLightBuildEntryAndHdrp()
        {
            const string path = "Assets/CommonHouse/Scenes/Bootstrap.unity";
            Assert.That(EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == path));
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Assert.That(scene.IsValid());
            Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).Count(), Is.EqualTo(1));
            Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>()).Any(light => light.type == LightType.Directional));
            // D-20: HDRP on Vulkan with linear colour.
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(GraphicsSettings.defaultRenderPipeline), Is.EqualTo("Assets/CommonHouse/Rendering/HDRenderPipelineAsset.asset"));
            Assert.That(GraphicsSettings.defaultRenderPipeline.GetType().FullName, Is.EqualTo("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset"));
            Assert.That(PlayerSettings.colorSpace, Is.EqualTo(ColorSpace.Linear));
            Assert.That(PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneLinux64), Is.EqualTo(new[] { GraphicsDeviceType.Vulkan }));
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
