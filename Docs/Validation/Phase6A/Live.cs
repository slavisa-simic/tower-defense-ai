using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
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

public static class Phase6ALive
{
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static async Task SetSize(int width, int height)
    {
        var type = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        var group = type.GetProperty("currentGroup", Flags).GetValue(sizes);
        var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        string label = "Phase6A " + width + "x" + height;
        var labels = (string[])group.GetType().GetMethod("GetDisplayTexts", Flags).Invoke(group, null);
        int index = Array.FindIndex(labels, s => s.Contains("(" + width + "x" + height + ")") || s.StartsWith(label));
        if (index < 0) {
            var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var constructor = sizeType.GetConstructors(Flags).First(c => c.GetParameters().Length == 4);
            var size = constructor.Invoke(new[] { Enum.Parse(constructor.GetParameters()[0].ParameterType, "FixedResolution"), (object)width, height, label });
            group.GetType().GetMethod("AddCustomSize", Flags).Invoke(group, new[] { size });
            index = (int)group.GetType().GetMethod("GetTotalCount", Flags).Invoke(group, null) - 1;
        }
        view.GetType().GetProperty("selectedSizeIndex", Flags).SetValue(view, index); view.Focus(); view.Repaint(); await Task.Delay(400);
        Check(Screen.width == width && Screen.height == height, "Actual render dimensions");
    }
    public static async Task<string> Resolution(){await SetSize(1920,1080);return await Menus();}
    public static async Task<string> WireRoundTrip()
    {
        await StartPort();Time.timeScale=0;var builder=Find<BuildManager>();builder.Economy.AddMoney(5000);var wire=Find<BuildHud>().Defenses.First(d=>d.Id=="barbed-wire");var center=new Vector2Int(14,11);
        foreach(var c in new[]{center,center+Vector2Int.up,center+Vector2Int.right,center+Vector2Int.down,center+Vector2Int.left})if(builder.TryPlaceDefense(wire,c,out _)!=PlacementResult.Valid)throw new Exception("Wire place "+c);
        var defense=builder.Battlefield.Grid.OccupantAt(center) as DefenseBase;
        int Mask()=>defense.GetComponent<ShorelineDefense.Presentation.ConnectedWirePresentation>().Mask;
        if(Mask()!=15)throw new Exception("Cross");builder.TryUpgrade(defense);builder.TryUpgrade(defense);
        builder.TryDismantle(builder.Battlefield.Grid.OccupantAt(center+Vector2Int.up) as DefenseBase,out _);if(Mask()!=14)throw new Exception("T mask");
        var expected=Find<RunSession>().Capture();if(!Find<RunSession>().SaveNow())throw new Exception("Wire save");Find<ProductMenuController>().ReturnToMenu();await WaitScene("MainMenu");Click("menu-continue");await WaitScene("DanubeIndustrialPort");Time.timeScale=0;
        builder=Find<BuildManager>();defense=builder.Battlefield.Grid.OccupantAt(center) as DefenseBase;
        if(Mask()!=14||defense.Level!=3||Find<RunSession>().Capture().money!=expected.money)throw new Exception("Wire restore");
        builder.TryDismantle(builder.Battlefield.Grid.OccupantAt(center+Vector2Int.left) as DefenseBase,out _);if(Mask()!=6)throw new Exception("Corner");
        builder.TryDismantle(builder.Battlefield.Grid.OccupantAt(center+Vector2Int.right) as DefenseBase,out _);builder.TryPlaceDefense(wire,center+Vector2Int.up,out _);if(Mask()!=5)throw new Exception("Straight");
        var result="Port wire cross=15, T=14, III upgrade, exact money/T restore through Continue, corner=6 and straight=5 passed; disclosed 5000 RSD fixture.";
        File.WriteAllText(Path.Combine(Folder,"port-wire-roundtrip.txt"),result);ScreenCapture("port-wire-restored");return result;
    }
    static void Check(bool condition,string reason){if(!condition)throw new Exception(reason);}
    static string Folder=>Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/Phase6A");
    static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
    static VisualElement Root=>Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
    static void Click(string id){var b=Root.Q<Button>(id);if(b==null||!b.enabledSelf)throw new Exception("Missing/disabled "+id);using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}}
    static async Task WaitScene(string name){for(int i=0;i<200&&(SceneManager.GetActiveScene().name!=name||Find<ProductMenuController>()==null||Find<ProductMenuController>().IsLoading);i++)await Task.Delay(20);await Task.Delay(200);if(SceneManager.GetActiveScene().name!=name)throw new Exception("Scene "+name);}
    static void ScreenCapture(string name){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);var encode=type.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);File.WriteAllBytes(Path.Combine(Folder,name+".png"),(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));}
    public static async Task<string> Menus()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        Time.timeScale=0;Find<GameStateController>().EndRun();await Task.Delay(200);ScreenCapture("port-gameover");
        Click("over-main");await WaitScene("MainMenu");Click("menu-new");await Task.Delay(150);ScreenCapture("map-selection");
        if(Root.Q<Button>("map-river_border")==null||Root.Q<Button>("map-industrial_port")==null)throw new Exception("Map cards");
        Click("map-river_border");Click("map-start");await WaitScene("CoastalDefense_TopDown");if(Find<RunSession>().MapId!="river_border")throw new Exception("Map1 identity");
        Find<GameStateController>().EndRun();await Task.Delay(200);Click("over-main");await WaitScene("MainMenu");Click("menu-history");await Task.Delay(150);ScreenCapture("both-map-history");
        var history=new RunHistoryService(LocalGameData.Files).Load();if(!history.runs.Any(r=>r.mapId=="river_border")||!history.runs.Any(r=>r.mapId=="industrial_port"))throw new Exception("History identities");
        Click("history-back");Click("menu-new");Click("map-industrial_port");await Task.Delay(150);ScreenCapture("port-start-confirmation");Click("map-start");await WaitScene("DanubeIndustrialPort");
        Find<ProductMenuController>().ReturnToMenu();await WaitScene("MainMenu");Click("menu-continue");await WaitScene("DanubeIndustrialPort");
        if(Find<RunSession>().MapId!="industrial_port")throw new Exception("Continue identity");ScreenCapture("port-hud");
        return "Both New Game cards, both scene launches, both-map history, port Game Over and port Continue passed at "+Screen.width+"x"+Screen.height;
    }
    public static string Isolate(){LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(),"BorderDefendersPhase6A",Guid.NewGuid().ToString()));RunSession.Pending=null;return "Isolated QA local data.";}
    public static async Task<string> StartPort(){Isolate();Application.runInBackground=true;EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();var op=SceneManager.LoadSceneAsync("DanubeIndustrialPort");while(!op.isDone)await Task.Delay(20);await Task.Delay(200);Time.timeScale=0;return "QA port started.";}
    public static void Capture(string name)
    {
        var camera=Camera.main; var rt=new RenderTexture(1920,1080,24); var old=camera.targetTexture; var active=RenderTexture.active;
        camera.targetTexture=rt; camera.Render();RenderTexture.active=rt;var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();
        File.WriteAllBytes(Path.Combine(Folder,name+".png"),texture.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(texture);Object.DestroyImmediate(rt);
    }
    public static async Task<string> Early()
    {
        Time.timeScale=0; var build=Find<BuildManager>(); var waves=Find<WaveManager>();var registry=Find<EnemyRegistry>(); var stats=Find<RunStatistics>();
        var mg=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");
        int initial=build.Economy.Money;
        foreach(int y in new[]{4,9,15}) if(build.TryPlaceDefense(mg,new Vector2Int(8,y),out _)!=PlacementResult.Valid)throw new Exception("Opening MG placement");
        var results=new System.Collections.Generic.List<object>();
        for(int wave=1;wave<=5;wave++)
        {
            waves.Events.Tick(100);if(!waves.TryStartWave())throw new Exception("Start "+wave);
            int iterations=0;
            for(;iterations<10000&&Find<GameStateController>().IsPlaying&&waves.State!=WaveState.WaveComplete;iterations++)
            {
                waves.Tick(.1f);foreach(var turret in Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None))turret.Tick(.1f);
                foreach(var e in registry.Active.ToArray())e.Tick(.1f);
                if(iterations%20==0)foreach(var cell in new[]{new Vector2Int(8,11),new Vector2Int(16,5),new Vector2Int(17,16),new Vector2Int(17,11),new Vector2Int(22,5),new Vector2Int(22,11),new Vector2Int(8,6),new Vector2Int(8,12)})
                    if(build.Economy.Money>=mg.Cost&&build.Validate(mg,cell)==PlacementResult.Valid)build.TryPlaceDefense(mg,cell,out _);
                if(iterations%10==0)await Task.Delay(1);
                if(wave==1&&iterations==100)Capture("early-natural-combat");
            }
            results.Add(new{wave,state=waves.State.ToString(),ticks=iterations,kills=stats.TotalKills,homicides=stats.Homicides,money=build.Economy.Money});
            if(!Find<GameStateController>().IsPlaying)break;
            waves.Tick(2);for(int i=0;i<100&&waves.Landings.ActiveBoats>0;i++)waves.Landings.Tick(1);
            foreach(var defense in Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None)) {build.TryUpgrade(defense);build.TryUpgrade(defense);build.TryRepair(defense);}
            // Reinvest only actually earned income in additional MGs beside separate corridors.
            foreach(var cell in new[]{new Vector2Int(14,5),new Vector2Int(15,11),new Vector2Int(17,16),new Vector2Int(22,5),new Vector2Int(22,11)})
                if(build.Economy.Money>=mg.Cost&&build.Validate(mg,cell)==PlacementResult.Valid)build.TryPlaceDefense(mg,cell,out _);
        }
        Capture("early-wave-result");string report=JsonConvert.SerializeObject(new{initialMoney=initial,fixture="Explicit 0.1-second shared wave/enemy/MG ticks; no extra money or health. Actual earned-income reinvestment.",results},Formatting.Indented);
        File.WriteAllText(Path.Combine(Folder,"early-earned-income.json"),report);return report;
    }
    public static async Task<string> Screens()
    {
        Time.timeScale=0;var build=Find<BuildManager>();build.Economy.AddMoney(50000);var hud=Find<BuildHud>();
        foreach(var cell in new[]{new Vector2Int(9,5),new Vector2Int(10,5),new Vector2Int(14,8),new Vector2Int(14,9)})build.TryPlaceDefense(hud.Defenses.First(d=>d.Id=="barbed-wire"),cell,out _);
        build.TryPlaceDefense(hud.Defenses.First(d=>d.Id=="flamethrower"),new Vector2Int(14,6),out _);
        Capture("container-defenses");var waves=Find<WaveManager>();var boat=Find<BoatLandingSystem>();
        foreach(var id in new[]{"multi-sector","mass-landing"})
        {
            var d=Find<RunSession>().Capture();d.wave=30;d.completedWaves=29;waves.RestorePreparation(d);waves.ForceEvent(id);waves.Events.Tick(100);waves.Events.BeginWave();boat.ConfigurePressure(.1f,8);
            for(int i=0;i<60;i++){boat.TryDispatch(waves.EndlessSettings.enemies[0],1);boat.Tick(.15f);await Task.Delay(1);}Capture(id);boat.Cancel();foreach(var e in Find<EnemyRegistry>().Active.ToArray())e.Health.TakeDefenseDamage(int.MaxValue);await Task.Delay(50);
        }
        waves.Events.Cancel();boat.ConfigurePressure(.1f,4);
        boat.TryDispatch(waves.EndlessSettings.bossEncounters.bosses[0],1);
        for(int i=0;i<150&&Find<EnemyRegistry>().Count==0;i++){boat.Tick(.15f);await Task.Delay(1);}await Task.Delay(150);ScreenCapture("boss-encounter-hud");Capture("boss-encounter");
        return "Captured container defenses, multi-sector boats, mass landing and boss.";
    }
}

