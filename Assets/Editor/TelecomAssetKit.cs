using UnityEngine;
using static CommonHouse.Editor.OfficeAssetKit;

namespace CommonHouse.Editor
{
    /// <summary>Original presentation props and deterministic print artwork, without simulated circuitry.</summary>
    public static class TelecomAssetKit
    {
        public static readonly Color Lime = new Color(0.65f, 0.95f, 0.08f);
        public static readonly Color Violet = new Color(0.65f, 0.18f, 0.94f);
        private static Material Steel => Surface("telecom-steel", new Color(0.43f, 0.48f, 0.48f), true);
        private static Material Dark => Surface("telecom-ink", Ink);

        public static void Route(Transform parent, string id, params Vector3[] points)
        {
            var route = Group(parent, "Cable route: " + id, Vector3.zero);
            for (var i = 1; i < points.Length; i++)
            {
                var start = points[i - 1];
                var end = points[i];
                var length = Vector3.Distance(start, end);
                var segment = Group(route, "Cable tray segment", (start + end) / 2f);
                segment.localRotation = Quaternion.LookRotation(end - start);
                foreach (var x in new[] { -0.16f, 0.16f })
                    Box(segment, "Tray side rail", new Vector3(x, 0f, 0f), new Vector3(0.025f, 0.07f, length), Steel);
                for (var z = -length / 2f; z <= length / 2f; z += 0.45f)
                    Box(segment, "Tray rung", new Vector3(0f, -0.025f, z), new Vector3(0.32f, 0.025f, 0.035f), Steel);
                for (var wire = 0; wire < 3; wire++)
                    Box(segment, "Cable bundle", new Vector3(-0.08f + wire * 0.08f, 0.035f, 0f), new Vector3(0.035f, 0.035f, length),
                        Surface("telecom-wire-" + wire, wire == 0 ? Teal : wire == 1 ? Yellow : Ink));
            }
        }

        public static void Junction(Transform parent, string id, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Junction: " + id, position, yaw);
            Box(root, "Junction enclosure", Vector3.zero, new Vector3(0.55f, 0.40f, 0.18f), Steel);
            Box(root, "Service lid", new Vector3(0f, 0f, 0.105f), new Vector3(0.48f, 0.33f, 0.025f), Surface("telecom-lid", Bone));
            foreach (var x in new[] { -0.20f, 0.20f })
                Box(root, "Lid fastener", new Vector3(x, 0f, 0.126f), new Vector3(0.03f, 0.03f, 0.01f), Dark);
            Sign(root, "telecom-" + id, new[] { id.ToUpperInvariant() }, new Vector3(0f, 0f, 0.14f), new Vector2(0.34f, 0.13f), Teal);
        }

        public static void PatchPanel(Transform parent, string id, Vector3 position, float yaw = 0f)
        {
            var root = Group(parent, "Patch panel: " + id, position, yaw);
            Box(root, "Panel face", Vector3.zero, new Vector3(0.72f, 0.30f, 0.055f), Steel);
            for (var row = 0; row < 2; row++)
            for (var port = 0; port < 8; port++)
                Box(root, "Patch socket", new Vector3(-0.29f + port * 0.083f, -0.07f + row * 0.14f, 0.035f), new Vector3(0.055f, 0.07f, 0.015f), Dark);
            for (var port = 0; port < 4; port++)
            {
                var x = -0.29f + port * 0.166f;
                var color = port % 2 == 0 ? Lime : Violet;
                // Rectilinear slack loops read at low resolution and finish at occupied sockets.
                PatchLead(root, new Vector3(x, 0.07f, 0.06f), new Vector3(x, 0.07f, 0.18f), color);
                PatchLead(root, new Vector3(x, 0.07f, 0.18f), new Vector3(x, -0.24f, 0.18f), color);
                PatchLead(root, new Vector3(x, -0.24f, 0.18f), new Vector3(x + 0.083f, -0.24f, 0.18f), color);
                PatchLead(root, new Vector3(x + 0.083f, -0.24f, 0.18f), new Vector3(x + 0.083f, -0.07f, 0.06f), color);
            }
        }

        private static void PatchLead(Transform parent, Vector3 start, Vector3 end, Color color)
        {
            var lead = Box(parent, "Patch lead", (start + end) / 2f,
                new Vector3(0.035f, 0.035f, Vector3.Distance(start, end)), Surface(color == Lime ? "telecom-lead-lime" : "telecom-lead-violet", color));
            lead.transform.localRotation = Quaternion.LookRotation(end - start);
        }

        public static void CrossConnect(Transform parent, Vector3 position, float yaw)
        {
            var root = Group(parent, "Former office cross-connect", position, yaw);
            Box(root, "Cabinet body", Vector3.zero, new Vector3(1.3f, 1.7f, 0.25f), Steel, true);
            Box(root, "Open cabinet recess", new Vector3(0f, 0f, 0.14f), new Vector3(1.15f, 1.53f, 0.035f), Dark);
            for (var row = 0; row < 6; row++)
            {
                Box(root, "Terminal block", new Vector3(0f, -0.60f + row * 0.23f, 0.18f), new Vector3(0.98f, 0.13f, 0.06f), Surface("telecom-terminal-block", Bone));
                for (var port = 0; port < 12; port++)
                    Box(root, "Copper termination", new Vector3(-0.44f + port * 0.08f, -0.60f + row * 0.23f, 0.217f), new Vector3(0.025f, 0.07f, 0.02f), Surface("telecom-copper", new Color(0.66f, 0.40f, 0.22f)));
            }
            Route(root, "cabinet loom", new Vector3(-0.55f, -0.65f, 0.22f), new Vector3(-0.55f, 0.82f, 0.22f));
            Sign(root, "telecom-cross-connect", new[] { "LINE", "FRAME" }, new Vector3(0f, 0.95f, 0.18f), new Vector2(0.85f, 0.40f), Teal);
        }

        public static void Print(Transform parent, string id, Vector3 position, Vector2 size, float yaw = 0f)
        {
            var root = Group(parent, "Original print: " + id, position, yaw);
            var material = Surface("print-" + id, Color.white);
            material.SetTexture("_MainTex", Texture("print-" + id, 128, 192, (x, y) => PrintPixel(id, x, y)));
            Box(root, "Printed paper", Vector3.zero, new Vector3(size.x, size.y, 0.012f), material);
            foreach (var x in new[] { -0.35f, 0.35f })
            {
                var tape = Box(root, "Paper tape", new Vector3(x * size.x, size.y * 0.46f, 0.015f), new Vector3(size.x * 0.18f, size.y * 0.06f, 0.01f), Surface("print-tape", new Color(0.69f, 0.65f, 0.46f)));
                tape.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0f ? -12f : 9f);
            }
        }

        private static bool Text(string text, int x, int y, int top, int scale = 2)
        {
            x -= 8;
            var index = x / (6 * scale);
            return x >= 0 && y <= top && y > top - 7 * scale && index < text.Length &&
                LetterPixel(text[index], (x % (6 * scale)) / scale, (top - y) / scale);
        }

        private static Color PrintPixel(string id, int x, int y)
        {
            var listen = id == "keep-listening";
            var repair = id == "patch-share";
            var accent = repair ? Lime : Violet;
            var heading = listen ? "KEEP" : repair ? "PATCH" : "USE IT";
            var second = listen ? "LISTENING" : repair ? "SHARE" : "PASS IT";
            if (Text(heading, x, y, 177) || Text(second, x, y, 157)) return Bone;
            if (y > 140) return Ink;
            // Jagged paper seams, an offset accent strip, and photocopy speckle surround the image.
            if (x < 5 + (y * 7 % 5) || x > 120 - (y * 11 % 4)) return Ink;
            if (y < 18 + x / 9 + (x % 7)) return (x + y) % 9 < 3 ? Ink : accent;
            if (x > 94 - y / 7 && x < 107 - y / 7) return accent;
            var paper = (x * 17 + y * 29) % 71 < 3 ? new Color(0.58f, 0.58f, 0.55f) : Bone;
            // Halftone shadow behind the original hardware silhouette.
            if (x > 18 && x < 111 && y > 35 && y < 126 && (x % 4 < 2 && y % 4 < 2)) paper = Ink;
            if (listen)
            {
                // A salvaged monitor inside a desktop-window frame, with a pointer collage below.
                if (x >= 15 && x <= 88 && y >= 58 && y <= 122)
                {
                    if (x < 19 || x > 84 || y < 62 || y > 118 || y > 111) return Ink;
                    if (y > 68 && y < 105 && x > 25 && x < 78)
                        return y % 9 < 2 && x < 37 + (y * 11 % 35) ? accent : Ink;
                    return Bone;
                }
                if (y > 36 && y < 56 && x > 42 && x < 56 + (y - 36) / 2) return Ink;
                if (y > 39 && y < 61 && x > 73 && x < 73 + (y - 39) / 2) return accent;
            }
            else if (repair)
            {
                // Large patch plug and a stepped cable loop, drawn from simple original linework.
                if (x > 25 && x < 72 && y > 83 && y < 119)
                {
                    if (x < 30 || x > 67 || y < 88 || y > 114) return Ink;
                    return x % 8 < 3 && y > 102 ? Ink : Bone;
                }
                if (x > 35 && x < 62 && y > 74 && y <= 83) return Ink;
                if ((x > 43 && x < 51 && y > 42 && y <= 74) ||
                    (y > 42 && y < 50 && x >= 51 && x < 85) ||
                    (x > 77 && x < 85 && y >= 50 && y < 80)) return Ink;
            }
            else
            {
                // Paired open-ended spanners, with a small offset registration copy.
                var dx = x - 49; var dy = y - 91;
                var r = dx * dx + dy * dy;
                if (r < 380 && r > 145 && !(dy > 5 && Mathf.Abs(dx) < 9)) return Ink;
                if (x > 42 && x < 55 && y > 39 && y < 77) return Ink;
                if (x > 67 && x < 78 && y > 48 && y < 103) return accent;
                if (y > 101 && y < 117 && x > 60 && x < 85 && !(x > 68 && x < 77 && y > 108)) return Ink;
            }
            return paper;
        }
    }
}
