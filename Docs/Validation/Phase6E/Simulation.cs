using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

public static class Phase6ESimulation
{
    const string Folder="Docs/Validation/Phase6E/";
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static void Check(bool ok,string m){if(!ok)throw new Exception(m);}
    static string Save(string name,object value){File.WriteAllText(Folder+name+".json",JsonConvert.SerializeObject(value,Formatting.Indented));return "Written "+name;}
    static async Task Load(string map,string profile="none")
    {
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefenders6E",Guid.NewGuid().ToString()));
        if(profile=="full")LocalGameData.Profile.DebugUnlock(ResearchCatalog.Load(),true,null,out _);
        if(profile=="partial")
        {
            LocalGameData.Profile.DebugGrantRP(130,out _);var tree=new ResearchTreeService(LocalGameData.Profile,ResearchCatalog.Load(),()=>true);
            foreach(string id in new[]{"emergency-fund","reinforced-emplacements","improved-ammunition","river-monitoring","advance-warning","supply-contracts"})Check(tree.TryPurchase(id,out var error),error);
        }
        LocalGameData.Tutorial.SetEnabled(false);
        RunSession.Pending=null;Application.runInBackground=true;await SceneManager.LoadSceneAsync(map);await Task.Delay(80);Time.timeScale=0;
    }
    static void Prepare(int wave)
    {var s=Find<RunSession>().Capture();s.wave=wave;s.completedWaves=wave-1;s.seed=1977;s.events=new EventSaveData{nextWave=wave+1};Find<WaveManager>().RestorePreparation(s);}
    static DefenseBase Place(string id,int x,int y,int level=1)
    {
        var b=Find<BuildManager>();var def=Find<BuildHud>().Defenses.First(d=>d.Id==id);
        // Closest legal placement to the intended lane; all footprints validated by production API.
        var cells=new List<Vector2Int>();for(int dx=-4;dx<=4;dx++)for(int dy=-3;dy<=3;dy++)cells.Add(new Vector2Int(x+dx,y+dy));
        foreach(var c in cells.OrderBy(c=>Math.Abs(c.x-x)+Math.Abs(c.y-y)))
            if(b.TryPlaceDefense(def,c,out var d)==PlacementResult.Valid){for(int l=1;l<level;l++)b.TryUpgrade(d);return d;}
        return null;
    }
    static void Tick(float dt)
    {
        Find<WaveManager>().Tick(dt);
        foreach(var t in Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))t.Tick(dt);
        foreach(var t in Object.FindObjectsByType<AreaProjectileWeapon>(FindObjectsSortMode.None))t.Tick(dt);
        foreach(var t in Object.FindObjectsByType<FlameWeapon>(FindObjectsSortMode.None))t.Tick(dt);
        foreach(var t in Object.FindObjectsByType<LandMine>(FindObjectsSortMode.None))t.Tick(dt);
        foreach(var e in Find<EnemyRegistry>().Active.ToArray()){e.Tick(dt);if(e!=null&&e.isActiveAndEnabled)e.GetComponent<EnemyDamageOverTime>()?.Tick(dt);}
        foreach(var p in Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None))p.Tick(dt);
        foreach(var p in Object.FindObjectsByType<CorrosiveSpit>(FindObjectsSortMode.None))
        {
            p.Tick(dt);
            // The fixture holds Time.timeScale=0. Finish the already-resolved cosmetic
            // delayed destruction explicitly; real Update samples use its normal .18s lifetime.
            if((bool)typeof(CorrosiveSpit).GetField("resolved",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(p))Object.Destroy(p.gameObject);
        }
    }
    static async Task<object> Fight(int limit=12000,bool fieldRepair=false)
    {
        var b=Find<BuildManager>();var w=Find<WaveManager>();var r=Find<EnemyRegistry>();var st=Find<RunStatistics>();var sp=Find<EnemySpawner>();
        long k=st.TotalKills,h=st.Homicides,earned=b.Economy.TotalEarned,spent=b.Economy.TotalSpent;int spawned=sp.TotalSpawned,peak=0,ticks=0;
        w.Events.Tick(100);Check(w.TryStartWave(),"start wave "+w.CurrentWaveNumber);
        for(;ticks<limit&&b.Game.IsPlaying&&w.State!=WaveState.WaveComplete;ticks++)
        {
            Tick(.1f);peak=Math.Max(peak,r.Count);
            if(fieldRepair&&ticks%20==0)foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None))if(d.IsAlive&&d.CurrentHealth<d.MaxHealth*.5f)b.TryRepair(d);
            if(ticks%5==0)await Task.Yield();
        }
        if(w.State==WaveState.WaveComplete)Check(w.AliveCount==0&&w.PendingCount==0&&r.Count==0,"Completed wave leaves no registered/alive/pending organisms");
        return new{wave=w.CurrentWaveNumber,state=w.State.ToString(),seconds=ticks*.1f,kills=st.TotalKills-k,homicides=st.Homicides-h,spawned=sp.TotalSpawned-spawned,peak,money=b.Economy.Money,earned=b.Economy.TotalEarned-earned,spent=b.Economy.TotalSpent-spent,structures=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Count(d=>d.IsAlive),registry=r.Count,alive=w.AliveCount,pending=w.PendingCount,pooledEnemies=sp.PooledEnemies,managedBytes=GC.GetTotalMemory(false),allocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),stalled=ticks>=limit};
    }
    public static async Task<string> Archetypes(string label,int wave=20,string eventId="",string profile="none",string onlyStrategy="")
    {
        var rows=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})foreach(string strategy in new[]{"mg","wire-flame","precision","area","mixed","wall"})
        {
            if(onlyStrategy!=""&&strategy!=onlyStrategy)continue;
            await Load(map,profile);Prepare(wave);var b=Find<BuildManager>();b.Economy.AddMoney(9000-b.Economy.Money);
            foreach(int y in new[]{4,9,14})
            {
                if(strategy=="mg")foreach(int x in new[]{8,11,15,19})Place("machine-gun-nest",x,y,3);
                if(strategy=="wire-flame"){foreach(int x in new[]{7,9,11}){Place("barbed-wire",x,y,3);Place("barbed-wire",x,y+1,3);}Place("flamethrower",12,y,3);Place("flamethrower",10,y+2,3);}
                if(strategy=="precision"){Place("sniper-tower",13,y,2);Place("autocannon",10,y,2);Place("autocannon",17,y,1);}
                if(strategy=="area"){Place("mortar",16,y,2);Place("rocket",12,y,2);}
                if(strategy=="mixed"){Place("machine-gun-nest",8,y,2);Place("barbed-wire",9,y+1,2);Place("flamethrower",11,y+1,2);Place("sniper-tower",15,y,2);Place("autocannon",17,y,1);Place("mortar",18,y+2,1);}
            }
            if(strategy=="wall")
            {
                var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");
                for(int y=0;y<18;y++)if(b.TryPlaceDefense(mg,new Vector2Int(10,y),out var wall)==PlacementResult.Valid){b.TryUpgrade(wall);b.TryUpgrade(wall);}
                foreach(int y in new[]{4,9,14})Place("barbed-wire",9,y,3);
            }
            else
            {
                // Spend comparable budgets without adding another archetype's counters.
                string extra=strategy=="mg"?"machine-gun-nest":strategy=="wire-flame"?"flamethrower":strategy=="precision"?"autocannon":strategy=="area"?"mortar":"sniper-tower";
                foreach(int x in new[]{21,24,27,16,13})foreach(int y in new[]{2,6,11,15})Place(extra,x,y,strategy=="area"?2:3);
            }
            long investment=b.Economy.TotalSpent;if(!string.IsNullOrEmpty(eventId))Check(Find<WaveManager>().ForceEvent(eventId),"force "+eventId);
            rows.Add(new{map,strategy,profile,budget=9000,investment,result=await Fight(12000,strategy=="wall")});Save(label,new{method="Prepared 9000 RSD budget ceiling (actual investment disclosed), production commands, .1s ticks with all weapon/projectile/burn clocks; yield every 5 ticks for navigation budget/pool cleanup. Wall buys a complete legal x=10 barrier and attempts repairs below 50% HP every 2 simulated seconds from remaining/earned funds. Threshold 1000, no enemy health overrides. Comparative fixture, not earned progression or native timing.",wave,eventId,rows});
        }
        return "Completed "+rows.Count+" archetypes: "+label;
    }
    public static async Task<string> Opening(string label,int x=9)
    {
        var rows=new List<object>();foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {await Load(map);Prepare(1);foreach(int y in new[]{4,9,14})Place("machine-gun-nest",x,y);rows.Add(new{map,x,result=await Fight()});}
        return Save(label,rows);
    }
    public static async Task<string> Events()
    {
        string[] ids={"swarm-surge","armored-migration","fog","mass-emergence","concentrated-emergence","multi-sector","flooded-shore","brood-tide","deep-water-disturbance","ammunition-shortage","emergency-supply","field-engineers","calm-before-storm"};
        foreach(string id in ids)await Archetypes("event-"+id,24,id,id=="fog"||id=="mass-emergence"?"full":id=="armored-migration"||id=="flooded-shore"?"partial":"none","mixed");
        return "All thirteen events fought on both maps with representative none/partial/full profiles";
    }
    public static async Task<string> Profiles()
    {
        await Progress("final-neutral-50","none",50,"lean");
        await Progress("final-partial-25","partial",25,"lean");
        await Progress("final-full-50","full",50,"lean");
        return "Three earned-income profile comparisons complete";
    }
    public static async Task<string> FinalMatrix()
    {
        await Profiles();
        await Archetypes("final-archetypes-20",20);
        await Archetypes("final-archetypes-40",40);
        await Events();
        return "Final combat matrix rerun after burn/cadence corrections";
    }
    static void Click(string id)
    {
        var root=Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;var button=root.Q<Button>(id);
        Check(button!=null&&button.enabledSelf,"Product button "+id);using(var e=NavigationSubmitEvent.GetPooled()){e.target=button;button.SendEvent(e);}
    }
    public static async Task<string> FreshIntro()
    {
        Application.runInBackground=true;
        if(SceneManager.GetActiveScene().name=="MainMenu"){Click("menu-new");Click("map-river_border");Click("map-start");}
        for(int i=0;i<200&&(Find<RunSession>()?.Ready!=true||Find<ShorelineDefense.Tutorial.TutorialRuntime>()?.Manager==null);i++)await Task.Delay(50);
        Time.timeScale=0;Check(Find<BuildManager>().Economy.Money==500,"Fresh 500 RSD");
        return Save("fresh-intro",new{profile=LocalGameData.Files.Root,money=Find<BuildManager>().Economy.Money,tutorial=Find<ShorelineDefense.Tutorial.TutorialRuntime>().Manager.Active?.Definition.Id});
    }
    public static async Task<string> FreshLoop()
    {
        await Task.Delay(300);await FreshIntro();await NaturalFirstWave();return await FinishFreshProductLoop();
    }
    public static async Task<string> NaturalFirstWave()
    {
        var tutorial=Find<ShorelineDefense.Tutorial.TutorialRuntime>();
        for(int i=0;i<5;i++){if(tutorial.Manager.Active!=null)Click("tutorial-continue");await Task.Delay(450);}
        Click("build-mg");await Task.Delay(500);var placement=Find<PlacementController>();
        foreach(int y in new[]{4,9,14}){placement.HoverCell(new Vector2Int(8,y));Check(placement.TryPlaceCurrent(out _)==PlacementResult.Valid,"UI-selected MG placement");}
        for(int i=0;i<4;i++){if(tutorial.Manager.Active!=null)Click("tutorial-continue");await Task.Delay(450);}
        placement.Cancel();var w=Find<WaveManager>();var snapshot=Find<RunSession>().Capture();snapshot.seed=1977;w.RestorePreparation(snapshot);
        Click("start-wave");Click("speed-normal");var clock=System.Diagnostics.Stopwatch.StartNew();
        while(Find<BuildManager>().Game.IsPlaying&&w.CurrentWaveNumber==1&&clock.Elapsed.TotalSeconds<120)await Task.Delay(50);
        Time.timeScale=0;Check(Find<BuildManager>().Game.IsPlaying&&w.CurrentWaveNumber==2,"Natural first-wave survival");
        return Save("natural-first-wave",new{method="Fresh profile, real UI Toolkit buttons and placement commands, normal Update x1, seed 1977, 500 initial RSD. No injected income, health or damage. Desktop input intentionally left alone because of concurrent user activity.",secondsReal=clock.Elapsed.TotalSeconds,kills=Find<RunStatistics>().TotalKills,homicides=Find<RunStatistics>().Homicides,money=Find<BuildManager>().Economy.Money});
    }
    public static async Task<string> FinishFreshProductLoop()
    {
        Time.timeScale=0;var w=Find<WaveManager>();var b=Find<BuildManager>();var results=new List<object>();
        Check(w.State==WaveState.Preparation&&w.CurrentWaveNumber==2,"Natural first wave must have completed");
        string directory=LocalGameData.Files.Root;var first=new{wave=1,kills=Find<RunStatistics>().TotalKills,homicides=Find<RunStatistics>().Homicides,money=b.Economy.Money};
        for(int wave=2;wave<=10&&b.Game.IsPlaying;wave++)
        {
            foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None))b.TryRepair(d);
            foreach(int y in new[]{4,9,14})
            {
                if(wave<=4)Place("machine-gun-nest",wave==2?12:wave==3?15:10,y);
                if(wave==5)Place("flamethrower",11,y);
                if(wave==6||wave==7)Place("sniper-tower",15,y+1);
                if(wave==8)Place("flamethrower",12,y);
                if(wave==9)Place("mortar",19,y);
                if(wave==10)Place("autocannon",17,y);
            }
            if(wave>=5)foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).OrderBy(d=>d.Level).ThenBy(d=>d.Definition.Cost))b.TryUpgrade(d);
            results.Add(await Fight());if(b.Game.IsPlaying){w.Tick(2);for(int i=0;i<100&&w.Landings.ActiveCarriers>0;i++)w.Landings.Tick(1);}
        }
        Check(w.CompletedWaves==10,"Fresh earned-income ten waves");b.Game.EndRun();await Task.Delay(200);
        var session=Find<RunSession>();int award=session.ResearchAward.Total;Check(session.FinalizeRun(),"Finalization");Check(new RunHistoryService(LocalGameData.Files).Load().runs.Count==1,"Once-only history");
        Check(!new SaveGameService(LocalGameData.Files).Load(out _,out _),"No dead Continue");
        Find<ProductMenuController>().ReturnToMenu();await Task.Delay(300);Click("menu-research");Click("research-node-emergency-fund");Click("research-purchase");
        Check(new PlayerProfileService(LocalGameData.Files).Rank("emergency-fund")==1,"Durable research purchase");Click("research-back");Click("menu-new");Click("map-river_border");Click("map-start");await Task.Delay(500);Time.timeScale=0;
        Check(Find<BuildManager>().Economy.Money==600,"New run receives purchased +100 RSD once");
        return Save("fresh-product-loop",new{directory,method="Script-assisted UI Toolkit onboarding and natural Update Wave 1; subsequent Waves 2-10 use .1s production ticks, earned income only and between-wave purchases. Actual summary/menu/research/new-run controls. No RSD/HP/kill grants. Native-input walkthrough was deferred because of concurrent desktop activity.",naturalWave1=first,waves2To10=results,rpAward=award,researchReloaded=true,nextRunRsd=Find<BuildManager>().Economy.Money,historyCount=new RunHistoryService(LocalGameData.Files).Load().runs.Count,tutorial=ShorelineDefense.Tutorial.TutorialCatalog.All.Where(d=>LocalGameData.Tutorial.HasSeen(d.Id)).Select(d=>d.Id).ToArray()});
    }
    public static async Task<string> RestartPersistence()
    {
        Application.runInBackground=true;Check(SceneManager.GetActiveScene().name=="MainMenu","Restart at menu");
        var receipt=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Folder+"fresh-product-loop.json"));
        int expectedRp=(int)receipt["rpAward"]-10;
        Check(LocalGameData.Profile.Rank("emergency-fund")==1&&LocalGameData.Profile.ResearchPoints==expectedRp,"Meta survives Play Mode restart");
        Check(new SaveGameService(LocalGameData.Files).Load(out var saved,out _),"New active run persisted");Click("menu-continue");
        for(int i=0;i<200&&Find<RunSession>()?.Ready!=true;i++)await Task.Delay(25);Time.timeScale=0;
        Check(Find<RunSession>().RunId==saved.runId&&Find<BuildManager>().Economy.Money==600,"Continue exact run and balance without repeated grant");
        return Save("product-restart-persistence",new{method="Exit/re-enter Unity Play Mode; independent services reload disk files. This resets game runtime state, not the Unity application process.",researchRank=LocalGameData.Profile.Rank("emergency-fund"),rp=LocalGameData.Profile.ResearchPoints,runId=saved.runId,money=Find<BuildManager>().Economy.Money});
    }
    public static async Task<string> Progress(string label,string profile="none",int last=30,string strategy="mixed")
    {
        var rows=new List<object>();foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            await Load(map,profile);Prepare(1);var b=Find<BuildManager>();var w=Find<WaveManager>();
            foreach(int y in new[]{4,9,14})Place("machine-gun-nest",8,y);
            var rounds=new List<object>();
            for(int wave=1;wave<=last&&b.Game.IsPlaying;wave++)
            {
                // Planned spending only between waves: no instant reaction or combat income script.
                if(wave>1)
                {
                    foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Where(d=>d.IsAlive).OrderBy(d=>d.CurrentHealth/(float)d.MaxHealth))b.TryRepair(d);
                    if(strategy=="lean")
                    {
                        foreach(int y in new[]{4,9,14})
                        {
                            if(wave<=4)Place("machine-gun-nest",wave==2?12:wave==3?15:10,y);
                            if(wave==5)Place("flamethrower",11,y);
                            if(wave==6||wave==7)Place("sniper-tower",15,y+1);
                            if(wave==8)Place("flamethrower",12,y);
                            if(wave==9)Place("mortar",19,y);
                            if(wave==10||wave==12)Place("autocannon",17,y);
                            if(wave>=15&&wave%5==0)Place(wave%10==0?"autocannon":"sniper-tower",20+(wave/5)%4,y);
                        }
                        if(wave>=5)foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Where(d=>d.IsAlive).OrderBy(d=>d.Level).ThenBy(d=>d.Definition.Cost))b.TryUpgrade(d);
                    }
                    else
                    {
                    foreach(int y in new[]{4,9,14})
                    {
                        if(wave==2)Place("machine-gun-nest",13,y);
                        if(wave==3)Place("flamethrower",10,y+1);
                        if(wave==4)Place("barbed-wire",9,y+1,2);
                        if(wave==5)Place("sniper-tower",15,y);
                        if(wave==7)Place("autocannon",17,y);
                        if(wave==9)Place("mortar",19,y);
                        if(wave==12)Place("rocket",14,y+2);
                        if(wave>=15&&wave%5==0)Place(strategy=="mg"?"machine-gun-nest":wave%10==0?"autocannon":"sniper-tower",20+(wave/5)%4,y);
                    }
                    foreach(var d in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Where(d=>d.IsAlive).OrderBy(d=>d.Level).ThenBy(d=>d.Definition.Cost))b.TryUpgrade(d);
                    }
                }
                rounds.Add(await Fight());Save(label,new{profile,map,method="500 base RSD, ordinary threshold and earned money only. Purchases/repairs/upgrades only between waves. Script-assisted production combat at .1s ticks, no lethal damage, income or HP grants.",rounds});
                if(b.Game.IsPlaying){w.Tick(2);for(int i=0;i<100&&w.Landings.ActiveCarriers>0;i++)w.Landings.Tick(1);}
            }
            if(b.Game.IsPlaying)b.Game.EndRun();rows.Add(new{map,profile,rounds,rp=Find<RunSession>().ResearchAward?.Total});
        }
        return Save(label,new{profile,rows});
    }
}
