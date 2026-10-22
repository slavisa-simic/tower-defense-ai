using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.Waves;
using ShorelineDefense.Base;

public static class DefenseArtWalkthrough
{
    const string Folder="Docs/Validation/Phase3A3/";
    static List<object> checks=new List<object>();
    static void Check(string name,bool pass,object detail=null)
    {
        checks.Add(new{name,pass,detail});
        File.WriteAllText(Folder+"live-checks.json",JsonConvert.SerializeObject(checks,Formatting.Indented));
        if(!pass)throw new Exception(name);
    }
    static SpriteRenderer Body(DefenseBase d) => d.GetComponentsInChildren<SpriteRenderer>().First(r=>r.enabled&&r.sortingLayerName!="Effects"&&r.transform.parent==d.transform);
    static void Capture(string name,int width=1920,int height=1080)
    {
        var c=Camera.main;var oldTarget=c.targetTexture;float aspect=c.aspect;var oldActive=RenderTexture.active;
        var rt=RenderTexture.GetTemporary(width,height,24);var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
        try
        {
            c.targetTexture=rt;c.aspect=(float)width/height;c.Render();RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Folder+name+".png",image.EncodeToPNG());
        }
        finally {c.targetTexture=oldTarget;c.aspect=aspect;RenderTexture.active=oldActive;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(image);}
    }
    static EnemyController Spawn(BuildManager b,Vector2Int cell)
    {
        var spawner=UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        if(!spawner.TrySpawn(spawner.DefaultPrefab,out var e))throw new Exception("Spawn failed");
        e.transform.position=b.Battlefield.Grid.CellToWorld(cell);
        e.Movement.Initialize(b.Battlefield.Grid,b.Battlefield.Paths,b.Battlefield.Definition.BaseCell,e.Definition.MovementSpeed,e.Definition.AttackRange);
        return e;
    }
    public static async Task<string> Run()
    {
        if(!Application.isPlaying)throw new Exception("Enter primary scene Play Mode first");
        Application.runInBackground=true;Time.timeScale=0;Time.maximumDeltaTime=.02f;
        var b=UnityEngine.Object.FindFirstObjectByType<BuildManager>();var p=UnityEngine.Object.FindFirstObjectByType<PlacementController>();
        var input=UnityEngine.Object.FindFirstObjectByType<PlacementInput>();input.enabled=false;
        b.Economy.AddMoney(2000); // Temporary validation budget; never serialized.
        var ghost=(SpriteRenderer)new SerializedObject(p).FindProperty("preview").objectReferenceValue;
        string[] names={"MachineGunNest","SniperTower","BarbedWire","LandMine"};
        var actors=new List<DefenseBase>();int before=b.Economy.Money;
        for(int i=0;i<names.Length;i++)
        {
            var data=AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+names[i]+".asset");
            Check(names[i]+" selection",p.Select(data));p.HoverCell(new Vector2Int(12+i*2,6));
            Check(names[i]+" valid actual-art ghost",p.Result==PlacementResult.Valid&&p.PreviewVisible&&ghost.sprite==data.Prefab.GetComponent<DefenseSpritePresentation>().PreviewSprite&&ghost.color.g>ghost.color.r&&ghost.transform.localScale==Vector3.one);
            Capture("ghost-"+names[i]+"-valid");
            p.HoverCell(new Vector2Int(4,8));Check(names[i]+" invalid red ghost",p.Result!=PlacementResult.Valid&&ghost.color.r>ghost.color.g);
            Capture("ghost-"+names[i]+"-invalid");p.HoverCell(new Vector2Int(12+i*2,6));
            Check(names[i]+" placement",p.TryPlaceCurrent(out var d)==PlacementResult.Valid);
            d.name="P3A3 "+names[i];actors.Add(d);
            Check(names[i]+" grid/root",d.Anchor==new Vector2Int(12+i*2,6)&&d.Definition.Footprint==Vector2Int.one&&d.transform.rotation==Quaternion.identity&&d.transform.position==b.Battlefield.Grid.FootprintCenter(d.Anchor,Vector2Int.one));
            if(d.TryGetComponent<TurretCombat>(out var combat))combat.enabled=false;
        }
        p.Cancel();Check("Economy charged original costs",before-b.Economy.Money==575);
        await Task.Delay(100);Capture("defenses-idle-1080");Capture("defenses-idle-1440",2560,1440);
        foreach(var d in actors)Check(d.name+" no visible shape placeholders",d.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled&&r.sortingLayerName!="Effects").All(r=>r.sprite!=null&&AssetDatabase.GetAssetPath(r.sprite).StartsWith("Assets/Game/Art/Defenses/")));
        var target=Spawn(b,new Vector2Int(14,9));target.Health.Initialize(10000);
        var mg=actors[0].GetComponent<TurretCombat>();var sniper=actors[1].GetComponent<TurretCombat>();
        int health=target.Health.CurrentHealth;mg.Tick(.2f);sniper.Tick(.2f);
        Check("MG/sniper original damage",target.Health.CurrentHealth==health-120&&mg.ShotsFired==1&&sniper.ShotsFired==1);
        Time.timeScale=1;await Task.Delay(80);Time.timeScale=0;
        foreach(var d in actors.Take(2))
        {
            var rotation=d.transform.Find("Rotating Weapon");var weapon=rotation.Find("Receiver").GetComponent<SpriteRenderer>();
            Check(d.name+" base fixed and weapon aims",d.transform.rotation==Quaternion.identity&&Body(d).transform.localRotation==Quaternion.identity&&Quaternion.Angle(rotation.rotation,Quaternion.identity)>10);
            Check(d.name+" recoil frame plays",weapon.sprite.name.Contains("__f"),weapon.sprite.name);
            var muzzle=rotation.Find("Muzzle");Check(d.name+" muzzle follows weapon",muzzle.parent==rotation&&muzzle.localPosition.y==0&&muzzle.localPosition.x>0,muzzle.localPosition.ToString());
        }
        Capture("defenses-firing-1080");float settle=Time.time+.3f;Time.timeScale=1;
        while(Time.time<settle)await Task.Delay(20);
        Time.timeScale=0;
        foreach(var d in actors.Take(2))Check(d.name+" returns to idle",d.transform.Find("Rotating Weapon/Receiver").GetComponent<SpriteRenderer>().sprite.name.Contains("idle"));
        target.Health.TakeDamage(10000);await Task.Delay(30);
        foreach(var d in actors.Take(3))d.TakeDamage(d.Definition.MaxHealth/2+1);
        await Task.Delay(60);
        foreach(var d in actors.Take(3))Check(d.name+" damaged state",Body(d).sprite.name.Contains("damaged"));
        Capture("defenses-damaged-1080");
        var slow=Spawn(b,actors[2].Anchor);Check("Wire remains walkable and slows",b.Battlefield.Grid.GetCell(actors[2].Anchor).Walkable&&Mathf.Approximately(slow.Movement.CurrentSpeed,slow.Definition.MovementSpeed*.5f));
        slow.transform.position=b.Battlefield.Grid.CellToWorld(actors[2].Anchor+Vector2Int.up);
        Check("Leaving wire restores speed",Mathf.Approximately(slow.Movement.CurrentSpeed,slow.Definition.MovementSpeed));slow.Health.TakeDamage(10000);
        var mine=actors[3].GetComponent<LandMine>();mine.Tick(1);await Task.Delay(60);
        Check("Mine uses existing armed state",mine.Armed&&Body(actors[3]).sprite.name.Contains("armed"));Capture("mine-armed-1080");
        var victim=Spawn(b,actors[3].Anchor);mine.Tick(.1f);
        Check("Mine triggers and releases occupancy",mine.Exploded&&!victim.Health.IsAlive&&b.Battlefield.Grid.OccupantAt(actors[3].Anchor)==null);
        await Task.Delay(60);Check("Consumed mine removed",actors[3]==null);
        var waves=UnityEngine.Object.FindFirstObjectByType<WaveManager>();Check("Start Wave works",waves.TryStartWave());waves.Tick(.8f);
        Check("Wave counts/spawn still work",waves.PendingCount==9&&waves.AliveCount==1,new{waves.PendingCount,waves.AliveCount,waves.EnemiesRemaining});
        Capture("wave-smoke-1080");
        UnityEngine.Object.FindFirstObjectByType<BaseHealth>().TakeDamage(10000);
        Check("Game Over stops building/waves",!b.Game.IsPlaying&&waves.State==WaveState.GameOver&&!waves.TryStartWave()&&!p.PreviewVisible);
        Capture("game-over-1080");
        input.enabled=true;Time.timeScale=0;
        return checks.Count+" live checks passed. Validation fixture paused at Game Over; stop Play Mode to discard.";
    }
}
