using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Profiling;
using Newtonsoft.Json;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;

// Editor-only disposable live fixture. Never save the scene in Play Mode.
public static class Phase3CStress
{
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static async Task<object> Sample(int count)
    {
        var samples=new List<double>(2048);var allocationSamples=new List<long>(2048);var drawSamples=new List<long>(2048);int last=-1;
        var allocations=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
        var draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);
        bool allocationCounter=allocations.Valid,drawCounter=draws.Valid;
        EditorApplication.CallbackFunction observe=()=>{if(Time.frameCount!=last){last=Time.frameCount;samples.Add(Time.unscaledDeltaTime*1000.0);if(allocationCounter)allocationSamples.Add(allocations.LastValue);if(drawCounter)drawSamples.Add(draws.LastValue);}};
        Time.timeScale=1;await Task.Delay(300);
        EditorApplication.update+=observe;
        try{await Task.Delay(3000);}finally{EditorApplication.update-=observe;Time.timeScale=0;allocations.Dispose();draws.Dispose();}
        samples.Sort();allocationSamples.Sort();drawSamples.Sort();var turrets=UnityEngine.Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None);
        return new{requestedEnemies=count,activeEnemies=Find<EnemyRegistry>().Active.Count(),turrets=turrets.Length,
            shots=turrets.Sum(t=>t.ShotsFired),samples=samples.Count,
            medianMs=samples.Count>0?samples[samples.Count/2]:0,
            p95Ms=samples.Count>0?samples[(int)((samples.Count-1)*.95)]:0,
            maxMs=samples.Count>0?samples.Last():0,
            allocationCounter,medianFrameGcBytes=allocationSamples.Count>0?allocationSamples[allocationSamples.Count/2]:-1,
            p95FrameGcBytes=allocationSamples.Count>0?allocationSamples[(int)((allocationSamples.Count-1)*.95)]:-1,
            drawCounter,medianDrawCalls=drawSamples.Count>0?drawSamples[drawSamples.Count/2]:-1};
    }
    static void AddEnemies(int start,int end)
    {
        var b=Find<BuildManager>();var spawner=Find<EnemySpawner>();
        for(int i=start;i<end;i++)
        {
            if(!spawner.TrySpawn(spawner.DefaultPrefab,out var e))throw new Exception("Spawn failed "+i);
            e.transform.position=b.Battlefield.Grid.CellToWorld(new Vector2Int(4+i%7,1+i%16));
            e.Movement.Initialize(b.Battlefield.Grid,b.Battlefield.Paths,b.Battlefield.Definition.BaseCell,e.Definition.MovementSpeed,e.Definition.AttackRange);
            e.Health.Initialize(100000);
        }
    }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        Time.timeScale=0;Time.maximumDeltaTime=.33333334f;Application.runInBackground=true;
        Find<PlacementInput>().enabled=false;
        var b=Find<BuildManager>();var fx=Find<VfxService>();b.Economy.AddMoney(100000);
        var data=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/MachineGunNest.asset");
        for(int x=12;x<=21;x+=3)for(int y=1;y<=15;y+=2)
            if(b.TryPlaceDefense(data,new Vector2Int(x,y),out var d)!=PlacementResult.Valid)throw new Exception("Placement "+x+","+y);
        foreach(string name in new[]{"SniperTower","BarbedWire","LandMine"})
        {
            var additional=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+name+".asset");
            var cell=name=="SniperTower"?new Vector2Int(23,9):name=="BarbedWire"?new Vector2Int(10,7):new Vector2Int(10,11);
            if(b.TryPlaceDefense(additional,cell,out _)!=PlacementResult.Valid)throw new Exception("Additional stress defense "+name);
        }
        AddEnemies(0,100);var first=await Sample(100);
        AddEnemies(100,200);var second=await Sample(200);
        var channels=Enum.GetValues(typeof(VfxChannel)).Cast<VfxChannel>().ToArray();
        var natural=channels.Select(c=>new{channel=c.ToString(),fx.Pool(c).ActiveCount,fx.Pool(c).PeakCount,fx.Pool(c).RecycledCount}).ToArray();
        int renderersBefore=fx.GetComponentsInChildren<SpriteRenderer>().Length;
        // Capture the populated operational scene before synthetic overflow changes effect contents.
        var captureType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        byte[] image=(byte[])captureType.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,new object[]{Screen.width,Screen.height});
        File.WriteAllBytes("Docs/Validation/Phase3C/heavy-combat-1080.png",image);
        var sequence=fx.Catalog.Get(VfxId.MgMuzzle);
        // Warm JIT and Unity native setters, then measure same-thread managed dispatch only.
        for(int i=0;i<100;i++)fx.Play(VfxId.MgMuzzle,Vector3.zero);
        var watch=new System.Diagnostics.Stopwatch();long bytesBefore=GC.GetAllocatedBytesForCurrentThread();watch.Start();
        for(int i=0;i<10000;i++)fx.Play(VfxId.MgMuzzle,Vector3.zero);
        watch.Stop();long allocated=GC.GetAllocatedBytesForCurrentThread()-bytesBefore;
        foreach(var channel in channels)
        {
            var pool=fx.Pool(channel);
            for(int i=0;i<pool.Capacity+10;i++)pool.Play(sequence.frames,sequence.fps,Vector3.zero,Quaternion.identity,Vector3.one,"Effects",1,1,1,.5f,Vector2.zero);
            if(pool.ActiveCount!=pool.Capacity)throw new Exception("Cap failure "+channel);
        }
        var caps=channels.Select(c=>new{channel=c.ToString(),fx.Pool(c).Capacity,fx.Pool(c).ActiveCount,fx.Pool(c).RecycledCount}).ToArray();
        int renderersAfter=fx.GetComponentsInChildren<SpriteRenderer>().Length;
        if(renderersBefore!=renderersAfter||channels.Sum(c=>fx.Pool(c).Capacity)!=256)throw new Exception("Effect objects accumulated");
        fx.Clear();foreach(var c in channels)if(fx.Pool(c).ActiveCount!=0)throw new Exception("Clear failed");
        File.WriteAllText("Docs/Validation/Phase3C/stress-results.json",JsonConvert.SerializeObject(new{
            date="2026-10-01",unity=Application.unityVersion,platform=SystemInfo.operatingSystem,cpu=SystemInfo.processorType,gpu=SystemInfo.graphicsDeviceName,
            context="Unity Editor, 1920x1080 Game View; unique rendered-frame delta samples observed through EditorApplication.update. Includes editor overhead. Not a Player benchmark.",
            runs=new[]{first,second},natural,dispatch=new{requests=10000,elapsedMs=watch.Elapsed.TotalMilliseconds,managedAllocatedBytes=allocated},caps,renderersBefore,renderersAfter,cleared=true
        },Formatting.Indented));
        return "100/200 enemy stress and all pool overflow checks passed; paused";
    }
}

