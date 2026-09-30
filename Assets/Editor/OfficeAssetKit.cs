using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Editor
{
    /// <summary>Original, reusable low-poly office adaptations. No character or story content.</summary>
    public static class OfficeAssetKit
    {
        public const string Root = "Assets/CommonHouse/Art/OfficeComplex";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        public static readonly Color Bone = new Color(0.78f, 0.79f, 0.75f);
        public static readonly Color Ink = new Color(0.10f, 0.13f, 0.18f);
        public static readonly Color Teal = new Color(0.12f, 0.75f, 0.70f);
        public static readonly Color Pink = new Color(0.84f, 0.20f, 0.43f);
        public static readonly Color Yellow = new Color(0.91f, 0.68f, 0.22f);

        public static Material Surface(string id, Color color, bool grain = false)
        {
            if (Materials.TryGetValue(id, out var cached) && cached != null) return cached;
            Directory.CreateDirectory(Root + "/Materials");
            var path = Root + "/Materials/" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("CommonHouse/Retro")) { name = "Office " + id };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_Color", color);
            material.SetFloat("_Cutoff", 0f);
            material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_WorldUV", grain ? 1f : 0f);
            material.SetFloat("_WorldUVScale", 1f);
            if (grain) material.SetTexture("_MainTex", Texture(id, 64, 64, (x, y) =>
            {
                var noise = ((x * 37 + y * 61 + x * y * 13) % 23) / 230f;
                if (id.Contains("glass"))
                    return y < 22 ? new Color(0.50f + noise, 0.32f + noise, 0.38f + noise)
                        : new Color(0.62f + noise, 0.72f + noise, 0.79f + noise);
                var seam = id.Contains("masonry") && (x % 32 == 0 || y % 32 == 0);
                var v = seam ? 0.68f : 0.88f + noise;
                return new Color(v, v, v);
            }));
            EditorUtility.SetDirty(material);
            return Materials[id] = material;
        }

        internal static Texture2D Texture(string id, int width, int height, Func<int, int, Color> pixel)
        {
            Directory.CreateDirectory(Root + "/Textures");
            var path = Root + "/Textures/" + id + ".png";
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) texture.SetPixel(x, y, pixel(x, y));
            texture.Apply();
            var bytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
            {
                File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool solid = false)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = size;
            item.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            return item;
        }

        public static Transform Group(Transform parent, string name, Vector3 position, float yaw = 0f)
        {
            var item = new GameObject(name).transform;
            item.SetParent(parent, false);
            item.localPosition = position;
            item.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return item;
        }

        public static void Cable(Transform parent, Vector3 start, Vector3 end, Color color)
        {
            var item = Box(parent, "Patched cable conduit", (start + end) / 2f,
                new Vector3(0.035f, 0.035f, Vector3.Distance(start, end)), Surface("cable", color));
            item.transform.localRotation = Quaternion.LookRotation(end - start);
        }

        public static Transform Desk(Transform parent, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Salvaged workbench", position, yaw);
            var steel = Surface("steel", Ink);
            Box(root, "Plywood top", new Vector3(0f, 0.78f, 0f), new Vector3(2.5f, 0.13f, 0.95f), Surface("plywood", new Color(0.58f, 0.40f, 0.24f), true), true);
            foreach (var x in new[] { -1.05f, 1.05f })
            foreach (var z in new[] { -0.35f, 0.35f })
                Box(root, "Steel leg", new Vector3(x, 0.36f, z), new Vector3(0.08f, 0.72f, 0.08f), steel);
            return root;
        }

        public static void Terminal(Transform parent, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Patched terminal", position, yaw);
            var body = Surface("terminal-shell", new Color(0.40f, 0.43f, 0.40f), true);
            Box(root, "CRT shell", new Vector3(0f, 0.26f, 0f), new Vector3(0.58f, 0.45f, 0.42f), body);
            var screen = Surface("terminal-screen", Teal);
            screen.SetTexture("_MainTex", Texture("terminal-screen", 64, 64, (x, y) =>
                x > 5 && x < 58 && y > 6 && y < 56 && (y % 7 < 2 && x < 12 + (y * 17 % 43)) ? Color.white : Ink));
            Box(root, "Terminal screen", new Vector3(0f, 0.27f, -0.216f), new Vector3(0.49f, 0.34f, 0.012f), screen);
            Box(root, "Keyboard", new Vector3(0f, 0.035f, -0.38f), new Vector3(0.62f, 0.06f, 0.20f), body);
            for (var row = 0; row < 3; row++)
                Box(root, "Key row", new Vector3(0f, 0.07f, -0.44f + row * 0.055f), new Vector3(0.52f, 0.015f, 0.025f), Surface("keycaps", Ink));
        }

        public static void Rack(Transform parent, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Rewired service rack", position, yaw);
            Box(root, "Rack frame", new Vector3(0f, 1f, 0f), new Vector3(0.8f, 2f, 0.65f), Surface("steel", Ink), true);
            for (var row = 0; row < 7; row++)
            {
                Box(root, "Equipment tray", new Vector3(0f, 0.3f + row * 0.23f, -0.335f), new Vector3(0.70f, 0.17f, 0.035f), Surface("rack-tray", new Color(0.35f, 0.38f, 0.40f)));
                Box(root, "Status lamps", new Vector3(-0.24f, 0.3f + row * 0.23f, -0.36f), new Vector3(0.12f, 0.025f, 0.01f), Surface("signal", Teal));
            }
            Cable(root, new Vector3(0.45f, 0.2f, -0.4f), new Vector3(0.45f, 1.9f, -0.4f), Yellow);
        }

        public static void Cot(Transform parent, Vector3 position)
        {
            var root = Group(parent, "Communal sleeping cot", position);
            Box(root, "Cot frame", new Vector3(0f, 0.30f, 0f), new Vector3(1.1f, 0.12f, 2.1f), Surface("steel", Ink), true);
            Box(root, "Mattress", new Vector3(0f, 0.44f, 0f), new Vector3(1f, 0.20f, 2f), Surface("bedding", new Color(0.48f, 0.54f, 0.52f), true));
            Box(root, "Folded blanket", new Vector3(0f, 0.56f, -0.48f), new Vector3(1.01f, 0.08f, 0.82f), Surface("blanket", new Color(0.51f, 0.25f, 0.33f), true));
            Box(root, "Pillow", new Vector3(0f, 0.58f, 0.67f), new Vector3(0.7f, 0.15f, 0.38f), Surface("linen", Bone, true));
            foreach (var x in new[] { -0.45f, 0.45f })
            foreach (var z in new[] { -0.85f, 0.85f })
                Box(root, "Cot leg", new Vector3(x, 0.14f, z), new Vector3(0.06f, 0.28f, 0.06f), Surface("steel", Ink));
        }

        public static void Locker(Transform parent, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Reused office locker", position, yaw);
            Box(root, "Locker body", new Vector3(0f, 0.9f, 0f), new Vector3(0.65f, 1.8f, 0.5f), Surface("locker", new Color(0.31f, 0.40f, 0.41f), true), true);
            Box(root, "Door", new Vector3(0f, 0.9f, -0.26f), new Vector3(0.58f, 1.67f, 0.02f), Surface("locker-door", new Color(0.43f, 0.49f, 0.46f)));
            Box(root, "Latch", new Vector3(0.20f, 0.9f, -0.285f), new Vector3(0.05f, 0.15f, 0.03f), Surface("steel", Ink));
            for (var i = 0; i < 3; i++) Box(root, "Vent", new Vector3(0f, 1.5f + i * 0.05f, -0.28f), new Vector3(0.36f, 0.02f, 0.01f), Surface("steel", Ink));
        }

        public static void Sofa(Transform parent, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Mismatched common sofa", position, yaw);
            var cloth = Surface("sofa-cloth", new Color(0.46f, 0.26f, 0.32f), true);
            Box(root, "Sofa base", new Vector3(0f, 0.33f, 0f), new Vector3(2.2f, 0.55f, 0.85f), cloth, true);
            Box(root, "Back", new Vector3(0f, 0.76f, 0.35f), new Vector3(2.2f, 0.65f, 0.2f), cloth);
            foreach (var x in new[] { -1f, 1f }) Box(root, "Arm", new Vector3(x, 0.65f, 0f), new Vector3(0.20f, 0.35f, 0.85f), cloth);
            Box(root, "Repaired cushion", new Vector3(-0.52f, 0.65f, 0f), new Vector3(0.85f, 0.10f, 0.60f), Surface("sofa-patch", new Color(0.37f, 0.45f, 0.43f), true));
        }

        public static void Toolboard(Transform parent, Vector3 position)
        {
            var root = Group(parent, "Shared repair toolboard", position);
            Box(root, "Pegboard", Vector3.zero, new Vector3(2.4f, 1.0f, 0.07f), Surface("plywood", new Color(0.58f, 0.40f, 0.24f), true));
            for (var i = 0; i < 9; i++)
            {
                var x = -1f + i * 0.25f;
                Box(root, "Tool shaft", new Vector3(x, -0.02f, 0.08f), new Vector3(0.035f, 0.40f - (i % 3) * 0.07f, 0.04f), Surface("tool-metal", Bone));
                Box(root, "Tool grip", new Vector3(x, -0.20f, 0.10f), new Vector3(0.065f, 0.15f, 0.07f), Surface("tool-grip-" + i % 2, i % 2 == 0 ? Pink : Yellow));
            }
        }

        // Tiny original bitmap lettering keeps signs legible within the low-resolution art treatment.
        private static readonly Dictionary<char, string> Glyphs = new Dictionary<char, string>
        {
            ['A']="01110100011000111111100011000110001", ['B']="11110100011000111110100011000111110",
            ['C']="01111100001000010000100001000001111", ['D']="11110100011000110001100011000111110",
            ['E']="11111100001000011110100001000011111", ['F']="11111100001000011110100001000010000",
            ['G']="01111100001000010111100011000101111", ['H']="10001100011000111111100011000110001",
            ['I']="11111001000010000100001000010011111", ['J']="00111000100001000010100101001001100",
            ['K']="10001100101010011000101001001010001", ['L']="10000100001000010000100001000011111",
            ['M']="10001110111010110101100011000110001", ['N']="10001110011010110011100011000110001",
            ['O']="01110100011000110001100011000101110", ['P']="11110100011000111110100001000010000",
            ['Q']="01110100011000110001101011001001101", ['R']="11110100011000111110101001001010001",
            ['S']="01111100001000001110000010000111110", ['T']="11111001000010000100001000010000100",
            ['U']="10001100011000110001100011000101110", ['V']="10001100011000110001100010101000100",
            ['W']="10001100011000110101101011101110001", ['X']="10001100010101000100010101000110001",
            ['Y']="10001100010101000100001000010000100", ['Z']="11111000010001000100010001000011111",
        };

        internal static bool LetterPixel(char letter, int x, int y)
            => x >= 0 && x < 5 && y >= 0 && y < 7 && Glyphs.TryGetValue(letter, out var glyph) && glyph[y * 5 + x] == '1';

        public static void Sign(Transform parent, string id, string[] lines, Vector3 position, Vector2 size, Color accent, float yaw = 0f)
        {
            var material = Surface("sign-" + id, Color.white);
            material.SetTexture("_MainTex", Texture("sign-" + id, 128, 64, (x, y) =>
            {
                if (x < 3 || y < 3 || y > 60) return accent;
                // Small offset blocks and a diagonal corner stripe give the paper an improvised print treatment.
                if (x > 105 && y < 17 && (x + y) % 11 < 4) return accent;
                for (var row = 0; row < lines.Length; row++)
                {
                    var text = lines[row];
                    var top = lines.Length > 2 ? 56 - row * 17 : 52 - row * 20;
                    var gy = (top - y) / 2;
                    if (y > top || y < top - 13 || gy < 0 || gy > 6) continue;
                    var index = (x - 8) / 12;
                    var gx = ((x - 8) % 12) / 2;
                    if (x >= 8 && index < text.Length && gx < 5 && Glyphs.TryGetValue(text[index], out var glyph) && glyph[gy * 5 + gx] == '1')
                        return row % 2 == 0 ? Bone : accent;
                }
                return Ink;
            }));
            var sign = Box(parent, "Sign: " + id, position, new Vector3(size.x, size.y, 0.015f), material);
            sign.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        public static void Graphic(Transform parent, string id, Vector3 position, Vector2 size, float yaw = 0f)
        {
            var material = Surface("graphic-" + id, Color.white);
            material.SetTexture("_MainTex", Texture("graphic-" + id, 128, 128, (x, y) =>
            {
                var distance = new Vector2(x - 60, y - 67).magnitude;
                // An original interrupted broadcast-ring print with offset color blocks and tape edges.
                if ((x < 10 && y % 19 < 5) || (y < 9 && (x + y) % 16 < 7)) return Yellow;
                if (x > 85 && y > 20 && y < 105 && (x + y) % 23 < 11) return Pink;
                if (distance > 24f && distance < 29f || distance > 41f && distance < 46f)
                    return x % 17 < 3 ? Ink : Teal;
                if (x > 30 && x < 82 && y > 44 && y < 83 && (x + y * 2) % 29 < 5) return Bone;
                if (y > 108 && x > 17 && x < 72) return Pink;
                return (x * 13 + y * 19) % 47 < 2 ? new Color(0.28f, 0.30f, 0.32f) : Ink;
            }));
            var panel = Box(parent, "Original broadcast-ring graphic: " + id, position, new Vector3(size.x, size.y, 0.015f), material);
            panel.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
