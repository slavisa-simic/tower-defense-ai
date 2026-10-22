using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase3C1Review
{
    static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
    static async Task Capture(string name)
    {
        var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));
        view.Repaint();EditorApplication.QueuePlayerLoopUpdate();
        await Task.Delay(150);
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var method=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes("Docs/Validation/Phase3C1/"+name+".png",(byte[])method.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    static DefenseBase Build(string name,int x,int y)
    {
        var data=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+name+".asset");
        if(Find<BuildManager>().TryPlaceDefense(data,new Vector2Int(x,y),out var defense)!=PlacementResult.Valid)throw new Exception("Build failed "+name);
        return defense;
    }
    public static async Task<string> Wire()
    {
        Time.timeScale=0;Find<PlacementInput>().enabled=false;Find<BuildManager>().Economy.AddMoney(5000);
        // Isolated, straight, corner, T and cross, all through normal placement commands.
        foreach(var c in new[]{new Vector2Int(10,3),new Vector2Int(12,3),new Vector2Int(13,3),new Vector2Int(14,3),
            new Vector2Int(10,7),new Vector2Int(10,8),new Vector2Int(11,7),new Vector2Int(15,8),new Vector2Int(15,9),new Vector2Int(15,7),new Vector2Int(16,8),
            new Vector2Int(21,11),new Vector2Int(21,12),new Vector2Int(21,10),new Vector2Int(22,11),new Vector2Int(20,11)})Build("BarbedWire",c.x,c.y);
        await Task.Delay(120);await Capture("wire-topologies");
        var masks=UnityEngine.Object.FindObjectsByType<ConnectedWirePresentation>(FindObjectsSortMode.None).Select(w=>new{position=w.transform.position.ToString(),w.Mask}).ToArray();
        File.WriteAllText("Docs/Validation/Phase3C1/wire-review.json",JsonConvert.SerializeObject(masks,Formatting.Indented));
        return "Captured placed wire topologies; test-only added currency";
    }
    public static async Task<string> Arrival()
    {
        Application.runInBackground=true;
        Time.timeScale=0;Find<PlacementInput>().enabled=false;
        Build("MachineGunNest",11,4);Build("MachineGunNest",17,4);Build("BarbedWire",8,5);Build("LandMine",8,2);
        await Task.Delay(120);await Capture("preparation");
        var waves=Find<WaveManager>();var boats=Find<BoatLandingSystem>();var enemies=Find<EnemyRegistry>();
        if(!waves.TryStartWave())throw new Exception("Cannot start wave");
        waves.Tick(.01f);waves.Tick(2.5f);await Task.Delay(120);await Capture("boat-arriving");
        while(enemies.Count==0) {waves.Tick(.25f);await Task.Delay(20);}
        await Task.Delay(120);await Capture("disembarking");
        File.WriteAllText("Docs/Validation/Phase3C1/arrival-review.json",JsonConvert.SerializeObject(new{waves.State,waves.PendingCount,waves.AliveCount,boats.ActiveBoats,boats.IncomingPassengers,boats.TotalArrivals,positions=enemies.Active.Select(e=>new{position=e.transform.position.ToString(),e.Movement.HasPath})},Formatting.Indented));
        Time.timeScale=0;
        return "Preparation, approach and first routed disembark captured; simulation paused for combat review";
    }
    public static async Task<string> Combat()
    {
        Application.runInBackground=true;
        bool fired=false;Action<Vector3> freeze=point=>{fired=true;Time.timeScale=0;};
        var turrets=UnityEngine.Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None);
        foreach(var turret in turrets)turret.Fired+=freeze;
        Time.timeScale=2;
        var until=System.Diagnostics.Stopwatch.StartNew();
        try
        {
            while(!fired&&until.Elapsed.TotalSeconds<25)await Task.Delay(10);
            Time.timeScale=0;if(!fired)throw new Exception("No actual turret shots observed");
            await Capture("active-combat");
            if(Find<EnemyRegistry>().Count==0)throw new Exception("No combat enemies");
            return "Captured active combat at a real turret Fired event";
        }
        finally{foreach(var turret in turrets)if(turret!=null)turret.Fired-=freeze;}
    }
    public static string Audit()
    {
        var vegetation=GameObject.Find("Riverbank Vegetation");
        var boats=Find<BoatLandingSystem>();
        var renderers=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        var result=new{Camera.main.orthographic,rotation=Camera.main.transform.eulerAngles.ToString(),
            vegetationRenderers=vegetation.GetComponentsInChildren<SpriteRenderer>().Length,
            vegetationBehaviors=vegetation.GetComponentsInChildren<MonoBehaviour>().Length,
            vegetationColliders=vegetation.GetComponentsInChildren<Collider>().Length+vegetation.GetComponentsInChildren<Collider2D>().Length,
            contactShadows=renderers.Count(r=>r.name=="Contact Shadow"),renderers=renderers.Length,
            materialInstances=renderers.Select(r=>r.sharedMaterial).Where(m=>m!=null&&m.name.EndsWith("(Instance)")).Count(),
            boats.Capacity,boats.ArrivalInterval,landingPoints=boats.LandingPoints.Select(p=>new{entry=p.riverEntry.ToString(),berth=p.berth.ToString(),unloading=p.unloadingPositions.Select(u=>u.ToString())})};
        File.WriteAllText("Docs/Validation/Phase3C1/scene-audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return JsonConvert.SerializeObject(result);
    }
}
