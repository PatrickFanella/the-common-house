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
    public class CityAssetKitTests
    {
        private const string Root = "Assets/CommonHouse/Art/CityKit";
        private const string Review = "Assets/CommonHouse/Scenes/CityKitReview.unity";

        [Test]
        public void TwentyPrefabsHavePersistentMaterialsMeshesAndPracticalCollisions()
        {
            var paths = Directory.GetFiles(Root + "/Prefabs", "*.prefab");
            Assert.That(paths.Length, Is.EqualTo(20));
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab, Is.Not.Null, path);
                Assert.That(prefab.transform.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(), Is.Empty, path);
                Assert.That(prefab.GetComponentsInChildren<Collider>(), Is.Not.Empty, path);
                foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
                {
                    Assert.That(renderer.GetComponent<MeshFilter>().sharedMesh, Is.Not.Null, path);
                    foreach (var material in renderer.sharedMaterials)
                    {
                        Assert.That(material, Is.Not.Null, path);
                        Assert.That(material.shader.name, Is.EqualTo("CommonHouse/Retro"));
                        Assert.That(AssetDatabase.GetAssetPath(material), Does.StartWith(Root));
                        if (material.name.EndsWith("sign") || material.name.EndsWith("notices"))
                        {
                            var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                            Assert.That(AssetDatabase.GetAssetPath(mesh), Is.EqualTo(Root + "/Meshes/sign-face.asset"));
                            var vertices = mesh.vertices; var uv = mesh.uv;
                            var left = Enumerable.Range(0, vertices.Length).Where(i => vertices[i].x < 0).Average(i => uv[i].x);
                            var right = Enumerable.Range(0, vertices.Length).Where(i => vertices[i].x > 0).Average(i => uv[i].x);
                            Assert.That(right, Is.GreaterThan(left), "Street-facing sign text must not be mirrored.");
                            Assert.That(mesh.normals.All(n => n.z < 0), Is.True);
                        }
                    }
                }
            }
        }

        [Test]
        public void StreetAndFacadeModulesMeetAtTheirDocumentedGridWithoutFloorGaps()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var road = Instantiate("road-straight", Vector3.zero);
            var adjacent = Instantiate("road-straight", new Vector3(8f, 0f, 0f));
            Physics.SyncTransforms();
            var a = road.GetComponentInChildren<Collider>().bounds;
            var b = adjacent.GetComponentInChildren<Collider>().bounds;
            Assert.That(a.size, Is.EqualTo(new Vector3(8f, 0.2f, 8f)));
            Assert.That(a.max.x, Is.EqualTo(b.min.x).Within(0.001f));
            Assert.That(a.max.y, Is.EqualTo(0f).Within(0.001f));
            Physics.SyncTransforms();
            foreach (var x in new[] { 3.99f, 4f, 4.01f })
                Assert.That(Physics.Raycast(new Vector3(x, 1f, 0f), Vector3.down, out var hit, 2f) && Mathf.Abs(hit.point.y) < 0.001f, Is.True, "Surface seam at " + x);
            var facadeObject = Instantiate("brick-window", Vector3.zero);
            var nextObject = Instantiate("concrete-window", new Vector3(4f, 3f, 0f));
            Physics.SyncTransforms();
            var facade = facadeObject.GetComponentsInChildren<Collider>().Single().bounds;
            var next = nextObject.GetComponentsInChildren<Collider>().Single().bounds;
            Assert.That(facade.size.x, Is.EqualTo(4f).Within(0.001f));
            Assert.That(facade.max.x, Is.EqualTo(next.min.x).Within(0.001f));
            Assert.That(facade.max.y, Is.EqualTo(next.min.y).Within(0.001f));
        }

        [Test]
        public void OriginalTexturesStaySmallPointFilteredAndUncompressed()
        {
            var paths = Directory.GetFiles(Root + "/Textures", "*.png");
            Assert.That(paths.Length, Is.GreaterThanOrEqualTo(12));
            foreach (var path in paths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width, Is.EqualTo(64)); Assert.That(texture.height, Is.EqualTo(64));
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(importer.mipmapEnabled, Is.False);
                Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
        }

        [Test]
        public void ReviewSceneUsesPrefabInstancesAndDoesNotJoinThePlayableBuild()
        {
            var scene = EditorSceneManager.OpenScene(Review, OpenSceneMode.Single);
            Assert.That(scene.IsValid(), Is.True);
            Assert.That(EditorBuildSettings.scenes.Any(s => s.path == Review), Is.False);
            var objects = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>()).ToArray();
            Assert.That(objects.Count(t => PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)), Is.GreaterThan(50));
            Assert.That(objects.SelectMany(t => t.GetComponents<MonoBehaviour>()).Any(m => m != null && m.GetType().Namespace != "CommonHouse.Look" && !m.GetType().Namespace.StartsWith("UnityEngine")), Is.False,
                "Review contains no case, player, or flashback controllers.");
        }

        [UnityTest]
        public IEnumerator RenderCityKitStreetFacadeAndFurnitureViews()
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null), "Run this fixture with --graphics.");
            var scene = EditorSceneManager.OpenScene(Review, OpenSceneMode.Single);
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).Single();
            foreach (var view in new[]
            {
                ("city-street", new Vector3(24f, 10f, -23f), new Vector3(0f, 3.5f, 7f)),
                ("city-storefront", new Vector3(-7f, 1.75f, -1.5f), new Vector3(-11.7f, 2.8f, 8f)),
                ("city-street-furniture", new Vector3(3f, 1.75f, 0f), new Vector3(-3f, 1.25f, -6f)),
                ("city-service-alley", new Vector3(4f, 1.75f, 5f), new Vector3(4f, 2.6f, 14f)),
            })
            {
                camera.transform.position = view.Item2; camera.transform.LookAt(view.Item3);
                yield return null; yield return null;
                Capture(camera, view.Item1);
            }
        }

        private static GameObject Instantiate(string id, Vector3 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/" + id + ".prefab"));
            instance.transform.position = position;
            return instance;
        }

        private static void Capture(Camera camera, string id)
        {
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            Assert.That(output, Is.Not.Null.And.Not.Empty);
            var target = new RenderTexture(1000, 700, 24);
            var image = new Texture2D(1000, 700, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                for (var i = 0; i < 3; i++) camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1000, 700), 0, 0); image.Apply();
                var colors = image.GetPixels32();
                Assert.That(colors.Select(c => (c.r / 16, c.g / 16, c.b / 16)).Distinct().Count(), Is.GreaterThan(12), "Broken or uniform render");
                File.WriteAllBytes(Path.Combine(output, id + ".png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null; RenderTexture.active = previous;
                Object.DestroyImmediate(target); Object.DestroyImmediate(image);
            }
        }
    }
}
