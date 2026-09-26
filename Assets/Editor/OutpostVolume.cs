using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Scene-wide HDRP overrides for the temporary blockout: fixed exposure so readability does not
    /// drift with auto-exposure, and no motion blur, which smears text and edges when the camera turns.
    /// </summary>
    public static class OutpostVolume
    {
        public const string ProfilePath = "Assets/CommonHouse/Rendering/OutpostVolumeProfile.asset";
        public const float FixedExposureEv100 = 4f;
        public const float AmbientSkyEv = 0f;

        public static void AddTo(Transform root)
        {
            var volume = new GameObject("Outpost Volume").AddComponent<Volume>();
            volume.transform.SetParent(root);
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = LoadOrCreateProfile();
        }

        private static VolumeProfile LoadOrCreateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile != null) return profile;
            Directory.CreateDirectory(Path.GetDirectoryName(ProfilePath) ?? "Assets");
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            var exposure = profile.Add<Exposure>(true);
            exposure.mode.Override(ExposureMode.Fixed);
            exposure.fixedExposure.Override(FixedExposureEv100);
            var motionBlur = profile.Add<MotionBlur>(true);
            motionBlur.intensity.Override(0f);
            // Without reflection probes HDRP reflects the sky indoors; keep it nearly black so rooms read by their lamps.
            var environment = profile.Add<VisualEnvironment>(true);
            environment.skyType.Override((int)SkyType.Gradient);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            var sky = profile.Add<GradientSky>(true);
            sky.top.Override(new Color(0.55f, 0.6f, 0.7f));
            sky.middle.Override(new Color(0.6f, 0.58f, 0.55f));
            sky.bottom.Override(new Color(0.35f, 0.32f, 0.3f));
            sky.exposure.Override(AmbientSkyEv);
            foreach (var component in profile.components)
            {
                component.name = component.GetType().Name;
                AssetDatabase.AddObjectToAsset(component, profile);
            }
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }
    }
}
