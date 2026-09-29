using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CommonHouse.Editor
{
    /// <summary>
    /// Turns the Mixamo animations imported by scripts/import_mixamo.py (decision D-25) into looping
    /// humanoid clips and a Residents Animator Controller. The first clip whose name contains
    /// "idle" is the default state; each resident can override it with its own idle.
    /// </summary>
    public static class ResidentAnimation
    {
        public const string ClipRoot = "Assets/CommonHouse/Art/Animations/Mixamo";
        public const string ControllerPath = "Assets/CommonHouse/Art/Animations/Residents.controller";

        /// <summary>Builds the controller, or returns null when no animations have been imported.</summary>
        public static RuntimeAnimatorController Build()
        {
            if (!Directory.Exists(ClipRoot)) return null;
            var files = Directory.GetFiles(ClipRoot, "*.fbx").Select(path => path.Replace('\\', '/')).OrderBy(path => path).ToList();
            if (files.Count == 0) return null;
            var clips = files.Select(Clip).ToList();

            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            AnimatorState idle = null;
            foreach (var clip in clips)
            {
                var state = machine.AddState(clip.name);
                state.motion = clip;
                if (idle == null && clip.name.Contains("idle")) idle = state;
            }
            machine.defaultState = idle ?? machine.states[0].state;
            AssetDatabase.SaveAssets();
            return controller;
        }

        /// <summary>
        /// Transitions and single actions play once; idles, poses, and repeated motions loop.
        /// </summary>
        public static bool Loops(string name)
        {
            var once = new[] { "-to-", "start-", "stop-", "turn", "open", "sit-to", "stand-to", "type-to" };
            return !once.Any(part => name.Contains(part));
        }

        /// <summary>
        /// An override of the Residents controller whose default state plays <paramref name="idle"/>,
        /// so residents do not move in step. Falls back to the shared controller.
        /// </summary>
        public static RuntimeAnimatorController ForResident(RuntimeAnimatorController shared, string character, string idle)
        {
            if (shared == null) return null;
            var controller = (AnimatorController)shared;
            var defaultClip = controller.layers[0].stateMachine.defaultState.motion as AnimationClip;
            var clip = AssetDatabase.LoadAllAssetsAtPath(ClipRoot + "/" + idle + ".fbx").OfType<AnimationClip>().FirstOrDefault(item => item.name == idle);
            if (defaultClip == null || clip == null) return shared;
            var path = Path.GetDirectoryName(ControllerPath).Replace('\\', '/') + "/" + character + ".overrideController";
            AssetDatabase.DeleteAsset(path);
            var overrides = new AnimatorOverrideController(shared) { name = character };
            overrides[defaultClip] = clip;
            AssetDatabase.CreateAsset(overrides, path);
            return overrides;
        }

        /// <summary>Imports one Mixamo FBX as a looping, in-place humanoid clip named after the file.</summary>
        private static AnimationClip Clip(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var importer = (ModelImporter)AssetImporter.GetAtPath(path)
                           ?? throw new InvalidOperationException("Cannot import " + path);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
            importer = (ModelImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultClipAnimations;
            if (settings.Length == 0) throw new InvalidOperationException(path + " contains no animation");
            var clip = settings[0];
            clip.name = name;
            clip.loopTime = Loops(name);
            // Bake root motion into the pose, so residents stay where the scene puts them.
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            importer.clipAnimations = new[] { clip };
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().First(item => item.name == name);
        }
    }
}
