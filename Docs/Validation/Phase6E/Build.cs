using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class Phase6EBuild
{
    public static string Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before building.");
        Directory.CreateDirectory("Builds/Phase6E");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Game/Scenes/MainMenu.unity", "Assets/Game/Scenes/CoastalDefense_TopDown.unity", "Assets/Game/Scenes/DanubeIndustrialPort.unity" },
            locationPathName = "Builds/Phase6E/BorderDefenders.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
        });
        var s = report.summary;
        string json = JsonConvert.SerializeObject(new { result = s.result.ToString(), s.totalErrors, s.totalWarnings, seconds = s.totalTime.TotalSeconds, s.totalSize, s.outputPath }, Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase6E/build.json", json);
        if (s.result != BuildResult.Succeeded) throw new Exception(json); return json;
    }
}

