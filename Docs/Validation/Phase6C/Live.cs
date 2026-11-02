using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
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
public static class Phase6CLive
{
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static string Folder=>"Docs/Validation/Phase6C/";
    static ResearchCatalog Catalog=>ResearchCatalog.Load();
    static VisualElement Root=>Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static string Save(string file,object value){string json=JsonConvert.SerializeObject(value,Formatting.Indented);File.WriteAllText(Folder+file+".json",json);return json;}
    static void Isolate()=>LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefendersPhase6C",Guid.NewGuid().ToString()));
    static async Task Load(string scene)
    {RunSession.Pending=null;Application.runInBackground=true;await SceneManager.LoadSceneAsync(scene);await Task.Delay(150);Time.timeScale=0;}
    static void Click(string id)
    {var b=Root.Q<Button>(id);Check(b!=null&&b.enabledSelf,"Button "+id);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
    static async Task Capture(string name)
    {var tip=Root.Q("runtime-tooltip");if(tip!=null)tip.style.display=DisplayStyle.None;await Task.Delay(100);var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);var encode=type.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);File.WriteAllBytes(Folder+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));}
    static async Task SetSize(int width,int height)
    {
        const System.Reflection.BindingFlags f=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
        var type=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).GetValue(null);
        var group=type.GetProperty("currentGroup",f).GetValue(sizes);var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));string label="Phase6C "+width+"x"+height;
        var labels=(string[])group.GetType().GetMethod("GetDisplayTexts",f).Invoke(group,null);int index=Array.FindIndex(labels,s=>s.Contains("("+width+"x"+height+")")||s.StartsWith(label));
        if(index<0){var st=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");var ctor=st.GetConstructors(f).First(c=>c.GetParameters().Length==4);var size=ctor.Invoke(new[]{Enum.Parse(ctor.GetParameters()[0].ParameterType,"FixedResolution"),(object)width,height,label});group.GetType().GetMethod("AddCustomSize",f).Invoke(group,new[]{size});index=(int)group.GetType().GetMethod("GetTotalCount",f).Invoke(group,null)-1;}
        view.GetType().GetProperty("selectedSizeIndex",f).SetValue(view,index);view.Focus();view.Repaint();await Task.Delay(350);Check(Screen.width==width&&Screen.height==height,"Actual resolution");
    }
    public static string Economy()
    {
        var results=new List<object>();
        foreach(var scenario in new[]{new{label="average",wave=40,bosses=1,homicides=500},new{label="stronger",wave=60,bosses=2,homicides=400}})
        {
            var profile=new PlayerProfileService(new LocalJsonStore(Path.Combine(Path.GetTempPath(),"BorderDefendersRPSim",Guid.NewGuid().ToString())));
            var tree=new ResearchTreeService(profile,Catalog,()=>true);var runs=new List<object>();int fullAt=0,firstAt=0;
            for(int i=1;i<=25;i++)
            {
                var record=new RunRecord{runId=Guid.NewGuid().ToString(),mapId=i%2==1?"river_border":"industrial_port",wave=scenario.wave,completedWaves=scenario.wave-1,bosses=scenario.bosses,dredgers=1,broodmothers=scenario.bosses>1?1:0,homicides=scenario.homicides};
                Check(profile.CompleteRun(record,Catalog.rewards,out var award,out var error),error);
                var bought=new List<string>();bool progress;
                do{progress=false;foreach(var n in Catalog.nodes.OrderBy(n=>n.tier).ThenBy(n=>n.costRP))if(tree.Status(n)==ResearchNodeStatus.Available){Check(tree.TryPurchase(n.id,out error),error);bought.Add(n.id);progress=true;}}while(progress);
                int owned=profile.Current.unlockedResearchNodes.Count;if(firstAt==0&&owned>0)firstAt=i;if(fullAt==0&&owned==20)fullAt=i;
                if(i<=10)runs.Add(new{run=i,earned=award.Total,milestones=award.milestoneRP,spent=profile.Current.lifetimeStatistics.rpSpent,remaining=profile.ResearchPoints,owned,bought});
            }
            results.Add(new{scenario.label,scenario.wave,scenario.bosses,scenario.homicides,firstUsefulRun=firstAt,fullTreeRun=fullAt,firstTen=runs});
        }
        return Save("rp-economy",new{totalTreeCost=Catalog.nodes.Sum(n=>n.costRP),method="Synthetic performance records through real reward, milestone, purchase and atomic profile services; no grant RP.",results});
    }
    public static async Task<string> Screens()
    {
        Isolate();await Load("MainMenu");LocalGameData.Profile.DebugGrantRP(70,out _);var results=new List<object>();
        foreach(var size in new[]{new Vector2Int(1366,768),new Vector2Int(1920,1080),new Vector2Int(2560,1440)})
        {
            await SetSize(size.x,size.y);Find<ProductMenuController>().ShowResearch();await Task.Delay(250);await Capture("research-"+size.x);
            var nodes=Root.Query<Button>(className:"research-node").ToList();Check(nodes.Count==20,"20 visible nodes");
            foreach(var node in nodes)Check(node.worldBound.width>10&&node.worldBound.height>10&&!float.IsNaN(node.worldBound.x),"Node layout");
            for(int i=0;i<nodes.Count;i++)for(int j=i+1;j<nodes.Count;j++)Check(!nodes[i].worldBound.Overlaps(nodes[j].worldBound),"Node overlap "+nodes[i].name+" / "+nodes[j].name);
            var card=Root.Q("product-card");var bounds=card.worldBound;Check(bounds.xMin>=-1&&bounds.xMax<=Root.worldBound.xMax+1,"Card containment");
            results.Add(new{width=size.x,height=size.y,root=Root.worldBound.ToString(),card=bounds.ToString(),nodeCount=nodes.Count,back=Root.Q<Button>("research-back").worldBound.ToString()});
        }
        await SetSize(1920,1080);Click("research-node-emergency-fund");Click("research-purchase");Click("research-node-supply-contracts");Click("research-purchase");await Task.Delay(200);await Capture("research-purchased");
        Check(LocalGameData.Profile.Rank("supply-contracts")==1,"Actual UI purchase");
        Click("research-node-reserve-stockpile");await Task.Delay(100);await Capture("research-locked-detail");
        Click("research-node-logistics-network");Click("research-purchase");Click("research-node-field-salvage");await Task.Delay(100);await Capture("research-insufficient");
        Check(!Root.Q<Button>("research-purchase").enabledSelf,"Insufficient RP state");
        LocalGameData.Profile.DebugUnlock(Catalog,true,null,out _);await Load("CoastalDefense_TopDown");var waves=Find<WaveManager>();var d=Find<RunSession>().Capture();d.wave=25;d.completedWaves=24;waves.RestorePreparation(d);await Task.Delay(150);await Capture("intelligence-boss-preparation");
        Check(Root.Q<Button>("build-mg").tooltip.Contains("Damage 21")&&Root.Q<Button>("build-mg").tooltip.Contains("Max HP 360"),"Effective build tooltip");
        Check(Root.Q<Label>("preparation-intelligence").text.Contains("MASSIVE SIGNATURE EXPECTED"),"Boss intel");
        Check(!Root.Q("preparation-intelligence").worldBound.Overlaps(Root.Q("speed-controls").worldBound),"Intel/speed separation");
        Find<GameStateController>().EndRun();await Task.Delay(100);return Save("ui-layout",results);
    }
    public static Task<string> Balance()=>BalanceCore(false);
    public static Task<string> LateBalance()=>BalanceCore(true);
    static async Task<string> BalanceCore(bool includeLate)
    {
        var results=new List<object>();
        foreach(bool full in new[]{false,true})foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            Isolate();if(full)LocalGameData.Profile.DebugUnlock(Catalog,true,null,out _);await Load(map);
            Find<BorderBreachService>().ConfigureThreshold(1000);var b=Find<BuildManager>();var w=Find<WaveManager>();var enemies=Find<EnemyRegistry>();var stats=Find<RunStatistics>();var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");
            int start=b.Economy.Money;Check(start==(full?750:500),"Research starting budget");
            var seed=Find<RunSession>().Capture();seed.seed=1977;seed.events=new EventSaveData{nextWave=5};w.RestorePreparation(seed);
            foreach(int y in new[]{4,9,15})
            {
                var choices=new List<Vector2Int>();for(int x=7;x<=11;x++)for(int dy=-2;dy<=2;dy++)choices.Add(new Vector2Int(x,y+dy));
                var cell=choices.OrderBy(c=>Math.Abs(c.x-8)+Math.Abs(c.y-y)).First(c=>b.Validate(mg,c)==PlacementResult.Valid);
                Check(b.TryPlaceDefense(mg,cell,out _)==PlacementResult.Valid,"Opening "+map+" "+cell);
            }
            var waves=new List<object>();
            for(int number=1;number<=10&&Find<GameStateController>().IsPlaying;number++)
            {
                w.Events.Tick(100);Check(w.TryStartWave(),"Balance wave start");int ticks=0;
                for(;ticks<12000&&Find<GameStateController>().IsPlaying&&w.State!=WaveState.WaveComplete;ticks++)
                {
                    w.Tick(.1f);foreach(var turret in Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))turret.Tick(.1f);foreach(var enemy in enemies.Active.ToArray())enemy.Tick(.1f);
                    foreach(var spit in Object.FindObjectsByType<CorrosiveSpit>(FindObjectsSortMode.None))spit.Tick(.1f);
                    if(ticks%20==0)foreach(var cell in new[]{new Vector2Int(8,11),new Vector2Int(16,5),new Vector2Int(17,16),new Vector2Int(17,11),new Vector2Int(22,5),new Vector2Int(22,11),new Vector2Int(8,6),new Vector2Int(8,12)})if(b.Economy.Money>=mg.Cost&&b.Validate(mg,cell)==PlacementResult.Valid)b.TryPlaceDefense(mg,cell,out _);
                    if(ticks%25==0)await Task.Yield();
                }
                Check(ticks<12000,"Balance stalled");waves.Add(new{wave=number,state=w.State.ToString(),ticks,kills=stats.TotalKills,homicides=stats.Homicides,money=b.Economy.Money});
                if(!Find<GameStateController>().IsPlaying)break;
                w.Tick(2);for(int i=0;i<100&&w.Landings.ActiveCarriers>0;i++)w.Landings.Tick(1);
                foreach(var defense in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None)){b.TryUpgrade(defense);b.TryUpgrade(defense);b.TryRepair(defense);}
                foreach(var cell in new[]{new Vector2Int(14,5),new Vector2Int(15,11),new Vector2Int(17,16),new Vector2Int(22,5),new Vector2Int(22,11)})if(b.Economy.Money>=mg.Cost&&b.Validate(mg,cell)==PlacementResult.Valid)b.TryPlaceDefense(mg,cell,out _);
            }
            var late=new List<object>();
            if(includeLate)foreach(int number in new[]{25,50,100})
            {
                if(!Find<GameStateController>().IsPlaying)break;
                var snapshot=Find<RunSession>().Capture();snapshot.wave=number;snapshot.completedWaves=number-1;snapshot.events=new EventSaveData{nextWave=number};w.RestorePreparation(snapshot);w.Events.Tick(100);
                Check(w.TryStartWave(),"Late start");int ticks=0;int initialStructures=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Length;long before=stats.Homicides;
                for(;ticks<18000&&Find<GameStateController>().IsPlaying&&w.State!=WaveState.WaveComplete;ticks++)
                {
                    w.Tick(.1f);foreach(var turret in Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))turret.Tick(.1f);foreach(var enemy in enemies.Active.ToArray())enemy.Tick(.1f);
                    foreach(var spit in Object.FindObjectsByType<CorrosiveSpit>(FindObjectsSortMode.None))spit.Tick(.1f);
                    if(ticks%25==0)await Task.Yield();
                }
                Check(ticks<18000,"Late stalled");late.Add(new{wave=number,state=w.State.ToString(),ticks,initialStructures,remainingStructures=Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).Length,homicidesBefore=before,homicides=stats.Homicides,bossesKilled=stats.BossesKilled});
                if(Find<GameStateController>().IsPlaying)w.Tick(2);
            }
            if(Find<GameStateController>().IsPlaying)Find<GameStateController>().EndRun();
            Check(Find<RunSession>().ResearchAward!=null,"Run awarded");
            results.Add(new{map,fullResearch=full,startingRsd=start,waves,late,earnedRP=Find<RunSession>().ResearchAward.Total});
        }
        return Save(includeLate?"late-balance":"balance",new{method="Both maps; seed 1977, real starting funds, threshold 1000 and earned income only. Shared .1s gameplay ticks and scripted MG purchases/upgrades/repairs. First 10 waves use normal earned income. Optional late checks jump the existing force to Waves 25/50/100 without additional construction, repairs or test damage: controlled research vulnerability tests, not earned-income boss victories or expert balance certification. No money grants. Not human mouse-only play.",results});
    }
    public static async Task<string> Progression()
    {
        Isolate();var rows=new List<object>();
        foreach(int run in new[]{1,2,3})
        {
            await Load("CoastalDefense_TopDown");var w=Find<WaveManager>();var enemies=Find<EnemyRegistry>();var session=Find<RunSession>();
            // Product/reward fixture: accelerated arrivals and explicit lethal test damage. Distinct from Balance().
            int runWaves=run==3?20:run*5;
            for(int wave=1;wave<=runWaves;wave++)
            {
                w.Events.Tick(100);Check(w.TryStartWave(),"Progression start");
                int ticks=0;for(;ticks<10000&&w.State!=WaveState.WaveComplete;ticks++) {w.Tick(.5f);foreach(var e in enemies.Active.ToArray())e.Health.TakeDefenseDamage(int.MaxValue);if(ticks%30==0)await Task.Yield();}
                Check(w.State==WaveState.WaveComplete,"Progression completion");w.Tick(2);
            }
            Check(Find<BuildManager>().Economy.Money>0,"RSD remains separate");
            Find<GameStateController>().EndRun();await Task.Delay(30);int award=session.ResearchAward.Total;long rp=LocalGameData.Profile.ResearchPoints;
            if(run==1)await Capture("run-research-summary");
            Find<ProductMenuController>().ReturnToMenu();await Task.Delay(250);Check(SceneManager.GetActiveScene().name=="MainMenu","Progression menu");
            Click("menu-research");string target=run==1?"emergency-fund":run==2?"river-monitoring":"supply-contracts";Click("research-node-"+target);
            bool can=Root.Q<Button>("research-purchase").enabledSelf;if(can)Click("research-purchase");
            rows.Add(new{run,wavesCompleted=runWaves,earnedRP=award,beforePurchase=rp,target,purchased=can,remaining=LocalGameData.Profile.ResearchPoints,startingNext=new MetaModifierService(MetaModifierService.FromProfile(LocalGameData.Profile,Catalog)).StartingRsd});
        }
        return Save("progression",new{method="Three real wave-scheduler runs with accelerated ticks and explicit lethal test damage, finalized through Game Over and actual Main Menu purchase buttons. Not an earned-income balance test.",rows});
    }
    public static async Task<string> FinalSmoke()
    {
        Isolate();await Load("MainMenu");Click("menu-research");Check(Root.Query<Button>(className:"research-node").ToList().Count==20,"Menu research");Click("research-back");
        Click("menu-new");Click("map-river_border");Click("map-start");await Task.Delay(400);Time.timeScale=0;
        Check(Find<RunSession>()?.Ready==true&&Find<BuildManager>().Economy.Money==500,"Fresh neutral run");
        var b=Find<BuildManager>();Check(b.TryPlaceDefense(Find<BuildHud>().Defenses.First(n=>n.Id=="machine-gun-nest"),new Vector2Int(12,8),out var mg)==PlacementResult.Valid,"Fresh build");Check(mg.MaxHealth==300&&mg.Damage==20,"Neutral stats");Check(Find<RunSession>().SaveNow(),"Final save");
        Find<ProductMenuController>().ReturnToMenu();await Task.Delay(400);Check(SceneManager.GetActiveScene().name=="MainMenu","Return menu");Click("menu-research");
        return Save("final-smoke",new{researchNodes=20,neutralStartingRsd=500,mgHealth=mg==null?300:mg.MaxHealth,profileRP=LocalGameData.Profile.ResearchPoints,result="Passed"});
    }
}
