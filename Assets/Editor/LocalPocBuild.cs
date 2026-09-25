using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CommonHouse.Editor
{
    /// <summary>Builds only the local Linux POC into the explicit disposable verification directory.</summary>
    public static class LocalPocBuild
    {
        public static void BuildLinux64()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable("COMMON_HOUSE_ENV"), "test", StringComparison.Ordinal))
                throw new InvalidOperationException("Local POC builds require the disposable test environment.");
            var testData = Environment.GetEnvironmentVariable("COMMON_HOUSE_TEST_DATA_PATH");
            if (string.IsNullOrWhiteSpace(testData) || !Path.IsPathRooted(testData))
                throw new InvalidOperationException("Local POC builds require an absolute COMMON_HOUSE_TEST_DATA_PATH.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64))
                throw new InvalidOperationException("Installed Editor does not support StandaloneLinux64 builds.");
            var output = Path.Combine(testData, "player", "TheCommonHouse.x86_64");
            var directory = Path.GetDirectoryName(output);
            if (string.IsNullOrWhiteSpace(directory)) throw new InvalidOperationException("Invalid Linux player output path.");
            if (Directory.Exists(directory)) throw new InvalidOperationException("Refusing to replace an existing build output directory: " + directory);
            Directory.CreateDirectory(directory);
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No enabled build scenes are configured.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.Development,
            });
            if (report.summary.result != BuildResult.Succeeded || !File.Exists(output) || !Directory.Exists(Path.Combine(directory, "TheCommonHouse_Data")))
                throw new InvalidOperationException("Linux standalone build failed or required player data is missing: " + report.summary.result + "; output=" + output);
            File.WriteAllText(Path.Combine(directory, "build-editor-receipt.json"),
                "{\n  \"target\": \"StandaloneLinux64\",\n  \"output\": \"" + Escape(output) + "\",\n  \"total_size\": " + report.summary.totalSize + "\n}\n");
        }

        private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
