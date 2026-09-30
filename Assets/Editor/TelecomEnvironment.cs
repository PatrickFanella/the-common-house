using UnityEngine;
using static CommonHouse.Editor.OfficeAssetKit;
using static CommonHouse.Editor.TelecomAssetKit;

namespace CommonHouse.Editor
{
    public static class TelecomEnvironment
    {
        public static void Build(Transform conversion)
        {
            var root = Group(conversion, "TelecommunicationsAdaptation", Vector3.zero);
            Route(root, "service trunk", new Vector3(6.1f, 2.65f, -14.8f), new Vector3(6.1f, 2.65f, -7.35f),
                new Vector3(-11.4f, 2.65f, -7.35f));
            Route(root, "repair branch", new Vector3(-11.4f, 2.65f, -7.35f), new Vector3(-11.4f, 2.65f, -14.1f),
                new Vector3(-11.4f, 1.05f, -14.1f));
            Route(root, "radio branch", new Vector3(-11.4f, 2.65f, -7.35f), new Vector3(-11.4f, 2.65f, -2.8f),
                new Vector3(-11.4f, 1.15f, -2.8f));
            Route(root, "service drop", new Vector3(6.1f, 2.65f, -14.8f), new Vector3(6.1f, 1.92f, -14.8f));
            // Finish the repair drop at an equipment box on the back edge of the existing bench.
            Junction(root, "repair", new Vector3(-11.4f, 1.05f, -14.1f));
            Junction(root, "radio", new Vector3(-11.4f, 1.15f, -2.8f));
            Junction(root, "trunk", new Vector3(6.1f, 2.6f, -7.35f), 180f);
            PatchPanel(root, "utility upper", new Vector3(6.1f, 1.6f, -14.42f));
            PatchPanel(root, "utility lower", new Vector3(6.1f, 1.05f, -14.42f));
            CrossConnect(root, new Vector3(13.68f, 1.2f, -14.5f), -90f);
            // Keep the existing household sign clear of the new wall cabinet.
            var powerSign = conversion.Find("UtilityRoom/Sign: power-print");
            powerSign.localPosition = new Vector3(13.82f, 1.9f, -12.9f);
            Route(root, "line frame feed", new Vector3(6.1f, 2.65f, -14.8f), new Vector3(13.5f, 2.65f, -14.8f),
                new Vector3(13.5f, 2.2f, -14.8f), new Vector3(13.5f, 2.2f, -14.5f));
            Sign(root, "telecom-service", new[] { "SERVICE", "LINES" }, new Vector3(6.1f, 2.2f, -14.40f), new Vector2(0.85f, 0.42f), Teal);
            // Prints sit on usable wall patches, with independent motifs and no case information.
            Print(root, "patch-share", new Vector3(-13.78f, 1.7f, -10f), new Vector2(1.05f, 1.58f), 90f);
            Print(root, "keep-listening", new Vector3(-6.9f, 1.7f, -7.70f), new Vector2(1.0f, 1.50f));
            Print(root, "use-pass", new Vector3(4.78f, 1.6f, -10.0f), new Vector2(0.90f, 1.35f), -90f);
        }
    }
}
