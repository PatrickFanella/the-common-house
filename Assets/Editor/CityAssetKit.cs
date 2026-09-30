using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static CommonHouse.Editor.OfficeAssetKit;

namespace CommonHouse.Editor
{
    /// <summary>Original metre-scale city modules. Local origin is pavement level; front is -Z.</summary>
    public static class CityAssetKit
    {
        public const string Root = "Assets/CommonHouse/Art/CityKit";
        public static readonly string[] Ids = { "road-straight", "road-crossing", "pavement", "alley", "brick-window", "concrete-window", "office-window", "storefront", "shutter", "doorway", "roof-cap", "fire-escape", "streetlamp", "utility-pole", "bus-shelter", "bench", "bollard", "litter-bin", "utility-cabinet", "notice-board" };
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        public static Material Material(string id)
        {
            if (Materials.TryGetValue(id, out var existing) && existing != null) return existing;
            Directory.CreateDirectory(Root + "/Materials");
            Directory.CreateDirectory(Root + "/Textures");
            var texturePath = Root + "/Textures/" + id + ".png";
            var texture = new Texture2D(64, 64, TextureFormat.RGB24, false);
            for (var y = 0; y < 64; y++)
            for (var x = 0; x < 64; x++) texture.SetPixel(x, y, Pixel(id, x, y));
            texture.Apply();
            var bytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);
            if (!File.Exists(texturePath) || !File.ReadAllBytes(texturePath).SequenceEqual(bytes))
            {
                File.WriteAllBytes(texturePath, bytes);
                AssetDatabase.ImportAsset(texturePath);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var path = Root + "/Materials/" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("CommonHouse/Retro")) { name = "City " + id };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_Color", Color.white);
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_Cutoff", 0f);
            material.SetFloat("_WorldUV", id == "brick" || id == "concrete" || id == "asphalt" || id == "paving" ? 1f : 0f);
            material.SetFloat("_WorldUVScale", id == "brick" ? 0.75f : 1f);
            EditorUtility.SetDirty(material);
            return Materials[id] = material;
        }

        private static Color Pixel(string id, int x, int y)
        {
            var noise = ((x * 37 + y * 61 + x * y * 13) % 23 - 11) / 400f;
            Color color;
            switch (id)
            {
                case "brick":
                    var mortar = y % 16 < 2 || (x + (y / 16 % 2) * 16) % 32 < 2;
                    color = mortar ? new Color(0.46f, 0.43f, 0.38f) : new Color(0.54f, 0.29f, 0.20f); break;
                case "concrete": color = new Color(0.68f, 0.66f, 0.60f); break;
                case "paving": color = x % 32 < 2 || y % 32 < 2 ? new Color(0.44f, 0.44f, 0.42f) : new Color(0.63f, 0.62f, 0.57f); break;
                case "asphalt": color = new Color(0.25f, 0.27f, 0.29f); break;
                case "glass": color = (x + y) % 43 < 7 ? new Color(0.32f, 0.43f, 0.47f) : new Color(0.13f, 0.21f, 0.27f); break;
                case "metal": color = new Color(0.21f, 0.25f, 0.27f); break;
                case "wood": color = x % 11 < 2 ? new Color(0.30f, 0.25f, 0.19f) : new Color(0.48f, 0.36f, 0.24f); break;
                case "paint": color = new Color(0.22f, 0.47f, 0.46f); break;
                case "marking": color = new Color(0.82f, 0.79f, 0.66f); break;
                case "lamp": color = new Color(1.3f, 1.18f, 0.76f); break;
                default:
                    // Original small shop/service labels and a street notice collage.
                    var word = id == "shop-sign" ? "SHOP" : id == "stop-sign" ? "BUS" : "NOTICE";
                    var index = (x - 4) / 9; var gx = (x - 4) % 9 / 1;
                    if (x >= 4 && index < word.Length && y <= 39 && y > 25 && LetterPixel(word[index], gx, (39 - y) / 2)) return Bone;
                    color = y < 9 || x < 3 ? Teal : Ink;
                    if (id == "notices" && y < 25 && x > 6 && x < 57)
                        color = (x / 18 + y / 7) % 3 == 0 ? Bone : (x + y) % 9 < 2 ? Ink : new Color(0.62f, 0.37f, 0.53f);
                    break;
            }
            return color + new Color(noise, noise, noise, 0f);
        }

        private static Mesh signFace;
        private static Mesh SignFace()
        {
            if (signFace != null) return signFace;
            Directory.CreateDirectory(Root + "/Meshes");
            var path = Root + "/Meshes/sign-face.asset";
            signFace = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (signFace == null)
            {
                signFace = new Mesh { name = "Original city sign face" };
                AssetDatabase.CreateAsset(signFace, path);
            }
            signFace.vertices = new[] { new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f) };
            signFace.uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
            signFace.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            signFace.RecalculateNormals(); signFace.RecalculateBounds();
            EditorUtility.SetDirty(signFace);
            return signFace;
        }

        private static void Part(Transform root, string name, Vector3 position, Vector3 size, string material, bool solid = false)
        {
            var part = Box(root, name, position, size, Material(material), solid);
            if (material.EndsWith("sign") || material == "notices") part.GetComponent<MeshFilter>().sharedMesh = SignFace();
        }

        public static void Generate()
        {
            Directory.CreateDirectory(Root + "/Prefabs");
            foreach (var id in Ids)
            {
                var root = new GameObject(id).transform;
                Build(root, id);
                foreach (var renderer in root.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode = ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(root.gameObject, Root + "/Prefabs/" + id + ".prefab");
                UnityEngine.Object.DestroyImmediate(root.gameObject);
            }
            AssetDatabase.SaveAssets();
            CityKitReview.Generate();
        }

        private static void Build(Transform root, string id)
        {
            if (id == "road-straight" || id == "road-crossing")
            {
                Part(root, "Road surface", new Vector3(0f, -0.10f, 0f), new Vector3(8f, 0.2f, 8f), "asphalt", true);
                if (id == "road-straight")
                    foreach (var z in new[] { -2.7f, 0f, 2.7f }) Part(root, "Lane dash", new Vector3(0f, 0.006f, z), new Vector3(0.12f, 0.012f, 1.4f), "marking");
                else
                    for (var z = -2.4f; z <= 2.4f; z += 0.8f) Part(root, "Crossing stripe", new Vector3(0f, 0.006f, z), new Vector3(6.8f, 0.012f, 0.4f), "marking");
                return;
            }
            if (id == "pavement" || id == "alley")
            {
                Part(root, "Walking surface", new Vector3(0f, id == "pavement" ? 0.025f : -0.10f, 0f), new Vector3(4f, id == "pavement" ? 0.25f : 0.2f, 8f), "paving", true);
                if (id == "pavement") Part(root, "Curb edge", new Vector3(-1.9f, 0.10f, 0f), new Vector3(0.2f, 0.10f, 8f), "concrete");
                else Part(root, "Drain cover", new Vector3(0.9f, 0.005f, 0f), new Vector3(0.55f, 0.01f, 0.75f), "metal");
                return;
            }
            if (id.EndsWith("window") || id == "storefront" || id == "shutter" || id == "doorway")
            {
                var masonry = id == "brick-window" || id == "storefront" ? "brick" : "concrete";
                Part(root, "Facade bay", new Vector3(0f, 1.5f, 0.12f), new Vector3(4f, 3f, 0.24f), masonry, true);
                Part(root, "Facade sill", new Vector3(0f, 0.10f, -0.06f), new Vector3(4f, 0.20f, 0.20f), "concrete");
                if (id.EndsWith("window"))
                {
                    var width = id == "office-window" ? 3.5f : 2.5f;
                    Part(root, "Window frame", new Vector3(0f, 1.7f, -0.10f), new Vector3(width, 1.8f, 0.13f), "metal");
                    Part(root, "Opaque window", new Vector3(0f, 1.7f, -0.18f), new Vector3(width - 0.16f, 1.6f, 0.02f), "glass");
                    Part(root, "Window mullion", new Vector3(0f, 1.7f, -0.20f), new Vector3(0.07f, 1.6f, 0.04f), "concrete");
                    Part(root, "Window transom", new Vector3(0f, 1.7f, -0.20f), new Vector3(width, 0.07f, 0.04f), "concrete");
                }
                else if (id == "shutter")
                {
                    Part(root, "Closed roller shutter", new Vector3(0f, 1.1f, -0.09f), new Vector3(3.3f, 2.2f, 0.14f), "metal");
                    for (var y = 0.12f; y < 2.2f; y += 0.15f) Part(root, "Shutter slat", new Vector3(0f, y, -0.18f), new Vector3(3.3f, 0.035f, 0.02f), "concrete");
                }
                else
                {
                    Part(root, "Closed entrance", new Vector3(1.1f, 1.1f, -0.11f), new Vector3(1f, 2.2f, 0.10f), "paint");
                    Part(root, "Door inset", new Vector3(1.1f, 1.4f, -0.18f), new Vector3(0.72f, 1.25f, 0.025f), "glass");
                    Part(root, "Door handle", new Vector3(0.8f, 1f, -0.22f), new Vector3(0.05f, 0.22f, 0.06f), "marking");
                    if (id == "storefront")
                    {
                        Part(root, "Shop window", new Vector3(-0.75f, 1.2f, -0.14f), new Vector3(2.25f, 1.75f, 0.08f), "glass");
                        Part(root, "Shop sign", new Vector3(0f, 2.6f, -0.17f), new Vector3(3.7f, 0.55f, 0.12f), "shop-sign");
                        Part(root, "Shop awning", new Vector3(0f, 2.3f, -0.60f), new Vector3(3.8f, 0.12f, 1.2f), "paint");
                    }
                }
                return;
            }
            if (id == "roof-cap")
            {
                Part(root, "Roof slab", new Vector3(0f, 0.08f, 0f), new Vector3(4f, 0.16f, 4f), "metal", true);
                Part(root, "Front parapet", new Vector3(0f, 0.40f, -1.9f), new Vector3(4f, 0.5f, 0.2f), "concrete", true);
                Part(root, "Roof vent", new Vector3(0.65f, 0.35f, 0.3f), new Vector3(0.65f, 0.40f, 0.65f), "metal"); return;
            }
            if (id == "fire-escape")
            {
                Part(root, "Escape landing", new Vector3(0f, 0.15f, -0.6f), new Vector3(3.6f, 0.1f, 1.2f), "metal", true);
                for (var x = -1.8f; x <= 1.8f; x += 0.30f) Part(root, "Escape railing", new Vector3(x, 0.70f, -1.15f), new Vector3(0.045f, 1f, 0.045f), "metal");
                Part(root, "Escape handrail", new Vector3(0f, 1.22f, -1.15f), new Vector3(3.7f, 0.055f, 0.055f), "metal");
                foreach (var x in new[] { -1.1f, -0.7f }) Part(root, "Ladder side", new Vector3(x, -1.3f, -0.8f), new Vector3(0.06f, 3f, 0.06f), "metal");
                for (var y = -2.7f; y < 0.2f; y += 0.3f) Part(root, "Ladder rung", new Vector3(-0.9f, y, -0.8f), new Vector3(0.45f, 0.04f, 0.04f), "metal"); return;
            }
            if (id == "streetlamp" || id == "utility-pole" || id == "bollard")
            {
                var height = id == "bollard" ? 0.85f : id == "streetlamp" ? 4.8f : 6f;
                Part(root, "Pole base", new Vector3(0f, 0.12f, 0f), new Vector3(0.3f, 0.24f, 0.3f), "metal", true);
                Part(root, "Pole shaft", new Vector3(0f, height / 2f, 0f), new Vector3(0.12f, height, 0.12f), id == "utility-pole" ? "wood" : "metal", true);
                if (id == "streetlamp")
                {
                    Part(root, "Lamp arm", new Vector3(0f, height - 0.10f, -0.65f), new Vector3(0.10f, 0.10f, 1.3f), "metal");
                    Part(root, "Lamp housing", new Vector3(0f, height - 0.12f, -1.2f), new Vector3(0.42f, 0.17f, 0.75f), "metal");
                    Part(root, "Lamp lens", new Vector3(0f, height - 0.22f, -1.2f), new Vector3(0.34f, 0.025f, 0.6f), "lamp");
                }
                else if (id == "utility-pole")
                {
                    Part(root, "Utility crossbar", new Vector3(0f, 5.6f, 0f), new Vector3(1.8f, 0.12f, 0.12f), "wood");
                    foreach (var x in new[] { -0.7f, 0f, 0.7f }) Part(root, "Cable insulator", new Vector3(x, 5.78f, 0f), new Vector3(0.12f, 0.25f, 0.12f), "marking");
                }
                else Part(root, "Bollard band", new Vector3(0f, 0.67f, 0f), new Vector3(0.14f, 0.1f, 0.14f), "marking");
                return;
            }
            if (id == "bench" || id == "bus-shelter")
            {
                foreach (var x in new[] { -0.75f, 0.75f }) Part(root, "Bench foot", new Vector3(x, 0.24f, 0f), new Vector3(0.10f, 0.48f, 0.55f), "metal");
                Part(root, "Bench seat", new Vector3(0f, 0.5f, 0f), new Vector3(2.0f, 0.12f, 0.55f), "wood", true);
                Part(root, "Bench back", new Vector3(0f, 0.85f, 0.25f), new Vector3(2f, 0.4f, 0.10f), "wood");
                if (id == "bus-shelter")
                {
                    foreach (var x in new[] { -1.5f, 1.5f }) Part(root, "Shelter post", new Vector3(x, 1.35f, 0.45f), new Vector3(0.09f, 2.7f, 0.09f), "metal", true);
                    Part(root, "Shelter roof", new Vector3(0f, 2.65f, 0f), new Vector3(3.4f, 0.16f, 1.8f), "paint");
                    Part(root, "Shelter opaque backing", new Vector3(0f, 1.5f, 0.5f), new Vector3(3f, 1.8f, 0.04f), "glass", true);
                    Part(root, "Bus stop label", new Vector3(-1.5f, 2.1f, -0.16f), new Vector3(0.65f, 0.45f, 0.04f), "stop-sign");
                }
                return;
            }
            if (id == "notice-board")
            {
                foreach (var x in new[] { -0.55f, 0.55f }) Part(root, "Notice post", new Vector3(x, 0.95f, 0f), new Vector3(0.08f, 1.9f, 0.08f), "metal");
                Part(root, "Notice frame", new Vector3(0f, 1.4f, 0f), new Vector3(1.4f, 1.1f, 0.10f), "paint", true);
                Part(root, "Original public notices", new Vector3(0f, 1.4f, -0.065f), new Vector3(1.25f, 0.95f, 0.025f), "notices"); return;
            }
            var cabinet = id == "utility-cabinet";
            Part(root, "Street enclosure", new Vector3(0f, cabinet ? 0.65f : 0.45f, 0f), new Vector3(cabinet ? 0.9f : 0.5f, cabinet ? 1.3f : 0.9f, 0.5f), cabinet ? "paint" : "metal", true);
            if (cabinet)
            {
                for (var y = 0.85f; y < 1.2f; y += 0.08f) Part(root, "Cabinet vent", new Vector3(0f, y, -0.26f), new Vector3(0.65f, 0.025f, 0.02f), "metal");
                Part(root, "Cabinet latch", new Vector3(0.3f, 0.6f, -0.28f), new Vector3(0.04f, 0.18f, 0.03f), "marking");
            }
            else Part(root, "Bin opening", new Vector3(0f, 0.70f, -0.26f), new Vector3(0.35f, 0.12f, 0.025f), "glass");
        }
    }
}
