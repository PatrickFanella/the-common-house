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
    public class MotionDetectedSourceSetTests
    {
        private const string Root="Assets/CommonHouse/Art/MotionDetected";
        private const string Review="Assets/CommonHouse/Scenes/MotionDetectedSourceReview.unity";

        [Test]
        public void SixSourcePropsHaveSavedMaterialsAndNoEpisodeBehaviour()
        {
            var paths=Directory.GetFiles(Root+"/Prefabs","*.prefab"); Assert.That(paths.Length,Is.EqualTo(6));
            foreach(var path in paths)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path); Assert.That(prefab,Is.Not.Null,path);
                Assert.That(prefab.transform.localPosition,Is.EqualTo(Vector3.zero)); Assert.That(prefab.transform.localScale,Is.EqualTo(Vector3.one));
                Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(),Is.Empty);
                foreach(var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
                {
                    Assert.That(renderer.GetComponent<MeshFilter>().sharedMesh,Is.Not.Null,path);
                    Assert.That(renderer.sharedMaterial.shader.name,Is.EqualTo("CommonHouse/Retro"));
                    Assert.That(AssetDatabase.GetAssetPath(renderer.sharedMaterial),Does.StartWith(Root));
                }
            }
            foreach(var path in Directory.GetFiles(Root+"/Textures","*.png"))
            {
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(texture.width,Is.EqualTo(64)); Assert.That(texture.height,Is.EqualTo(64));
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point)); Assert.That(importer.mipmapEnabled,Is.False);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
        }

        [Test]
        public void DoorVariantsShareOneHingeAndRepairedRailHasPhysicalFasteners()
        {
            var closed=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/basement-door-closed.prefab");
            var open=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/basement-door-open.prefab");
            var a=closed.transform.Find("Door hinge"); var b=open.transform.Find("Door hinge");
            Assert.That(a.localPosition,Is.EqualTo(new Vector3(-.5f,0,0))); Assert.That(b.localPosition,Is.EqualTo(a.localPosition));
            Assert.That(Quaternion.Angle(a.localRotation,b.localRotation),Is.EqualTo(78f).Within(.01f));
            Assert.That(a.Find("Door leaf").localPosition,Is.EqualTo(b.Find("Door leaf").localPosition));
            Assert.That(open.transform.Find("Interior coverage stop").GetComponent<Collider>(),Is.Not.Null);
            var rail=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/repaired-handrail.prefab");
            Assert.That(rail.transform.Find("Repair sleeve"),Is.Not.Null);
            Assert.That(rail.GetComponentsInChildren<Transform>().Count(t=>t.name=="New screw head"),Is.EqualTo(2));
        }

        [Test]
        public void SixStairTreadsAndLandingHaveTheirDocumentedCollisionHeights()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/basement-steps.prefab"));
            Physics.SyncTransforms();
            for(var i=0;i<6;i++)
            {
                Assert.That(Physics.Raycast(new Vector3(0,1,.15f+i*.30f),Vector3.down,out var hit,4),Is.True);
                Assert.That(hit.point.y,Is.EqualTo(-(i+1)*.20f).Within(.002f));
            }
            Assert.That(Physics.Raycast(new Vector3(0,1,2.40f),Vector3.down,out var landing,4),Is.True);
            Assert.That(landing.point.y,Is.EqualTo(-1.20f).Within(.002f));
        }

        [Test]
        public void FixedSourceViewIncludesExteriorCluesAndOccludesBeyondTheThreshold()
        {
            EditorSceneManager.OpenScene(Review,OpenSceneMode.Single);
            Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Review),Is.False);
            var camera=GameObject.Find("NetworkCamera").GetComponent<Camera>();
            Assert.That(camera.transform.position,Is.EqualTo(new Vector3(-1.15f,1.80f,-.85f)));
            var lens=GameObject.Find("Original source camera housing").transform.Find("Lens");
            Assert.That(Vector3.Distance(lens.position,camera.transform.position),Is.LessThan(.001f),"Physical source lens must match recording viewpoint.");
            AssertVisible(camera,new Vector3(.36f,-1.14f,2.38f),"Leaf drain");
            AssertVisible(camera,new Vector3(-.69f,.20f,.77f),"Rail repair");
            AssertVisible(camera,new Vector3(0,-1.18f,3f),"Threshold");
            var bin=GameObject.Find("scraping-bin").transform.Find("Lid pivot/Loose lid").GetComponent<Renderer>().bounds;
            Assert.That(new[]{bin.min,bin.max,bin.center}.Any(p=>InFrame(camera,p)),Is.True,"Bin lid must reach the source frame.");
            var wall=Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Single(c=>c.name=="Boundary wall" && c.transform.position.x>0);
            Assert.That(bin.max.x,Is.GreaterThan(wall.bounds.min.x),"Loose lid must physically reach the wall it can scrape.");
            foreach(var isOpen in new[]{false,true})
            {
                SetDoor(isOpen); Physics.SyncTransforms();
                foreach(var x in new[]{-.30f,.30f}) foreach(var y in new[]{-1f,0f,.65f})
                {
                    var hidden=new Vector3(x,y,3.70f); var delta=hidden-camera.transform.position;
                    Assert.That(Physics.Raycast(camera.transform.position,delta.normalized,out var hit,delta.magnitude),Is.True,"Uncovered interior probe");
                    Assert.That(hit.point.z,Is.LessThan(3.30f),"Coverage must stop at the exterior source boundary.");
                }
            }
            var objects=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Assert.That(objects.Any(m=>m!=null && m.GetType().Namespace!="CommonHouse.Look" && !m.GetType().Namespace.StartsWith("UnityEngine")),Is.False);
        }

        [UnityTest]
        public IEnumerator RenderFixedSourceDoorVariantsAndArtistDetailViews()
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(GraphicsDeviceType.Null),"Run with --graphics.");
            EditorSceneManager.OpenScene(Review,OpenSceneMode.Single);
            var source=GameObject.Find("NetworkCamera").GetComponent<Camera>();
            var origin=source.transform.position; var rotation=source.transform.rotation;
            foreach(var open in new[]{false,true})
            {
                SetDoor(open); yield return null; yield return null;
                Capture(source,open?"md-source-door-open":"md-source-door-closed");
                Assert.That(source.transform.position,Is.EqualTo(origin)); Assert.That(source.transform.rotation,Is.EqualTo(rotation));
            }
            SetDoor(false);
            var artist=GameObject.Find("ArtistReviewCamera").GetComponent<Camera>();
            yield return null; yield return null; Capture(artist,"md-artist-set");
            foreach(var view in new[]{("rail",new Vector3(-1.20f,.55f,.40f),new Vector3(-.69f,.20f,.77f)),
                ("drain",new Vector3(.50f,-.60f,1.70f),new Vector3(.36f,-1.13f,2.38f)),
                ("bin",new Vector3(.20f,1.20f,-.40f),new Vector3(1.40f,.40f,.40f))})
            {
                artist.transform.position=view.Item2; artist.transform.LookAt(view.Item3); artist.fieldOfView=55;
                yield return null; yield return null; Capture(artist,"md-artist-"+view.Item1);
            }
        }

        private static void SetDoor(bool open)
        {
            var scene=EditorSceneManager.GetActiveScene();
            var set=scene.GetRootGameObjects().Single(g=>g.name=="Source exterior only").transform;
            set.Find("basement-door-closed").gameObject.SetActive(!open);
            set.Find("basement-door-open").gameObject.SetActive(open);
        }

        private static bool InFrame(Camera camera,Vector3 point)
        { var p=camera.WorldToViewportPoint(point); return p.z>0 && p.x>0 && p.x<1 && p.y>0 && p.y<1; }
        private static void AssertVisible(Camera camera,Vector3 point,string clue) => Assert.That(InFrame(camera,point),Is.True,clue+" outside source frame");

        private static void Capture(Camera camera,string id)
        {
            var output=Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH"); Assert.That(output,Is.Not.Null.And.Not.Empty);
            var target=new RenderTexture(960,720,24); var image=new Texture2D(960,720,TextureFormat.RGB24,false); var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=target; for(var i=0;i<3;i++) camera.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,960,720),0,0); image.Apply();
                Assert.That(image.GetPixels32().Select(c=>(c.r/16,c.g/16,c.b/16)).Distinct().Count(),Is.GreaterThan(12));
                File.WriteAllBytes(Path.Combine(output,id+".png"),image.EncodeToPNG());
            }
            finally { camera.targetTexture=null; RenderTexture.active=previous; Object.DestroyImmediate(target); Object.DestroyImmediate(image); }
        }
    }
}
