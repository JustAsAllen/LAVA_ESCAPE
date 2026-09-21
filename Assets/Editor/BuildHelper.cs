using MainCourseEditor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildHelper
{
    const string ScenePath = "Assets/Scenes/MainCourse.unity";
    const string Output = "Build/ParkourGame.exe";

    public static void PerformRegen()
    {
        Debug.Log("[BuildHelper] Regen: URP setup");
        URPSetup.EnsureUrp();

        Debug.Log("[BuildHelper] Regen: generate level + HUD");
        CourseBuilder.Build();

        AssetDatabase.SaveAssets();
        Debug.Log("[BuildHelper] REGEN_SUCCEEDED");
        EditorApplication.Exit(0);
    }

    public static void PerformBuild()
    {
        Debug.Log("[BuildHelper] Phase 1: URP setup");
        URPSetup.EnsureUrp();

        Debug.Log("[BuildHelper] Phase 2/3: generate level + HUD");
        CourseBuilder.Build();

        Debug.Log("[BuildHelper] Register scene in EditorBuildSettings");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        AssetDatabase.SaveAssets();

        Debug.Log("[BuildHelper] Phase 4: build Windows64 player");
        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = Output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.CompressWithLz4HC
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        bool ok = report.summary.result == BuildResult.Succeeded;

        if (ok)
        {
            Debug.Log("[BuildHelper] BUILD_SUCCEEDED " + report.summary.outputPath);
        }
        else
        {
            Debug.LogError("[BuildHelper] BUILD_FAILED result=" + report.summary.result + " totalErrors=" + report.summary.totalErrors);
            foreach (var step in report.steps)
                Debug.LogError("[BuildHelper] step=" + step.name + " duration=" + step.duration + " errors=" + step.messages.Length);
        }

        EditorApplication.Exit(ok ? 0 : 1);
    }
}