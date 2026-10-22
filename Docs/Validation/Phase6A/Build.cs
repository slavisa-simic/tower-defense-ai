using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
public static class Phase6ABuild
{
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Stop Play Mode before building.");
        Directory.CreateDirectory("Builds/Phase6A");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Phase6A/BorderDefenders.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        var s=report.summary;string result=$"{s.result}; errors={s.totalErrors}; warnings={s.totalWarnings}; bytes={s.totalSize}; time={s.totalTime.TotalSeconds:F2}s";
        File.WriteAllText("Docs/Validation/Phase6A/windows-build.txt",result);return result;
    }
}
