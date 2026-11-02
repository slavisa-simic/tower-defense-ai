using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class Phase6BBuild
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
        Directory.CreateDirectory("Builds/Phase6B");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Game/Scenes/MainMenu.unity","Assets/Game/Scenes/CoastalDefense_TopDown.unity","Assets/Game/Scenes/DanubeIndustrialPort.unity"},locationPathName="Builds/Phase6B/BorderDefenders.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        var summary=report.summary;string json=JsonConvert.SerializeObject(new{result=summary.result.ToString(),summary.totalErrors,summary.totalWarnings,seconds=summary.totalTime.TotalSeconds,summary.totalSize,summary.outputPath},Formatting.Indented);File.WriteAllText("Docs/Validation/Phase6B/build.json",json);if(summary.result!=BuildResult.Succeeded)throw new Exception(json);return json;
    }
}
