using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class VfxWalkthrough
{
    const string Folder="Docs/Validation/Phase3B/";
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static void Check(string name,bool pass,object detail=null)
    {
        string path=Folder+"live-checks.json";var rows=File.Exists(path)?JArray.Parse(File.ReadAllText(path)):new JArray();
        rows.Add(JObject.FromObject(new{name,pass,detail}));File.WriteAllText(path,rows.ToString());
        if(!pass)throw new Exception(name+" "+JsonConvert.SerializeObject(detail));
    }
    static void Capture(string name,int width=1920,int height=1080)
    {
        var c=Camera.main;var target=c.targetTexture;float aspect=c.aspect;var active=RenderTexture.active;
        var rt=RenderTexture.GetTemporary(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);
        try {c.targetTexture=rt;c.aspect=(float)width/height;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Folder+name+".png",tex.EncodeToPNG());}
        finally {c.targetTexture=target;c.aspect=aspect;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(tex);}
    }
    static async Task Advance(float seconds)
    {
        float until=Time.time+seconds;var watch=System.Diagnostics.Stopwatch.StartNew();Time.timeScale=1;
        while(Time.time<until && watch.Elapsed.TotalSeconds<15)await Task.Delay(15);
        Time.timeScale=0;if(Time.time<until)throw new Exception("Player time stalled");
    }
    static EnemyController Spawn(Vector2Int cell,int hp=10000)
    {
        var b=Find<BuildManager>();var s=Find<EnemySpawner>();
        if(!s.TrySpawn(s.DefaultPrefab,out var e))throw new Exception("Spawn blocked");
        e.transform.position=b.Battlefield.Grid.CellToWorld(cell);
        e.Movement.Initialize(b.Battlefield.Grid,b.Battlefield.Paths,b.Battlefield.Definition.BaseCell,e.Definition.MovementSpeed,e.Definition.AttackRange);
        e.Health.Initialize(hp);return e;
    }
    static DefenseBase Build(string name,Vector2Int cell)
    {
        var data=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+name+".asset");
        var b=Find<BuildManager>();if(b.TryPlaceDefense(data,cell,out var d)!=PlacementResult.Valid)throw new Exception("Placement failed "+name);
        d.name="P3B "+name+" "+cell;
        if(d.TryGetComponent<TurretCombat>(out var turret))turret.enabled=false;
        if(d.TryGetComponent<LandMine>(out var mine))mine.enabled=false;
        return d;
    }
    public static async Task<string> Combat()
    {
        if(!Application.isPlaying)throw new Exception("Enter primary scene Play Mode");
        File.WriteAllText(Folder+"live-checks.json","[]");
        Application.runInBackground=true;Time.timeScale=0;Time.maximumDeltaTime=.02f;
        Find<PlacementInput>().enabled=false;
        var b=Find<BuildManager>();var fx=Find<VfxService>();b.Economy.AddMoney(6000);
        var mg=Build("MachineGunNest",new Vector2Int(12,7));var sniper=Build("SniperTower",new Vector2Int(15,11));
        var wire=Build("BarbedWire",new Vector2Int(9,9));Build("LandMine",new Vector2Int(9,4));
        for(int i=0;i<3;i++)Build("MachineGunNest",new Vector2Int(12,3+i*5));
        var enemy=Spawn(new Vector2Int(10,8));enemy.name="P3B shot target";
        for(int i=0;i<10;i++)Spawn(new Vector2Int(7+i%3,2+i));
        await Task.Delay(100);
        int before=enemy.Health.CurrentHealth;
        mg.GetComponent<TurretCombat>().Tick(.01f);sniper.GetComponent<TurretCombat>().Tick(.01f);
        Check("MG and sniper preserve hitscan damage",enemy.Health.CurrentHealth==before-120,new{before,after=enemy.Health.CurrentHealth});
        Check("Both muzzle sprites spawned",fx.GetComponentsInChildren<SpriteRenderer>().Count(r=>r.enabled&&r.sprite!=null&&r.sprite.name.Contains("muzzle"))==2);
        Check("Both tracers spawned",fx.Pool(VfxChannel.Tracer).ActiveCount==2);
        Check("Hit tint visible",enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>().color!=Color.white);
        Capture("combat-shots-1080");Capture("combat-shots-1440",2560,1440);
        await Advance(.12f);
        Check("Hit tint restored",enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>().color==Color.white);
        Check("Tracers expire",fx.Pool(VfxChannel.Tracer).ActiveCount==0);
        var turrets=UnityEngine.Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None);
        foreach(var t in turrets)t.enabled=true;
        await Advance(3);
        foreach(var t in turrets)t.enabled=false;
        Check("Several turrets fire repeatedly",turrets.Sum(t=>t.ShotsFired)>30,turrets.Sum(t=>t.ShotsFired));
        Check("Frequent effects stay capped",fx.Pool(VfxChannel.Burst).PeakCount<=96&&fx.Pool(VfxChannel.Tracer).PeakCount<=64);
        Capture("combat-live-1080");
        int money=b.Economy.Money;int reward=enemy.Definition.KillReward;
        enemy.Health.TakeDefenseDamage(100000);
        Check("Death removes target immediately",!enemy.IsTargetable);
        Check("Kill reward exactly once",b.Economy.Money==money+reward);
        Check("Reward popup value",Find<CombatFeedbackHud>().LastRewardText=="+"+reward);
        Check("Death animation survives gameplay removal",fx.Pool(VfxChannel.Corpse).ActiveCount>0);
        await Task.Delay(80);Capture("death-reward-1080");
        return "Combat and death checks passed; paused for HUD capture";
    }
    public static async Task<string> WaterAndMine()
    {
        var b=Find<BuildManager>();var fx=Find<VfxService>();fx.Clear();
        var swimmer=Spawn(new Vector2Int(3,13));swimmer.name="P3B swimmer";
        await Task.Delay(100);Check("Wake visible in water",swimmer.GetComponent<EnemySpritePresentation>().WakeVisible);
        Capture("water-wake-1080");
        // Advance the real movement owner across the exact Water->Beach cell edge.
        swimmer.transform.position=b.Battlefield.Grid.CellToWorld(new Vector2Int(3,9));
        swimmer.Movement.Initialize(b.Battlefield.Grid,b.Battlefield.Paths,b.Battlefield.Definition.BaseCell,swimmer.Definition.MovementSpeed,swimmer.Definition.AttackRange);
        await Task.Delay(80);
        int guard=0;while(swimmer.GetComponent<EnemySpritePresentation>().WakeVisible&&guard++<100){swimmer.Movement.Tick(.05f);await Task.Delay(25);}
        Check("Wake stops after river exit",!swimmer.GetComponent<EnemySpritePresentation>().WakeVisible,swimmer.transform.position.ToString());
        var splash=fx.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r=>r.enabled&&r.sprite!=null&&r.sprite.name.Contains("water_splash_small"));
        Check("River exit splash uses small sheet",splash!=null);
        fx.Pool(VfxChannel.Burst).Tick(.12f);Capture("water-exit-splash-1080");
        var mine=UnityEngine.Object.FindObjectsByType<LandMine>(FindObjectsSortMode.None).Single();var defense=mine.GetComponent<DefenseBase>();
        var center=defense.transform.position;float radius=defense.Range;
        var inside=Spawn(defense.Anchor);inside.transform.position=center+Vector3.right*(radius-.05f);
        var outside=Spawn(defense.Anchor+Vector2Int.up*3);outside.transform.position=center+Vector3.right*(radius+.05f);
        var trigger=Spawn(defense.Anchor);int hpIn=inside.Health.CurrentHealth,hpOut=outside.Health.CurrentHealth;
        mine.Tick(1);
        Check("Mine detonates through gameplay",mine.Exploded);
        Check("Mine AOE stays authoritative",inside.Health.CurrentHealth==hpIn-defense.Definition.Damage&&outside.Health.CurrentHealth==hpOut,new{radius,hpIn,insideHp=inside.Health.CurrentHealth,hpOut,outsideHp=outside.Health.CurrentHealth});
        Check("Mine occupancy released immediately",b.Battlefield.Grid.OccupantAt(defense.Anchor)==null);
        fx.Pool(VfxChannel.Burst).Tick(.14f);Capture("mine-explosion-1080");Capture("mine-explosion-1440",2560,1440);
        await Advance(.6f);Capture("mine-smoke-1080");await Advance(.6f);
        Check("Mine smoke dissipates",fx.Pool(VfxChannel.Smoke).ActiveCount==0);
        var mg=UnityEngine.Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None).First(d=>d.GetComponent<TurretCombat>()!=null);
        Check("Full-health structures have no smoke",fx.Pool(VfxChannel.Smoke).ActiveCount==0);
        mg.TakeDamage(Mathf.CeilToInt(mg.Definition.MaxHealth*.7f));await Advance(1.3f);
        Check("Heavy damage emits restrained smoke",fx.Pool(VfxChannel.Smoke).ActiveCount>0);
        Capture("structure-smoke-1080");
        return "Water, mine, and smoke checks passed";
    }
    public static string WaveStart()
    {
        foreach(var e in Find<EnemyRegistry>().Active.ToArray())e.Health.TakeDamage(100000);
        Find<VfxService>().Clear();Check("Wave starts",Find<WaveManager>().TryStartWave());
        Check("Wave incoming banner",Find<CombatFeedbackHud>().BannerText.Contains("INCOMING"));
        return "Wave started; paused for screen capture";
    }
    public static async Task<string> WaveComplete()
    {
        var waves=Find<WaveManager>();var b=Find<BuildManager>();int before=b.Economy.Money,completion=waves.CurrentWave.CompletionReward;
        int guard=0;while(waves.State!=WaveState.WaveComplete&&guard++<100)
        {
            waves.Tick(10);
            foreach(var e in Find<EnemyRegistry>().Active.ToArray())e.Health.TakeDamage(100000);
            await Task.Delay(20);
        }
        Check("Wave completion reward exactly once",waves.State==WaveState.WaveComplete&&b.Economy.Money==before+completion,new{before,after=b.Economy.Money,completion});
        Check("Wave complete banner",Find<CombatFeedbackHud>().BannerText=="WAVE COMPLETE   +"+completion);
        return "Wave complete; paused for screen capture";
    }
    public static string GameOver()
    {
        var fx=Find<VfxService>();fx.Play(VfxId.BloodHit,Vector3.zero);Find<BaseHealth>().TakeDamage(100000);
        Check("Game Over stops waves/build",!Find<BuildManager>().Game.IsPlaying&&!Find<WaveManager>().TryStartWave());
        Check("Game Over clears effects",Enum.GetValues(typeof(VfxChannel)).Cast<VfxChannel>().All(c=>fx.Pool(c).ActiveCount==0));
        Check("Game Over rejects new effects",fx.Play(VfxId.MgMuzzle,Vector3.zero)==null);
        Check("Existing Game Over HUD stays visible",Find<PrototypeHud>().GameOverVisible);
        return "Game Over checks passed; paused for screen capture";
    }
}
