using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

internal static class ExternalPlaytestRcBuild
{
    private const string OutputDirectory = "Builds/ExternalPlaytest/NETBREAK_Area1_VS_RC1";

    [MenuItem("Tools/NETBREAK/Build External Playtest RC1 (Windows x64)")]
    public static void Build()
    {
        string[] scenes = Array.ConvertAll(
            Array.FindAll(EditorBuildSettings.scenes, scene => scene.enabled),
            scene => scene.path);
        if (scenes.Length != 1 || scenes[0] != "Assets/Scenes/Main.unity")
            throw new InvalidOperationException("RC build requires Main as its only enabled scene.");

        Directory.CreateDirectory(OutputDirectory);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(OutputDirectory, "NETBREAK.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded ||
            report.summary.totalErrors != 0)
            throw new InvalidOperationException($"RC build failed: {report.summary.result}, " +
                $"{report.summary.totalErrors} error(s).");

        Debug.Log($"External Playtest RC1: {report.summary.outputPath}, " +
            $"{report.summary.totalSize} bytes, 0 build errors, release options.");
    }
}
