using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static CommonHouse.Editor.OfficeAssetKit;

namespace CommonHouse.Editor
{
    /// <summary>Modern office shell and a provisional household conversion of its ground floor.</summary>
    public static class OfficeComplex
    {
        private static Material Wall => Surface("pale-masonry", Bone, true);
        private static Material Metal => Surface("steel", Ink);
        private static Material Glass => Surface("faux-glass", new Color(0.28f, 0.32f, 0.39f), true);
        private static Material Carpet => Surface("office-carpet", new Color(0.32f, 0.35f, 0.35f), true);

        public static void Build(Transform root)
        {
            MaterialsAndOpenings(root);
            var conversion = Group(root, "OfficeConversion", Vector3.zero);
            var corridor = Room(conversion, "OfficeConcourse", new Vector3(0f, 0f, -6f), new Vector2(28f, 4f));
            var workshop = Room(conversion, "RepairHackerspace", new Vector3(-9.5f, 0f, -12f), new Vector2(9f, 8f));
            var commons = Room(conversion, "ReceptionCommons", new Vector3(0f, 0f, -12f), new Vector2(10f, 8f));
            var sleeping = Room(conversion, "CommunalSleeping", new Vector3(9.5f, 0f, -10f), new Vector2(9f, 4f));
            var utility = Room(conversion, "UtilityRoom", new Vector3(9.5f, 0f, -14f), new Vector2(9f, 4f));

            Box(conversion, "OfficeWestBoundary", new Vector3(-14f, 1.5f, -10f), new Vector3(0.25f, 3f, 12f), Wall, true);
            Box(conversion, "OfficeEastBoundary", new Vector3(14f, 1.5f, -10f), new Vector3(0.25f, 3f, 12f), Wall, true);
            Box(conversion, "OfficeFrontBoundary", new Vector3(0f, 1.5f, -16f), new Vector3(28f, 3f, 0.25f), Glass, true);
            DoorWall(conversion, "SouthOfficeDoors", -8f);
            foreach (var x in new[] { -5f, 5f })
                Box(conversion, "FormerOfficePartition", new Vector3(x, 1.5f, -12f), new Vector3(0.16f, 3f, 8f), Wall, true);
            // Utilities occupy a former small server room beyond the sleeping meeting-room bay.
            SplitDoor(conversion, "ServiceRoomDoor", -12f, 5f, 14f, 9.5f);

            OfficeDetails(conversion);
            FurnishWorkshop(workshop);
            FurnishCommons(commons);
            FurnishSleeping(sleeping);
            FurnishUtility(utility);
            FurnishConcourse(corridor);
            Exterior(root);
            AddLamp(conversion, "Office corridor light", new Vector3(0f, 2.65f, -6f), 900f, Bone, 18f);
            AddLamp(conversion, "Workshop task light", new Vector3(-9.5f, 2.4f, -12f), 1400f, new Color(0.68f, 0.91f, 0.90f), 9f);
            AddLamp(conversion, "Commons warm light", new Vector3(0f, 2.4f, -12f), 1100f, new Color(1f, 0.78f, 0.54f), 9f);
            AddLamp(conversion, "Service wing light", new Vector3(9.5f, 2.5f, -12f), 900f, new Color(0.86f, 0.90f, 1f), 8f);
        }

        private static void MaterialsAndOpenings(Transform root)
        {
            UnityEngine.Object.DestroyImmediate(root.Find("SouthWall").gameObject);
            DoorWall(root, "NorthOfficeDoors", -4f);
        }

        private static Transform Room(Transform parent, string name, Vector3 center, Vector2 size)
        {
            var room = Group(parent, name, Vector3.zero);
            Box(room, "Office floor", center + Vector3.down * 0.15f, new Vector3(size.x, 0.3f, size.y), Carpet, true);
            Box(room, "Office suspended ceiling", center + Vector3.up * 3.12f, new Vector3(size.x, 0.24f, size.y), Wall);
            // Ceiling tile grid, interrupted by the original fluorescent fixtures.
            for (var x = center.x - size.x / 2f + 1.5f; x < center.x + size.x / 2f; x += 1.5f)
                Box(room, "Ceiling grid rail", new Vector3(x, 2.98f, center.z), new Vector3(0.025f, 0.025f, size.y), Surface("ceiling-rail", new Color(0.54f, 0.58f, 0.58f)));
            for (var z = center.z - size.y / 2f + 1.5f; z < center.z + size.y / 2f; z += 1.5f)
                Box(room, "Ceiling grid rail", new Vector3(center.x, 2.98f, z), new Vector3(size.x, 0.025f, 0.025f), Surface("ceiling-rail", new Color(0.54f, 0.58f, 0.58f)));
            return room;
        }

        private static void DoorWall(Transform parent, string name, float z)
        {
            var start = -14f;
            foreach (var door in new[] { -9.5f, 0f, z == -4f ? 11.5f : 9.5f })
            {
                Segment(parent, name, start, door - 1.3f, z);
                Box(parent, name + " lintel", new Vector3(door, 2.75f, z), new Vector3(2.6f, 0.5f, 0.25f), Wall, true);
                foreach (var side in new[] { -1.3f, 1.3f })
                    Box(parent, "Former office door frame", new Vector3(door + side, 1.25f, z), new Vector3(0.06f, 2.5f, 0.32f), Metal);
                start = door + 1.3f;
            }
            Segment(parent, name, start, 14f, z);
        }

        private static void SplitDoor(Transform parent, string name, float z, float left, float right, float door)
        {
            Segment(parent, name, left, door - 1.3f, z);
            Segment(parent, name, door + 1.3f, right, z);
            Box(parent, name + " lintel", new Vector3(door, 2.75f, z), new Vector3(2.6f, 0.5f, 0.16f), Wall, true);
        }

        private static void Segment(Transform parent, string name, float left, float right, float z)
        {
            Box(parent, name, new Vector3((left + right) / 2f, 1.5f, z), new Vector3(right - left, 3f, 0.25f), Wall, true);
        }

        private static void OfficeDetails(Transform root)
        {
            // Mullions and a low sill retain the broad curtain-wall rhythm of the supplied reference.
            for (var x = -13f; x <= 13f; x += 2f)
                Box(root, "Curtain wall mullion", new Vector3(x, 1.5f, -15.83f), new Vector3(0.07f, 3f, 0.09f), Wall);
            Box(root, "Curtain wall sill", new Vector3(0f, 0.45f, -15.8f), new Vector3(28f, 0.12f, 0.12f), Wall);
            Box(root, "Curtain wall transom", new Vector3(0f, 2.5f, -15.8f), new Vector3(28f, 0.06f, 0.10f), Wall);
            // Practical surface wiring sits above door height, leaving walking routes clear.
            Cable(root, new Vector3(-13.5f, 2.55f, -7.75f), new Vector3(13.5f, 2.55f, -7.75f), Yellow);
            foreach (var x in new[] { -9.5f, 0f, 9.5f })
            {
                Box(root, "Floor wayfinding tape", new Vector3(x, 0.015f, -6f), new Vector3(0.10f, 0.015f, 3.6f), Surface("wayfinding", Teal));
                Sign(root, "north-" + x, new[] { x < 0f ? "RADIO" : x > 0f ? "ARCHIVE" : "KITCHEN" }, new Vector3(x > 0f ? 11.5f : x, 2.67f, -4.15f), new Vector2(1.7f, 0.35f), Teal, 180f);
                Sign(root, "south-" + x, new[] { x < 0f ? "REPAIR" : x > 0f ? "QUIET" : "COMMON" }, new Vector3(x, 2.67f, -7.85f), new Vector2(1.7f, 0.35f), x > 0f ? Pink : Yellow);
            }
        }

        private static void FurnishWorkshop(Transform room)
        {
            var desk = Desk(room, new Vector3(-11.4f, 0f, -14.3f));
            Terminal(desk, new Vector3(-0.6f, 0.85f, 0.1f), 180f);
            Terminal(desk, new Vector3(0.6f, 0.85f, 0.1f), 168f);
            Toolboard(room, new Vector3(-11.4f, 1.9f, -15.6f));
            Rack(room, new Vector3(-6.1f, 0f, -14.5f), 180f);
            var repair = Desk(room, new Vector3(-6.4f, 0f, -10.5f), 90f);
            Box(repair, "Repair housing", new Vector3(0f, 1.0f, 0f), new Vector3(0.6f, 0.30f, 0.4f), Surface("repair-shell", Bone));
            for (var i = 0; i < 5; i++)
                Box(repair, "Loose component tray", new Vector3(-0.8f + i * 0.35f, 0.88f, -0.28f), new Vector3(0.20f, 0.04f, 0.15f), Surface("circuit", Teal));
            Sign(room, "repair-print", new[] { "FIX SHARE", "REPEAT" }, new Vector3(-13.82f, 1.9f, -12f), new Vector2(2.8f, 1.4f), Pink, 90f);
            Graphic(room, "repair-wall", new Vector3(-13.85f, 1.7f, -12f), new Vector2(3.6f, 2.3f), 90f);
            for (var i = 0; i < 4; i++)
            {
                var shelf = new Vector3(-12.8f, 0.2f + i * 0.42f, -9.2f);
                Box(room, "Parts shelf", shelf, new Vector3(1.5f, 0.06f, 0.60f), Metal);
                Box(room, "Parts bin", shelf + Vector3.up * 0.14f, new Vector3(1.2f, 0.22f, 0.48f), Surface("parts-bin", new Color(0.45f, 0.37f, 0.27f), true));
            }
            Box(room, "Workshop floor patch", new Vector3(-9.5f, 0.013f, -12.5f), new Vector3(2.5f, 0.012f, 2.5f), Surface("workshop-rug", new Color(0.21f, 0.31f, 0.30f), true));
        }

        private static void FurnishCommons(Transform room)
        {
            var desk = Group(room, "Converted reception counter", new Vector3(3.1f, 0f, -14.6f));
            Box(desk, "Reception base", new Vector3(0f, 0.55f, 0f), new Vector3(2.4f, 1.1f, 0.85f), Wall, true);
            Box(desk, "Plywood counter", new Vector3(0f, 1.13f, 0f), new Vector3(2.6f, 0.12f, 1f), Surface("plywood", new Color(0.58f, 0.40f, 0.24f), true));
            Sign(desk, "welcome", new[] { "COMMON", "HOUSE" }, new Vector3(0f, 0.65f, 0.44f), new Vector2(1.5f, 0.7f), Teal);
            Sofa(room, new Vector3(-2.8f, 0f, -13.7f), -90f);
            Sofa(room, new Vector3(2.8f, 0f, -10.3f), 90f);
            Box(room, "Low communal table", new Vector3(0f, 0.30f, -12f), new Vector3(1.4f, 0.55f, 1.0f), Surface("plywood", new Color(0.58f, 0.40f, 0.24f), true), true);
            Box(room, "Commons rug", new Vector3(0f, 0.012f, -12f), new Vector3(4.7f, 0.012f, 4.0f), Surface("commons-rug", new Color(0.35f, 0.27f, 0.25f), true));
            Sign(room, "shared-print", new[] { "MAKE ROOM", "FOR EACH", "OTHER" }, new Vector3(-4.85f, 1.8f, -10.5f), new Vector2(2.3f, 1.5f), Yellow, 90f);
            Sign(room, "office-directory", new[] { "SHARED", "SPACES" }, new Vector3(4.85f, 1.7f, -12f), new Vector2(1.8f, 1.1f), Teal, -90f);
            Graphic(room, "commons-wall", new Vector3(-1.1f, 1.75f, -15.7f), new Vector2(3.1f, 1.8f));
        }

        private static void FurnishSleeping(Transform room)
        {
            Cot(room, new Vector3(6.6f, 0f, -10f));
            Cot(room, new Vector3(12.3f, 0f, -10f));
            Locker(room, new Vector3(7.9f, 0f, -11.5f), 180f);
            Locker(room, new Vector3(11f, 0f, -11.5f), 180f);
            // Portable dividers suggest shared privacy without assigning any bed to a character.
            Box(room, "Sleeping privacy divider", new Vector3(7.5f, 1.0f, -9.8f), new Vector3(0.08f, 2f, 1.8f), Surface("privacy-fabric", new Color(0.32f, 0.41f, 0.43f), true), true);
            Sign(room, "quiet-print", new[] { "QUIET", "PLEASE" }, new Vector3(13.82f, 1.9f, -10f), new Vector2(1.6f, 0.8f), Pink, -90f);
        }

        private static void FurnishUtility(Transform room)
        {
            Rack(room, new Vector3(6.1f, 0f, -14.8f), 180f);
            Rack(room, new Vector3(7.2f, 0f, -14.8f), 180f);
            Locker(room, new Vector3(12.8f, 0f, -14.8f), 180f);
            var batteries = Group(room, "Salvaged battery bank", new Vector3(11.1f, 0f, -14.8f));
            for (var i = 0; i < 3; i++)
            {
                Box(batteries, "Battery casing", new Vector3(0f, 0.23f + i * 0.45f, 0f), new Vector3(0.95f, 0.4f, 0.6f), Metal, true);
                Box(batteries, "Battery caution strip", new Vector3(0f, 0.23f + i * 0.45f, 0.31f), new Vector3(0.7f, 0.04f, 0.01f), Surface("caution", Yellow));
            }
            Sign(room, "power-print", new[] { "POWER", "IS SHARED" }, new Vector3(13.82f, 1.9f, -13.6f), new Vector2(1.8f, 1f), Yellow, -90f);
            Cable(room, new Vector3(6f, 2.7f, -15.5f), new Vector3(13f, 2.7f, -15.5f), Yellow);
        }

        private static void FurnishConcourse(Transform room)
        {
            var board = Group(room, "Shared notice wall", new Vector3(-4.1f, 1.7f, -7.75f));
            Box(board, "Notice backing", Vector3.zero, new Vector3(1.3f, 1.1f, 0.04f), Surface("plywood", new Color(0.58f, 0.40f, 0.24f), true));
            Sign(board, "notice", new[] { "TAKE PART", "TAKE CARE" }, new Vector3(0f, 0f, 0.035f), new Vector2(1.2f, 0.9f), Pink);
            Locker(room, new Vector3(-12.7f, 0f, -7.3f), 180f);
            Box(room, "Closed upper-floor access", new Vector3(12.9f, 1.2f, -7.75f), new Vector3(1.5f, 2.4f, 0.04f), Metal);
            Sign(room, "upper-access", new[] { "UPPER", "FLOORS" }, new Vector3(12.9f, 1.6f, -7.71f), new Vector2(1.25f, 0.65f), Yellow);
        }

        private static void Exterior(Transform parent)
        {
            var root = Group(parent, "OfficeExteriorMassing", Vector3.zero);
            // The retro shader has no directional sunlight. Brighter facade colors stand in for
            // outdoor illumination without adding unshadowed lights to the occupied rooms.
            var exteriorWall = Surface("exterior-masonry", new Color(3.2f, 3.3f, 3.2f), true);
            var exteriorGlass = Surface("exterior-glass", new Color(1.0f, 0.88f, 0.98f), true);
            // Upper floors are a closed shell, not implied playable rooms.
            Box(root, "Upper office volume", new Vector3(0f, 11.7f, -6f), new Vector3(28f, 17f, 20f), Wall);
            for (var floor = 1; floor <= 5; floor++)
            {
                var y = 1.6f + floor * 3.2f;
                Box(root, "Upper curtain glass band", new Vector3(0f, y, -16.15f), new Vector3(28f, 2.7f, 0.10f), Glass);
                Box(root, "Horizontal facade ribbon", new Vector3(0f, y - 1.45f, -16.22f), new Vector3(28.1f, 0.25f, 0.13f), Wall);
            }
            for (var x = -14f; x <= 14f; x += 1.4f)
                Box(root, "Facade mullion", new Vector3(x, 10f, -16.25f), new Vector3(0.045f, 20f, 0.09f), Surface("facade-aluminium", new Color(0.64f, 0.68f, 0.70f)));
            foreach (var x in new[] { -16.2f, 16.2f })
            {
                Box(root, "Pale masonry wing", new Vector3(x, 11f, -5f), new Vector3(4.2f, 22f, 22f), Wall);
                // Dark inset is framed by projecting white piers and a deep overhead beam.
                Box(root, "Recessed vertical glazing", new Vector3(x, 10.5f, -16.02f), new Vector3(2.6f, 20f, 0.08f), Glass);
                foreach (var side in new[] { -1.8f, 1.8f })
                    Box(root, "Entrance recess pier", new Vector3(x + side, 11f, -17f), new Vector3(0.6f, 22f, 2f), Wall);
                Box(root, "Entrance recess crown", new Vector3(x, 21.5f, -17f), new Vector3(4.2f, 1f, 2f), Wall);
            }
            // Repeated vertical fins along the side wing echo the supplied reference's quieter facade.
            for (var z = -14f; z <= 3f; z += 1.7f)
            {
                Box(root, "East vertical glazing slot", new Vector3(18.32f, 9.5f, z + 0.65f), new Vector3(0.03f, 18f, 0.85f), Glass);
                Box(root, "East facade fin", new Vector3(18.5f, 9.5f, z), new Vector3(0.7f, 18f, 0.22f), Wall);
            }
            Box(root, "Roof service parapet", new Vector3(0f, 20.65f, 3.8f), new Vector3(28f, 0.8f, 0.3f), Wall);
            Box(root, "Roof radio mast", new Vector3(-8f, 23f, 0f), new Vector3(0.10f, 5.5f, 0.10f), Metal);
            Box(root, "Mast crossbar", new Vector3(-8f, 24.5f, 0f), new Vector3(3f, 0.08f, 0.08f), Metal);
            Box(root, "Reused entrance canopy", new Vector3(0f, 3.0f, -17.2f), new Vector3(6f, 0.18f, 2.3f), Wall);
            Sign(root, "exterior-house", new[] { "COMMON", "HOUSE" }, new Vector3(0f, 2f, -16.3f), new Vector2(3.5f, 1.1f), Teal);
            // A landing is visual context only; the ground-floor curtain wall remains a collision boundary.
            Box(root, "Entrance landing", new Vector3(0f, -0.12f, -18f), new Vector3(38f, 0.2f, 5f), Surface("landing", new Color(0.49f, 0.52f, 0.51f), true));
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == Wall) renderer.sharedMaterial = exteriorWall;
                else if (renderer.sharedMaterial == Glass) renderer.sharedMaterial = exteriorGlass;
            }
        }

        private static void AddLamp(Transform parent, string name, Vector3 position, float lumens, Color color, float range)
        {
            var root = Group(parent, name, position);
            Box(root, "Reused light strip", Vector3.zero, new Vector3(1.5f, 0.08f, 0.2f), Surface("fixture", Bone));
            var light = root.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            root.gameObject.AddComponent<HDAdditionalLightData>();
            light.lightUnit = UnityEngine.Rendering.LightUnit.Lumen;
            light.intensity = lumens;
            light.range = range;
            light.color = color;
            light.shadows = LightShadows.None;
        }
    }
}
