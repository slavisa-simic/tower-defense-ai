using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;

public static class Phase3CEnemyAttack
{
    public static async Task<string> Run()
    {
        Time.timeScale=0;
        foreach(var t in UnityEngine.Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))t.enabled=false;
        var b=UnityEngine.Object.FindFirstObjectByType<BuildManager>();var spawn=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        if(!spawn.TrySpawn(spawn.DefaultPrefab,out var enemy))throw new Exception("Spawn blocked");
        enemy.transform.position=b.Battlefield.Grid.CellToWorld(new Vector2Int(28,9));
        enemy.Movement.Initialize(b.Battlefield.Grid,b.Battlefield.Paths,b.Battlefield.Definition.BaseCell,enemy.Definition.MovementSpeed,enemy.Definition.AttackRange);
        enemy.Movement.Tick(.01f);await Task.Delay(100);
        var body=enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
        if(!body.sprite.name.Contains("attack"))throw new Exception("Attack pose missing");
        int before=UnityEngine.Object.FindFirstObjectByType<BaseHealth>().CurrentHealth;
        float until=Time.time+1.05f;Time.timeScale=1;while(Time.time<until)await Task.Delay(15);Time.timeScale=0;await Task.Delay(100);
        int after=UnityEngine.Object.FindFirstObjectByType<BaseHealth>().CurrentHealth;
        if(after!=before-enemy.Definition.AttackDamage)throw new Exception("Authoritative attack damage mismatch");
        if(enemy.transform.rotation!=Quaternion.identity)throw new Exception("Gameplay root rotated");
        File.WriteAllText("Docs/Validation/Phase3C/base-attack.json",JsonConvert.SerializeObject(new{before,after,enemy.Definition.AttackDamage,pose=body.sprite.name,rootRotation=enemy.transform.rotation.ToString()},Formatting.Indented));
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        File.WriteAllBytes("Docs/Validation/Phase3C/base-attack-1080.png",(byte[])type.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,new object[]{Screen.width,Screen.height}));
        return "Enemy attack sprite, unrotated root and gameplay-owned 10 damage verified";
    }
}
