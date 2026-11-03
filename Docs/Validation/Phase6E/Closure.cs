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

public static class Phase6E1Closure
{
    const string Folder="Docs/Validation/Phase6E/";
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static void Check(bool ok,string m){if(!ok)throw new Exception(m);}
    static string Save(string name,object value){File.WriteAllText(Folder+"closure-"+name+".json",JsonConvert.SerializeObject(value,Formatting.Indented));return "Written "+name;}
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
        RunSession.Pending=null;Application.runInBackground=true;void Quiet(Scene scene,LoadSceneMode mode){Time.timeScale=0;var menu=Find<ProductMenuController>();if(menu!=null)menu.enabled=false;var input=Find<PlacementInput>();if(input!=null)input.enabled=false;var events=Find<UnityEngine.EventSystems.EventSystem>();if(events!=null)events.enabled=false;}
        SceneManager.sceneLoaded+=Quiet;
        try{await SceneManager.LoadSceneAsync(map);}finally{SceneManager.sceneLoaded-=Quiet;}
        for(int i=0;i<200&&Find<RunSession>()?.Ready!=true;i++)await Task.Delay(25);
        await Task.Delay(80);Time.timeScale=0;Check(Find<RunSession>().CanSave,"Ready preparation");
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
        return new{wave=w.CurrentWaveNumber,state=w.State.ToString(),seconds=ticks*.1f,kills=st.TotalKills-k,homicides=st.Homicides-h,spawned=sp.TotalSpawned-spawned,peak,money=b.Economy.Money,earned=b.Economy.TotalEarned-earned,spent=b.Economy.TotalSpent-spent,structures=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Count(d=>d.IsAlive),registry=r.Count,alive=w.AliveCount,pending=w.PendingCount,sectors=w.Landings.SectorArrivals.ToArray(),arrivals=w.Landings.ArrivalCounts.ToArray(),stalled=ticks>=limit};
    }
    public static async Task<string> Progress(string label,string profile="none",int last=30,string strategy="mixed",string onlyMap="")
    {
        var rows=new List<object>();foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            if(onlyMap!=""&&map!=onlyMap)continue;await Load(map,profile);Prepare(1);var b=Find<BuildManager>();var w=Find<WaveManager>();
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
            if(b.Game.IsPlaying)b.Game.EndRun();rows.Add(new{map,profile,rounds,totalEarned=b.Economy.TotalEarned,totalSpent=b.Economy.TotalSpent,rp=Find<RunSession>().ResearchAward?.Total});
        }
        return Save(label,new{profile,rows});
    }
    public static async Task<string> Bosses(string onlyKind="")
    {
        var rows=new List<object>();
        foreach(string map in new[]{"CoastalDefense_TopDown"})foreach(string kind in new[]{"Dredger","Broodmother"})foreach(string force in new[]{"midgame","specialized"})
        {
            if(onlyKind!=""&&kind!=onlyKind)continue;await Load(map);Find<BorderBreachService>().ConfigureThreshold(0);
            var b=Find<BuildManager>();b.Economy.AddMoney(force=="midgame"?6500:12000);
            foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",9,y,2);Place("sniper-tower",15,y,2);Place("autocannon",18,y,force=="midgame"?1:3);Place("flamethrower",11,y+1,1);if(force=="specialized"){Place("sniper-tower",20,y,3);Place("rocket",13,y+2,1);}}
            int wave=kind=="Dredger"?25:50;Prepare(wave);
            // Isolated boss exposure. Full concurrent encounter pressure is tested separately.
            var sp=Find<EnemySpawner>();var current=Find<WaveManager>().CurrentWave;sp.ConfigureWave(400,current.HealthMultiplier,current.SpeedMultiplier,current.HealthMultiplier);sp.ReinforcementsEnabled=true;
            var bossPrefab=Find<WaveManager>().EndlessSettings.bossEncounters.bosses.First(p=>p.Definition.Kind.ToString()==kind);
            EnemyController boss=null;
            foreach(int y in new[]{9,4,14,6,12})if(sp.TrySpawnAt(bossPrefab,b.Battlefield.Grid.CellToWorld(new Vector2Int(7,y)),out boss))break;
            Check(boss!=null,"boss spawn");double? deathAt=null;int maximum=boss.Health.MaxHealth;float start=Time.time;boss.Health.Died+=()=>deathAt=Time.time-start;
            Time.timeScale=1;var clock=System.Diagnostics.Stopwatch.StartNew();while(boss!=null&&boss.IsCombatActive&&clock.Elapsed.TotalSeconds<75)await Task.Delay(50);
            Time.timeScale=0;
            rows.Add(new{map,kind,force,paid=b.Economy.TotalSpent,hp=maximum,ttkSeconds=deathAt,alive=boss.IsCombatActive,children=boss.GetComponent<BroodmotherSpawn>()?.TotalSpawned,breaches=Find<RunStatistics>().TotalBreaches});
            Save("boss-ttk"+onlyKind,new{method="Natural Update x1, zero research. Moving isolated boss at authored base HP, normal armor/structure attacks and real defense payments. Excludes escorts/other-wave damage competition; functional encounter suites test both maps separately. Midgame and specialized fixtures disclose granted budgets.",rows});
        }
        return "Completed boss TTK";
    }
}


