using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase4BLive
{
    const string Folder="Docs/Validation/Phase4B/";
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Write(string name,object result) => File.WriteAllText(Folder+name+".json",JsonConvert.SerializeObject(result,Formatting.Indented));
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes(Folder+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    static bool Buy(string id,int x,int y)
    {
        var data=Find<BuildHud>().Defenses.Single(d=>d.Id==id);
        return Find<BuildManager>().TryPlaceDefense(data,new Vector2Int(x,y),out _)==PlacementResult.Valid;
    }
    static void Reinforce(int wave)
    {
        if(wave==1){Buy("sniper-tower",23,9);Buy("machine-gun-nest",23,4);Buy("barbed-wire",27,4);}
        if(wave==2){Buy("machine-gun-nest",23,14);Buy("barbed-wire",27,14);}
        if(wave==3){Buy("machine-gun-nest",23,7);Buy("barbed-wire",27,9);}
        if(wave==4)Buy("mortar",20,11);
        if(wave==5){Buy("flamethrower",25,9);Buy("barbed-wire",26,9);Buy("barbed-wire",26,8);Buy("barbed-wire",26,10);}
        if(wave==6)Buy("autocannon",20,5);
        if(wave==7)Buy("rocket",20,14);
        if(wave==8){Buy("sniper-tower",23,13);Buy("machine-gun-nest",23,11);}
        if(wave==9){Buy("flamethrower",25,4);Buy("barbed-wire",26,4);Buy("flamethrower",25,14);Buy("barbed-wire",26,14);}
        if(wave==10){Buy("mortar",19,8);Buy("autocannon",22,16);}
        if(wave==11){Buy("rocket",18,3);Buy("barbed-wire",26,3);Buy("barbed-wire",26,5);}
        if(wave==12){Buy("mortar",18,13);Buy("barbed-wire",26,13);Buy("barbed-wire",26,15);Buy("land-mine",27,8);}
    }
    public static async Task<string> PreparationReview()
    {
        var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Application.runInBackground=true;
        Time.timeScale=0;await Task.Delay(100);Capture("build-panel-hud");
        var input=Find<PlacementInput>();var grid=Find<BuildManager>().Battlefield.Grid;int accessible=0;
        for(int y=0;y<grid.Height;y++)for(int x=0;x<grid.Width;x++)
        {
            var cell=new Vector2Int(x,y);var point=Camera.main.WorldToScreenPoint(grid.CellToWorld(cell));
            if(!input.TryScreenToCell(point,out var actual)||actual!=cell||input.IsOverHud(point))throw new Exception("Covered cell "+cell);
            accessible++;
        }
        Write("cells-"+Screen.width,new{Screen.width,Screen.height,accessible});return Audit();
    }
    public static async Task<string> SimulateTo15()
    {
        var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Application.runInBackground=true;
        Time.timeScale=0;Find<PlacementInput>().enabled=false;
        var waves=Find<WaveManager>();var registry=Find<EnemyRegistry>();var build=Find<BuildManager>();var stats=Find<RunStatistics>();
        // Fix seed in this fixture only, preserving saved EndlessSurvival settings.
        typeof(WaveManager).GetField("generator",Flags).SetValue(waves,new EndlessWaveGenerator(waves.EndlessSettings,1977));
        var rows=new List<object>();int peak=0;
        for(int wave=1;wave<=14;wave++)
        {
            Reinforce(wave);int count=waves.CurrentWave.ScheduledCount;long kills=stats.TotalKills,breaches=stats.Homicides;int balance=build.Economy.Money;
            var guns=UnityEngine.Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None);
            var areas=UnityEngine.Object.FindObjectsByType<AreaProjectileWeapon>(FindObjectsSortMode.None);
            var flames=UnityEngine.Object.FindObjectsByType<FlameWeapon>(FindObjectsSortMode.None);
            var mines=UnityEngine.Object.FindObjectsByType<LandMine>(FindObjectsSortMode.None);
            if(!waves.TryStartWave())throw new Exception("Start failed "+wave);
            int ticks=0;
            while(waves.State!=WaveState.WaveComplete && build.Game.IsPlaying && ticks<10000)
            {
                waves.Tick(.1f);
                foreach(var e in registry.Active.ToArray()){e.Tick(.1f);e.GetComponent<EnemyDamageOverTime>()?.Tick(.1f);}
                foreach(var g in guns)if(g!=null)g.Tick(.1f);
                foreach(var a in areas)if(a!=null)a.Tick(.1f);
                foreach(var f in flames)if(f!=null)f.Tick(.1f);
                foreach(var m in mines)if(m!=null)m.Tick(.1f);
                foreach(var p in UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None))p.Tick(.1f);
                peak=Math.Max(peak,registry.Count);ticks++;
                if(ticks%10==0)await Task.Delay(1);
            }
            rows.Add(new{wave,count,kills=stats.TotalKills-kills,homicides=stats.Homicides,breaches=stats.Homicides-breaches,startBalance=balance,balance=build.Economy.Money,simulationSeconds=ticks*.1f,completed=waves.State==WaveState.WaveComplete});
            if(!build.Game.IsPlaying||waves.State!=WaveState.WaveComplete){Write("economy-simulation",rows);throw new Exception("Combat run failed wave "+wave);}
            waves.Tick(2);await Task.Delay(5);
        }
        Write("economy-simulation",new{rows,peak,startingMoney=500,externalMoney=0,totalEarned=build.Economy.TotalEarned,totalSpent=build.Economy.TotalSpent,
            defenses=UnityEngine.Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Select(d=>new{id=d.Definition.Id,anchor=d.Anchor.ToString()})});
        Capture("wave-15-preparation");return "Earned-money deterministic combat progressed to wave 15; natural-update checks next";
    }
    public static async Task<string> NaturalWave()
    {
        var waves=Find<WaveManager>();var stats=Find<RunStatistics>();var build=Find<BuildManager>();var registry=Find<EnemyRegistry>();
        int wave=waves.CurrentWaveNumber,count=waves.CurrentWave.ScheduledCount,balance=build.Economy.Money;long kills=stats.TotalKills,homicides=stats.Homicides;
        var button=Find<WaveHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave");using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
        if(waves.State!=WaveState.Spawning)throw new Exception("Start Wave callback failed");Time.timeScale=6;
        var timer=System.Diagnostics.Stopwatch.StartNew();bool captured=false;int peak=0;
        while(waves.CompletedWaves<wave&&build.Game.IsPlaying&&timer.Elapsed.TotalSeconds<50)
        {
            peak=Math.Max(peak,registry.Count);
            if(!captured&&registry.Count>=5){Time.timeScale=0;await Task.Delay(60);Capture("wave-"+wave+"-combat");captured=true;Time.timeScale=6;}
            await Task.Delay(20);
        }
        Time.timeScale=0;var row=new{wave,count,completed=waves.CompletedWaves==wave,kills=stats.TotalKills-kills,homicides=stats.Homicides-homicides,
            startBalance=balance,balance=build.Economy.Money,peak,seconds=timer.Elapsed.TotalSeconds};
        string path=Folder+"natural-waves.json";var rows=File.Exists(path)?Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(path)):new Newtonsoft.Json.Linq.JArray();rows.Add(Newtonsoft.Json.Linq.JObject.FromObject(row));File.WriteAllText(path,rows.ToString());
        Capture("wave-"+wave+"-complete");if(!row.completed)throw new Exception(JsonConvert.SerializeObject(row));
        Time.timeScale=6;while(waves.State==WaveState.WaveComplete)await Task.Delay(20);Time.timeScale=0;
        return JsonConvert.SerializeObject(row);
    }
    public static async Task<string> Stress()
    {
        Time.timeScale=0;Find<BorderBreachService>().ConfigureThreshold(0);
        var registry=Find<EnemyRegistry>();var spawner=Find<EnemySpawner>();var prefabs=Find<WaveManager>().EndlessSettings.enemies;
        foreach(var e in registry.Active.ToArray()){e.gameObject.SetActive(false);UnityEngine.Object.Destroy(e.gameObject);}
        foreach(var p in UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(p.gameObject);
        await Task.Delay(100);
        var build=Find<BuildManager>();build.Economy.AddMoney(100000);
        string[] ids={"mortar","flamethrower","autocannon","rocket"};int index=0;
        for(int y=2;y<=14;y+=4)for(int x=9;x<=21;x+=4)
        {
            string desired=ids[index++%4];var data=Find<BuildHud>().Defenses.Single(d=>d.Id==desired);
            build.TryPlaceDefense(data,new Vector2Int(x,y),out _);
        }
        int defenses=UnityEngine.Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Length;
        int rendererBefore=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length;
        typeof(EnemySpawner).GetField("minimumSpawnSeparation",Flags).SetValue(spawner,0f);spawner.ConfigureWave(300,4,1);
        for(int i=0;i<300;i++)if(!spawner.TrySpawnAt(prefabs[i%prefabs.Length],spawner.Battlefield.Grid.CellToWorld(new Vector2Int(8+i%18,3+(i/18)%12)),out _))throw new Exception("Stress spawn failed "+i);
        var clock=System.Diagnostics.Stopwatch.StartNew();long before=GC.GetAllocatedBytesForCurrentThread();
        var workspace=new ClusterTargeting.Workspace();before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<1000;i++)ClusterTargeting.TrySelect(registry,spawner.Battlefield.Grid.CellToWorld(new Vector2Int(22,9)),3.75f,15,2.8125f,.8f,16,workspace,out _);
        clock.Stop();long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Time.timeScale=1;await Task.Delay(150); // Exclude the synchronous microbenchmark from natural frame samples.
        var timings=new List<double>();int lastFrame=-1;
        EditorApplication.CallbackFunction sample=()=>{if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;timings.Add(Time.unscaledDeltaTime*1000);};EditorApplication.update+=sample;Time.timeScale=1;
        try{await Task.Delay(3000);}finally{EditorApplication.update-=sample;Time.timeScale=0;}
        timings.Sort();var result=new{spawned=300,defenses,clusterQueries=1000,clusterTotalMs=clock.Elapsed.TotalMilliseconds,clusterMeanMs=clock.Elapsed.TotalMilliseconds/1000,
            clusterAllocatedBytes=allocated,samples=timings.Count,medianMs=timings[timings.Count/2],p95Ms=timings[(int)(timings.Count*.95)],maximumMs=timings.Last(),enemiesAtEnd=registry.Count,
            projectiles=UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None).Length,vfxCapacity=Find<VfxService>().TotalCapacity};
        Write("stress",result);foreach(var e in registry.Active.ToArray()){e.gameObject.SetActive(false);UnityEngine.Object.Destroy(e.gameObject);}await Task.Delay(100);
        foreach(var p in UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None))UnityEngine.Object.Destroy(p.gameObject);
        Find<VfxService>().Clear();await Task.Delay(100);
        Write("stress-cleanup",new{enemies=registry.Count,projectiles=UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None).Length,rendererBefore,
            rendererAfter=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length});
        return JsonConvert.SerializeObject(result);
    }
    public static string Audit()
    {
        var root=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement;var buttons=root.Query<Button>(className:"defense-button").ToList();
        var panel=root.Q("build-panel");var camera=Camera.main;
        var result=new{screen=new[]{Screen.width,Screen.height},camera.orthographic,rotation=camera.transform.eulerAngles.ToString(),
            buttons=buttons.Select(b=>new{b.name,b.text,rect=b.worldBound.ToString(),inside=b.worldBound.xMin>=panel.worldBound.xMin&&b.worldBound.xMax<=panel.worldBound.xMax,
                b.worldBound.width,b.worldBound.height}),footprints=Find<BuildHud>().Defenses.Select(d=>new{d.Id,d.Cost,d.IsValid,footprint=d.Footprint.ToString()}),
            projectiles=UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None).Length};
        Write("live-audit",result);return JsonConvert.SerializeObject(result);
    }
}
