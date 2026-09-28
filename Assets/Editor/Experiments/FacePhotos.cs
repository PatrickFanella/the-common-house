using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// GoldenEye-style face photographs for the retro experiments (issue #67): a flat, frontal
    /// "photo" of a character's detailed face, cropped to an oval and shrunk, then projected onto a
    /// low-poly head with a decal. The photo is a render of our own generated character, not of a
    /// real person; real faces would need the person's consent.
    /// </summary>
    public static class FacePhotos
    {
        public const string Root = "Assets/CommonHouse/Experiments/Faces";
        public const int Width = 64;
        public const int Height = 80;
        // The photographed area, in metres, centred just above the head bone.
        public static readonly Vector2 FaceSize = new Vector2(0.17f, 0.2125f);
        public const float FaceCentreAboveHeadBone = 0.08f;

        public static string PhotoPath(string id) => Root + "/" + id + "_face.png";

        /// <summary>Renders the photo for a character into a new, unsaved scene and saves it.</summary>
        public static Texture2D Capture(string id)
        {
            Directory.CreateDirectory(Root);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var character = (GameObject)PrefabUtility.InstantiatePrefab(CharacterAssets.Prepare(id));
            var head = character.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head)
                       ?? throw new InvalidOperationException(id + " has no head bone");
            var centre = head.position + Vector3.up * FaceCentreAboveHeadBone;

            // Even, frontal light, like a passport photo.
            Light("Front", centre + new Vector3(0f, 0.3f, 1.2f), 250f);
            Light("Left", centre + new Vector3(-1f, 0.1f, 0.8f), 100f);
            Light("Right", centre + new Vector3(1f, 0.1f, 0.8f), 100f);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var exposure = profile.Add<Exposure>(true);
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.value = 7f;
            profile.Add<MotionBlur>(true).intensity.value = 0f;
            // Without this, HDRP's default sky floods the face with bright, bluish ambient light.
            var environment = profile.Add<VisualEnvironment>(true);
            environment.skyType.value = (int)SkyType.Gradient;
            environment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
            var sky = profile.Add<GradientSky>(true);
            sky.top.value = sky.middle.value = sky.bottom.value = new Color(0.25f, 0.24f, 0.23f) * 1.2f * Mathf.Pow(2f, 7f) * 0.05f;
            var volume = new GameObject("Photo volume").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            // Characters from the MPFB pipeline face +Z.
            var camera = new GameObject("Photo camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = FaceSize.y * 0.5f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 3f;
            camera.transform.position = centre + Vector3.forward * 1f;
            camera.transform.LookAt(centre);
            var hd = camera.gameObject.AddComponent<HDAdditionalCameraData>();
            hd.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            hd.backgroundColorHDR = Color.black;

            const int scale = 4;
            var render = new RenderTexture(Width * scale, Height * scale, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var full = new Texture2D(render.width, render.height, TextureFormat.RGBA32, false);
            try
            {
                render.Create();
                camera.targetTexture = render;
                for (var frame = 0; frame < 3; frame++) camera.Render();
                RenderTexture.active = render;
                full.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
                full.Apply();
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                render.Release();
            }
            var photo = Oval(Shrink(full, scale));
            var path = PhotoPath(id);
            var png = photo.EncodeToPNG();
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(png))
            {
                File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>Adds a decal that projects the photo onto a character's face, following its head.</summary>
        public static void Attach(GameObject character, Material decal, Vector3 faceForward)
        {
            var head = character.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;
            var projector = new GameObject("Face photo").AddComponent<DecalProjector>();
            projector.transform.SetParent(head, false);
            var centre = head.position + Vector3.up * FaceCentreAboveHeadBone;
            // Projects along its +Z, from in front of the face back into it.
            projector.transform.position = centre + faceForward * 0.12f;
            projector.transform.rotation = Quaternion.LookRotation(-faceForward, Vector3.up);
            projector.size = new Vector3(FaceSize.x, FaceSize.y, 0.2f);
            projector.pivot = new Vector3(0f, 0f, 0.1f);
            projector.material = decal;
        }

        /// <summary>An HDRP decal material showing the photo, saved beside it.</summary>
        public static Material DecalMaterial(string id, Texture2D photo, FilterMode filter)
        {
            var path = Root + "/" + id + "_face_" + filter.ToString().ToLowerInvariant() + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("HDRP/Decal")) { name = Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }
            photo.filterMode = filter;
            material.SetTexture("_BaseColorMap", photo);
            material.SetFloat("_AffectAlbedo", 1f);
            material.SetFloat("_AffectNormal", 0f);
            material.SetFloat("_AffectMetal", 0f);
            material.SetFloat("_AffectAO", 0f);
            material.SetFloat("_AffectSmoothness", 0f);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Light(string name, Vector3 position, float lumens)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            go.AddComponent<HDAdditionalLightData>();
            light.lightUnit = LightUnit.Lumen;
            light.intensity = lumens;
            light.range = 5f;
            light.shadows = LightShadows.None;
        }

        private static Texture2D Shrink(Texture2D full, int scale)
        {
            var source = full.GetPixels32();
            var result = new Color32[Width * Height];
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                int r = 0, g = 0, b = 0;
                for (var sy = 0; sy < scale; sy++)
                for (var sx = 0; sx < scale; sx++)
                {
                    var p = source[(y * scale + sy) * full.width + x * scale + sx];
                    r += p.r; g += p.g; b += p.b;
                }
                var n = scale * scale;
                result[y * Width + x] = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
            }
            var small = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            small.SetPixels32(result);
            small.Apply();
            return small;
        }

        /// <summary>Keeps an oval around the face, with a one-pixel soft edge.</summary>
        private static Texture2D Oval(Texture2D photo)
        {
            var pixels = photo.GetPixels32();
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var dx = (x + 0.5f - Width * 0.5f) / (Width * 0.46f);
                var dy = (y + 0.5f - Height * 0.47f) / (Height * 0.48f);
                var d = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01((1f - d) * 12f);
                var p = pixels[y * Width + x];
                pixels[y * Width + x] = new Color32(p.r, p.g, p.b, (byte)(alpha * 255));
            }
            photo.SetPixels32(pixels);
            photo.Apply();
            return photo;
        }
    }
}
