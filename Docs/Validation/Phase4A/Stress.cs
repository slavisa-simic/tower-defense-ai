using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Core;
using ShorelineDefense.Presentation;
public static class Phase4AStress
{
    public static async Task<string> Run()
    {
        Time.timeScale=0;var spawner=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();var registry=UnityEngine.Object.FindFirstObjectByType<EnemyRegistry>();var waves=UnityEngine.Object.FindFirstObjectByType<WaveManager>();
        UnityEngine.Object.FindFirstObjectByType<BorderBreachService>().ConfigureThreshold(0);
        int before=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length;spawner.ConfigureWave(150,1,1);var roster=waves.EndlessSettings.enemies;
        var watch=System.Diagnostics.Stopwatch.StartNew();int spawned=0;
        for(int y=0;y<18&&spawned<150;y++)for(int x=7;x<26&&spawned<150;x++)
            if(spawner.TrySpawnAt(roster[spawned%3],spawner.Battlefield.Grid.CellToWorld(new Vector2Int(x,y)),out var enemy))spawned++;
        double spawnMs=watch.Elapsed.TotalMilliseconds;
        bool capBlocked=!spawner.TrySpawnAt(roster[0],spawner.Battlefield.Grid.CellToWorld(new Vector2Int(26,16)),out var ignored);
        if(spawned!=150||registry.Count!=150||!capBlocked)throw new Exception("150-enemy cap failure");
        var frameMs=new List<float>();Time.timeScale=1;var timer=System.Diagnostics.Stopwatch.StartNew();int previousFrame=-1;
        while(timer.Elapsed.TotalSeconds<3)
        {await Task.Delay(1);if(Time.frameCount==previousFrame)continue;previousFrame=Time.frameCount;frameMs.Add(Time.unscaledDeltaTime*1000);}
        Time.timeScale=0;int peak=registry.Count;
        foreach(var enemy in registry.Active.ToArray())enemy.Health.TakeDefenseDamage(int.MaxValue);
        Time.timeScale=2;await Task.Delay(1500);Time.timeScale=0;
        int after=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length;
        var sorted=frameMs.OrderBy(v=>v).ToArray();var effects=UnityEngine.Object.FindFirstObjectByType<VfxService>();
        var result=new{spawned,capBlocked,spawnMs,sampledEnemies=peak,frames=sorted.Length,medianMs=sorted[sorted.Length/2],p95Ms=sorted[(int)(sorted.Length*.95)],maxMs=sorted.Last(),renderersBefore=before,renderersAfter=after,remainingEnemies=registry.Count,uiPopups=UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.UI.CombatFeedbackHud>().ActivePopups,effectCapacity=effects.TotalCapacity};
        File.WriteAllText("Docs/Validation/Phase4A/stress.json",JsonConvert.SerializeObject(result,Formatting.Indented));if(before!=after||registry.Count!=0)throw new Exception("Object cleanup failed");return JsonConvert.SerializeObject(result);
    }
}

