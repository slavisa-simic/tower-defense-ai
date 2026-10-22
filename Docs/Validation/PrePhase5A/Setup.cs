using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Waves;
public static class PrePhase5ASetup
{
    public static string Apply()
    {
        if (EditorApplication.isPlaying) throw new Exception("Apply only in Edit Mode");
        var s = AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        s.enemyQuantityMultiplier = 5;
        s.globalEnemyHealthMultiplier = s.bossHealthMultiplier = 1;
        // Keep the original 240 reference formations at the new expansion scale.
        s.maximumEnemiesPerWave = 1200;
        s.earlyPressureTarget = 25;
        s.pressureGrowthPerWave = 4;
        s.maximumPressureTarget = 160;
        EditorUtility.SetDirty(s);
        AssetDatabase.SaveAssets();
        string[] categories = { "Weapons", "Explosions", "Impacts", "Enemies", "Bosses", "Boats", "Environment", "UI", "Music", "Radio" };
        Folder("Assets/Game/Audio");
        foreach (var branch in new[] { "Source", "Processed" })
        {
            Folder("Assets/Game/Audio/" + branch);
            foreach (var category in categories) Folder("Assets/Game/Audio/" + branch + "/" + category);
        }
        Folder("Assets/Game/Audio/Mixers"); Folder("Assets/Game/Audio/Definitions");
        var rows = new[] { 1, 2, 3, 4, 5, 10, 20, 25, 50, 1000 }.Select(n => {
            var wave = new EndlessWaveGenerator(s, 1977).Generate(n);
            var row = new { wave = n, count = wave.ScheduledCount, normalHealth = s.RuntimeHealthMultiplier(wave.HealthMultiplier, false), bossHealth = s.RuntimeHealthMultiplier(wave.HealthMultiplier, true), target = s.PressureTarget(n), boats = s.BoatConcurrency(n), interval = s.ArrivalInterval(n) };
            UnityEngine.Object.DestroyImmediate(wave); return row;
        }).ToArray();
        File.WriteAllText("Docs/Validation/PrePhase5A/tuning.json", Newtonsoft.Json.JsonConvert.SerializeObject(rows, Newtonsoft.Json.Formatting.Indented));
        return "5x quantity, 1x normal/boss health; pressure 25 + 4/wave capped at 160; audio folders ready";
    }
    static void Folder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
}
