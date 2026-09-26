using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.StyleBoard
{
    /// <summary>Small placement helpers for composing studies from generated meshes.</summary>
    internal sealed class StudyBuilder
    {
        private readonly Font font;
        public readonly Transform Root;
        public readonly StyleMaterials Materials;

        public StudyBuilder(string name, Vector3 origin, StyleMaterials materials)
        {
            Root = new GameObject(name).transform;
            Root.position = origin;
            Materials = materials;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            // Fill the dynamic atlas before any label exists, so the copy made by
            // StyleMaterials.ApplyFontAtlas already holds every glyph.
            font.RequestCharactersInTexture(Glyphs, FontSize);
        }

        public Font Font => font;

        private const int FontSize = 64;
        private static readonly string Glyphs = BuildGlyphs();

        private static string BuildGlyphs()
        {
            var glyphs = new System.Text.StringBuilder("—─┬└");
            for (var c = ' '; c <= '~'; c++) glyphs.Append(c);
            return glyphs.ToString();
        }

        public GameObject Box(string name, Vector3 center, Vector3 size, Material material, Vector3 euler = default)
        {
            return Place(name, StyleMeshes.Box(size), center, euler, material);
        }

        public GameObject Cylinder(string name, Vector3 center, float radius, float height, Material material, Vector3 euler = default, int sides = 28)
        {
            return Place(name, StyleMeshes.Cylinder(radius, height, sides), center, euler, material);
        }

        public GameObject Sphere(string name, Vector3 center, Vector3 scale, Material material)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(sphere.GetComponent<Collider>());
            sphere.name = name;
            sphere.transform.SetParent(Root, false);
            sphere.transform.localPosition = center;
            sphere.transform.localScale = scale;
            sphere.GetComponent<MeshRenderer>().sharedMaterial = material;
            return sphere;
        }

        public GameObject Cable(string name, Material material, float radius, params Vector3[] points)
        {
            return Place(name, StyleMeshes.Tube(points, radius), Vector3.zero, Vector3.zero, material);
        }

        /// <summary>Lit text; readable when viewed along the label's +Z axis.</summary>
        public GameObject Label(string text, Vector3 position, float lineHeight, Material ink, Vector3 euler = default, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var label = new GameObject("Label: " + text.Split('\n')[0]);
            label.transform.SetParent(Root, false);
            label.transform.localPosition = position;
            label.transform.localEulerAngles = euler;
            var mesh = label.AddComponent<TextMesh>();
            mesh.font = font;
            mesh.fontSize = FontSize;
            mesh.characterSize = lineHeight * 10f / FontSize;
            mesh.anchor = anchor;
            mesh.alignment = TextAlignment.Left;
            mesh.text = text;
            var renderer = label.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = ink;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return label;
        }

        /// <summary>A sheet of paper with lines of text on its local -Z face (a back wall by default).</summary>
        public GameObject Note(string name, Vector3 center, Vector2 size, Material paper, string text, float lineHeight, Material ink, Vector3 euler = default)
        {
            var sheet = Box(name, center, new Vector3(size.x, size.y, 0.002f), paper, euler);
            var label = Label(text, new Vector3(-size.x * 0.42f, size.y * 0.4f, -0.0025f), lineHeight, ink, default, TextAnchor.UpperLeft);
            label.transform.SetParent(sheet.transform, false);
            return sheet;
        }

        /// <summary>A point light in HDRP's physical units; intensity is in lumens.</summary>
        public Light Point(string name, Vector3 position, Color color, float range, float lumens, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            // Added before units and intensity; otherwise HDRP replaces them with its defaults on load.
            go.AddComponent<HDAdditionalLightData>();
            light.lightUnit = UnityEngine.Rendering.LightUnit.Lumen;
            light.intensity = lumens;
            light.color = color;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            return light;
        }

        public Light Spot(string name, Vector3 position, Vector3 target, Color color, float range, float angle, float lumens)
        {
            var light = Point(name, position, color, range, lumens);
            light.type = LightType.Spot;
            light.spotAngle = angle;
            light.lightUnit = UnityEngine.Rendering.LightUnit.Lumen;
            light.intensity = lumens;
            light.transform.LookAt(Root.TransformPoint(target));
            return light;
        }

        /// <summary>Floor and two walls meeting in a corner, so light has something to fall on.</summary>
        public void Corner(Material floor, Material wall, Material wainscot, float width, float depth, float height, float backZ, float sideX)
        {
            Box("Floor", new Vector3(0, -0.02f, backZ - depth * 0.5f), new Vector3(width, 0.04f, depth), floor);
            Box("Back wall", new Vector3(0, height * 0.5f, backZ + 0.05f), new Vector3(width, height, 0.1f), wall);
            Box("Side wall", new Vector3(sideX - 0.05f, height * 0.5f, backZ - depth * 0.5f), new Vector3(0.1f, height, depth), wall);
            if (wainscot == null) return;
            Box("Back wainscot", new Vector3(0, 0.45f, backZ - 0.01f), new Vector3(width, 0.9f, 0.02f), wainscot);
            Box("Side wainscot", new Vector3(sideX + 0.01f, 0.45f, backZ - depth * 0.5f), new Vector3(0.02f, 0.9f, depth), wainscot);
            Box("Back rail", new Vector3(0, 0.91f, backZ - 0.025f), new Vector3(width, 0.04f, 0.05f), Materials.DarkTimber);
            Box("Side rail", new Vector3(sideX + 0.025f, 0.91f, backZ - depth * 0.5f), new Vector3(0.05f, 0.04f, depth), Materials.DarkTimber);
        }

        public void Chair(string name, Vector3 seatCenter, float yaw, Material frame, Material seat)
        {
            var chair = new GameObject(name).transform;
            chair.SetParent(Root, false);
            chair.localPosition = seatCenter;
            chair.localEulerAngles = new Vector3(0, yaw, 0);
            var parts = new List<GameObject>
            {
                Box("Seat", new Vector3(0, 0, 0), new Vector3(0.42f, 0.03f, 0.4f), seat),
                Box("Leg FL", new Vector3(-0.18f, -0.23f, -0.17f), new Vector3(0.035f, 0.44f, 0.035f), frame),
                Box("Leg FR", new Vector3(0.18f, -0.23f, -0.17f), new Vector3(0.035f, 0.44f, 0.035f), frame),
                Box("Leg BL", new Vector3(-0.18f, 0.0f, 0.18f), new Vector3(0.035f, 0.9f, 0.035f), frame),
                Box("Leg BR", new Vector3(0.18f, 0.0f, 0.18f), new Vector3(0.035f, 0.9f, 0.035f), frame),
                Box("Back rail top", new Vector3(0, 0.4f, 0.18f), new Vector3(0.4f, 0.07f, 0.025f), frame),
                Box("Back rail mid", new Vector3(0, 0.2f, 0.18f), new Vector3(0.4f, 0.04f, 0.025f), frame),
                Box("Stretcher", new Vector3(0, -0.33f, 0f), new Vector3(0.36f, 0.025f, 0.025f), frame),
            };
            foreach (var part in parts) part.transform.SetParent(chair, false);
        }

        public void Lamp(string name, Vector3 basePosition, Material shade, bool lit)
        {
            Cylinder(name + " base", basePosition + new Vector3(0, 0.015f, 0), 0.08f, 0.03f, Materials.Steel);
            Cylinder(name + " stem", basePosition + new Vector3(0, 0.2f, 0), 0.012f, 0.38f, Materials.Steel);
            Cylinder(name + " arm", basePosition + new Vector3(-0.08f, 0.4f, -0.02f), 0.01f, 0.2f, Materials.Steel, new Vector3(0, 0, 70));
            Cylinder(name + " shade", basePosition + new Vector3(-0.17f, 0.4f, -0.02f), 0.08f, 0.12f, shade, new Vector3(0, 0, -20));
            Sphere(name + " bulb", basePosition + new Vector3(-0.19f, 0.34f, -0.02f), Vector3.one * 0.05f, lit ? Materials.Bulb : Materials.Glass);
            if (lit) Point(name + " light", basePosition + new Vector3(-0.2f, 0.3f, -0.03f), new Color(1f, 0.74f, 0.46f), 3.2f, 450f);
        }

        private GameObject Place(string name, Mesh mesh, Vector3 position, Vector3 euler, Material material)
        {
            var item = new GameObject(name);
            item.transform.SetParent(Root, false);
            item.transform.localPosition = position;
            item.transform.localEulerAngles = euler;
            item.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = item.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return item;
        }
    }
}
