using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Editor.StyleBoard
{
    /// <summary>
    /// Proposed style studies built entirely from generated meshes and textures. Nothing here
    /// is saved into the project or used by the playable outpost scene.
    /// </summary>
    public static class StyleStudies
    {
        private const int TextureSize = 512;
        private const int Width = 1600;
        private const int Height = 900;

        private sealed class Study
        {
            public string Id;
            public Transform Root;
            public Vector3 Camera;
            public Vector3 Target;
            public float Fov = 55f;
            public Color Ambient;
        }

        [MenuItem("Common House/Style Board/Build Studies In New Scene")]
        public static void BuildInEditor()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var studies = BuildAll(new StyleMaterials(TextureSize));
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = studies[0].Ambient;
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                var first = studies[0];
                view.LookAt(first.Root.TransformPoint(first.Target), Quaternion.LookRotation(first.Target - first.Camera), 2.5f);
            }
        }

        // Explicit graphical evidence operation in a disposable project. Does not save the scene.
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required for rendered studies.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 12f;
            QualitySettings.pixelLightCount = 8;
            QualitySettings.antiAliasing = 0;

            var materials = new StyleMaterials(TextureSize);
            var studies = BuildAll(materials);
            var camera = new GameObject("Study camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.025f);
            camera.nearClipPlane = 0.02f;
            camera.farClipPlane = 30f;
            camera.allowMSAA = true;
            camera.allowHDR = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.fog = false;
            foreach (var study in studies)
            {
                foreach (var other in studies) other.Root.gameObject.SetActive(other == study);
                RenderSettings.ambientLight = study.Ambient;
                camera.fieldOfView = study.Fov;
                camera.transform.position = study.Root.TransformPoint(study.Camera);
                camera.transform.LookAt(study.Root.TransformPoint(study.Target));
                Save(camera, Path.Combine(output, study.Id + ".jpg"));
            }
            File.WriteAllBytes(Path.Combine(output, "palette.png"), PaletteStrip().EncodeToPNG());
            var inventory = new StringBuilder();
            inventory.AppendLine("# Generated materials (" + TextureSize + "px, deterministic seeds)");
            foreach (var entry in materials.Library) inventory.AppendLine(entry.Name);
            inventory.AppendLine("# Palette");
            foreach (var entry in StylePalette.Entries()) inventory.AppendLine(entry.Name + " #" + ColorUtility.ToHtmlStringRGB(entry.Color));
            File.WriteAllText(Path.Combine(output, "style-inventory.txt"), inventory.ToString());
            File.WriteAllText(Path.Combine(output, "graphics-device.txt"), SystemInfo.graphicsDeviceName + "\n" + SystemInfo.graphicsDeviceVersion + "\n");
        }

        private static List<Study> BuildAll(StyleMaterials materials)
        {
            return new List<Study>
            {
                RadioDesk(materials, new Vector3(0, 0, 0), night: false),
                RadioDesk(materials, new Vector3(0, 0, 12), night: true),
                Kitchen(materials, new Vector3(10, 0, 0)),
                Archive(materials, new Vector3(20, 0, 0)),
                Swatches(materials, new Vector3(30, 0, 0)),
            };
        }

        private static Study RadioDesk(StyleMaterials m, Vector3 origin, bool night)
        {
            var b = new StudyBuilder(night ? "Radio desk, later" : "Radio desk", origin, m);
            b.Corner(m.Floorboards, m.Plaster, m.TealPaint, 4f, 3.2f, 3f, 0.6f, -1.6f);

            // Desk, with a splinted front leg from an old repair.
            b.Box("Desk top", new Vector3(0, 0.76f, 0.2f), new Vector3(1.6f, 0.05f, 0.7f), m.Timber);
            foreach (var x in new[] { -0.75f, 0.75f })
            foreach (var z in new[] { -0.1f, 0.5f })
                b.Box("Desk leg", new Vector3(x, 0.37f, z), new Vector3(0.05f, 0.74f, 0.05f), m.DarkTimber);
            b.Box("Leg splint", new Vector3(-0.72f, 0.3f, -0.13f), new Vector3(0.025f, 0.34f, 0.045f), m.WeatheredTimber);
            b.Cylinder("Splint wrap", new Vector3(-0.735f, 0.4f, -0.1f), 0.045f, 0.03f, m.ClothTape);
            b.Cylinder("Splint wrap", new Vector3(-0.735f, 0.22f, -0.1f), 0.045f, 0.03f, m.ClothTape);
            b.Box("Desk apron", new Vector3(0, 0.7f, 0.53f), new Vector3(1.5f, 0.08f, 0.02f), m.DarkTimber);

            // Older receiver, with a newer salvaged module strapped on top.
            b.Box("Receiver", new Vector3(-0.3f, 0.965f, 0.38f), new Vector3(0.62f, 0.36f, 0.34f), m.RadioPaint);
            b.Box("Dial window", new Vector3(-0.4f, 1.04f, 0.208f), new Vector3(0.26f, 0.075f, 0.006f), m.Dial);
            b.Box("Dial needle", new Vector3(-0.44f, 1.04f, 0.2035f), new Vector3(0.004f, 0.07f, 0.002f), m.RedCable);
            b.Box("Speaker cloth", new Vector3(-0.1f, 0.99f, 0.208f), new Vector3(0.16f, 0.2f, 0.006f), m.ClothTape);
            var knobX = new[] { -0.53f, -0.4f, -0.27f };
            for (var i = 0; i < knobX.Length; i++)
                b.Cylinder("Knob", new Vector3(knobX[i], 0.87f, 0.195f), 0.024f, 0.03f, i == 2 ? m.RedEnamel : m.Polymer, new Vector3(90, 0, 0));
            b.Box("Band tape", new Vector3(-0.4f, 0.93f, 0.2085f), new Vector3(0.15f, 0.026f, 0.002f), m.Tape);
            b.Label("BAND 2 (NIGHT)", new Vector3(-0.4f, 0.93f, 0.2062f), 0.013f, m.Pencil);
            b.Box("Future module", new Vector3(-0.28f, 1.175f, 0.4f), new Vector3(0.36f, 0.06f, 0.26f), m.Polymer);
            b.Box("Module indicator", new Vector3(-0.33f, 1.18f, 0.2685f), new Vector3(0.18f, 0.008f, 0.004f), m.Indicator);
            b.Box("Strap", new Vector3(-0.44f, 1.14f, 0.4f), new Vector3(0.02f, 0.1f, 0.3f), m.Copper);
            b.Box("Strap", new Vector3(-0.12f, 1.14f, 0.4f), new Vector3(0.02f, 0.1f, 0.3f), m.Copper);
            b.Box("Module tape", new Vector3(-0.2f, 1.19f, 0.2685f), new Vector3(0.09f, 0.022f, 0.002f), m.Tape);
            b.Label("RELAY", new Vector3(-0.2f, 1.19f, 0.2663f), 0.012f, m.Ink);

            // Cables that go somewhere: receiver to the wall switch; module up to the aerial run.
            b.Cable("Speaker lead", m.Cable, 0.008f, new Vector3(-0.1f, 0.9f, 0.56f), new Vector3(0.1f, 0.79f, 0.57f),
                new Vector3(0.45f, 0.79f, 0.58f), new Vector3(0.62f, 1.0f, 0.585f), new Vector3(0.7f, 1.37f, 0.585f));
            b.Cable("Aerial lead", m.RedCable, 0.009f, new Vector3(-0.15f, 1.18f, 0.5f), new Vector3(0.05f, 1.3f, 0.585f),
                new Vector3(0.18f, 1.9f, 0.588f), new Vector3(0.2f, 2.99f, 0.588f));
            b.Box("Cable clip", new Vector3(0.18f, 2.2f, 0.585f), new Vector3(0.03f, 0.012f, 0.02f), m.Steel);

            // A switch box whose original label has been corrected by hand.
            var switchBox = b.Box("Switch box", new Vector3(0.72f, 1.52f, 0.56f), new Vector3(0.22f, 0.28f, 0.08f), m.UtilityPaint);
            // Keeps the lamp from throwing a large, confusing box shadow across the wall.
            switchBox.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            b.Cylinder("Switch lever", new Vector3(0.72f, 1.57f, 0.5f), 0.008f, 0.07f, m.Steel, new Vector3(60, 0, 0));
            b.Box("Switch tape", new Vector3(0.72f, 1.44f, 0.5195f), new Vector3(0.18f, 0.03f, 0.002f), m.Tape);
            b.Label("SPEAKER", new Vector3(0.72f, 1.44f, 0.5173f), 0.016f, m.Pencil);
            b.Box("Strike", new Vector3(0.72f, 1.44f, 0.517f), new Vector3(0.12f, 0.0025f, 0.001f), m.RedCable);
            b.Label("headset? check", new Vector3(0.73f, 1.405f, 0.5175f), 0.014f, m.RedInk, new Vector3(0, 0, 4));

            // Microphone.
            b.Cylinder("Mic base", new Vector3(0.28f, 0.797f, 0.22f), 0.06f, 0.025f, m.Steel);
            b.Cylinder("Mic stem", new Vector3(0.28f, 0.88f, 0.22f), 0.008f, 0.16f, m.Steel);
            b.Sphere("Mic head", new Vector3(0.28f, 0.98f, 0.21f), new Vector3(0.05f, 0.075f, 0.05f), m.RadioPaint);

            b.Lamp("Desk lamp", new Vector3(0.62f, 0.785f, 0.4f), m.MustardPaint, lit: !night);
            b.Point("Dial glow", new Vector3(-0.4f, 1.04f, 0.12f), new Color(1f, 0.62f, 0.3f), 0.9f, night ? 0.6f : 0.35f, false);
            b.Point("Equipment glow", new Vector3(-0.3f, 1.22f, 0.15f), new Color(0.4f, 0.82f, 0.95f), night ? 1.6f : 1.1f, night ? 0.8f : 0.4f);
            if (!night) b.Point("Room bounce", new Vector3(0.6f, 2.6f, -0.8f), new Color(1f, 0.82f, 0.62f), 5f, 0.35f, false);
            else b.Point("Hall spill", new Vector3(1.8f, 1.6f, -1.2f), new Color(0.55f, 0.62f, 0.8f), 4f, 0.25f);

            if (!night)
            {
                // Care left by a resident: a covered bowl on a folded cloth, and a mug.
                b.Box("Folded cloth", new Vector3(0.05f, 0.787f, -0.02f), new Vector3(0.22f, 0.004f, 0.18f), m.Tablecloth);
                b.Cylinder("Bowl", new Vector3(0.05f, 0.812f, -0.02f), 0.07f, 0.045f, m.CreamEnamel);
                b.Sphere("Bowl cover", new Vector3(0.05f, 0.832f, -0.02f), new Vector3(0.13f, 0.03f, 0.13f), m.Cardboard);
                b.Cylinder("Mug", new Vector3(0.45f, 0.83f, 0.0f), 0.038f, 0.09f, m.RedEnamel);
                b.Box("Mug handle", new Vector3(0.494f, 0.83f, 0.0f), new Vector3(0.012f, 0.05f, 0.015f), m.RedEnamel);
                b.Chair("Desk chair", new Vector3(-0.85f, 0.46f, -0.35f), 160f, m.DarkTimber, m.WeatheredTimber);
            }
            else
            {
                // The same desk, later: lamp off, the bowl gone, the mug on its side, the chair pushed back.
                b.Cylinder("Mug", new Vector3(0.4f, 0.822f, 0.05f), 0.038f, 0.09f, m.RedEnamel, new Vector3(0, 30, 90));
                b.Chair("Desk chair", new Vector3(-0.95f, 0.46f, -0.85f), 205f, m.DarkTimber, m.WeatheredTimber);
            }

            b.Note("Rota", new Vector3(-0.25f, 1.62f, 0.597f), new Vector2(0.26f, 0.3f), m.Paper,
                "CHECK-IN ROTA\n\nMON   IVO\nTUE   REN\nWED   (new)\nTHU   IVO\nFRI   ask", 0.022f, m.Ink, new Vector3(0, 0, 1.5f));
            b.Note("Rule", new Vector3(0.28f, 1.78f, 0.597f), new Vector2(0.3f, 0.14f), m.OldPaper,
                "LOG EVERY CALL.\nINITIAL IT.", 0.024f, m.Ink, new Vector3(0, 0, -2f));
            b.Label("+ and say who hears it", new Vector3(0.3f, 1.67f, 0.5955f), 0.017f, m.RedInk, new Vector3(0, 0, 3f));
            b.Note("Band chart", new Vector3(-1.594f, 1.45f, 0.3f), new Vector2(0.44f, 0.32f), m.Paper,
                "BAND 1   DAY TRAFFIC\nBAND 2   NIGHT / FAR\nBAND 3   (dead since\n         the storm)", 0.02f, m.Ink, new Vector3(0, -90, 0));

            return new Study
            {
                Id = night ? "radio-desk-later" : "radio-desk",
                Root = b.Root,
                Camera = new Vector3(0.45f, 1.45f, -1.35f),
                Target = new Vector3(-0.15f, 1.05f, 0.35f),
                Fov = 52f,
                Ambient = night ? new Color(0.018f, 0.022f, 0.035f) : new Color(0.04f, 0.042f, 0.05f),
            };
        }

        private static Study Kitchen(StyleMaterials m, Vector3 origin)
        {
            var b = new StudyBuilder("Kitchen table", origin, m);
            b.Corner(m.KitchenTiles, m.Plaster, m.WallTiles, 4.4f, 3.6f, 3f, 1.8f, -1.8f);

            b.Box("Table top", new Vector3(0.1f, 0.74f, 0.6f), new Vector3(1.3f, 0.05f, 0.8f), m.Timber);
            foreach (var x in new[] { -0.48f, 0.68f })
            foreach (var z in new[] { 0.27f, 0.93f })
                b.Box("Table leg", new Vector3(x, 0.357f, z), new Vector3(0.06f, 0.715f, 0.06f), m.DarkTimber);
            b.Box("Runner", new Vector3(0.1f, 0.767f, 0.6f), new Vector3(1.0f, 0.004f, 0.36f), m.Tablecloth);
            b.Chair("Chair", new Vector3(-0.68f, 0.46f, 0.6f), -90f, m.Timber, m.Timber);
            b.Chair("Repainted chair", new Vector3(0.95f, 0.46f, 0.3f), 115f, m.TealPaint, m.WeatheredTimber);

            b.Cylinder("Mug", new Vector3(-0.2f, 0.815f, 0.5f), 0.038f, 0.09f, m.RedEnamel);
            b.Cylinder("Mug", new Vector3(0.35f, 0.815f, 0.72f), 0.036f, 0.085f, m.TealPaint);
            b.Cylinder("Plate", new Vector3(0.1f, 0.775f, 0.62f), 0.11f, 0.012f, m.CreamEnamel);
            b.Sphere("Bread", new Vector3(0.08f, 0.8f, 0.62f), new Vector3(0.12f, 0.06f, 0.08f), m.Cardboard);
            b.Note("Dinner note", new Vector3(-0.3f, 0.7705f, 0.78f), new Vector2(0.16f, 0.11f), m.Paper,
                "DINNER 7\nall of us\n(yes, you)", 0.017f, m.Pencil, new Vector3(90, 12, 0));

            // Stove and a kettle whose handle has been re-wrapped.
            b.Box("Stove", new Vector3(-1.2f, 0.425f, 1.45f), new Vector3(0.6f, 0.85f, 0.6f), m.CreamEnamel);
            b.Box("Stove top", new Vector3(-1.2f, 0.86f, 1.45f), new Vector3(0.6f, 0.02f, 0.6f), m.Steel);
            b.Box("Oven door", new Vector3(-1.2f, 0.4f, 1.147f), new Vector3(0.5f, 0.45f, 0.01f), m.CreamEnamel);
            b.Cylinder("Oven handle", new Vector3(-1.2f, 0.66f, 1.13f), 0.01f, 0.4f, m.Steel, new Vector3(0, 0, 90));
            b.Sphere("Kettle", new Vector3(-1.25f, 0.95f, 1.4f), new Vector3(0.22f, 0.17f, 0.22f), m.CreamEnamel);
            b.Cylinder("Kettle spout", new Vector3(-1.12f, 0.97f, 1.4f), 0.014f, 0.12f, m.CreamEnamel, new Vector3(0, 0, -55));
            b.Sphere("Kettle knob", new Vector3(-1.25f, 1.04f, 1.4f), Vector3.one * 0.025f, m.Polymer);
            b.Cable("Kettle handle", m.ClothTape, 0.012f, new Vector3(-1.34f, 0.99f, 1.4f), new Vector3(-1.3f, 1.1f, 1.4f),
                new Vector3(-1.2f, 1.1f, 1.4f), new Vector3(-1.16f, 0.99f, 1.4f));

            // Window at night, with a little cold light falling in.
            foreach (var (pos, size) in new[]
                     {
                         (new Vector3(-0.35f, 1.97f, 1.77f), new Vector3(0.8f, 0.06f, 0.06f)),
                         (new Vector3(-0.35f, 1.13f, 1.77f), new Vector3(0.8f, 0.06f, 0.08f)),
                         (new Vector3(-0.72f, 1.55f, 1.77f), new Vector3(0.06f, 0.9f, 0.06f)),
                         (new Vector3(0.02f, 1.55f, 1.77f), new Vector3(0.06f, 0.9f, 0.06f)),
                         (new Vector3(-0.35f, 1.55f, 1.775f), new Vector3(0.7f, 0.03f, 0.03f)),
                     })
                b.Box("Window frame", pos, size, m.DarkTimber);
            var pane = b.Box("Window pane", new Vector3(-0.35f, 1.55f, 1.795f), new Vector3(0.7f, 0.8f, 0.01f), m.NightWindow);
            pane.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            b.Spot("Night spill", new Vector3(-0.35f, 1.6f, 1.7f), new Vector3(0.3f, 0f, 0.2f), new Color(0.55f, 0.65f, 0.9f), 4f, 55f, 0.45f);

            // Shelf of jars with competing labels.
            b.Box("Shelf", new Vector3(0.9f, 1.55f, 1.69f), new Vector3(0.9f, 0.03f, 0.22f), m.Timber);
            b.Box("Bracket", new Vector3(0.55f, 1.49f, 1.74f), new Vector3(0.03f, 0.1f, 0.12f), m.DarkTimber);
            b.Box("Bracket", new Vector3(1.25f, 1.49f, 1.74f), new Vector3(0.03f, 0.1f, 0.12f), m.DarkTimber);
            var jars = new[] { ("OATS", m.Cardboard), ("BEANS", m.RedEnamel), ("?", m.MustardPaint), ("SALT", m.CreamEnamel) };
            for (var i = 0; i < jars.Length; i++)
            {
                var x = 0.6f + i * 0.2f;
                b.Cylinder("Jar contents", new Vector3(x, 1.605f, 1.69f), 0.044f, 0.08f, jars[i].Item2);
                b.Cylinder("Jar", new Vector3(x, 1.636f, 1.69f), 0.05f, 0.14f, m.Glass);
                b.Cylinder("Jar lid", new Vector3(x, 1.713f, 1.69f), 0.051f, 0.015f, m.Steel);
                b.Box("Jar tape", new Vector3(x, 1.64f, 1.638f), new Vector3(0.07f, 0.025f, 0.002f), m.Tape);
                b.Label(jars[i].Item1, new Vector3(x, 1.64f, 1.636f), 0.013f, m.Pencil);
            }

            // Drawers: the old label crossed out and rewritten by someone else.
            b.Box("Drawer chest", new Vector3(1.6f, 0.4f, 1.55f), new Vector3(0.7f, 0.8f, 0.45f), m.TealPaint);
            foreach (var y in new[] { 0.58f, 0.22f })
            {
                b.Box("Drawer front", new Vector3(1.6f, y, 1.318f), new Vector3(0.62f, 0.3f, 0.02f), m.TealPaint);
                b.Sphere("Drawer knob", new Vector3(1.6f, y, 1.3f), Vector3.one * 0.035f, m.Brass);
            }
            b.Box("Drawer tape", new Vector3(1.6f, 0.68f, 1.3065f), new Vector3(0.16f, 0.03f, 0.002f), m.Tape);
            b.Label("SPOONS", new Vector3(1.6f, 0.68f, 1.3043f), 0.018f, m.Pencil);
            b.Box("Strike", new Vector3(1.6f, 0.68f, 1.304f), new Vector3(0.11f, 0.0025f, 0.001f), m.RedCable);
            b.Label("string, tape, fuses", new Vector3(1.61f, 0.645f, 1.3045f), 0.014f, m.RedInk, new Vector3(0, 0, -3));

            // The kitchen bell on the side wall, with its pull cord.
            b.Box("Bell bracket", new Vector3(-1.72f, 2.02f, 0.9f), new Vector3(0.16f, 0.03f, 0.04f), m.DarkTimber);
            b.Sphere("Bell", new Vector3(-1.64f, 1.95f, 0.9f), new Vector3(0.13f, 0.11f, 0.13f), m.Brass);
            b.Sphere("Clapper", new Vector3(-1.64f, 1.885f, 0.9f), Vector3.one * 0.025f, m.Steel);
            b.Cable("Bell cord", m.RedCable, 0.004f, new Vector3(-1.64f, 1.88f, 0.9f), new Vector3(-1.62f, 1.5f, 0.92f), new Vector3(-1.63f, 1.25f, 0.95f));

            // Pendant light over the table.
            b.Cable("Pendant cord", m.Cable, 0.006f, new Vector3(0.1f, 3f, 0.6f), new Vector3(0.1f, 1.76f, 0.6f));
            b.Cylinder("Pendant shade", new Vector3(0.1f, 1.72f, 0.6f), 0.17f, 0.09f, m.CreamEnamel);
            b.Sphere("Pendant bulb", new Vector3(0.1f, 1.65f, 0.6f), Vector3.one * 0.07f, m.Bulb);
            b.Point("Pendant light", new Vector3(0.1f, 1.6f, 0.6f), new Color(1f, 0.76f, 0.5f), 5.5f, 1.5f);
            b.Point("Room bounce", new Vector3(0.8f, 2.4f, -0.8f), new Color(1f, 0.82f, 0.64f), 5f, 0.3f, false);

            return new Study
            {
                Id = "kitchen-table",
                Root = b.Root,
                Camera = new Vector3(1.0f, 1.55f, -1.25f),
                Target = new Vector3(-0.2f, 1.0f, 1.0f),
                Ambient = new Color(0.06f, 0.052f, 0.046f),
            };
        }

        private static Study Archive(StyleMaterials m, Vector3 origin)
        {
            var b = new StudyBuilder("Archive and workshop", origin, m);
            b.Corner(m.Floorboards, m.Plaster, null, 4.2f, 3.2f, 3f, 1.2f, -1.7f);

            // Shelving of tape boxes with dated, handwritten spines.
            foreach (var x in new[] { -1.62f, -0.5f })
                b.Box("Upright", new Vector3(x, 0.95f, 1.02f), new Vector3(0.04f, 1.9f, 0.32f), m.WeatheredTimber);
            var shelves = new[] { 0.3f, 0.75f, 1.2f, 1.65f };
            var seed = 7;
            foreach (var y in shelves)
            {
                b.Box("Shelf", new Vector3(-1.06f, y, 1.02f), new Vector3(1.08f, 0.025f, 0.32f), m.WeatheredTimber);
                if (y > 1.6f) continue;
                var x = -1.56f;
                while (x < -0.62f)
                {
                    seed = seed * 1103515245 + 12345;
                    var r = (seed >> 8 & 1023) / 1023f;
                    if (r < 0.12f) { x += 0.09f; continue; }
                    var height = 0.17f + r * 0.06f;
                    b.Box("Tape box", new Vector3(x + 0.035f, y + 0.0125f + height * 0.5f, 1.04f), new Vector3(0.068f, height, 0.2f), r > 0.8f ? m.OldPaper : m.Cardboard);
                    if (r > 0.3f)
                    {
                        b.Box("Spine tape", new Vector3(x + 0.035f, y + 0.08f, 0.9385f), new Vector3(0.05f, 0.03f, 0.002f), m.Tape);
                        b.Label("Y" + (3 + (int)(r * 9)) + "\n" + (1 + (int)(r * 40) % 12), new Vector3(x + 0.035f, y + 0.08f, 0.9363f), 0.009f, m.Pencil);
                    }
                    x += 0.075f;
                }
            }
            b.Box("Lying box", new Vector3(-1.2f, 1.7f, 1.04f), new Vector3(0.2f, 0.07f, 0.2f), m.Cardboard, new Vector3(0, 8, 0));

            // Workbench with a reel-to-reel machine.
            b.Box("Bench top", new Vector3(0.45f, 0.88f, 0.85f), new Vector3(1.3f, 0.06f, 0.65f), m.Timber);
            foreach (var x in new[] { -0.15f, 1.05f })
            foreach (var z in new[] { 0.58f, 1.12f })
                b.Box("Bench leg", new Vector3(x, 0.425f, z), new Vector3(0.07f, 0.85f, 0.07f), m.WeatheredTimber);
            b.Box("Bench shelf", new Vector3(0.45f, 0.2f, 0.85f), new Vector3(1.2f, 0.025f, 0.55f), m.WeatheredTimber);
            b.Box("Tape machine", new Vector3(0.2f, 0.98f, 0.85f), new Vector3(0.46f, 0.14f, 0.36f), m.UtilityPaint);
            foreach (var x in new[] { 0.08f, 0.32f })
            {
                b.Cylinder("Reel", new Vector3(x, 1.056f, 0.88f), 0.09f, 0.008f, m.Steel);
                b.Cylinder("Wound tape", new Vector3(x, 1.062f, 0.88f), x < 0.2f ? 0.07f : 0.045f, 0.01f, m.Cable);
                b.Cylinder("Hub", new Vector3(x, 1.07f, 0.88f), 0.018f, 0.02f, m.Polymer);
            }
            b.Box("Tape path", new Vector3(0.2f, 1.062f, 0.77f), new Vector3(0.3f, 0.008f, 0.002f), m.Cable);
            b.Box("Level meter", new Vector3(0.05f, 0.99f, 0.668f), new Vector3(0.07f, 0.035f, 0.004f), m.Dial);
            b.Box("Machine tape", new Vector3(0.28f, 0.99f, 0.6685f), new Vector3(0.14f, 0.025f, 0.002f), m.Tape);
            b.Label("SLOW SPEED ONLY", new Vector3(0.28f, 0.99f, 0.6663f), 0.01f, m.RedInk);
            var coil = new List<Vector3>();
            for (var i = 0; i <= 40; i++)
            {
                var a = i / 40f * Mathf.PI * 2f * 2.5f;
                var radius = 0.1f - i * 0.0005f;
                coil.Add(new Vector3(0.7f + Mathf.Cos(a) * radius, 0.922f + i * 0.0004f, 0.72f + Mathf.Sin(a) * radius));
            }
            b.Cable("Coiled lead", m.RedCable, 0.007f, coil.ToArray());
            foreach (var (y, mat) in new[] { (0.95f, m.MustardPaint), (1.03f, m.RedEnamel), (1.11f, m.UtilityPaint) })
                b.Box("Parts bin", new Vector3(0.95f, y, 0.95f), new Vector3(0.2f, 0.075f, 0.26f), mat);
            b.Lamp("Bench lamp", new Vector3(0.62f, 0.91f, 1.05f), m.RedEnamel, lit: true);

            // Shared tool board with a revised rule.
            b.Box("Pegboard", new Vector3(0.45f, 1.55f, 1.188f), new Vector3(1.2f, 0.8f, 0.02f), m.PegboardPaint);
            b.Cylinder("Screwdriver grip", new Vector3(0.0f, 1.62f, 1.16f), 0.014f, 0.09f, m.RedEnamel);
            b.Cylinder("Screwdriver shaft", new Vector3(0.0f, 1.52f, 1.16f), 0.004f, 0.12f, m.Steel);
            b.Cylinder("Screwdriver grip", new Vector3(0.08f, 1.62f, 1.16f), 0.014f, 0.09f, m.MustardPaint);
            b.Cylinder("Screwdriver shaft", new Vector3(0.08f, 1.54f, 1.16f), 0.004f, 0.09f, m.Steel);
            b.Box("Wrench", new Vector3(0.3f, 1.58f, 1.17f), new Vector3(0.03f, 0.22f, 0.008f), m.Steel, new Vector3(0, 0, 12));
            b.Box("Pliers", new Vector3(0.45f, 1.6f, 1.17f), new Vector3(0.04f, 0.17f, 0.01f), m.UtilityPaint, new Vector3(0, 0, -6));
            b.Cable("Solder", m.Steel, 0.004f, new Vector3(0.62f, 1.72f, 1.17f), new Vector3(0.66f, 1.6f, 1.16f), new Vector3(0.6f, 1.5f, 1.165f), new Vector3(0.68f, 1.42f, 1.17f));
            b.Box("Rule tape", new Vector3(0.45f, 1.87f, 1.1765f), new Vector3(0.5f, 0.04f, 0.002f), m.Tape);
            b.Label("RETURN WHAT YOU BORROW", new Vector3(0.45f, 1.87f, 1.1743f), 0.018f, m.Ink);
            b.Label("or write down who has it", new Vector3(0.48f, 1.83f, 1.174f), 0.016f, m.RedInk, new Vector3(0, 0, -2));

            // Cork board on the side wall with an old roster and a repair sketch.
            b.Box("Cork board", new Vector3(-1.68f, 1.5f, 0.05f), new Vector3(0.02f, 0.62f, 0.9f), m.Cork);
            b.Note("Roster", new Vector3(-1.668f, 1.52f, -0.15f), new Vector2(0.24f, 0.32f), m.OldPaper,
                "WORKSHOP ROSTER\n\nT. ALDER    wed\nB. OKOYE    fri\nJ. VANCE    —\nS. PIRIE    sat", 0.015f, m.Ink, new Vector3(0, -90, -2));
            b.Note("Sketch", new Vector3(-1.668f, 1.45f, 0.25f), new Vector2(0.26f, 0.2f), m.Paper,
                "RELAY BOARD\nA ─┬─ spkr\n  └─ hdst\n(swapped 2x)", 0.017f, m.Pencil, new Vector3(0, -90, 3));

            // Overhead tube and its fixture.
            b.Box("Tube fixture", new Vector3(0.1f, 2.6f, 0.4f), new Vector3(1.2f, 0.05f, 0.14f), m.Steel);
            b.Cylinder("Tube", new Vector3(0.1f, 2.56f, 0.4f), 0.018f, 1.1f, m.Tube, new Vector3(0, 0, 90), 12);
            b.Point("Tube light", new Vector3(0.1f, 2.45f, 0.35f), new Color(0.82f, 0.9f, 1f), 6f, 1.2f);
            b.Point("Room bounce", new Vector3(0.9f, 2f, -1f), new Color(0.75f, 0.8f, 0.9f), 4f, 0.2f, false);
            b.Chair("Stool", new Vector3(0.1f, 0.46f, 0.35f), 200f, m.WeatheredTimber, m.DarkTimber);

            return new Study
            {
                Id = "archive-workshop",
                Root = b.Root,
                Camera = new Vector3(1.05f, 1.55f, -1.3f),
                Target = new Vector3(-0.35f, 1.15f, 0.85f),
                Ambient = new Color(0.045f, 0.048f, 0.055f),
            };
        }

        private static Study Swatches(StyleMaterials m, Vector3 origin)
        {
            var b = new StudyBuilder("Material swatches", origin, m);
            var backdrop = new Material(Shader.Find("Standard")) { color = new Color(0.32f, 0.32f, 0.32f) };
            backdrop.SetFloat("_Glossiness", 0.1f);
            b.Box("Backdrop", new Vector3(0, 1.4f, 0.1f), new Vector3(6f, 3.6f, 0.05f), backdrop);
            var labelInk = m.Ink;
            var library = m.Library.ToList();
            const int columns = 6;
            for (var i = 0; i < library.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var center = new Vector3((col - (columns - 1) * 0.5f) * 0.7f, 2.6f - row * 0.62f, 0f);
                b.Box("Swatch " + library[i].Name, center, new Vector3(0.44f, 0.44f, 0.08f), library[i].Material, new Vector3(0, 0, 0));
                b.Box("Swatch card", center + new Vector3(0, -0.27f, -0.03f), new Vector3(0.66f, 0.075f, 0.004f), m.Paper);
                b.Label(library[i].Name, center + new Vector3(0, -0.27f, -0.0325f), 0.05f, labelInk);
            }
            b.Point("Key", new Vector3(-2f, 3.2f, -2.5f), new Color(1f, 0.95f, 0.88f), 10f, 1.6f);
            b.Point("Fill", new Vector3(2.5f, 1.2f, -2.5f), new Color(0.85f, 0.9f, 1f), 10f, 1.0f, false);
            return new Study
            {
                Id = "material-swatches",
                Root = b.Root,
                Camera = new Vector3(0, 1.3f, -4.2f),
                Target = new Vector3(0, 1.28f, 0),
                Fov = 45f,
                Ambient = new Color(0.12f, 0.12f, 0.12f),
            };
        }

        private static Texture2D PaletteStrip()
        {
            var entries = StylePalette.Entries().ToList();
            const int cell = 64;
            var texture = new Texture2D(cell * entries.Count, cell, TextureFormat.RGB24, false);
            for (var i = 0; i < entries.Count; i++)
            for (var x = 0; x < cell; x++)
            for (var y = 0; y < cell; y++)
                texture.SetPixel(i * cell + x, y, x < 2 ? Color.black : entries[i].Color);
            texture.Apply();
            return texture;
        }

        private static void Save(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 8 };
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                target.Create();
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToJPG(90));
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(pixels);
            }
        }
    }
}
