using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonHouse.Look;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Applies the chosen look (decision D-24) to the generated outpost: PS1 surfaces through the
    /// CommonHouse/Retro shader, 64 px textures, decimated props, vertex lighting and fog, and a
    /// PS2-era frame: 448 lines, haze, and a light colour-depth dither.
    /// </summary>
    public static class RetroLook
    {
        public const string Root = "Assets/CommonHouse/Art/Retro";
        public const string ModelRoot = Root + "/Models";
        public const string TextureRoot = Root + "/Textures";
        public const string MaterialRoot = Root + "/Materials";
        public const string ProfilePath = Root + "/RetroVolumeProfile.asset";
        public const string SourcesPath = TextureRoot + "/sources.json";
        public const int TextureSize = 64;
        private const string ShaderName = "CommonHouse/Retro";
        // Wall, floor, and ceiling textures repeat at this size, densely, as those games tiled them.
        private const float WorldTileMetres = 1.6f;

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

        /// <summary>Registers the post-processes and enables dynamic resolution in the HDRP asset.</summary>
        public static void Configure()
        {
            RegisterPostProcesses(typeof(HazeFilter), typeof(ColorDepth));
            var asset = GraphicsSettings.defaultRenderPipeline as HDRenderPipelineAsset
                        ?? throw new InvalidOperationException("The default render pipeline is not HDRP.");
            var settings = asset.currentPlatformRenderPipelineSettings;
            var resolution = settings.dynamicResolutionSettings;
            resolution.enabled = true;
            resolution.dynResType = DynamicResolutionType.Software;
            // Catmull-Rom is HDRP's cheap, soft filter, close to the PS2's soft video output.
            resolution.upsampleFilter = DynamicResUpscaleFilter.CatmullRom;
            resolution.defaultInjectionPoint = DynamicResolutionHandler.UpsamplerScheduleType.AfterPost;
            resolution.minPercentage = 10f;
            resolution.maxPercentage = 100f;
            resolution.forceResolution = false;
            settings.dynamicResolutionSettings = resolution;
            asset.currentPlatformRenderPipelineSettings = settings;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Converts the generated outpost below <paramref name="root"/> to the look.</summary>
        public static void Apply(Transform root)
        {
            var shader = Shader.Find(ShaderName) ?? throw new InvalidOperationException("Missing shader " + ShaderName);
            Directory.CreateDirectory(TextureRoot);
            Directory.CreateDirectory(MaterialRoot);
            var variants = new Dictionary<Material, Material>();
            var sources = new SourceList();

            ReplaceProps(root, shader, variants, sources);
            PlaceResidents(root, shader);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => Variant(material, shader, variants, sources)).ToArray();
            }
            foreach (var light in root.GetComponentsInChildren<Light>(true)) light.shadows = LightShadows.None;

            var lighting = new GameObject("Retro lighting").AddComponent<RetroLighting>();
            lighting.transform.SetParent(root, false);
            lighting.Apply();

            var volume = root.GetComponentsInChildren<Volume>(true).Single(item => item.isGlobal);
            volume.sharedProfile = Profile();

            var camera = root.GetComponentsInChildren<Camera>(true).Single(item => item.CompareTag("MainCamera"));
            var hd = camera.GetComponent<HDAdditionalCameraData>() ?? camera.gameObject.AddComponent<HDAdditionalCameraData>();
            hd.allowDynamicResolution = true;
            camera.gameObject.AddComponent<RetroScreen>();

            File.WriteAllText(SourcesPath, JsonUtility.ToJson(sources, true) + "\n");
            AssetDatabase.SaveAssets();
        }

        // Placeholder residents: pipeline test characters, not resident designs.
        private static readonly (string Capsule, string Character, string Idle)[] Residents =
        {
            ("IvoConversation", "test-resident-01", "breathing-idle"),
            ("RenPrivateConversation", "test-resident-02", "neutral-idle"),
        };

        /// <summary>
        /// Stands a low-poly character (see scripts/characters.py retro) at each resident capsule.
        /// The capsules keep their colliders and dialogue targets; only their meshes are hidden.
        /// </summary>
        private static void PlaceResidents(Transform root, Shader shader)
        {
            var animation = ResidentAnimation.Build();
            foreach (var (capsuleName, character, idle) in Residents)
            {
                var model = RetroCharacter(character, shader);
                if (model == null) continue;
                var capsule = root.GetComponentsInChildren<Transform>(true).Single(item => item.name == capsuleName);
                capsule.GetComponent<MeshRenderer>().enabled = false;
                var resident = (GameObject)PrefabUtility.InstantiatePrefab(model, capsule.parent);
                resident.name = "Resident " + character + " at " + capsuleName;
                // MPFB characters face +Z; turn them to face into the room.
                resident.transform.SetPositionAndRotation(new Vector3(capsule.position.x, 0f, capsule.position.z), Quaternion.Euler(0f, 180f, 0f));
                var animator = resident.GetComponent<Animator>();
                if (animation != null && animator != null)
                {
                    animator.runtimeAnimatorController = ResidentAnimation.ForResident(animation, character, idle);
                    animator.applyRootMotion = false;
                    var target = capsule.GetComponent<CommonHouse.Dialogue.ResidentDialogueTarget>();
                    var dialogue = root.GetComponentInChildren<CommonHouse.Dialogue.DialoguePresenter>();
                    var modal = root.GetComponentInChildren<CommonHouse.Interaction.ModalInputCoordinator>();
                    var seat = resident.transform.position + Vector3.right * (target.ResidentId == "ivo" ? -1f : 0.9f);
                    var chair = OutpostProps.Place(root, "painted_wooden_chair_01", seat, Vector3.back);
                    chair.name = target.ResidentId + " routine chair";
                    resident.AddComponent<CommonHouse.Dialogue.ResidentMotion>().Configure(
                        dialogue, modal, capsule, target.ResidentId, seat, target.ResidentId == "ivo" ? 20f : 32f);
                }
            }
        }

        /// <summary>Imports a retro character as a humanoid with one retro material from its baked texture.</summary>
        private static GameObject RetroCharacter(string character, Shader shader)
        {
            var folder = CharacterAssets.Root + "/Retro/" + character;
            var fbx = folder + "/" + character + ".fbx";
            var albedoPath = folder + "/" + character + "_albedo.png";
            if (!File.Exists(fbx) || !File.Exists(albedoPath)) return null;
            var textureImporter = (TextureImporter)AssetImporter.GetAtPath(albedoPath);
            if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
            {
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
            }
            var path = MaterialRoot + "/" + character + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = character };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath));
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Cutoff", 0f);
            material.SetFloat("_WorldUV", 0f);
            material.SetFloat("_Cull", (float)CullMode.Back);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            var importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
            var names = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().Select(item => item.name).Distinct().ToList();
            importer = (ModelImporter)AssetImporter.GetAtPath(fbx);
            foreach (var name in names) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), material);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
        }

        /// <summary>Puts post-processes at the front of HDRP's after-post list, in the given order.</summary>
        private static void RegisterPostProcesses(params Type[] ordered)
        {
            var settings = GraphicsSettings.GetSettingsForRenderPipeline<HDRenderPipeline>()
                           ?? throw new InvalidOperationException("Missing HDRP global settings.");
            var names = ordered.Select(type => type.AssemblyQualifiedName).ToList();
            var serialized = new SerializedObject(settings);
            var found = 0;
            var property = serialized.GetIterator();
            while (property.Next(true))
            {
                if (!property.isArray || property.propertyType == SerializedPropertyType.String) continue;
                if (!property.propertyPath.EndsWith("m_AfterPostProcessCustomPostProcesses.m_CustomPostProcessTypesAsString", StringComparison.Ordinal)) continue;
                found++;
                var others = new List<string>();
                for (var i = 0; i < property.arraySize; i++)
                {
                    var value = property.GetArrayElementAtIndex(i).stringValue;
                    if (!names.Contains(value)) others.Add(value);
                }
                var wanted = names.Concat(others).ToList();
                property.arraySize = wanted.Count;
                for (var i = 0; i < wanted.Count; i++) property.GetArrayElementAtIndex(i).stringValue = wanted[i];
            }
            if (found == 0) throw new InvalidOperationException("Cannot find HDRP's after-post-process list.");
            if (serialized.ApplyModifiedPropertiesWithoutUndo())
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>Swaps each placed prop for its decimated model, matching the original's bounds.</summary>
        private static void ReplaceProps(Transform root, Shader shader, Dictionary<Material, Material> variants, SourceList sources)
        {
            foreach (var holder in root.GetComponentsInChildren<Transform>(true).Where(item => item.name.StartsWith("Prop: ", StringComparison.Ordinal)).ToList())
            {
                var asset = holder.name.Substring("Prop: ".Length);
                var model = DecimatedModel(asset, shader, variants, sources);
                if (model == null || holder.childCount != 1) continue;
                var original = holder.GetChild(0);
                var target = Bounds(original);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, holder);
                // The Blender round trip can change the FBX root's scale and axis conversion: keep
                // the new root, take a 90-degree turn only when it clearly matches the original's
                // extents better, then line the bounds up.
                var baseRotation = holder.rotation * model.transform.localRotation;
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
                        if (materials[i] == null || materials[i].shader != shader)
                            materials[i] = Variant(fallback, shader, variants, sources);
                    renderer.sharedMaterials = materials;
                }
                UnityEngine.Object.DestroyImmediate(original.gameObject);
            }
        }

        private static Bounds Bounds(Transform item)
        {
            var renderers = item.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        /// <summary>The decimated FBX for a prop, with its materials remapped to retro variants of the originals.</summary>
        private static GameObject DecimatedModel(string asset, Shader shader, Dictionary<Material, Material> variants, SourceList sources)
        {
            var path = ModelRoot + "/" + asset + ".fbx";
            if (!File.Exists(path)) return null;
            var original = OutpostProps.Model(asset);
            var originalMap = ((ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(original))).GetExternalObjectMap();
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.addCollider = false;
            importer.bakeAxisConversion = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
            var remaps = new List<(string, Material)>();
            // Once remapped, a model's materials are no longer embedded sub-assets, so take their
            // names from the existing remaps as well as from the embedded materials.
            var names = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Material>().Select(material => material.name)
                .Concat(importer.GetExternalObjectMap().Keys.Where(key => key.type == typeof(Material)).Select(key => key.name))
                .Distinct().ToList();
            foreach (var name in names)
            {
                var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), name);
                if (originalMap.TryGetValue(key, out var mapped) && mapped is Material source)
                    remaps.Add((name, Variant(source, shader, variants, sources)));
            }
            // Material and texture creation reimports assets, which reloads the importer.
            AssetDatabase.SaveAssets();
            importer = (ModelImporter)AssetImporter.GetAtPath(path);
            foreach (var (name, material) in remaps)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), material);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static Material Variant(Material source, Shader shader, Dictionary<Material, Material> variants, SourceList sources)
        {
            if (source == null) return null;
            if (source.shader == shader) return source;
            if (variants.TryGetValue(source, out var existing)) return existing;
            var path = MaterialRoot + "/" + Safe(source.name) + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = source.name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            var albedo = source.HasProperty("_BaseColorMap") ? source.GetTexture("_BaseColorMap") as Texture2D : null;
            material.SetTexture("_MainTex", albedo != null ? LowRes(albedo, sources) : null);
            material.SetTextureScale("_MainTex", source.HasProperty("_BaseColorMap") ? source.GetTextureScale("_BaseColorMap") : Vector2.one);
            material.SetColor("_Color", source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : Color.white);
            var clipped = source.HasProperty("_AlphaCutoffEnable") && source.GetFloat("_AlphaCutoffEnable") > 0.5f;
            var transparent = source.HasProperty("_SurfaceType") && source.GetFloat("_SurfaceType") > 0.5f;
            // No blending on these surfaces: cut-out parts stay cut out, and faint glass disappears.
            material.SetFloat("_Cutoff", clipped ? source.GetFloat("_AlphaCutoff") : transparent ? 0.5f : 0f);
            var worldMapped = source.HasProperty("_UVBase") && source.GetFloat("_UVBase") > 4.5f;
            material.SetFloat("_WorldUV", worldMapped ? 1f : 0f);
            material.SetFloat("_WorldUVScale", 1f / WorldTileMetres);
            var doubleSided = source.HasProperty("_DoubleSidedEnable") && source.GetFloat("_DoubleSidedEnable") > 0.5f;
            material.SetFloat("_Cull", doubleSided ? (float)CullMode.Off : (float)CullMode.Back);
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
            if (importer.textureCompression != TextureImporterCompression.Uncompressed || !importer.alphaIsTransparency)
            {
                // Uncompressed keeps the small texels exact; the shader samples without filtering.
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
            AssetDatabase.DeleteAsset(ProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            profile.Add<Exposure>(true).mode.value = ExposureMode.Fixed;
            profile.Add<MotionBlur>(true).intensity.value = 0f;
            profile.Add<Bloom>(true).intensity.value = 0f;
            profile.Add<Tonemapping>(true).mode.value = TonemappingMode.None;
            profile.Add<ScreenSpaceAmbientOcclusion>(true).intensity.value = 0f;
            profile.Add<ContactShadows>(true).enable.value = false;
            // The retro shader fogs each vertex itself.
            profile.Add<Fog>(true).enabled.value = false;
            profile.Add<HazeFilter>(true).enabled.value = true;
            var depth = profile.Add<ColorDepth>(true);
            depth.enabled.value = true;
            // The PS2 usually drew 32-bit colour; keep a light trace of the PS1's dither.
            depth.bitsPerChannel.value = 6;
            depth.dither.value = 0.6f;
            depth.saturation.value = 1f;
            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static string Safe(string name) => new string(name.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());
    }
}
