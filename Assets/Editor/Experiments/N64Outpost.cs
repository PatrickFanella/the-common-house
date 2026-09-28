using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonHouse.Experiments.N64;
using CommonHouse.Outpost;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor.Experiments
{
    /// <summary>
    /// Experimental N64 / GoldenEye 007 variant of the outpost (issue #67). Builds a separate scene
    /// from the furnished outpost: 64 px bilinear textures without normal or mask maps, no shadows,
    /// flat lighting with fog, and a 5-bit dithered framebuffer. Rendered at 320x240 for review.
    /// </summary>
    public static class N64Outpost
    {
        public const string Root = "Assets/CommonHouse/Experiments/N64";
        public const string ScenePath = Root + "/OutpostN64.unity";
        public const string TextureRoot = Root + "/Textures";
        public const string MaterialRoot = Root + "/Materials";
        public const string ModelRoot = Root + "/Models";
        public const string CharacterRoot = Root + "/Characters";
        // The test resident stands in for the resident capsules in this experiment only.
        private const string StandIn = "test-resident-01";
        private static readonly string[] ResidentCapsules = { "IvoConversation", "RenPrivateConversation" };
        private static Material faceDecal;
        public const string ProfilePath = Root + "/N64VolumeProfile.asset";
        public const string SourcesPath = TextureRoot + "/sources.json";
        public const int TextureSize = 64;
        public const int ScreenWidth = 320;
        public const int ScreenHeight = 240;

        [Serializable]
        private sealed class SourceEntry
        {
            public string texture;
            public string source;
        }

        [Serializable]
        private sealed class SourceList
        {
            public List<SourceEntry> textures = new List<SourceEntry>();
        }

        [MenuItem("Common House/Experiments/Build N64 Outpost")]
        public static void Build()
        {
            RegisterScreen();
            Directory.CreateDirectory(TextureRoot);
            Directory.CreateDirectory(MaterialRoot);
            // The face photo is rendered in its own scene before the outpost is opened.
            var facePhoto = File.Exists(CharacterAssets.FbxPath(StandIn)) ? FacePhotos.Capture(StandIn) : null;
            faceDecal = facePhoto != null ? FacePhotos.DecalMaterial(StandIn, facePhoto, FilterMode.Bilinear) : null;
            var scene = EditorSceneManager.OpenScene(OutpostLayout.ScenePath);
            var variants = new Dictionary<Material, Material>();
            var sources = new SourceList();
            ReplaceProps(variants, sources);
            PlaceResidents(variants, sources);
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => Variant(material, variants, sources)).ToArray();
            }
            // Dimmer lamps against a brighter flat ambient: GoldenEye's lighting was baked and even.
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                light.shadows = LightShadows.None;
                if (light.type != LightType.Directional) light.intensity *= 0.35f;
            }
            var volume = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None).Single(item => item.isGlobal);
            volume.sharedProfile = Profile();
            File.WriteAllText(SourcesPath, JsonUtility.ToJson(sources, true) + "\n");
            AssetDatabase.SaveAssets();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
        }

        /// <summary>Swaps each placed prop for its decimated model, keeping the holder's transform.</summary>
        private static void ReplaceProps(Dictionary<Material, Material> variants, SourceList sources)
        {
            var holders = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item.name.StartsWith("Prop: ", StringComparison.Ordinal)).ToList();
            foreach (var holder in holders)
            {
                var asset = holder.name.Substring("Prop: ".Length);
                var model = N64Model(asset, variants, sources);
                if (model == null || holder.childCount != 1) continue;
                var original = holder.GetChild(0);
                var target = Bounds(original);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, holder);
                // The Blender round trip can change the FBX root's scale and axis conversion, so
                // keep the new prefab's own root, pick the 90-degree correction whose extents best
                // match the original, then line the bounds up with the original's.
                var baseRotation = holder.rotation * model.transform.localRotation;
                // A 90-degree turn is only taken when it clearly beats none: decimation changes the
                // extents slightly, and symmetric objects would otherwise be turned for no reason.
                instance.transform.rotation = baseRotation;
                var best = Quaternion.identity;
                var bestError = (Bounds(instance.transform).size - target.size).sqrMagnitude;
                foreach (var correction in new[] { new Vector3(90, 0, 0), new Vector3(-90, 0, 0), new Vector3(0, 0, 90), new Vector3(0, 0, -90) })
                {
                    instance.transform.rotation = Quaternion.Euler(correction) * baseRotation;
                    var error = (Bounds(instance.transform).size - target.size).sqrMagnitude;
                    if (error < bestError * 0.25f)
                    {
                        bestError = error;
                        best = Quaternion.Euler(correction);
                    }
                }
                instance.transform.rotation = best * baseRotation;
                var placed = Bounds(instance.transform);
                instance.transform.position += new Vector3(target.center.x - placed.center.x, target.min.y - placed.min.y, target.center.z - placed.center.z);
                var fallback = AssetDatabase.LoadAssetAtPath<Material>(OutpostProps.MaterialRoot + "/" + asset + ".mat");
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var materials = renderer.sharedMaterials;
                    for (var i = 0; i < materials.Length; i++)
                        if (materials[i] == null || !AssetDatabase.GetAssetPath(materials[i]).StartsWith(MaterialRoot, StringComparison.Ordinal))
                            materials[i] = Variant(fallback, variants, sources);
                    renderer.sharedMaterials = materials;
                }
                Debug.Log($"[N64Outpost] {asset}: original size {target.size.ToString("F2")}, N64 size {Bounds(instance.transform).size.ToString("F2")}");
                UnityEngine.Object.DestroyImmediate(original.gameObject);
            }
        }

        private static Bounds Bounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        /// <summary>
        /// Stands the decimated test resident where each resident capsule is. The capsules keep
        /// their colliders and dialogue targets; only their meshes are hidden.
        /// </summary>
        private static void PlaceResidents(Dictionary<Material, Material> variants, SourceList sources)
        {
            var path = CharacterRoot + "/" + StandIn + ".fbx";
            if (!File.Exists(path)) return;
            var model = N64Model(path, CharacterAssets.FbxPath(StandIn), variants, sources, humanoid: true);
            foreach (var name in ResidentCapsules)
            {
                var capsule = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single(item => item.name == name);
                capsule.GetComponent<MeshRenderer>().enabled = false;
                var resident = (GameObject)PrefabUtility.InstantiatePrefab(model, capsule.parent);
                resident.name = "N64 resident at " + name;
                // The Blender round trip leaves the decimated character facing -Z, into the room.
                resident.transform.position = new Vector3(capsule.position.x, 0f, capsule.position.z);
                if (faceDecal != null) FacePhotos.Attach(resident, faceDecal, Vector3.back);
            }
        }

        /// <summary>The decimated model for a prop, with its FBX materials remapped to N64 variants.</summary>
        private static GameObject N64Model(string asset, Dictionary<Material, Material> variants, SourceList sources)
        {
            var path = ModelRoot + "/" + asset + ".fbx";
            if (!File.Exists(path)) return null;
            var originalPath = Directory.GetFiles(ScanTextures.PolyHavenRoot + asset, "*.fbx").Single().Replace('\\', '/');
            return N64Model(path, originalPath, variants, sources, humanoid: false);
        }

        /// <summary>Imports a decimated FBX with its materials remapped to N64 variants of the original's.</summary>
        private static GameObject N64Model(string path, string originalPath, Dictionary<Material, Material> variants, SourceList sources, bool humanoid)
        {
            var originalMap = ((ModelImporter)AssetImporter.GetAtPath(originalPath)).GetExternalObjectMap();
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            if (humanoid)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            }
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.addCollider = false;
            importer.bakeAxisConversion = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
            var remaps = new List<(string, Material)>();
            foreach (var name in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>().Select(material => material.name).Distinct().ToList())
            {
                var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (originalMap.TryGetValue(key, out var mapped) && mapped is Material source)
                    remaps.Add((name, Variant(source, variants, sources)));
            }
            AssetDatabase.SaveAssets();
            importer = (ModelImporter)AssetImporter.GetAtPath(path);
            foreach (var (name, material) in remaps)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), material);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        /// <summary>
        /// Adds the N64 screen effect to HDRP's after-post-process list. HDRP 17 keeps that list's
        /// API internal, so edit the serialized global settings instead.
        /// </summary>
        public static void RegisterScreen()
        {
            var settings = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>()
                           ?? throw new InvalidOperationException("Missing HDRP global settings.");
            var type = typeof(N64Screen).AssemblyQualifiedName;
            var serialized = new SerializedObject(settings);
            var found = 0;
            var property = serialized.GetIterator();
            while (property.Next(true))
            {
                if (!property.isArray || property.propertyType == SerializedPropertyType.String) continue;
                if (!property.propertyPath.EndsWith("m_AfterPostProcessCustomPostProcesses.m_CustomPostProcessTypesAsString", StringComparison.Ordinal)) continue;
                found++;
                var present = false;
                for (var i = 0; i < property.arraySize; i++)
                    present |= property.GetArrayElementAtIndex(i).stringValue == type;
                if (present) continue;
                property.InsertArrayElementAtIndex(property.arraySize);
                property.GetArrayElementAtIndex(property.arraySize - 1).stringValue = type;
            }
            if (found == 0) throw new InvalidOperationException("Cannot find HDRP's after-post-process list.");
            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        private static Material Variant(Material source, Dictionary<Material, Material> variants, SourceList sources)
        {
            if (source == null) return null;
            if (variants.TryGetValue(source, out var existing)) return existing;
            var path = MaterialRoot + "/" + Safe(source.name) + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source) { name = source.name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.CopyPropertiesFromMaterial(source);
            }
            var albedo = source.HasProperty("_BaseColorMap") ? source.GetTexture("_BaseColorMap") as Texture2D : null;
            if (albedo != null) material.SetTexture("_BaseColorMap", LowRes(albedo, sources));
            if (material.HasProperty("_NormalMap")) material.SetTexture("_NormalMap", null);
            if (material.HasProperty("_MaskMap")) material.SetTexture("_MaskMap", null);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.05f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            // GoldenEye tiled small textures densely; show each wall and floor texture at 1.6 m.
            if (material.HasProperty("_UVBase") && material.GetFloat("_UVBase") > 4.5f) material.SetFloat("_TexWorldScale", 1f / 1.6f);
            if (material.HasProperty("_MaterialID")) material.SetFloat("_MaterialID", 1f);
            HDMaterial.ValidateMaterial(material);
            EditorUtility.SetDirty(material);
            variants[source] = material;
            return material;
        }

        /// <summary>A 64 px area-averaged copy of a texture's source file, saved as PNG.</summary>
        private static Texture2D LowRes(Texture2D texture, SourceList sources)
        {
            var sourcePath = AssetDatabase.GetAssetPath(texture);
            var target = TextureRoot + "/" + Safe(Path.GetFileNameWithoutExtension(sourcePath)) + "_" + TextureSize + ".png";
            var full = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            try
            {
                if (!full.LoadImage(File.ReadAllBytes(sourcePath))) throw new InvalidOperationException("Cannot decode " + sourcePath);
                var png = Downsample(full, TextureSize).EncodeToPNG();
                if (!File.Exists(target) || !File.ReadAllBytes(target).SequenceEqual(png))
                {
                    File.WriteAllBytes(target, png);
                    AssetDatabase.ImportAsset(target);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(full);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(target);
            if (importer.filterMode != FilterMode.Bilinear || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.alphaIsTransparency != true)
            {
                // Bilinear, as the N64 filtered textures, and uncompressed to keep the small texels exact.
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            if (sources.textures.All(entry => entry.texture != target))
                sources.textures.Add(new SourceEntry { texture = target, source = sourcePath });
            return AssetDatabase.LoadAssetAtPath<Texture2D>(target);
        }

        private static Texture2D Downsample(Texture2D full, int size)
        {
            var source = full.GetPixels32();
            var result = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                int x0 = x * full.width / size, x1 = (x + 1) * full.width / size;
                int y0 = y * full.height / size, y1 = (y + 1) * full.height / size;
                long r = 0, g = 0, b = 0, a = 0, n = 0;
                for (var sy = y0; sy < y1; sy++)
                for (var sx = x0; sx < x1; sx++)
                {
                    var p = source[sy * full.width + sx];
                    r += p.r; g += p.g; b += p.b; a += p.a; n++;
                }
                result[y * size + x] = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), (byte)(a / n));
            }
            var small = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            small.SetPixels32(result);
            small.Apply();
            return small;
        }

        private static VolumeProfile Profile()
        {
            // Rebuilt each time so that edits to these values take effect.
            AssetDatabase.DeleteAsset(ProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            var exposure = profile.Add<Exposure>(true);
            exposure.mode.value = ExposureMode.Fixed;
            exposure.fixedExposure.value = 4f;
            profile.Add<MotionBlur>(true).intensity.value = 0f;
            profile.Add<Bloom>(true).intensity.value = 0f;
            profile.Add<Tonemapping>(true).mode.value = TonemappingMode.None;
            profile.Add<ScreenSpaceAmbientOcclusion>(true).intensity.value = 0f;
            profile.Add<ContactShadows>(true).enable.value = false;
            var environment = profile.Add<VisualEnvironment>(true);
            environment.skyType.value = (int)SkyType.Gradient;
            environment.skyAmbientMode.value = SkyAmbientMode.Dynamic;
            // A brighter, flatter sky ambient stands in for the vertex-lit look.
            var sky = profile.Add<GradientSky>(true);
            sky.top.value = new Color(0.55f, 0.55f, 0.6f);
            sky.middle.value = new Color(0.5f, 0.48f, 0.45f);
            sky.bottom.value = new Color(0.35f, 0.33f, 0.3f);
            sky.exposure.value = 3f;
            var fog = profile.Add<Fog>(true);
            fog.enabled.value = true;
            fog.colorMode.value = FogColorMode.ConstantColor;
            fog.color.value = new Color(0.12f, 0.12f, 0.13f);
            fog.meanFreePath.value = 14f;
            fog.maximumHeight.value = 10f;
            fog.maxFogDistance.value = 60f;
            fog.enableVolumetricFog.value = false;
            var screen = profile.Add<N64Screen>(true);
            screen.enabled.value = true;
            screen.bitsPerChannel.value = 5;
            screen.dither.value = 1f;
            screen.saturation.value = 0.9f;
            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static string Safe(string name)
        {
            var chars = name.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray();
            return new string(chars);
        }

        // ---- Review capture --------------------------------------------------------------------

        /// <summary>Renders the same views from the realistic outpost and the N64 variant.</summary>
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                throw new InvalidOperationException("A graphics device is required.");
            var output = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrEmpty(output) || Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV") != "test")
                throw new InvalidOperationException("Use the disposable verification environment.");
            Directory.CreateDirectory(output);
            RegisterScreen();
            foreach (var (scenePath, style, width, height) in new[]
                     {
                         (OutpostLayout.ScenePath, "realistic", 1280, 960),
                         (ScenePath, "n64", ScreenWidth, ScreenHeight),
                     })
            {
                EditorSceneManager.OpenScene(scenePath);
                var camera = Camera.main ?? throw new InvalidOperationException("Missing camera in " + scenePath);
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 100f;
                foreach (var (name, eye, target) in Views())
                {
                    camera.fieldOfView = 60f;
                    camera.transform.position = eye;
                    camera.transform.LookAt(target);
                    Render(camera, width, height, Path.Combine(output, $"{name}-{style}.png"));
                }
            }
        }

        public static IEnumerable<(string Name, Vector3 Eye, Vector3 Target)> Views()
        {
            yield return ("corridor", new Vector3(-12.5f, 1.6f, 0f), new Vector3(0f, 1.4f, 0f));
            // Close enough to see the face photo on the resident at the workshop bench.
            yield return ("resident", new Vector3(11.2f, 1.65f, 1.1f), new Vector3(11.5f, 1.6f, 2.5f));
            foreach (var view in OutpostEvidence.DetailViews) yield return view;
        }

        /// <summary>Renders at the given size; small frames are also saved scaled up with bilinear filtering.</summary>
        private static void Render(Camera camera, int width, int height, string path)
        {
            var previous = RenderTexture.active;
            var frame = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var display = width < 1280 ? new RenderTexture(1280, 960, 0, RenderTextureFormat.ARGB32) : null;
            try
            {
                frame.Create();
                camera.targetTexture = frame;
                for (var i = 0; i < 3; i++) camera.Render();
                Save(frame, path);
                if (display != null)
                {
                    // The N64's video output softened the image; bilinear scaling approximates it.
                    frame.filterMode = FilterMode.Bilinear;
                    display.Create();
                    Graphics.Blit(frame, display);
                    Save(display, path.Replace(".png", "-display.png"));
                }
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                frame.Release();
                UnityEngine.Object.DestroyImmediate(frame);
                if (display != null)
                {
                    display.Release();
                    UnityEngine.Object.DestroyImmediate(display);
                }
            }
        }

        private static void Save(RenderTexture texture, string path)
        {
            RenderTexture.active = texture;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
