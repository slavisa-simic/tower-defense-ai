using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
using Object = UnityEngine.Object;
public static class Phase6BBaseline
{
    public static async Task<string> Run()
    {
        Application.runInBackground = true;
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(), "BorderDefendersPhase6B", Guid.NewGuid().ToString()));
        var results = new System.Collections.Generic.List<object>();
        foreach (string scene in new[]{"CoastalDefense_TopDown", "DanubeIndustrialPort"})
        {
            await SceneManager.LoadSceneAsync(scene); await Task.Delay(300); Time.timeScale = 0;
            Object.FindFirstObjectByType<BorderBreachService>().ConfigureThreshold(0);
            var context = Object.FindFirstObjectByType<MapRuntimeContext>();
            string map = context.Validate(out int invalid); if (invalid != 0) throw new Exception(map);
            var waves = Object.FindFirstObjectByType<WaveManager>();
            var registry = Object.FindFirstObjectByType<EnemyRegistry>();
            int planned = waves.CurrentWave.ScheduledCount;
            if (!waves.TryStartWave()) throw new Exception("Cannot start " + scene);
            int ticks = 0;
            for (; ticks < 10000 && waves.State != WaveState.WaveComplete; ticks++)
            {
                waves.Tick(.2f);
                foreach(var enemy in registry.Active.ToArray()) enemy.Tick(2);
                if(ticks % 20 == 0) await Task.Yield();
            }
            if(waves.State != WaveState.WaveComplete) throw new Exception("Stalled " + scene);
            results.Add(new {scene, map, planned, ticks, state=waves.State.ToString(), spawned=Object.FindFirstObjectByType<EnemySpawner>().TotalSpawned, homicides=Object.FindFirstObjectByType<RunStatistics>().Homicides});
        }
        string json = JsonConvert.SerializeObject(results, Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase6B/baseline.json", json); return json;
    }
}
