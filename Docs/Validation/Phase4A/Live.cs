using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using ShorelineDefense.Core;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.UI;
using ShorelineDefense.Presentation;
public static class Phase4ALive
{
    const string Folder="Docs/Validation/Phase4A/";
    static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes(Folder+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    static bool Build(string name,int x,int y)
    {
        var data=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+name+".asset");
        return Find<BuildManager>().TryPlaceDefense(data,new Vector2Int(x,y),out var d)==PlacementResult.Valid;
    }
    public static async Task<string> Prepare()
    {
        var view=UnityEditor.EditorWindow.GetWindow(System.Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Application.runInBackground=true;
        Time.timeScale=0;Find<PlacementInput>().enabled=false;await Task.Delay(100);Capture("preparation");
        if(!Build("SniperTower",23,9)||!Build("MachineGunNest",23,4)||!Build("BarbedWire",27,4))throw new Exception("Initial budget layout failed");
        await Task.Delay(100);Capture("built-preparation");return "Initial 500 RSD spent; production HUD, border and build layout captured";
    }
    public static async Task<string> Wave()
    {
        var waves=Find<WaveManager>();var stats=Find<RunStatistics>();var builder=Find<BuildManager>();
        int number=waves.CurrentWaveNumber,count=waves.CurrentWave.ScheduledCount,bonus=waves.CurrentWave.CompletionReward,balance=builder.Economy.Money;
        long kills=stats.TotalKills,homicides=stats.Homicides,earned=stats.TotalMoneyEarned;
        var button=Find<WaveHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave");using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
        if(waves.State!=WaveState.Spawning)throw new Exception("HUD start failed");Time.timeScale=4;
        var timer=System.Diagnostics.Stopwatch.StartNew();bool captured=false;
        while(waves.CompletedWaves<number&&builder.Game.IsPlaying&&timer.Elapsed.TotalSeconds<65)
        {
            if(!captured&&Find<EnemyRegistry>().Count>=3){Time.timeScale=0;await Task.Delay(100);Capture("wave-"+number+"-combat");captured=true;Time.timeScale=4;}
            await Task.Delay(30);
        }
        Time.timeScale=0;var row=new{number,seed=waves.RunSeed,count,completed=waves.CompletedWaves==number,gamePlaying=builder.Game.IsPlaying,kills=stats.TotalKills-kills,basicKills=stats.BasicKills,runnerKills=stats.RunnerKills,bruteKills=stats.BruteKills,homicides=stats.Homicides,homicideIncrease=stats.Homicides-homicides,balance=builder.Economy.Money,earned=stats.TotalMoneyEarned-earned,bonus,seconds=timer.Elapsed.TotalSeconds};
        string path=Folder+"natural-waves.json";var rows=File.Exists(path)?Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText(path)):new Newtonsoft.Json.Linq.JArray();rows.Add(Newtonsoft.Json.Linq.JObject.FromObject(row));File.WriteAllText(path,rows.ToString());
        await Task.Delay(100);Capture("wave-"+number+"-complete");if(!row.completed)throw new Exception(JsonConvert.SerializeObject(row));
        Time.timeScale=2;while(waves.State==WaveState.WaveComplete)await Task.Delay(30);Time.timeScale=0;
        if(!waves.CanStart)throw new Exception("Preparation did not return");
        return JsonConvert.SerializeObject(row);
    }
    public static string Reinforce()
    {
        int completed=Find<WaveManager>().CompletedWaves;
        if(completed==1){Build("MachineGunNest",23,14);Build("BarbedWire",27,14);Build("LandMine",27,9);}
        else if(completed==2){Build("SniperTower",23,13);Build("BarbedWire",27,9);}
        else if(completed==3){Build("MachineGunNest",23,7);Build("LandMine",26,4);}
        else if(completed==4){Build("MachineGunNest",23,11);Build("LandMine",26,14);}
        else if(completed==5){Build("SniperTower",23,5);Build("LandMine",26,9);}
        else {Build("MachineGunNest",21,completed%2==0?14:4);Build("LandMine",25,completed%2==0?14:4);}
        return "Reinforcements attempted within earned money; balance "+Find<BuildManager>().Economy.Money;
    }
    public static async Task<string> BoundaryReview()
    {
        var spawner=Find<EnemySpawner>();var waves=Find<WaveManager>();Time.timeScale=0;
        foreach(var prefab in waves.EndlessSettings.enemies)
        {
            if(!spawner.TrySpawnAt(prefab,spawner.Battlefield.Grid.CellToWorld(new Vector2Int(29,14)),out var e))throw new Exception("Review spawn failed");
            await Task.Delay(100);Capture("enemy-"+prefab.Definition.Kind);e.Tick(10);await Task.Delay(100);Capture("breach-"+prefab.Definition.Kind);await Task.Delay(900);
        }
        var border=Find<BorderBreachService>();border.ConfigureThreshold((int)Find<RunStatistics>().Homicides+25);
        spawner.TrySpawnAt(spawner.DefaultPrefab,spawner.Battlefield.Grid.CellToWorld(new Vector2Int(29,9)),out var last);last.Tick(10);await Task.Delay(150);Capture("game-over");
        var root=Find<PrototypeHud>().GetComponent<UIDocument>().rootVisualElement;
        if(!root.Q<Label>("game-over").text.Contains("BORDER DEFENSE COLLAPSED")||Find<WaveHud>().StartAvailable||Find<BuildHud>().BuildAvailable)throw new Exception("Loss presentation failed");
        return Find<RunStatistics>().Report;
    }
    public static async Task<string> HighWavePreview()
    {
        var waves=Find<WaveManager>();var flags=BindingFlags.NonPublic|BindingFlags.Instance;
        typeof(WaveManager).GetField("waveIndex",flags).SetValue(waves,99);
        typeof(WaveManager).GetMethod("ReleaseGenerated",flags).Invoke(waves,null);
        typeof(WaveManager).GetMethod("SetState",flags).Invoke(waves,new object[]{WaveState.Preparation});
        await Task.Delay(150);var root=Find<WaveHud>().GetComponent<UIDocument>().rootVisualElement;var label=root.Q<Label>("wave-number");var count=root.Q<Label>("enemies-remaining");
        if(label.text!="WAVE 100"||label.worldBound.xMax>count.worldBound.xMin+.5f||count.worldBound.xMax>root.Q("wave-panel").worldBound.xMax)throw new Exception("Three-digit layout failed");
        Capture("wave-100-hud");return "Three-digit wave/enemy preview fits in actual 1920x1080 HUD; no wave combat simulated by this fixture";
    }
    public static string Audit()
    {
        var camera=Camera.main;var grid=Find<BuildManager>().Battlefield.Grid;var root=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement;
        var result=new{screen=new[]{Screen.width,Screen.height},orthographic=camera.orthographic,rotation=camera.transform.eulerAngles.ToString(),lanes=Find<BorderBreachService>().BreachCells.Select(c=>c.ToString()),money=root.Q<Label>("money").text,wave=root.Q<Label>("wave-number").text,homicides=root.Q<Label>("base-hp").text,borderColliderFree=!GameObject.Find("Final Border Checkpoint").GetComponentsInChildren<Component>().Any(c=>c.GetType().Name.Contains("Collider")),visibleCoin=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(r=>r.enabled&&r.sprite!=null&&AssetDatabase.GetAssetPath(r.sprite).Contains("money_icon_v001")),managers=new{waves=UnityEngine.Object.FindObjectsByType<WaveManager>(FindObjectsSortMode.None).Length,statistics=UnityEngine.Object.FindObjectsByType<RunStatistics>(FindObjectsSortMode.None).Length,border=UnityEngine.Object.FindObjectsByType<BorderBreachService>(FindObjectsSortMode.None).Length}};
        File.WriteAllText(Folder+"live-audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));return JsonConvert.SerializeObject(result);
    }
}



