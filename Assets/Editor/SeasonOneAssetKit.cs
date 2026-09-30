using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonHouse.Look;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>Original small props. Metres, surface-level origin, front -Z. No story state.</summary>
    public static class SeasonOneAssetKit
    {
        public const string Root = "Assets/CommonHouse/Art/SeasonOne";
        public const string ScenePath = "Assets/CommonHouse/Scenes/SeasonOneAssetReview.unity";
        public static readonly string[] Ids = { "label-printer", "off-air-switch", "diagnostic-microphone", "covered-microphone",
            "loose-patch-lead", "folder-closed", "folder-open", "contact-ledger", "roster-board",
            "restricted-input-card", "chess-board", "biscuit-tin", "saved-meal", "folding-chair", "station-card", "exterior-camera" };
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        private static Material Surface(string id)
        {
            if (Materials.TryGetValue(id, out var cached) && cached != null) return cached;
            Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Textures");
            var path = Root + "/Textures/" + id + ".png";
            var texture = new Texture2D(64, 64, TextureFormat.RGB24, false);
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++) texture.SetPixel(x, y, Pixel(id, x, y));
            texture.Apply(); var bytes = texture.EncodeToPNG(); UnityEngine.Object.DestroyImmediate(texture);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
            { File.WriteAllBytes(path, bytes); AssetDatabase.ImportAsset(path); }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.SaveAndReimport();
            var materialPath = Root + "/Materials/" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            { material = new Material(Shader.Find("CommonHouse/Retro")) { name = "Season one " + id }; AssetDatabase.CreateAsset(material, materialPath); }
            material.SetColor("_Color", Color.white); material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_Cutoff", 0f); material.SetFloat("_WorldUV", 0f);
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            EditorUtility.SetDirty(material); return Materials[id] = material;
        }

        private static Color Pixel(string id, int x, int y)
        {
            var noise = ((x * 31 + y * 17 + x * y) % 13 - 6) / 400f;
            Color color;
            switch (id)
            {
                case "steel": color = new Color(.21f, .25f, .28f); break;
                case "shell": color = new Color(.65f, .66f, .59f); break;
                case "teal": color = new Color(.12f, .64f, .58f); break;
                case "pink": color = new Color(.79f, .23f, .40f); break;
                case "ochre": color = new Color(.86f, .61f, .22f); break;
                case "paper": color = new Color(.88f, .84f, .71f); break;
                case "folder": color = new Color(.51f, .36f, .23f); break;
                case "cloth": color = (x + y) % 4 == 0 ? new Color(.42f, .29f, .35f) : new Color(.62f, .42f, .47f); break;
                case "grille": color = x % 4 < 2 && y % 4 < 2 ? new Color(.10f, .12f, .14f) : new Color(.39f, .44f, .45f); break;
                case "biscuit": color = x % 12 == 4 && y % 12 == 4 ? new Color(.47f, .29f, .12f) : new Color(.74f, .51f, .26f); break;
                case "ledger": color = y % 12 < 1 || x == 19 || x == 45 ? new Color(.43f, .52f, .51f) : new Color(.88f, .84f, .71f); break;
                default:
                    // Generic operating labels only. Account text belongs in a future authorized reader.
                    var word = id == "label-off-air" ? "OFF AIR" : id == "label-local" ? "LOCAL" : "ASK";
                    var index = (x - 4) / 8;
                    if (x >= 4 && index < word.Length && OfficeAssetKit.LetterPixel(word[index], (x - 4) % 8, 35 - y)) return OfficeAssetKit.Bone;
                    color = y < 4 || x < 3 ? OfficeAssetKit.Teal : OfficeAssetKit.Ink; break;
            }
            return color + new Color(noise, noise, noise, 0f);
        }

        private static GameObject Box(Transform root, string name, Vector3 p, Vector3 size, string material)
            => OfficeAssetKit.Box(root, name, p, size, Surface(material));

        private static Mesh cylinder;
        private static Mesh CylinderMesh()
        {
            if (cylinder != null) return cylinder;
            Directory.CreateDirectory(Root + "/Meshes");
            var path = Root + "/Meshes/twelve-sided-cylinder.asset";
            cylinder = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (cylinder == null) { cylinder = new Mesh { name = "Original twelve sided cylinder" }; AssetDatabase.CreateAsset(cylinder, path); }
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var triangles = new List<int>();
            void Face(Vector3 a, Vector3 b, Vector3 c, Vector2 ta, Vector2 tb, Vector2 tc)
            { var start = vertices.Count; vertices.AddRange(new[] { a, b, c }); uv.AddRange(new[] { ta, tb, tc }); triangles.AddRange(new[] { start, start + 1, start + 2 }); }
            for (var i = 0; i < 12; i++)
            {
                var a = i * Mathf.PI / 6; var b = (i + 1) * Mathf.PI / 6;
                var lowA = new Vector3(Mathf.Cos(a) * .5f, -.5f, Mathf.Sin(a) * .5f);
                var lowB = new Vector3(Mathf.Cos(b) * .5f, -.5f, Mathf.Sin(b) * .5f);
                var highA = lowA + Vector3.up; var highB = lowB + Vector3.up;
                Face(lowA, highA, highB, Vector2.zero, Vector2.up, Vector2.one);
                Face(lowA, highB, lowB, Vector2.zero, Vector2.one, Vector2.right);
                Face(Vector3.up * .5f, highB, highA, new Vector2(.5f, .5f), new Vector2(highB.x + .5f, highB.z + .5f), new Vector2(highA.x + .5f, highA.z + .5f));
                Face(Vector3.down * .5f, lowA, lowB, new Vector2(.5f, .5f), new Vector2(lowA.x + .5f, lowA.z + .5f), new Vector2(lowB.x + .5f, lowB.z + .5f));
            }
            cylinder.Clear(); cylinder.SetVertices(vertices); cylinder.SetUVs(0, uv); cylinder.SetTriangles(triangles, 0);
            cylinder.RecalculateNormals(); cylinder.RecalculateBounds(); EditorUtility.SetDirty(cylinder); return cylinder;
        }

        private static GameObject Round(Transform root, string name, Vector3 p, Vector3 size, string material)
        {
            var item = new GameObject(name); item.transform.SetParent(root, false); item.transform.localPosition = p; item.transform.localScale = size;
            item.AddComponent<MeshFilter>().sharedMesh = CylinderMesh(); item.AddComponent<MeshRenderer>().sharedMaterial = Surface(material); return item;
        }

        private static void Rod(Transform root, string name, Vector3 a, Vector3 b, float width, string material)
        {
            var rod = Round(root, name, (a + b) / 2f, new Vector3(width, Vector3.Distance(a, b), width), material);
            rod.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        private static void Label(Transform root, string id, Vector3 p, Vector2 size, bool flat = false)
        {
            // The front face's UV runs left to right. A dedicated plane avoids mirrored cube labels.
            Directory.CreateDirectory(Root + "/Meshes"); var path = Root + "/Meshes/label-face.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = "Original label front" };
                mesh.vertices = new[] { new Vector3(-.5f,-.5f,0), new Vector3(-.5f,.5f,0), new Vector3(.5f,.5f,0), new Vector3(.5f,-.5f,0) };
                mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }; mesh.triangles = new[] { 0,1,2,0,2,3 };
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path);
            }
            var item = new GameObject(id); item.transform.SetParent(root, false); item.transform.localPosition = p;
            item.transform.localScale = new Vector3(size.x, size.y, 1); if (flat) item.transform.localRotation = Quaternion.Euler(90, 0, 0);
            item.AddComponent<MeshFilter>().sharedMesh = mesh; item.AddComponent<MeshRenderer>().sharedMaterial = Surface(id);
        }

        private static void Microphone(Transform root, bool covered)
        {
            Round(root, "Weighted desk base", new Vector3(0,.025f,0), new Vector3(.19f,.05f,.19f), "steel");
            Rod(root, "Short stand", new Vector3(0,.05f,0), new Vector3(0,.21f,0), .025f, "steel");
            Box(root, "Microphone body", new Vector3(0,.29f,0), new Vector3(.09f,.16f,.08f), "shell");
            Box(root, "Grille", new Vector3(0,.30f,-.046f), new Vector3(.075f,.12f,.012f), "grille");
            Rod(root, "Local recorder lead", new Vector3(.045f,.23f,.02f), new Vector3(.13f,.01f,.12f), .012f, "steel");
            if (covered)
            {
                Box(root, "Opaque cloth cover", new Vector3(0,.30f,-.01f), new Vector3(.13f,.19f,.12f), "cloth");
                Box(root, "Cover tape", new Vector3(.01f,.27f,-.074f), new Vector3(.11f,.025f,.008f), "ochre");
            }
        }

        private static void Folder(Transform root, bool open)
        {
            Box(root, "Folder back", new Vector3(0,.009f,0), new Vector3(.27f,.018f,.34f), "folder");
            Box(root, "Folder tab", new Vector3(-.08f,.01f,.18f), new Vector3(.09f,.018f,.04f), "folder");
            if (open)
            {
                Box(root, "Blank insert", new Vector3(0,.021f,0), new Vector3(.245f,.005f,.31f), "paper");
                Box(root, "Opened cover", new Vector3(-.275f,.009f,0), new Vector3(.27f,.018f,.34f), "folder");
                Box(root, "Hinge", new Vector3(-.1375f,.01f,0), new Vector3(.015f,.018f,.34f), "cloth");
            }
            else
            {
                Box(root, "Closed cover", new Vector3(0,.029f,0), new Vector3(.27f,.016f,.34f), "folder");
                Box(root, "Blank label", new Vector3(0,.040f,-.06f), new Vector3(.18f,.005f,.08f), "paper");
                Box(root, "Elastic closure", new Vector3(.06f,.045f,0), new Vector3(.012f,.008f,.34f), "teal");
            }
        }

        private static void Build(Transform root, string id)
        {
            switch (id)
            {
                case "label-printer":
                    Box(root,"Printer shell",new Vector3(0,.075f,0),new Vector3(.30f,.15f,.23f),"shell");
                    Box(root,"Lid",new Vector3(0,.16f,.03f),new Vector3(.28f,.03f,.15f),"steel");
                    Box(root,"Output slot",new Vector3(0,.08f,-.12f),new Vector3(.16f,.025f,.01f),"steel");
                    Box(root,"Blank label strip",new Vector3(0,.025f,-.19f),new Vector3(.12f,.007f,.13f),"paper");
                    Round(root,"Feed button",new Vector3(.10f,.156f,-.065f),new Vector3(.04f,.01f,.04f),"teal"); break;
                case "off-air-switch":
                    Box(root,"Salvaged switch box",new Vector3(0,.04f,0),new Vector3(.26f,.08f,.18f),"steel");
                    Box(root,"Large mechanical switch",new Vector3(.07f,.10f,.02f),new Vector3(.05f,.04f,.08f),"pink");
                    Label(root,"label-off-air",new Vector3(-.025f,.081f,-.035f),new Vector2(.18f,.07f),true); break;
                case "diagnostic-microphone": Microphone(root,false); break;
                case "covered-microphone": Microphone(root,true); break;
                case "loose-patch-lead":
                    for(var i=0;i<15;i++)
                    { float a=i*Mathf.PI/10, b=(i+1)*Mathf.PI/10; Rod(root,"Cable segment",new Vector3(Mathf.Cos(a)*.18f,.013f,Mathf.Sin(a)*.12f),new Vector3(Mathf.Cos(b)*.18f,.013f,Mathf.Sin(b)*.12f),.018f,"teal"); }
                    Box(root,"Free plug A",new Vector3(.20f,.022f,0),new Vector3(.07f,.035f,.028f),"shell");
                    Box(root,"Free plug B",new Vector3(0,.022f,-.14f),new Vector3(.028f,.035f,.07f),"shell"); break;
                case "folder-closed": Folder(root,false); break;
                case "folder-open": Folder(root,true); break;
                case "contact-ledger":
                    Box(root,"Worn cover",new Vector3(0,.015f,0),new Vector3(.45f,.03f,.32f),"teal");
                    foreach(var x in new[]{-.115f,.115f}) Box(root,"Blank ruled page",new Vector3(x,.04f,0),new Vector3(.215f,.025f,.29f),"ledger");
                    Box(root,"Bookmark",new Vector3(.14f,.055f,-.17f),new Vector3(.025f,.004f,.06f),"pink"); break;
                case "roster-board":
                    Box(root,"Board",new Vector3(0,.36f,0),new Vector3(.52f,.70f,.035f),"folder");
                    Box(root,"Blank roster",new Vector3(0,.38f,-.023f),new Vector3(.45f,.59f,.006f),"paper");
                    for(var i=0;i<4;i++) Box(root,"Empty removable slot",new Vector3(.01f,.18f+i*.12f,-.028f),new Vector3(.36f,.075f,.005f),"ledger");
                    Box(root,"Top clip",new Vector3(0,.68f,-.03f),new Vector3(.12f,.04f,.02f),"steel"); break;
                case "restricted-input-card":
                case "station-card":
                    Box(root,"Index card",new Vector3(0,.075f,0),new Vector3(.18f,.13f,.005f),"paper");
                    Box(root,"Clip foot",new Vector3(0,.008f,0),new Vector3(.08f,.016f,.06f),"steel");
                    Label(root,id=="station-card"?"label-local":"label-ask",new Vector3(0,.085f,-.003f),new Vector2(.15f,.065f)); break;
                case "chess-board":
                    Box(root,"Repaired board",new Vector3(0,.025f,0),new Vector3(.42f,.05f,.42f),"folder");
                    for(var z=0;z<8;z++) for(var x=0;x<8;x++) Box(root,"Square",new Vector3(-.175f+x*.05f,.052f,-.175f+z*.05f),new Vector3(.049f,.004f,.049f),(x+z)%2==0?"paper":"steel");
                    foreach(var p in new[]{new Vector3(-.075f,.079f,-.125f),new Vector3(.125f,.079f,.075f)})
                    { Round(root,"Hardware pawn",p,new Vector3(.028f,.05f,.028f),"ochre"); Round(root,"Pawn cap",p+Vector3.up*.027f,new Vector3(.036f,.015f,.036f),"ochre"); } break;
                case "biscuit-tin":
                    Round(root,"Tin base",new Vector3(0,.045f,0),new Vector3(.20f,.09f,.20f),"teal");
                    Round(root,"Inset",new Vector3(0,.091f,0),new Vector3(.17f,.003f,.17f),"steel");
                    foreach(var p in new[]{new Vector3(-.03f,.102f,0),new Vector3(.035f,.104f,.035f)}) Round(root,"Plain biscuit",p,new Vector3(.08f,.015f,.08f),"biscuit");
                    Round(root,"Set aside lid",new Vector3(.23f,.006f,0),new Vector3(.21f,.012f,.21f),"shell"); break;
                case "saved-meal":
                    Round(root,"Plate",new Vector3(0,.012f,0),new Vector3(.26f,.024f,.26f),"shell");
                    Box(root,"Covered portion",new Vector3(0,.045f,0),new Vector3(.18f,.05f,.15f),"cloth");
                    Box(root,"Butter knife",new Vector3(.19f,.008f,0),new Vector3(.015f,.016f,.18f),"steel"); break;
                case "folding-chair":
                    foreach(var x in new[]{-.21f,.21f})
                    { Rod(root,"Front frame",new Vector3(x,.02f,-.25f),new Vector3(x,.84f,.17f),.025f,"steel"); Rod(root,"Rear frame",new Vector3(x,.02f,.27f),new Vector3(x,.48f,-.16f),.025f,"steel"); }
                    Box(root,"Seat",new Vector3(0,.46f,0),new Vector3(.46f,.05f,.42f),"shell");
                    Box(root,"Back",new Vector3(0,.75f,.18f),new Vector3(.43f,.18f,.04f),"shell");
                    Box(root,"Repair tape",new Vector3(.12f,.755f,.153f),new Vector3(.06f,.19f,.008f),"ochre"); break;
                case "exterior-camera":
                    Box(root,"Mount base",new Vector3(0,.02f,.09f),new Vector3(.14f,.04f,.14f),"steel");
                    Rod(root,"Mount arm",new Vector3(0,.04f,.09f),new Vector3(0,.17f,0),.04f,"steel");
                    Box(root,"Weather shell",new Vector3(0,.22f,-.035f),new Vector3(.12f,.11f,.23f),"shell");
                    Box(root,"Hood",new Vector3(0,.28f,-.06f),new Vector3(.15f,.015f,.28f),"shell");
                    var lens=Round(root,"Lens",new Vector3(0,.22f,-.157f),new Vector3(.064f,.016f,.064f),"steel"); lens.transform.localRotation=Quaternion.Euler(90,0,0); break;
                default: throw new ArgumentException("Unknown season prop " + id);
            }
        }

        [MenuItem("Common House/Season One/Generate Props And Review Scene")]
        public static void BuildInEditor()
        { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Generate(); }

        public static void Generate()
        {
            Directory.CreateDirectory(Root + "/Prefabs");
            foreach(var id in Ids)
            {
                var root=new GameObject(id).transform; Build(root,id);
                foreach(var renderer in root.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/"+id+".prefab"); UnityEngine.Object.DestroyImmediate(root.gameObject);
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var display=new GameObject("Season one asset study").transform;
            for(var i=0;i<Ids.Length;i++)
            {
                var position=new Vector3((i%4)*1.2f,0,(i/4)*1.2f);
                Box(display,"Review plinth "+Ids[i],position+Vector3.up*.35f,new Vector3(1f,.70f,1f),"steel");
                var item=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+Ids[i]+".prefab"));
                item.transform.SetParent(display,false); item.transform.localPosition=position+Vector3.up*.70f;
            }
            var lighting=new GameObject("Review lighting").AddComponent<RetroLighting>();
            lighting.ambient=new Color(.9f,.88f,.83f); lighting.fogStart=20; lighting.fogEnd=50; lighting.Apply();
            var volume=new GameObject("Review volume").AddComponent<Volume>(); volume.isGlobal=true;
            volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(RetroLook.ProfilePath);
            var camera=new GameObject("Review camera").AddComponent<Camera>();
            camera.transform.position=new Vector3(6.4f,6.5f,-5.5f); camera.transform.LookAt(new Vector3(1.8f,.8f,1.8f));
            camera.fieldOfView=42; camera.nearClipPlane=.03f; camera.farClipPlane=60;
            var hd=camera.gameObject.AddComponent<HDAdditionalCameraData>(); hd.clearColorMode=HDAdditionalCameraData.ClearColorMode.Color;
            hd.backgroundColorHDR=new Color(.15f,.18f,.22f);
            EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets();
        }

        public static void GenerateAndExport()
        {
            var output=Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("Use the disposable Editor runner.");
            Generate();
            foreach(var file in Directory.GetFiles(Root,"*",SearchOption.AllDirectories).Concat(new[]{Root+".meta",ScenePath,ScenePath+".meta"}))
            { var target=Path.Combine(output,"export",file); Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(file,target,true); }
        }
    }
}
