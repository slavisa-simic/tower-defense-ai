using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using ShorelineDefense.Base;
using Newtonsoft.Json;

public static class Phase3A1Smoke
{
    public static string Start()
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != "CoastalDefense_TopDown")
            throw new InvalidOperationException("Primary scene must be playing");
        var build = UnityEngine.Object.FindFirstObjectByType<BuildManager>();
        var hud = UnityEngine.Object.FindFirstObjectByType<BuildHud>();
        var waves = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
        var definition = AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/MachineGunNest.asset");
        int before = build.Economy.Money;
        var result = build.TryPlaceDefense(definition, new Vector2Int(8,9), out var defense);
        if (result != PlacementResult.Valid || defense == null || build.Economy.Money != before-definition.Cost)
            throw new InvalidOperationException("Placement/payment smoke failed");
        bool started = waves.TryStartWave();
        if (!started || hud.MoneyText != $"Money: ${build.Economy.Money}")
            throw new InvalidOperationException("Wave/HUD smoke failed");
        return JsonConvert.SerializeObject(new { result=result.ToString(), before, after=build.Economy.Money, hud.MoneyText, started });
    }
    public static string Sample()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Play mode required");
        Time.timeScale = 0;
        var waves = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
        var hud = UnityEngine.Object.FindFirstObjectByType<WaveHud>();
        var health = UnityEngine.Object.FindFirstObjectByType<BaseHealth>();
        var terrain = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(r=>r.sprite != null && AssetDatabase.GetAssetPath(r.sprite).StartsWith("Assets/Game/Art/Environment/"))
            .Select(r=>new { r.name, sprite=r.sprite.name, scale=r.transform.localScale.ToString(), mode=r.drawMode.ToString(), size=r.size.ToString(), sorting=r.sortingLayerName }).ToArray();
        if (terrain.Length != 5 || waves.AliveCount < 1 || hud.StartAvailable)
            throw new InvalidOperationException("Terrain/enemy/wave-state smoke failed");
        string json = JsonConvert.SerializeObject(new { waves.AliveCount, waves.PendingCount, state=waves.State.ToString(), hud.RemainingText, hud.StartAvailable, health.CurrentHealth, terrain }, Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase3A1/play-smoke.json", json);
        return json;
    }
}
