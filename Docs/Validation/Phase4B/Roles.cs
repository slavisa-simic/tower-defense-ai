using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase4BRoles
{
    static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
    static async Task Reset()
    {
        var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Application.runInBackground=true;
        Time.timeScale=0;await SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
        for(int i=0;i<100&&!Find<GameStateController>().IsPlaying;i++)await Task.Delay(20);
        if(!Find<GameStateController>().IsPlaying)throw new Exception("Fresh scene did not enter Playing");
        Find<BuildManager>().Economy.AddMoney(10000);Find<PlacementInput>().enabled=false;
        typeof(EnemySpawner).GetField("minimumSpawnSeparation",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(Find<EnemySpawner>(),0f);
    }
    static DefenseBase Place(string id,int x=18,int y=8)
    {
        var data=Find<BuildHud>().Defenses.Single(d=>d.Id==id);
        if(Find<BuildManager>().TryPlaceDefense(data,new Vector2Int(x,y),out var defense)!=PlacementResult.Valid)throw new Exception("Placement failed "+id);
        return defense;
    }
    static EnemyController Spawn(Vector3 point,bool brute)
    {
        var prefab=brute?Find<WaveManager>().EndlessSettings.enemies.Single(e=>e.Definition.Kind==EnemyKind.Brute):Find<EnemySpawner>().DefaultPrefab;
        if(!Find<EnemySpawner>().TrySpawnAt(prefab,point,out var enemy))throw new Exception("Role spawn failed");return enemy;
    }
    static void Step(DefenseBase defense,float dt)
    {
        defense.GetComponent<TurretCombat>()?.Tick(dt);defense.GetComponent<AreaProjectileWeapon>()?.Tick(dt);defense.GetComponent<FlameWeapon>()?.Tick(dt);
        foreach(var p in UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None))p.Tick(dt);
        foreach(var e in Find<EnemyRegistry>().Active.ToArray())e.GetComponent<EnemyDamageOverTime>()?.Tick(dt);
    }
    public static async Task<string> Run()
    {
        var rows=new List<object>();var groupKills=new Dictionary<string,int>();var singleDamage=new Dictionary<string,int>();
        foreach(string id in new[]{"machine-gun-nest","mortar","rocket","flamethrower","autocannon"})
        {
            await Reset();var defense=Place(id);bool brute=id=="autocannon";var targets=new List<EnemyController>();
            for(int i=0;i<(brute?1:6);i++)targets.Add(Spawn(defense.transform.position+Vector3.left*(id=="flamethrower"?2:5)+Vector3.up*((i-2.5f)*.22f),brute));
            int initial=targets.Sum(e=>e.Health.CurrentHealth);float seconds=id=="mortar"||id=="rocket"?4:2;
            for(int i=0;i<Mathf.RoundToInt(seconds*10);i++){Step(defense,.1f);if(i%5==0)await Task.Delay(1);}
            int remaining=targets.Where(e=>e!=null&&e.Health.IsAlive).Sum(e=>e.Health.CurrentHealth);int kills=targets.Count(e=>e==null||!e.Health.IsAlive);
            rows.Add(new{id,scenario=brute?"one Brute":"six clustered Basic",seconds,kills,damage=initial-remaining,remaining});groupKills[id]=kills;
        }
        foreach(string id in new[]{"machine-gun-nest","autocannon","sniper-tower"})
        {
            await Reset();var defense=Place(id);var enemy=Spawn(defense.transform.position+Vector3.left*5,true);
            for(int i=0;i<20;i++){Step(defense,.1f);if(i%5==0)await Task.Delay(1);}
            int damage=600-enemy.Health.CurrentHealth;singleDamage[id]=damage;rows.Add(new{id,scenario="one Brute",seconds=2,damage,remaining=enemy.Health.CurrentHealth});
        }
        var wireDamage=new Dictionary<bool,int>();
        foreach(bool wire in new[]{false,true})
        {
            await Reset();var defense=Place("flamethrower");var cell=new Vector2Int(16,8);if(wire)Place("barbed-wire",cell.x,cell.y);
            var enemy=Spawn(Find<BuildManager>().Battlefield.Grid.CellToWorld(cell),true);float initialSpeed=enemy.Movement.CurrentSpeed;
            for(int i=0;i<80;i++){enemy.Tick(.1f);Step(defense,.1f);if(i%5==0)await Task.Delay(1);}
            int damage=600-enemy.Health.CurrentHealth;wireDamage[wire]=damage;rows.Add(new{id="flamethrower",scenario=wire?"moving Brute starts on Wire":"moving Brute, free ground",seconds=8,initialSpeed,damage});
        }
        if(groupKills["mortar"]<=groupKills["machine-gun-nest"]||singleDamage["autocannon"]<=singleDamage["machine-gun-nest"]||wireDamage[true]<=wireDamage[false])throw new Exception("Role comparison failed: "+JsonConvert.SerializeObject(rows));
        File.WriteAllText("Docs/Validation/Phase4B/balance-roles.json",JsonConvert.SerializeObject(rows,Formatting.Indented));return JsonConvert.SerializeObject(rows);
    }
    public static async Task<string> Visuals()
    {
        await Reset();var mortar=Place("mortar",18,5);var rocket=Place("rocket",18,12);var flame=Place("flamethrower",23,9);var cannon=Place("autocannon",23,15);
        foreach(var defense in new[]{mortar,rocket,flame,cannon})
        {
            float distance=defense==flame?2:5;
            for(int i=0;i<3;i++)Spawn(defense.transform.position+Vector3.left*distance+Vector3.up*(i*.3f),true);
        }
        var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Application.runInBackground=true;Time.timeScale=1;
        await Task.Delay(300);Capture("heavy-firing");await Task.Delay(700);Capture("heavy-impact-burn");await Task.Delay(1200);Time.timeScale=0;Capture("burn-after-flame");
        return "Natural-update controlled firing fixture captured: arc/rocket/cone/burn/heavy tracer";
    }
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.Static|BindingFlags.NonPublic);
        File.WriteAllBytes("Docs/Validation/Phase4B/"+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
}
