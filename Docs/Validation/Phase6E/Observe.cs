using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using Object=UnityEngine.Object;
using SettingsService=ShorelineDefense.Persistence.SettingsService;

public static class Phase6EObserve
{
    const string Folder="Docs/Validation/Phase6E/";
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static void Check(bool b,string m){if(!b)throw new Exception(m);}
    static string Save(string name,object o){File.WriteAllText(Folder+name+".json",JsonConvert.SerializeObject(o,Formatting.Indented));return "Written "+name;}
    static async Task Load(string map)
    {
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefenders6EObserve",Guid.NewGuid().ToString()));
        LocalGameData.Tutorial.SetEnabled(false);RunSession.Pending=null;Application.runInBackground=true;
        void SilenceInput(Scene loaded,LoadSceneMode mode)
        {
            Time.timeScale=0;
            var menu=Find<ProductMenuController>();if(menu!=null)menu.enabled=false;
            var ui=Find<UnityEngine.EventSystems.EventSystem>();if(ui!=null)ui.enabled=false;
            var pointer=Find<PlacementInput>();if(pointer!=null)pointer.enabled=false;
        }
        SceneManager.sceneLoaded+=SilenceInput;
        try{await SceneManager.LoadSceneAsync(map);}finally{SceneManager.sceneLoaded-=SilenceInput;}
        for(int i=0;i<200&&Find<RunSession>()?.Ready!=true;i++)await Task.Delay(25);
        // Bind the normal product view once, while its Update input polling stays disabled.
        var controller=Find<ProductMenuController>();
        typeof(ProductMenuController).GetMethod("Start",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(controller,null);
        await Task.Delay(150);Time.timeScale=0;Check(Find<RunSession>().CanSave,"Observation starts in a ready Preparation without external input");
    }
    static DefenseBase Place(string id,int x,int y,int level=1)
    {
        var b=Find<BuildManager>();var d=Find<BuildHud>().Defenses.First(n=>n.Id==id);
        var cells=new List<Vector2Int>();for(int dx=-4;dx<=4;dx++)for(int dy=-3;dy<=3;dy++)cells.Add(new Vector2Int(x+dx,y+dy));
        foreach(var c in cells.OrderBy(c=>Math.Abs(c.x-x)+Math.Abs(c.y-y)))
            if(b.TryPlaceDefense(d,c,out var instance)==PlacementResult.Valid){for(int l=1;l<level;l++)b.TryUpgrade(instance);return instance;}
        return null;
    }
    static void Prepare(int wave)
    {var data=Find<RunSession>().Capture();data.wave=wave;data.completedWaves=wave-1;data.seed=1977;data.events=new EventSaveData{nextWave=wave+1};Find<WaveManager>().RestorePreparation(data);}
    public static async Task SetSize(int width,int height)
    {
        const BindingFlags f=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        var type=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null);
        var group=type.GetProperty("currentGroup",f).GetValue(sizes);var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"),false,null,false);string label="Phase6E "+width+"x"+height;
        var labels=(string[])group.GetType().GetMethod("GetDisplayTexts",f).Invoke(group,null);int index=Array.FindIndex(labels,s=>s.Contains("("+width+"x"+height+")")||s.StartsWith(label));
        if(index<0){var st=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");var ctor=st.GetConstructors(f).First(c=>c.GetParameters().Length==4);var size=ctor.Invoke(new[]{Enum.Parse(ctor.GetParameters()[0].ParameterType,"FixedResolution"),(object)width,height,label});group.GetType().GetMethod("AddCustomSize",f).Invoke(group,new[]{size});index=(int)group.GetType().GetMethod("GetTotalCount",f).Invoke(group,null)-1;}
        view.GetType().GetProperty("selectedSizeIndex",f).SetValue(view,index);view.Repaint();await Task.Delay(300);Check(Screen.width==width&&Screen.height==height,"Actual resolution");
    }
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes(Folder+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    public static async Task<string> Frame(string name)
    {EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"),false,null,false).Repaint();await Task.Delay(150);Capture(name);return "Captured "+name;}
    public static async Task<string> Stress(string label="stress",string map="DanubeIndustrialPort")
    {
        var rows=new List<object>();
        foreach(int requested in new[]{100,200,300,400})
        {
            await Load(map);await SetSize(1920,1080);Find<BorderBreachService>().ConfigureThreshold(0);Prepare(40);
            var b=Find<BuildManager>();b.Economy.AddMoney(100000);
            foreach(int y in new[]{3,8,13}){Place("machine-gun-nest",10,y,3);Place("sniper-tower",17,y,3);Place("autocannon",20,y,2);Place("mortar",23,y,2);Place("rocket",14,y+2,2);Place("flamethrower",12,y,3);Place("barbed-wire",11,y,3);}
            for(int y=1;y<17;y++)Place("machine-gun-nest",27,y,1);
            var w=Find<WaveManager>();Check(w.ForceEvent("mass-emergence"),"stress event");
            var sp=Find<EnemySpawner>();sp.ConfigureWave(400,1,1);sp.ConfigureDeployment(.1f);var registry=Find<EnemyRegistry>();
            var grid=b.Battlefield.Grid;int i=0;
            var cells=new List<Vector2Int>();for(int x=6;x<27;x++)for(int y=1;y<17;y++)if(grid.IsTraversable(new Vector2Int(x,y)))cells.Add(new Vector2Int(x,y));
            for(int pass=0;registry.Count<requested&&pass<8;pass++)foreach(var c in cells)
            {
                if(registry.Count==requested)break;Vector3 pos=grid.CellToWorld(c)+new Vector3((pass%3-1)*.23f,(pass/3-1)*.23f,0);
                var prefab=w.EndlessSettings.enemies[i++%w.EndlessSettings.enemies.Length];
                if(sp.TrySpawnAt(prefab,pos,out var enemy)){enemy.Health.Initialize(100000);enemy.Movement.Modifiers.Set(sp,.15f);}
            }
            Check(registry.Count==requested,"Exact requested population "+requested);Check(w.TryStartWave(),"Stress wave");
            Time.timeScale=1;await Task.Delay(1000);
            var times=new List<double>();var allocations=new List<long>();int min=registry.Count,max=registry.Count,frames=-1;
            long memBefore=GC.GetTotalMemory(false);int gc0=GC.CollectionCount(0);long searches=b.Battlefield.Paths.Searches;
            using(var main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1))
            using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1))
            using(var scripts=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"BehaviourUpdate",1))
            {
                var clock=Stopwatch.StartNew();while(clock.Elapsed.TotalSeconds<5)
                {
                    await Task.Delay(4);if(Time.frameCount==frames)continue;frames=Time.frameCount;
                    if(main.Valid&&main.LastValue>0)times.Add(main.LastValue/1000000.0);if(gc.Valid)allocations.Add(gc.LastValue);
                    min=Math.Min(min,registry.Count);max=Math.Max(max,registry.Count);
                }
                times.Sort();Check(times.Count>10,"Profiler samples");
                Capture(label+"-"+requested);Time.timeScale=0;
                rows.Add(new{requested,min,max,resolution="1920x1080",vfx=SettingsService.Instance.Current.vfxDensity.ToString(),frames=times.Count,medianMs=times[times.Count/2],p95Ms=times[(int)(times.Count*.95)],maxMs=times.Last(),meanGcBytes=allocations.Count>0?allocations.Average():0,gcCollections=GC.CollectionCount(0)-gc0,managedGrowth=GC.GetTotalMemory(false)-memBefore,searches=b.Battlefield.Paths.Searches-searches,carriers=w.Landings.ActiveCarriers,areaShots=Object.FindObjectsByType<AreaProjectileWeapon>(FindObjectsSortMode.None).Sum(t=>t.ShotsFired),directShots=Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None).Sum(t=>t.ShotsFired)});
            }
            if(requested==400)CpuSamples(registry,b);
            b.Game.EndRun();foreach(var e in registry.Active.ToArray())e.Health.TakeDamage(int.MaxValue);await Task.Delay(250);
            Check(registry.Count==0&&sp.PooledEnemies<=400,"Enemy cleanup");Check(Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None).Length==0,"Projectile cleanup");
            Save(label,new{map,method="Editor, natural Update x1, 1s warmup/5s sample, mixed six-creature combat, real carrier scheduling and all weapon types. Explicit 100000 RSD, stress-only 100000 enemy HP and 0.15 movement modifier retain load. Initial requested population is exact; arrivals may raise it to 400. Unique rendered frame samples. Not standalone FPS certification.",rows});
        }
        return "Completed stress sweep";
    }
    static void CpuSamples(EnemyRegistry registry,BuildManager builder)
    {
        var rows=new List<object>();var workspace=new ClusterTargeting.Workspace();Vector3 origin=builder.Battlefield.Grid.CellToWorld(new Vector2Int(18,8));
        Action<string,int,Action> sample=(label,count,action)=>
        {
            for(int i=0;i<20;i++)action();long before=GC.GetAllocatedBytesForCurrentThread();var clock=Stopwatch.StartNew();for(int i=0;i<count;i++)action();clock.Stop();
            rows.Add(new{label,iterations=count,meanMs=clock.Elapsed.TotalMilliseconds/count,allocatedBytes=GC.GetAllocatedBytesForCurrentThread()-before});
        };
        sample("Registry acquisition, 400 mixed targets",1000,()=>registry.FindTarget(origin,20,TargetingMode.HighValue));
        sample("Cluster selection, up to 16 candidates x 400 targets",200,()=>ClusterTargeting.TrySelect(registry,origin,0,20,3,.65f,16,workspace,out _));
        var path=new List<Vector2Int>();sample("Cached route request",1000,()=>builder.Battlefield.Paths.TryFindPath(new Vector2Int(6,9),new Vector2Int(31,5),path));
        sample("Movement batch, 400 organisms",100,()=>{for(int i=0;i<registry.Active.Count;i++)registry.Active[i].Movement.Tick(.0001f);});
        sample("Structure attack batch, 400 organisms",100,()=>{for(int i=0;i<registry.Active.Count;i++)registry.Active[i].StructureAttack.Tick(.0001f);});
        var director=new AssaultPressureDirector();var settings=Find<WaveManager>().EndlessSettings;sample("Pressure policy",1000,()=>director.Tick(.2f,settings,40,400,1200,8,40,true));
        Save("cpu-samples",new{method="Warm subsystem microbenchmarks in the 400-unit stress fixture, TimeScale 0. Movement/attack steps advance only .0001s per iteration. Stopwatch and calling harness are included; these are query/batch costs, not whole-frame attribution. Allocation includes stopwatch object; no claim of zero whole-game allocations.",rows});
    }
    public static async Task<string> Bosses()
    {
        var rows=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})foreach(string kind in new[]{"Dredger","Broodmother"})foreach(string force in new[]{"midgame","specialized"})
        {
            await Load(map);await SetSize(1920,1080);Find<BorderBreachService>().ConfigureThreshold(0);
            var b=Find<BuildManager>();b.Economy.AddMoney(force=="midgame"?6500:12000);
            foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",9,y,2);Place("sniper-tower",15,y,2);Place("autocannon",18,y,force=="midgame"?1:3);Place("flamethrower",11,y+1,1);if(force=="specialized"){Place("sniper-tower",20,y,3);Place("rocket",13,y+2,1);}}
            int wave=kind=="Dredger"?25:50;Prepare(wave);
            // Isolated boss exposure. Full concurrent encounter pressure is tested separately.
            var sp=Find<EnemySpawner>();var current=Find<WaveManager>().CurrentWave;sp.ConfigureWave(400,current.HealthMultiplier,current.SpeedMultiplier,current.HealthMultiplier);sp.ReinforcementsEnabled=true;
            var bossPrefab=Find<WaveManager>().EndlessSettings.bossEncounters.bosses.First(p=>p.Definition.Kind.ToString()==kind);
            EnemyController boss=null;
            foreach(int y in new[]{9,4,14,6,12})if(sp.TrySpawnAt(bossPrefab,b.Battlefield.Grid.CellToWorld(new Vector2Int(7,y)),out boss))break;
            Check(boss!=null,"boss spawn");double? deathAt=null;float start=Time.time;boss.Health.Died+=()=>deathAt=Time.time-start;
            Time.timeScale=1;var clock=Stopwatch.StartNew();while(boss!=null&&boss.IsCombatActive&&clock.Elapsed.TotalSeconds<75)await Task.Delay(50);
            Time.timeScale=0;Capture("boss-"+kind+"-"+force+"-"+map);
            rows.Add(new{map,kind,force,paid=b.Economy.TotalSpent,hp=boss.Health.MaxHealth,ttkSeconds=deathAt,alive=boss.IsCombatActive,children=boss.GetComponent<BroodmotherSpawn>()?.TotalSpawned,breaches=Find<RunStatistics>().TotalBreaches});
            Save("boss-ttk",new{method="Natural Update x1, zero research. Moving isolated boss at authored base HP, normal armor/structure attacks and real defense payments. Excludes escorts/other-wave damage competition; functional encounter suites test both maps separately. Midgame and specialized fixtures disclose granted budgets.",rows});
        }
        return "Completed boss TTK";
    }
    public static async Task<string> FinalObservation()
    {
        await Bosses();await Encounters();await EffectiveStats();return "Final moving bosses, full encounters and effective UI stats observed";
    }
    public static async Task<string> VfxReview()
    {
        await Load("DanubeIndustrialPort");await SetSize(1920,1080);Prepare(24);Find<BorderBreachService>().ConfigureThreshold(0);
        var b=Find<BuildManager>();b.Economy.AddMoney(9000);foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",9,y,2);Place("flamethrower",11,y,2);Place("mortar",17,y,2);Place("rocket",20,y,1);}
        Check(Find<WaveManager>().ForceEvent("mass-emergence"),"VFX event");Check(Find<WaveManager>().TryStartWave(),"VFX start");
        var rows=new List<object>();foreach(int density in new[]{0,1,2})
        {
            var s=SettingsService.Instance.Current;Check(SettingsService.Instance.Save(new GameSettings{width=s.width,height=s.height,fullscreen=s.fullscreen,vsync=s.vsync,rewardPopups=s.rewardPopups,vfxDensity=density}),"Density save");
            Time.timeScale=1;await Task.Delay(3500);Time.timeScale=0;Capture("vfx-density-"+density);
            rows.Add(new{density,active=Find<EnemyRegistry>().Count,kills=Find<RunStatistics>().TotalKills,areaShots=Object.FindObjectsByType<AreaProjectileWeapon>(FindObjectsSortMode.None).Sum(p=>p.ShotsFired),directShots=Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None).Sum(p=>p.ShotsFired)});
        }
        return Save("vfx-review",new{method="One continuous natural Update Mass Emergence encounter, isolated disclosed 9000 RSD build grant. Settings change in real time; count/kill differences reflect advancing battle, not density damage modifiers.",rows});
    }
    public static async Task<string> EffectiveStats()
    {
        await Load("CoastalDefense_TopDown");Find<BuildManager>().Economy.AddMoney(2000);
        var sniper=Place("sniper-tower",15,8,3);Check(sniper!=null&&sniper.Level==3,"Selected upgraded sniper");Find<StructureSelection>().Select(sniper);
        var root=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement;var rows=new List<object>();
        foreach(var size in new[]{new Vector2Int(1366,768),new Vector2Int(1600,900),new Vector2Int(1920,1080)})
        foreach(string id in new[]{"sniper-tower","flamethrower"})
        {
            await SetSize(size.x,size.y);var button=root.Q<Button>("build-"+id);Check(button!=null,"Build tooltip button");
            using(var e=PointerOverEvent.GetPooled()){e.target=button;button.SendEvent(e);}await Task.Delay(150);
            var tooltip=root.Q<Label>("runtime-tooltip");Check(tooltip!=null&&tooltip.resolvedStyle.display!=DisplayStyle.None,"Visible runtime tooltip");
            var bounds=tooltip.worldBound;var viewport=root.worldBound;Check(bounds.xMin>=viewport.xMin&&bounds.yMin>=viewport.yMin&&bounds.xMax<=viewport.xMax+1&&bounds.yMax<=viewport.yMax+1,"Tooltip inside scaled UI viewport");
            var stats=root.Q<Label>("structure-stats");Check(stats.text.Contains("360")&&stats.text.Contains("100"),"Effective damage and penetration displayed");
            Capture("effective-"+id+"-"+size.x+"x"+size.y);rows.Add(new{width=size.x,height=size.y,id,tooltip=tooltip.text,selected=stats.text,bounds=new{x=bounds.x,y=bounds.y,width=bounds.width,height=bounds.height},viewport=new{x=viewport.x,y=viewport.y,width=viewport.width,height=viewport.height}});
        }
        return Save("effective-stats",new{method="Real HUD and runtime PointerOver bindings, upgraded Level III Sniper selection, native Game View resolutions; no desktop input.",rows});
    }
    public static async Task<string> Encounters()
    {
        var rows=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})foreach(int wave in new[]{25,50})
        {
            await Load(map);await SetSize(1920,1080);Prepare(wave);var b=Find<BuildManager>();b.Economy.AddMoney(9000-b.Economy.Money);
            foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",8,y,2);Place("flamethrower",11,y,2);Place("sniper-tower",15,y,2);Place("autocannon",18,y,2);Place("mortar",22,y,2);Place("rocket",24,y,1);}
            var registry=Find<EnemyRegistry>();var w=Find<WaveManager>();EnemyController boss=null;float born=0;double? ttk=null;int peak=0;
            registry.EnemyRegistered+=e=>{if(e.Definition is BossDefinition){boss=e;born=Time.time;}};
            registry.EnemyKilled+=e=>{if(e==boss)ttk=Time.time-born;};
            bool capturedBoss=false;var timings=new List<double>();var clock=Stopwatch.StartNew();Check(w.TryStartWave(),"Encounter start");Time.timeScale=2;
            using(var main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1))
            {
                int frame=-1;while(b.Game.IsPlaying&&w.State!=WaveState.WaveComplete&&clock.Elapsed.TotalSeconds<75)
                {await Task.Delay(10);peak=Math.Max(peak,registry.Count);if(!capturedBoss&&boss!=null&&boss.IsCombatActive&&Time.time-born>2){Capture("active-boss-"+wave+"-"+map);capturedBoss=true;}if(frame!=Time.frameCount&&main.Valid&&main.LastValue>0){frame=Time.frameCount;timings.Add(main.LastValue/1000000.0);}}
            }
            Time.timeScale=0;Capture("encounter-"+wave+"-"+map);timings.Sort();
            rows.Add(new{map,wave,paid=b.Economy.TotalSpent,result=w.State.ToString(),bossSpawned=boss!=null,ttkSeconds=ttk,bossHealth=boss?.Health.CurrentHealth,peak,kills=Find<RunStatistics>().TotalKills,homicides=Find<RunStatistics>().Homicides,children=boss?.GetComponent<BroodmotherSpawn>()?.TotalSpawned,p95Ms=timings.Count>0?timings[(int)(timings.Count*.95)]:0,secondsReal=clock.Elapsed.TotalSeconds});
            Save("full-encounters",new{method="Natural Update x2, normal 1000 Homicides threshold, full seeded Wave 25/50 including normal units and escorts. Disclosed 9000 RSD preparation budget, zero research. No test damage, HP, income or movement changes. Failures and unresolved bosses are retained.",rows});
        }
        return "Both full encounters observed on both maps";
    }
}
