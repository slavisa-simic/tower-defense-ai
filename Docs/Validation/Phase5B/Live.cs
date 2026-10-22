using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Unity.Profiling;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.UI;
using ShorelineDefense.Presentation;

public static class Phase5BLive
{
    const BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
    static void Save(string name,object data)=>File.WriteAllText("Docs/Validation/Phase5B/"+name+".json",JsonConvert.SerializeObject(data,Formatting.Indented));
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes("Docs/Validation/Phase5B/"+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    static void Click(string name)
    {
        var b=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>(name);Check(b!=null&&b.enabledSelf,"Button "+name);
        using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}
    }
    static async Task Fresh()
    {
        var priorSettings=Find<WaveManager>()?.EndlessSettings;Time.timeScale=0;var operation=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!operation.isDone)await Task.Delay(30);await Task.Delay(100);
        if(priorSettings!=null&&!UnityEditor.AssetDatabase.Contains(priorSettings))UnityEngine.Object.Destroy(priorSettings);Application.runInBackground=true;UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        Find<GameSpeedController>().SetSpeed(GameSpeed.Pause);Find<BorderBreachService>().ConfigureThreshold(0);
        var w=Find<WaveManager>();var settings=UnityEngine.Object.Instantiate(w.EndlessSettings);settings.randomizeSeed=false;settings.seed=1977;w.ConfigureEndless(settings);
        typeof(WaveManager).GetMethod("ReleaseGenerated",Private).Invoke(w,null);typeof(WaveManager).GetField("waveIndex",Private).SetValue(w,23);typeof(WaveManager).GetMethod("EnsureGenerated",Private).Invoke(w,null);
    }
    static DefenseBase Place(string id,int x,int y,bool upgrade=true)
    {
        var b=Find<BuildManager>();var definition=Find<BuildHud>().Defenses.Single(d=>d.Id==id);Check(b.TryPlaceDefense(definition,new Vector2Int(x,y),out var d)==PlacementResult.Valid,"Place "+id);
        if(upgrade){Check(b.TryUpgrade(d),"II");Check(b.TryUpgrade(d),"III");}return d;
    }
    public static async Task<string> Batch(int start,int count)
    {
        Check(Application.isPlaying,"Requires Play Mode");var rows=new List<object>();
        for(int i=start;i<start+count;i++)
        {
            await Fresh();var w=Find<WaveManager>();var b=Find<BuildManager>();b.Economy.AddMoney(50000);
            foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",12,y);Place("mortar",17,y);Place("rocket",20,y);Place("flamethrower",24,y);Place("autocannon",14,y);Place("barbed-wire",23,y);}
            var sniper=Place("sniper-tower",7,8,false);sniper.TakeDamage(100);Find<StructureSelection>().Select(sniper);
            var e=w.EndlessSettings.eventSettings.events[i];int before=b.Economy.Money;Check(w.ForceEvent(e.id),"Force "+e.id);
            float previewRange=sniper.Range;int upgrade=b.UpgradeCost(sniper),repair=b.RepairCost(sniper);int supplied=b.Economy.Money-before;
            await Task.Delay(180);Capture(e.id+"-preparation");
            if(e.preparationWarningSeconds>0){Click("speed-double");while(!w.CanStart)await Task.Delay(50);Click("speed-pause");}
            Click("start-wave");Click("speed-double");await Task.Delay(3000);Click("speed-pause");await Task.Delay(100);Capture(e.id+"-active");
            var presentation=Find<EndlessEventPresentation>();
            Check(presentation.FogVisible==e.modifiers.fog,"Fog feedback");Check(presentation.FloodVisible==(e.modifiers.floodedColumns>0),"Flood feedback");
            rows.Add(new{e.id,wave=w.CurrentWaveNumber,planned=w.CurrentWave.ScheduledCount,prepUpgrade=upgrade,prepRepair=repair,combatRepair=b.RepairCost(sniper),supplied,previewRange,activeRange=sniper.Range,
                boats=w.Landings.ActiveBoats,peakBoats=w.Landings.PeakConcurrentBoats,w.Landings.ConcurrencyLimit,sector=w.Events.Current?.Sector.ToString(),sectors=w.Landings.SectorArrivals.ToArray(),
                enemies=Find<EnemyRegistry>().Count,kills=Find<RunStatistics>().TotalKills,fog=presentation.FogVisible,flood=presentation.FloodVisible,width=Screen.width,height=Screen.height});
            w.Events.Cancel();Check(Math.Abs(sniper.Range-sniper.Definition.RangeInCells*sniper.Grid.CellSize)<.001,"Restored range");
            Check(b.UpgradeCost(sniper)==sniper.Definition.LevelData(2).cost,"Restored costs");Check(!presentation.FogVisible&&!presentation.FloodVisible,"Restored visuals");
            Save("live-batch-"+start,new{fixtureMoney=50000,unlimitedBreaches=true,scriptAssisted=true,secondsPerAssault=6,rows});
        }
        return "Passed live events "+start+" through "+(start+count-1);
    }
    public static async Task<string> Performance(string id)
    {
        await Fresh();var w=Find<WaveManager>();if(id!="normal")Check(w.ForceEvent(id),"Performance event");else {w.Events.Cancel();typeof(WaveManager).GetMethod("ReleaseGenerated",Private).Invoke(w,null);typeof(WaveManager).GetField("generated",Private).SetValue(w,new EndlessWaveGenerator(w.EndlessSettings,1977).Generate(24));}
        Find<BuildManager>().Economy.AddMoney(50000);foreach(int y in new[]{4,9,14}){Place("machine-gun-nest",12,y);Place("mortar",17,y);Place("rocket",20,y);Place("flamethrower",24,y);Place("barbed-wire",23,y);}
        Click("start-wave");Click("speed-normal");await Task.Delay(1000);
        using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",2048))
        using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",2048))
        {
            await Task.Delay(3000);Click("speed-pause");
            var times=cpu.ToArray().Select(s=>s.Value/1000000.0).OrderBy(n=>n).ToArray();var allocations=gc.ToArray();
            Save("performance-"+id,new{id,fixtureMoney=50000,unlimitedBreaches=true,sampleSeconds=3,cpuValid=cpu.Valid,gcValid=gc.Valid,frames=times.Length,medianMs=times.Length>0?times[times.Length/2]:0,p95Ms=times.Length>0?times[(int)(times.Length*.95)]:0,maxMs=times.Length>0?times.Last():0,meanGcBytes=allocations.Length>0?allocations.Average(s=>(double)s.Value):0,
                enemies=Find<EnemyRegistry>().Count,boats=w.Landings.ActiveBoats,createdBoats=w.Landings.CreatedBoatVisuals,renderers=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length,
                eventRenderers=Find<EndlessEventPresentation>().GetComponentsInChildren<SpriteRenderer>(true).Count(s=>s.name=="Event Mist"||s.name=="Flooded No Build Strip")});
        }
        return "Sampled "+id;
    }
}


