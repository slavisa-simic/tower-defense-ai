using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
public static class Phase6ETimingProbe
{
    public static async Task<string> Run(string name)
    {
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefenders6ETiming",Guid.NewGuid().ToString()));LocalGameData.Tutorial.SetEnabled(false);Application.runInBackground=true;
        await SceneManager.LoadSceneAsync("CoastalDefense_TopDown");await Task.Delay(100);Time.timeScale=0;
        var spawn=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();var prefab=UnityEngine.Object.FindFirstObjectByType<WaveManager>().EndlessSettings.enemies.Single(p=>p.Definition.Kind==EnemyKind.Bulwark);
        int Run(int x,float dt,int steps)
        {
            if(!spawn.TrySpawnAt(prefab,spawn.Battlefield.Grid.CellToWorld(new Vector2Int(x,8)),out var e))throw new Exception("Probe spawn");
            var burn=e.GetComponent<EnemyDamageOverTime>();int hp=e.Health.CurrentHealth;burn.ApplyBurn(2,15);for(int i=0;i<steps;i++)burn.Tick(dt);return hp-e.Health.CurrentHealth;
        }
        string json=JsonConvert.SerializeObject(new{armor=prefab.Definition.Armor,burnDps=15,duration=2,singleStepDamage=Run(12,2,1),smallStepDamage=Run(14,.02f,100)},Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase6E/"+name+".json",json);return json;
    }
}
