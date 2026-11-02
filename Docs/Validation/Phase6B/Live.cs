using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using Unity.Profiling;
using UnityEditor;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using Object=UnityEngine.Object;
public static class Phase6BLive
{
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static string Folder=>Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/Phase6B");
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static async Task Load(string map)
    {RunSession.Pending=null;Application.runInBackground=true;EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();await SceneManager.LoadSceneAsync(map);await Task.Delay(200);Time.timeScale=0;Find<BorderBreachService>().ConfigureThreshold(0);}
    public static string Isolate(){LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefendersPhase6B",Guid.NewGuid().ToString()));return "Isolated QA persistence directory";}
    public static void Capture(string name)
    {
        var camera=Camera.main;var target=new RenderTexture(1920,1080,24);var old=camera.targetTexture;var active=RenderTexture.active;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(Folder,name+".png"),image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(image);Object.DestroyImmediate(target);
    }
    static void HudCapture(string name)
    {var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);var encode=type.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);File.WriteAllBytes(Path.Combine(Folder,name+".png"),(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));}
    static async Task SetSize(int width,int height)
    {
        const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        var type=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.Public).GetValue(null);
        var group=type.GetProperty("currentGroup",flags).GetValue(sizes);var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));string label="Phase6B "+width+"x"+height;
        var labels=(string[])group.GetType().GetMethod("GetDisplayTexts",flags).Invoke(group,null);int index=Array.FindIndex(labels,s=>s.Contains("("+width+"x"+height+")")||s.StartsWith(label));
        if(index<0){var sizeType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");var constructor=sizeType.GetConstructors(flags).First(c=>c.GetParameters().Length==4);var size=constructor.Invoke(new[]{Enum.Parse(constructor.GetParameters()[0].ParameterType,"FixedResolution"),(object)width,height,label});group.GetType().GetMethod("AddCustomSize",flags).Invoke(group,new[]{size});index=(int)group.GetType().GetMethod("GetTotalCount",flags).Invoke(group,null)-1;}
        view.GetType().GetProperty("selectedSizeIndex",flags).SetValue(view,index);view.Focus();view.Repaint();await Task.Delay(400);Check(Screen.width==width&&Screen.height==height,"Actual resolution");
    }
    public static async Task<string> Screens()
    {
        Isolate();await SetSize(1920,1080);var results=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            await Load(map);var waves=Find<WaveManager>();var build=Find<BuildManager>();build.Economy.AddMoney(20000);
            foreach(int y in new[]{4,9,15})foreach(int x in new[]{8,9})build.TryPlaceDefense(Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest"),new Vector2Int(x,y),out _);
            var snapshot=Find<RunSession>().Capture();snapshot.wave=24;snapshot.completedWaves=23;waves.RestorePreparation(snapshot);Check(waves.ForceEvent("mass-emergence"),"Mass event");waves.Events.Tick(100);Check(waves.TryStartWave(),"Mass start");Time.timeScale=1;
            await Task.Delay(5500);Time.timeScale=0;HudCapture(map=="CoastalDefense_TopDown"?"aquatic-hud":"industrial-aquatic-hud");Capture(map+"-mass-emergence");
            results.Add(new{map,active=Find<EnemyRegistry>().Count,carriers=waves.Landings.ActiveCarriers,arrivals=waves.Landings.ArrivalCounts.ToArray(),resolution=Screen.width+"x"+Screen.height});
        }
        foreach(int number in new[]{25,50})
        {
            await Load("DanubeIndustrialPort");var waves=Find<WaveManager>();var d=Find<RunSession>().Capture();d.wave=number;d.completedWaves=number-1;waves.RestorePreparation(d);Check(waves.TryStartWave(),"Boss screen start");Time.timeScale=1;
            for(int i=0;i<400&&Find<BossEncounterManager>().ActiveBoss==null;i++)await Task.Delay(20);await Task.Delay(600);Time.timeScale=0;Check(Find<BossEncounterManager>().ActiveBoss!=null,"Boss HUD appeared");HudCapture(number==25?"dredger-hud":"broodmother-hud");
        }
        string json=JsonConvert.SerializeObject(results,Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"screens.json"),json);return json;
    }
    public static async Task<string> EarlyBothMaps()
    {
        Isolate();var results=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            await Load(map);var waves=Find<WaveManager>();var registry=Find<EnemyRegistry>();var build=Find<BuildManager>();var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");
            // Prepared inspection budget, explicitly not an earned-income balance claim.
            build.Economy.AddMoney(15000);
            foreach(int y in new[]{4,9,15})for(int x=8;x<=9;x++)build.TryPlaceDefense(mg,new Vector2Int(x,y),out _);
            for(int number=1;number<=5;number++)
            {
                waves.Events.Tick(100);Check(waves.TryStartWave(),"Start "+map+" "+number);int planned=waves.CurrentWave.ScheduledCount;long before=Find<RunStatistics>().TotalKills+Find<RunStatistics>().TotalBreaches;
                int ticks=0;
                for(;ticks<12000&&waves.State!=WaveState.WaveComplete;ticks++)
                {
                    waves.Tick(.1f);foreach(var enemy in registry.Active.ToArray())enemy.Tick(.1f);foreach(var weapon in Object.FindObjectsByType<MachineGunCombat>(FindObjectsSortMode.None))weapon.Tick(.1f);
                    foreach(var shot in Object.FindObjectsByType<CorrosiveSpit>(FindObjectsSortMode.None))shot.Tick(.1f);
                    if(ticks==45&&number==5)Capture(map+"-wave5");if(ticks%20==0)await Task.Yield();
                }
                Check(waves.State==WaveState.WaveComplete,"Wave stalled "+number);Check(Find<RunStatistics>().TotalKills+Find<RunStatistics>().TotalBreaches-before==planned,"Resolve mismatch");
                results.Add(new{map,wave=number,planned,ticks,kills=Find<RunStatistics>().TotalKills,breaches=Find<RunStatistics>().TotalBreaches,arrivals=waves.Landings.ArrivalCounts.ToArray()});waves.Tick(2);await Task.Yield();
                Check(Find<RunSession>().SaveNow(),"Preparation autosave "+map);
            }
        }
        string json=JsonConvert.SerializeObject(results,Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"early-both-maps.json"),json);return json;
    }
    public static async Task<string> Encounters()
    {
        Isolate();var results=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        foreach(int number in new[]{25,50})
        {
            await Load(map);var waves=Find<WaveManager>();var d=Find<RunSession>().Capture();d.wave=number;d.completedWaves=number-1;waves.RestorePreparation(d);Check(waves.TryStartWave(),"Boss start");var registry=Find<EnemyRegistry>();
            int tick=0;bool captured=false;int spawned=0;
            for(;tick<20000&&waves.State!=WaveState.WaveComplete;tick++)
            {
                waves.Tick(.1f);
                foreach(var enemy in registry.Active.ToArray())
                {
                    if(enemy.Definition is BossDefinition)
                    {
                        var ability=enemy.GetComponent<BroodmotherSpawn>();enemy.Tick(.1f);if(ability!=null)spawned=Math.Max(spawned,ability.TotalSpawned);
                        if(!captured){Capture(map+"-"+enemy.Definition.Kind);captured=true;}
                        if(tick%5==0)enemy.Health.TakeDefenseDamage(400);
                    }
                    else {enemy.Tick(.1f);if(tick%15==0)enemy.Health.TakeDefenseDamage(int.MaxValue);}
                }
                if(tick%20==0)await Task.Yield();
            }
            Check(waves.State==WaveState.WaveComplete,"Boss wave stalled "+map+number);Check(captured,"Boss not deployed");Check(Find<RunStatistics>().BossesKilled==1,"Boss kill accounting");
            results.Add(new{map,wave=number,tick,spawned,kills=Find<RunStatistics>().TotalKills,breaches=Find<RunStatistics>().TotalBreaches,pending=waves.PendingCount,alive=waves.AliveCount,arrivals=waves.Landings.ArrivalCounts.ToArray()});
        }
        string json=JsonConvert.SerializeObject(results,Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"boss-both-maps.json"),json);return json;
    }
    public static async Task<string> Stress()
    {
        Isolate();var results=new List<object>();
        foreach(int targetCount in new[]{100,200,350})
        {
            await Load("CoastalDefense_TopDown");var spawn=Find<EnemySpawner>();spawn.ConfigureWave(400,1,1);spawn.ConfigureDeployment(.1f);var larva=Find<WaveManager>().EndlessSettings.enemies.First(e=>e.Definition.Kind==EnemyKind.Broodling);var registry=Find<EnemyRegistry>();
            for(int x=5;x<31&&registry.Count<targetCount;x++)for(int y=0;y<18&&registry.Count<targetCount;y++)spawn.TrySpawnAt(larva,spawn.Battlefield.Grid.CellToWorld(new Vector2Int(x,y)),out _);
            Check(registry.Count==targetCount,"Stress count");Capture("broodling-"+targetCount);Time.timeScale=1;
            // Hold the requested population in-frame while retaining normal Update/path/animation work.
            // This is a disclosed stationary-load profile, not ordinary-speed survival balance.
            foreach(var e in registry.Active)e.Movement.Modifiers.Set(typeof(Phase6BLive),.05f);
            await Task.Delay(300);
            using(var main=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1))
            using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1))
            {
                var ms=new List<double>();var allocations=new List<long>();var clock=Stopwatch.StartNew();int min=registry.Count,max=registry.Count;
                while(clock.Elapsed.TotalSeconds<3){await Task.Delay(8);if(main.Valid&&main.LastValue>0)ms.Add(main.LastValue/1000000.0);if(gc.Valid)allocations.Add(gc.LastValue);min=Math.Min(min,registry.Count);max=Math.Max(max,registry.Count);}
                ms.Sort();Check(ms.Count>10,"No timing samples");results.Add(new{requested=targetCount,min,max,samples=ms.Count,medianMs=ms[ms.Count/2],p95Ms=ms[(int)(ms.Count*.95)],maxMs=ms.Last(),meanGcBytes=allocations.Count>0?allocations.Average():0,created=spawn.CreatedEnemies,reused=spawn.ReusedEnemies});
            }
            Time.timeScale=0;foreach(var e in registry.Active.ToArray())e.Health.TakeDefenseDamage(int.MaxValue);await Task.Delay(100);Check(registry.Count==0,"Stress registry cleanup");Check(spawn.PooledEnemies<=400,"Pool bound");
        }
        string json=JsonConvert.SerializeObject(results,Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"broodling-stress.json"),json);return json;
    }
    public static async Task<string> EarnedEarly()
    {
        Isolate();var results=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            await Load(map);Find<BorderBreachService>().ConfigureThreshold(1000);var build=Find<BuildManager>();var waves=Find<WaveManager>();var registry=Find<EnemyRegistry>();var stats=Find<RunStatistics>();var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");
            Check(build.Economy.Money==500,"Normal starting money");foreach(int y in new[]{4,9,15})Check(build.TryPlaceDefense(mg,new Vector2Int(8,y),out _)==PlacementResult.Valid,"Opening MG");
            for(int number=1;number<=5;number++)
            {
                waves.Events.Tick(100);Check(waves.TryStartWave(),"Earned start");int ticks=0;
                for(;ticks<12000&&Find<GameStateController>().IsPlaying&&waves.State!=WaveState.WaveComplete;ticks++)
                {
                    waves.Tick(.1f);foreach(var turret in Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))turret.Tick(.1f);foreach(var e in registry.Active.ToArray())e.Tick(.1f);
                    if(ticks%20==0)foreach(var cell in new[]{new Vector2Int(8,11),new Vector2Int(16,5),new Vector2Int(17,16),new Vector2Int(17,11),new Vector2Int(22,5),new Vector2Int(22,11),new Vector2Int(8,6),new Vector2Int(8,12)})
                        if(build.Economy.Money>=mg.Cost&&build.Validate(mg,cell)==PlacementResult.Valid)build.TryPlaceDefense(mg,cell,out _);
                    if(ticks%10==0)await Task.Delay(1);
                }
                results.Add(new{map,wave=number,state=waves.State.ToString(),ticks,kills=stats.TotalKills,homicides=stats.Homicides,money=build.Economy.Money});
                if(!Find<GameStateController>().IsPlaying)break;
                waves.Tick(2);for(int t=0;t<100&&waves.Landings.ActiveCarriers>0;t++)waves.Landings.Tick(1);
                foreach(var defense in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None)){build.TryUpgrade(defense);build.TryUpgrade(defense);build.TryRepair(defense);}
                foreach(var cell in new[]{new Vector2Int(14,5),new Vector2Int(15,11),new Vector2Int(17,16),new Vector2Int(22,5),new Vector2Int(22,11)})if(build.Economy.Money>=mg.Cost&&build.Validate(mg,cell)==PlacementResult.Valid)build.TryPlaceDefense(mg,cell,out _);
            }
            Capture(map+"-earned-result");
        }
        string json=JsonConvert.SerializeObject(new{fixture="Real 500 RSD start, threshold 1000, earned income only; explicit .1-second ticks and scripted purchases/upgrades. Not human mouse-only balance certification.",results},Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"earned-early.json"),json);return json;
    }
    public static async Task<string> LegacyContinue()
    {
        Isolate();var results=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})foreach(string id in new[]{"swarm-surge","armored-migration"})
        {
            await Load(map);var waves=Find<WaveManager>();var build=Find<BuildManager>();build.Economy.AddMoney(2000);var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");Check(build.TryPlaceDefense(mg,new Vector2Int(14,5),out var defense)==PlacementResult.Valid,"Migration defense");build.TryUpgrade(defense);build.TryUpgrade(defense);defense.TakeDamage(30);
            var data=Find<RunSession>().Capture();data.wave=24;data.completedWaves=23;waves.RestorePreparation(data);Check(waves.ForceEvent(id),"Migration event");data=Find<RunSession>().Capture();data.saveVersion=map=="CoastalDefense_TopDown"?1:2;data.mapId=data.saveVersion==1?null:data.mapId;
            data.statistics.kills=new long[]{10,20,30,40,50,60,70,80,90};
            string Old(string value)=>value=="swarm-surge"?"pirate-raid":value=="armored-migration"?"militant-assault":value=="mass-emergence"?"mass-landing":value=="brood-tide"?"runner-swarm":value=="deep-water-disturbance"?"heavy-assault":value=="concentrated-emergence"?"concentrated-landing":value;
            data.events.currentId=Old(data.events.currentId);data.events.mostRecent=Old(data.events.mostRecent);for(int i=0;i<data.events.recent.Count;i++)data.events.recent[i]=Old(data.events.recent[i]);foreach(var list in new[]{data.events.history,data.events.counts,data.events.cooldowns})foreach(var item in list)item.id=Old(item.id);
            string run=data.runId;int money=data.money,health=defense.CurrentHealth;Check(LocalGameData.Files.Write("active_run",data,out _),"Legacy write");
            string before=File.ReadAllText(LocalGameData.Files.PathFor("active_run"));Check(new SaveGameService(LocalGameData.Files).Load(out var converted,out _),"Legacy service load");Check(converted.saveVersion==3&&converted.events.currentId==id,"Legacy conversion");Check(File.ReadAllText(LocalGameData.Files.PathFor("active_run"))==before,"Read must not rewrite durable save");
            await SceneManager.LoadSceneAsync("MainMenu");await Task.Delay(200);Find<ProductMenuController>().Continue();
            for(int i=0;i<300&&(SceneManager.GetActiveScene().name!=map||Find<RunSession>()==null||!Find<RunSession>().Ready);i++)await Task.Delay(20);Time.timeScale=0;
            Check(SceneManager.GetActiveScene().name==map&&Find<RunSession>().RunId==run,"Legacy Continue map/run");var actual=Find<RunSession>().Capture();Check(actual.saveVersion==3&&actual.money==money&&actual.statistics.kills.Sum()==450,"Legacy totals/money");Check(actual.events.currentId==id,"Committed event restoration");Check(actual.structures.Count==1&&actual.structures[0].level==3&&actual.structures[0].health==health,"Upgraded damaged structure restoration");
            results.Add(new{map,version=data.saveVersion,committedOld=Old(id),restored=id,money,health,level=3,kills=actual.statistics.kills.Sum(),newVersion=actual.saveVersion});
        }
        string json=JsonConvert.SerializeObject(results,Formatting.Indented);File.WriteAllText(Path.Combine(Folder,"legacy-continue.json"),json);return json;
    }
}
