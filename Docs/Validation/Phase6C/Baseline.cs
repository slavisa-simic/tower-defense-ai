using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
public static class Phase6CBaseline
{
    public static async Task<string> Run()
    {
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(), "BorderDefendersPhase6C", Guid.NewGuid().ToString()));
        Application.runInBackground = true;
        string result = "";
        foreach (string scene in new[] { "CoastalDefense_TopDown", "DanubeIndustrialPort" })
        {
            await SceneManager.LoadSceneAsync(scene); await Task.Delay(100); Time.timeScale = 0;
            UnityEngine.Object.FindFirstObjectByType<BorderBreachService>().ConfigureThreshold(0);
            var waves = UnityEngine.Object.FindFirstObjectByType<WaveManager>();
            var enemies = UnityEngine.Object.FindFirstObjectByType<EnemyRegistry>();
            if (!waves.TryStartWave()) throw new Exception("Start failed");
            int ticks = 0;
            for (; ticks < 10000 && waves.State != WaveState.WaveComplete; ticks++)
            { waves.Tick(.2f); foreach (var enemy in enemies.Active.ToArray()) enemy.Tick(2); if (ticks % 20 == 0) await Task.Yield(); }
            if (waves.State != WaveState.WaveComplete) throw new Exception("Wave stalled");
            result += scene + ": Wave 1 resolved; " + ticks + " ticks; homicides " + UnityEngine.Object.FindFirstObjectByType<RunStatistics>().Homicides + "\n";
        }
        File.WriteAllText("Docs/Validation/Phase6C/baseline.txt", result); return result;
    }
}
