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
    /// <summary>Exterior camera-source study. No interior, actors, or episode state.</summary>
    public static class MotionDetectedSourceSet
    {
        public const string Root = "Assets/CommonHouse/Art/MotionDetected";
        public const string ScenePath = "Assets/CommonHouse/Scenes/MotionDetectedSourceReview.unity";
        public static readonly string[] Ids = { "basement-steps", "basement-door-closed", "basement-door-open", "repaired-handrail", "leaf-drain", "scraping-bin" };
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        private static Material Surface(string id)
        {
            if (Materials.TryGetValue(id, out var cached) && cached != null) return cached;
            Directory.CreateDirectory(Root + "/Textures"); Directory.CreateDirectory(Root + "/Materials");
            var path = Root + "/Textures/" + id + ".png";
            var texture = new Texture2D(64,64,TextureFormat.RGB24,false);
            for(var y=0;y<64;y++) for(var x=0;x<64;x++)
            {
                var noise=(((x*73856093)^(y*19349663))&31)/1000f-.015f;
                Color color;
                switch(id)
                {
                    case "concrete": color=new Color(.46f,.49f,.46f); if(x>42 && y<20) color*=.94f; break;
                    case "riser": color=new Color(.32f,.35f,.33f); break;
                    case "brick": color=y%16<2 || (x+(y/16%2)*16)%32<2 ? new Color(.46f,.45f,.39f) : new Color(.48f,.31f,.24f); break;
                    case "door-paint": color=new Color(.26f,.39f,.37f); if(x<3 || y<6) color=new Color(.45f,.40f,.28f); break;
                    case "metal": color=new Color(.25f,.28f,.29f); break;
                    case "repair": color=new Color(.63f,.65f,.59f); break;
                    case "leaf": color=(x+y)%9<2 ? new Color(.30f,.29f,.16f) : new Color(.46f,.34f,.17f); break;
                    case "bin": color=new Color(.27f,.33f,.29f); break;
                    case "void": color=new Color(.035f,.04f,.045f); noise=0; break;
                    default: throw new ArgumentException(id);
                }
                texture.SetPixel(x,y,color+new Color(noise,noise,noise,0));
            }
            texture.Apply(); var bytes=texture.EncodeToPNG(); UnityEngine.Object.DestroyImmediate(texture);
            if(!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes)) { File.WriteAllBytes(path,bytes); AssetDatabase.ImportAsset(path); }
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.npotScale=TextureImporterNPOTScale.None; importer.SaveAndReimport();
            var materialPath=Root+"/Materials/"+id+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null) { material=new Material(Shader.Find("CommonHouse/Retro")) { name="Motion detected "+id }; AssetDatabase.CreateAsset(material,materialPath); }
            material.SetColor("_Color",Color.white); material.SetFloat("_Cull",(float)CullMode.Back); material.SetFloat("_Cutoff",0);
            material.SetFloat("_WorldUV",id=="brick" || id=="concrete" ? 1:0); material.SetFloat("_WorldUVScale",.8f);
            material.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(path)); EditorUtility.SetDirty(material);
            return Materials[id]=material;
        }

        private static GameObject Box(Transform root,string name,Vector3 position,Vector3 size,string material,bool solid=false)
            => OfficeAssetKit.Box(root,name,position,size,Surface(material),solid);

        private static void Beam(Transform root,string name,Vector3 a,Vector3 b,float width,string material)
        {
            var beam=Box(root,name,(a+b)/2,new Vector3(width,width,Vector3.Distance(a,b)),material);
            beam.transform.localRotation=Quaternion.LookRotation(b-a);
        }

        private static void Build(Transform root,string id)
        {
            switch(id)
            {
                case "basement-steps":
                    // Six 0.20m risers and 0.30m treads. Surface origin is upper ground level.
                    for(var i=0;i<6;i++)
                    {
                        var top=-(i+1)*.20f; var height=1.50f+top;
                        Box(root,"Tread "+i,new Vector3(0,-1.50f+height/2,.15f+i*.30f),new Vector3(1.50f,height,.30f),"concrete",true);
                        Box(root,"Worn nosing "+i,new Vector3(0,top+.003f,i*.30f+.02f),new Vector3(1.43f,.006f,.035f),"repair");
                        Box(root,"Riser face "+i,new Vector3(0,top-.10f,i*.30f-.002f),new Vector3(1.43f,.19f,.008f),"riser");
                    }
                    Box(root,"Bottom landing",new Vector3(0,-1.35f,2.40f),new Vector3(1.50f,.30f,1.20f),"concrete",true); break;
                case "basement-door-closed":
                case "basement-door-open":
                    Box(root,"Left jamb",new Vector3(-.56f,1.04f,0),new Vector3(.10f,2.08f,.16f),"metal",true);
                    Box(root,"Right jamb",new Vector3(.56f,1.04f,0),new Vector3(.10f,2.08f,.16f),"metal",true);
                    Box(root,"Lintel",new Vector3(0,2.10f,0),new Vector3(1.22f,.12f,.16f),"metal",true);
                    Box(root,"Threshold",new Vector3(0,.018f,-.025f),new Vector3(1.12f,.036f,.25f),"repair",true);
                    var hinge=OfficeAssetKit.Group(root,"Door hinge",new Vector3(-.50f,0,0),id.EndsWith("open")?-78:0);
                    Box(hinge,"Door leaf",new Vector3(.50f,1.025f,0),new Vector3(1f,2.05f,.065f),"door-paint",true);
                    Box(hinge,"Handle plate",new Vector3(.90f,.96f,-.038f),new Vector3(.06f,.20f,.012f),"repair");
                    Box(hinge,"Lever handle",new Vector3(.85f,.99f,-.065f),new Vector3(.15f,.024f,.05f),"metal");
                    Box(hinge,"Kick plate",new Vector3(.50f,.18f,-.037f),new Vector3(.90f,.22f,.012f),"metal");
                    foreach(var y in new[]{.30f,1.70f}) Box(root,"Hinge barrel",new Vector3(-.50f,y,-.04f),new Vector3(.045f,.14f,.055f),"repair");
                    // A flat, opaque backstop at the source boundary. No indoor setting is authored.
                    Box(root,"Interior coverage stop",new Vector3(0,1.04f,.13f),new Vector3(1.16f,2.08f,.04f),"void",true); break;
                case "repaired-handrail":
                    Beam(root,"Continuous handrail",new Vector3(0,.78f,-.10f),new Vector3(0,-.40f,1.70f),.045f,"metal");
                    foreach(var point in new[]{new Vector3(0,.67f,.07f),new Vector3(0,-.22f,1.43f)})
                    {
                        Beam(root,"Support",point,new Vector3(0,point.y-.70f,point.z),.04f,"metal");
                        Box(root,"Wall plate",new Vector3(-.06f,point.y-.65f,point.z),new Vector3(.15f,.14f,.035f),"metal");
                    }
                    Beam(root,"Repair sleeve",new Vector3(0,.29f,.65f),new Vector3(0,.14f,.88f),.073f,"repair");
                    foreach(var z in new[]{.68f,.84f}) Box(root,"New screw head",new Vector3(-.043f,.78f-(z+.10f)*1.18f/1.80f,z),new Vector3(.012f,.025f,.025f),"metal"); break;
                case "leaf-drain":
                    Box(root,"Drain frame",new Vector3(0,.012f,0),new Vector3(.32f,.024f,.24f),"metal");
                    Box(root,"Dark recess",new Vector3(0,.025f,0),new Vector3(.28f,.002f,.20f),"void");
                    for(var i=0;i<7;i++) Box(root,"Grate bar",new Vector3(-.12f+i*.04f,.032f,0),new Vector3(.012f,.014f,.20f),"repair");
                    for(var i=0;i<9;i++)
                    {
                        var leaf=Box(root,"Caught leaf "+i,new Vector3(-.105f+(i%4)*.064f,.045f+(i%2)*.008f,-.07f+(i/4)*.057f),new Vector3(.064f,.006f,.031f),"leaf");
                        leaf.transform.localRotation=Quaternion.Euler(0,i*47,0);
                    } break;
                case "scraping-bin":
                    Box(root,"Bin body",new Vector3(0,.31f,0),new Vector3(.42f,.62f,.36f),"bin",true);
                    for(var i=0;i<4;i++) Box(root,"Body rib",new Vector3(-.15f+i*.10f,.30f,-.19f),new Vector3(.02f,.50f,.025f),"bin");
                    var lid=OfficeAssetKit.Group(root,"Lid pivot",new Vector3(0,.64f,.18f));
                    Box(lid,"Loose lid",new Vector3(0,0,-.18f),new Vector3(.46f,.025f,.40f),"metal");
                    Box(lid,"Lid grip",new Vector3(0,.028f,-.24f),new Vector3(.12f,.04f,.035f),"repair");
                    Box(root,"Set aside brick",new Vector3(-.34f,.045f,-.03f),new Vector3(.20f,.09f,.10f),"brick"); break;
                default: throw new ArgumentException(id);
            }
        }

        private static GameObject Place(Transform parent,string id,Vector3 position,bool active=true)
        {
            var item=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+id+".prefab"));
            item.transform.SetParent(parent,false); item.transform.localPosition=position; item.SetActive(active); return item;
        }

        [MenuItem("Common House/Season One/Generate Motion Detected Source Set")]
        public static void BuildInEditor() { if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Generate(); }

        public static void Generate()
        {
            Directory.CreateDirectory(Root+"/Prefabs");
            foreach(var id in Ids)
            {
                var root=new GameObject(id).transform; Build(root,id);
                foreach(var renderer in root.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"/Prefabs/"+id+".prefab"); UnityEngine.Object.DestroyImmediate(root.gameObject);
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var set=new GameObject("Source exterior only").transform;
            Place(set,"basement-steps",Vector3.zero);
            Place(set,"basement-door-closed",new Vector3(0,-1.20f,3.04f));
            Place(set,"basement-door-open",new Vector3(0,-1.20f,3.04f),false);
            Place(set,"repaired-handrail",new Vector3(-.69f,0,0));
            Place(set,"leaf-drain",new Vector3(.36f,-1.195f,2.38f));
            Place(set,"scraping-bin",new Vector3(1.40f,0,.40f));
            Box(set,"Upper approach",new Vector3(0,-.12f,-1.10f),new Vector3(3.30f,.24f,2.20f),"concrete",true);
            foreach(var x in new[]{-1.18f,1.18f})
            {
                Box(set,"Side bank",new Vector3(x,-.77f,1.52f),new Vector3(.86f,1.54f,3.04f),"concrete",true);
                Box(set,"Boundary wall",new Vector3(x*1.40f,.70f,1.50f),new Vector3(.10f,1.40f,5.20f),"brick",true);
            }
            foreach(var x in new[]{-1.12f,1.12f}) Box(set,"Foundation beside doorway",new Vector3(x,-.12f,3.16f),new Vector3(1.12f,2.40f,.20f),"brick",true);
            Box(set,"Wall above doorway",new Vector3(0,1.42f,3.16f),new Vector3(3.30f,.72f,.20f),"brick",true);
            var lighting=new GameObject("Source study lighting").AddComponent<RetroLighting>();
            lighting.ambient=new Color(.46f,.51f,.55f); lighting.fogStart=15; lighting.fogEnd=40;
            var skyFill=new GameObject("Cool exterior review fill").AddComponent<Light>();
            skyFill.type=LightType.Point; skyFill.transform.position=new Vector3(-2.5f,4f,-1f);
            skyFill.gameObject.AddComponent<HDAdditionalLightData>(); skyFill.lightUnit=LightUnit.Lumen;
            skyFill.intensity=1700f; skyFill.range=12f; skyFill.color=new Color(.76f,.85f,1f); skyFill.shadows=LightShadows.None;
            lighting.Apply();
            var volume=new GameObject("Source study volume").AddComponent<Volume>(); volume.isGlobal=true;
            volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(RetroLook.ProfilePath);
            var source=Camera("NetworkCamera",new Vector3(-1.15f,1.80f,-.85f),new Vector3(0,-.75f,1.40f),68);
            source.aspect=4f/3f;
            var cameraPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(SeasonOneAssetKit.Root+"/Prefabs/exterior-camera.prefab");
            if(cameraPrefab==null) throw new InvalidOperationException("Generate the Season 1 camera prop first.");
            var hardware=(GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab); hardware.name="Original source camera housing";
            hardware.transform.SetParent(set,false); hardware.transform.rotation=Quaternion.LookRotation(-source.transform.forward);
            hardware.transform.position=source.transform.position-hardware.transform.rotation*new Vector3(0,.22f,-.157f);
            Beam(set,"Source camera wall bracket",new Vector3(-1.64f,1.35f,-.85f),hardware.transform.TransformPoint(new Vector3(0,.02f,.09f)),.035f,"repair");
            // Artist review camera is not a source recording viewpoint.
            var review=Camera("ArtistReviewCamera",new Vector3(4.8f,4.5f,-4.6f),new Vector3(0,-.3f,1.3f),50); review.enabled=false;
            EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets();
        }

        private static Camera Camera(string name,Vector3 position,Vector3 target,float fov)
        {
            var camera=new GameObject(name).AddComponent<Camera>(); camera.transform.position=position; camera.transform.LookAt(target);
            camera.fieldOfView=fov; camera.nearClipPlane=.03f; camera.farClipPlane=40;
            var hd=camera.gameObject.AddComponent<HDAdditionalCameraData>(); hd.clearColorMode=HDAdditionalCameraData.ClearColorMode.Color;
            hd.backgroundColorHDR=new Color(.16f,.20f,.22f); return camera;
        }

        public static void GenerateAndExport()
        {
            var output=Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if(string.IsNullOrEmpty(output)) throw new InvalidOperationException("Use the disposable Editor runner.");
            Generate();
            foreach(var file in Directory.GetFiles(Root,"*",SearchOption.AllDirectories).Concat(new[]{Root+".meta",ScenePath,ScenePath+".meta"}))
            { var target=Path.Combine(output,"export",file); Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(file,target,true); }
        }
    }
}
