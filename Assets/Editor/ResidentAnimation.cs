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
    /// "idle" is the default state.
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
            clip.loopTime = true;
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
