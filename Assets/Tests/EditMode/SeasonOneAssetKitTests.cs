using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CommonHouse.Tests
{
    public class SeasonOneAssetKitTests
    {
        private const string Root = "Assets/CommonHouse/Art/SeasonOne";
        private const string Review = "Assets/CommonHouse/Scenes/SeasonOneAssetReview.unity";

        [Test]
        public void PropsHavePortableGeometryMaterialsAndSurfaceLevelOrigins()
        {
            var paths = Directory.GetFiles(Root + "/Prefabs", "*.prefab");
            Assert.That(paths.Length, Is.EqualTo(16));
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(prefab.transform.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(), Is.Empty, "No permissions or story behaviour baked into props.");
                var renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                Assert.That(renderers, Is.Not.Empty);
                foreach (var renderer in renderers)
                {
                    Assert.That(renderer.GetComponent<MeshFilter>().sharedMesh, Is.Not.Null, path);
                    Assert.That(renderer.sharedMaterial.shader.name, Is.EqualTo("CommonHouse/Retro"));
                    Assert.That(AssetDatabase.GetAssetPath(renderer.sharedMaterial), Does.StartWith(Root));
                }
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
                Assert.That(bounds.min.y, Is.InRange(-.002f,.02f), path + " origin must sit on a surface");
                Assert.That(bounds.size.magnitude, Is.InRange(.10f,1.3f), path + " metre scale");
            }
        }

        [Test]
        public void PrivacyVariantsChangePhysicalGeometryAndGenericLabelsFaceForward()
        {
            var covered = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/covered-microphone.prefab");
            var uncovered = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/diagnostic-microphone.prefab");
            Assert.That(covered.transform.Find("Opaque cloth cover"), Is.Not.Null);
            Assert.That(uncovered.transform.Find("Opaque cloth cover"), Is.Null);
            var closed = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/folder-closed.prefab");
            var open = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/folder-open.prefab");
            Assert.That(closed.transform.Find("Blank insert"), Is.Null, "Closed folder must hide its insert.");
            Assert.That(open.transform.Find("Blank insert"), Is.Not.Null);
            Assert.That(open.transform.Find("Opened cover").localPosition.x, Is.LessThan(-.25f));
            var label = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Meshes/label-face.asset");
            Assert.That(label.normals.All(n => n.z < 0), Is.True);
            Assert.That(label.uv[3].x, Is.GreaterThan(label.uv[0].x), "Text must not be mirrored.");
        }

        [Test]
        public void TextureImportsAndSeparateReviewRemainWithinTheAssetBoundary()
        {
            foreach (var path in Directory.GetFiles(Root + "/Textures", "*.png"))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width, Is.EqualTo(64)); Assert.That(texture.height, Is.EqualTo(64));
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(importer.mipmapEnabled, Is.False);
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
            var scene = EditorSceneManager.OpenScene(Review, OpenSceneMode.Single);
            Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Review), Is.False);
            var objects = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>()).ToArray();
            Assert.That(objects.Count(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)), Is.EqualTo(16));
            Assert.That(objects.SelectMany(t => t.GetComponents<MonoBehaviour>()).Any(m => m != null && m.GetType().Namespace != "CommonHouse.Look" && !m.GetType().Namespace.StartsWith("UnityEngine")), Is.False);
        }

        [UnityTest]
        public IEnumerator RenderAllPropsAndReadableOperatingLabels()
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null), "Run with --graphics.");
            var scene = EditorSceneManager.OpenScene(Review, OpenSceneMode.Single);
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).Single();
            yield return null; yield return null;
            Capture(camera, "season-props-overview");
            for(var row=0;row<4;row++)
            {
                camera.transform.position = new Vector3(1.8f,3.6f,row*1.2f-3.5f);
                camera.transform.LookAt(new Vector3(1.8f,.85f,row*1.2f));
                yield return null; yield return null;
                Capture(camera,"season-props-row-"+row);
            }
            foreach(var view in new[]{("switch",new Vector3(1.2f,.75f,0)),("cards",new Vector3(1.2f,.82f,2.4f)),("camera",new Vector3(3.6f,.85f,3.6f))})
            {
                camera.transform.position=view.Item2+new Vector3(.2f,.38f,-.65f); camera.transform.LookAt(view.Item2);
                yield return null; yield return null; Capture(camera,"season-prop-"+view.Item1);
            }
        }

        private static void Capture(Camera camera, string id)
        {
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            Assert.That(output, Is.Not.Null.And.Not.Empty);
            var target = new RenderTexture(1200,800,24); var image = new Texture2D(1200,800,TextureFormat.RGB24,false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture=target; for(var i=0;i<3;i++) camera.Render();
                RenderTexture.active=target; image.ReadPixels(new Rect(0,0,1200,800),0,0); image.Apply();
                Assert.That(image.GetPixels32().Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count(), Is.GreaterThan(12),"Broken/uniform capture");
                File.WriteAllBytes(Path.Combine(output,id+".png"),image.EncodeToPNG());
            }
            finally { camera.targetTexture=null; RenderTexture.active=previous; Object.DestroyImmediate(target); Object.DestroyImmediate(image); }
        }
    }
}
