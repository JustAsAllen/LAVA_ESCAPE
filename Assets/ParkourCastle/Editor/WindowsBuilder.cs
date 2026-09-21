using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ParkourCastle.EditorTools
{
    public static class WindowsBuild
    {
        const string Root = "Assets/ParkourCastle";
        const string ScenePath = Root + "/Scenes/TakeshiCastle.unity";

        [UnityEditor.MenuItem("Parkour Castle/Build Windows Player")]
        public static void BuildPlayer()
        {
            var scenes = new[] { ScenePath };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/Windows/ParkourCastle.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
                Debug.Log("[ParkourCastle] PLAYER_BUILD_OK " + summary.outputPath);
            else
                Debug.LogError("[ParkourCastle] PLAYER_BUILD_FAILED " + summary.result + " totalErrors=" + summary.totalErrors);
        }
    }
}