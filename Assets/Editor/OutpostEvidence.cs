using System;
using System.IO;
using System.Linq;
using CommonHouse.Outpost;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CommonHouse.Editor
{
    // Explicit graphical evidence operation in a disposable project. Does not save the scene.
    public static class OutpostEvidence
    {
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required for rendered evidence.");
            var output = System.Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || System.Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            var layout = UnityEngine.Object.FindFirstObjectByType<OutpostLayout>();
            if (layout == null) throw new InvalidOperationException("Missing outpost layout.");
            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("Missing scene camera.");
            camera.fieldOfView = 75;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100;
            var ceilings = layout.GetComponentsInChildren<Renderer>().Where(r => r.name == "Ceiling").ToArray();
            foreach (var ceiling in ceilings) ceiling.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 9;
            camera.transform.position = new Vector3(0, 24, -14);
            camera.transform.LookAt(new Vector3(0, 0, 0));
            Save(camera, Path.Combine(output, "outpost-cutaway.png"));
            foreach (var ceiling in ceilings) ceiling.enabled = true;
            camera.orthographic = false;

            // Eye-level views of each furnished area, for reviewing props and their lighting.
            camera.fieldOfView = 60;
            foreach (var (name, eye, target) in DetailViews)
            {
                camera.transform.position = eye;
                camera.transform.LookAt(target);
                Save(camera, Path.Combine(output, "outpost-view-" + name + ".png"));
            }
            camera.fieldOfView = 75;

            foreach (var existing in layout.GetComponentsInChildren<CharacterController>()) existing.enabled = false;
            var probe = new GameObject("Verification traversal probe");
            var controller = probe.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = Vector3.up * 0.9f;
            controller.skinWidth = 0.02f;
            probe.transform.position = new Vector3(-10, 0.05f, 0);
            Physics.SyncTransforms();
            var observations = new System.Collections.Generic.List<string>();
            var checkpoints = new[] { -10f, 0f, 10f, 0f, -10f };
            for (var index = 0; index < checkpoints.Length; index++)
            {
                var target = new Vector3(checkpoints[index], probe.transform.position.y, 0);
                var count = 0;
                while (Mathf.Abs(probe.transform.position.x - target.x) > 0.06f && count++ < 240)
                {
                    var direction = Mathf.Sign(target.x - probe.transform.position.x);
                    controller.Move(new Vector3(direction * 0.1f, -0.02f, 0));
                    Physics.SyncTransforms();
                }
                if (Mathf.Abs(probe.transform.position.x - target.x) > 0.1f)
                    throw new InvalidOperationException("Traversal probe blocked before checkpoint " + index);
                camera.transform.position = probe.transform.position + Vector3.up * 1.65f;
                camera.transform.rotation = Quaternion.Euler(0, index < 2 ? 90 : -90, 0);
                Save(camera, Path.Combine(output, "outpost-traverse-" + index + ".png"));
                observations.Add("checkpoint " + index + ": " + probe.transform.position.ToString("F3") + "; moves=" + count);
            }
            // Travel toward the exterior east wall and observe that collision holds the probe inside.
            for (var step = 0; step < 400; step++) controller.Move(new Vector3(0.1f, -0.02f, 0));
            if (probe.transform.position.x > 13.8f || probe.transform.position.x < 13f)
                throw new InvalidOperationException("Exterior boundary did not stop the traversal probe at the wall.");
            observations.Add("east wall stops probe at " + probe.transform.position.ToString("F3"));
            File.WriteAllLines(Path.Combine(output, "traversal-observation.txt"), observations);
            File.WriteAllText(Path.Combine(output, "graphics-device.txt"), SystemInfo.graphicsDeviceName + "\n" + SystemInfo.graphicsDeviceVersion + "\n");
            UnityEngine.Object.DestroyImmediate(probe);
        }

        public static readonly (string Name, Vector3 Eye, Vector3 Target)[] DetailViews =
        {
            ("radio-desk", new Vector3(-9.6f, 1.6f, 0.4f), new Vector3(-10.2f, 0.9f, -2.6f)),
            ("kitchen", new Vector3(2.8f, 1.6f, -1.4f), new Vector3(0.3f, 0.9f, 1.8f)),
            ("archive-shelf", new Vector3(8.4f, 1.6f, 0.6f), new Vector3(9f, 1f, -3f)),
            ("workshop", new Vector3(9.6f, 1.6f, -0.6f), new Vector3(10.2f, 1f, 3f)),
        };

        private static void Save(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                target.Create();
                camera.targetTexture = target;
                // HDRP applies exposure from the previous frame; the first render only primes it.
                camera.Render();
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
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
