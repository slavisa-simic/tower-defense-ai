using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase3CWalkthrough
{
    const string Folder="Docs/Validation/Phase3C/";
    static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
    static void Check(string name,bool pass,object detail=null)
    {
        string path=Folder+"live-checks.json";var rows=File.Exists(path)?JArray.Parse(File.ReadAllText(path)):new JArray();
        rows.Add(JObject.FromObject(new{name,pass,detail}));File.WriteAllText(path,rows.ToString());if(!pass)throw new Exception(name);
    }
    public static string Capture(string name)
    {
        // Use the installed pipeline's composited-backbuffer capture without creating temporary Unity assets.
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        byte[] bytes=(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height});File.WriteAllBytes(Folder+name+".png",bytes);
        return "Captured "+name+" at actual "+Screen.width+"x"+Screen.height;
    }
    static DefenseDefinition Data(string name)=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>("Assets/Game/Data/Defenses/"+name+".asset");
    static DefenseBase Build(string name,int x,int y)
    {
        var builder=Find<BuildManager>();var data=Data(name);
        if(builder.TryPlaceDefense(data,new Vector2Int(x,y),out var d)!=PlacementResult.Valid)throw new Exception("Cannot build "+name+" "+x+","+y);
        return d;
    }
    public static async Task<string> Prepare()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required");
        File.WriteAllText(Folder+"live-checks.json","[]");Time.timeScale=0;Application.runInBackground=true;
        Find<PlacementInput>().enabled=false;await Task.Delay(100);
        Check("Scene uses authoritative 32x18 XY grid",Find<BuildManager>().Battlefield.Grid.Width==32&&Find<BuildManager>().Battlefield.Grid.Height==18);
        Check("Preparation never auto spawns",Find<EnemyRegistry>().Count==0&&Find<WaveManager>().CanStart);
        Check("Normal scene has no visible placeholder sprites",!UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Any(r=>r.enabled&&r.gameObject.activeInHierarchy&&AssetDatabase.GetAssetPath(r.sprite).Contains("Placeholders")));
        Check("Decorative props have no colliders",GameObject.Find("Danube Quarantine Dressing").GetComponentsInChildren<Collider2D>().Length==0&&GameObject.Find("Danube Quarantine Dressing").GetComponentsInChildren<Collider>().Length==0);
        Capture("preparation-1080");
        var placement=Find<PlacementController>();
        foreach(var data in Find<BuildHud>().Defenses)
        {
            Check(data.Id+" select",placement.Select(data));placement.HoverCell(new Vector2Int(15,6));
            var ghost=GameObject.Find("Placement Preview").GetComponent<SpriteRenderer>();
            Check(data.Id+" valid production ghost",placement.Result==PlacementResult.Valid&&ghost.sprite!=null&&!AssetDatabase.GetAssetPath(ghost.sprite).Contains("Placeholders")&&ghost.color.g>ghost.color.r);
            placement.HoverCell(new Vector2Int(3,6));Check(data.Id+" invalid red ghost",placement.Result!=PlacementResult.Valid&&ghost.color.r>ghost.color.g);
        }
        placement.Cancel();
        Build("MachineGunNest",11,6);Build("MachineGunNest",11,11);Build("BarbedWire",8,8);Build("LandMine",8,9);
        Check("Purchases preserve economy and HUD",Find<BuildManager>().Economy.Money==75&&Find<BuildHud>().MoneyText=="Money: $75");
        Capture("built-preparation-1080");return "Preparation, all previews and 425-cost player layout verified";
    }
    public static async Task<string> Wave()
    {
        var waves=Find<WaveManager>();var b=Find<BuildManager>();var hp=Find<BaseHealth>();
        int number=waves.CurrentWaveNumber,before=b.Economy.Money,kills=0,bonus=waves.CurrentWave.CompletionReward;
        Action<EnemyController> killed=e=>{if(e.Health.KilledByDefense)kills++;};Find<EnemyRegistry>().EnemyKilled+=killed;
        try
        {
            // Submit through the real UI Toolkit control, then let movement/combat/waves tick naturally.
            var button=Find<WaveHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave");
            using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}
            Check("Wave "+number+" started by UI",waves.State==WaveState.Spawning);
            Check("Wave "+number+" controls reflect combat",!Find<WaveHud>().StartAvailable);
            Capture("wave-"+number+"-incoming");Time.timeScale=3;
            var timer=System.Diagnostics.Stopwatch.StartNew();bool captured=false,shotsCaptured=false;
            while(waves.CompletedWaves<number&&b.Game.IsPlaying&&timer.Elapsed.TotalSeconds<40)
            {
                if(!captured&&Find<EnemyRegistry>().Count>=4)
                {Time.timeScale=0;await Task.Delay(80);Capture("wave-"+number+"-combat");captured=true;Time.timeScale=3;}
                if(!shotsCaptured&&Find<VfxService>().Pool(VfxChannel.Tracer).ActiveCount>0)
                {Time.timeScale=0;await Task.Delay(80);Capture("wave-"+number+"-firing");shotsCaptured=true;Time.timeScale=3;}
                await Task.Delay(20);
            }
            Time.timeScale=0;
            Check("Wave "+number+" completes through real combat",waves.CompletedWaves==number,new{waves.CompletedWaves,waves.State,hp.CurrentHealth,kills});
            Check("Wave "+number+" pays exact kills and completion",b.Economy.Money==before+kills*25+bonus,new{before,after=b.Economy.Money,kills,bonus});
            Capture("wave-"+number+"-complete");
            Time.timeScale=1;float until=Time.time+1.7f;while(Time.time<until)await Task.Delay(20);Time.timeScale=0;
            Check("Wave "+number+" returns cleanly",number==waves.Waves.Count?waves.State==WaveState.AllWavesComplete:waves.CanStart);
            return "Natural wave "+number+" complete; kills="+kills+", base HP="+hp.CurrentHealth+", money="+b.Economy.Money;
        }finally{Find<EnemyRegistry>().EnemyKilled-=killed;Time.timeScale=0;}
    }
    public static string Reinforce()
    {
        int completed=Find<WaveManager>().CompletedWaves;
        if(completed==1)Build("SniperTower",17,9);
        else if(completed==2){Build("MachineGunNest",15,4);Build("MachineGunNest",15,13);}
        else if(completed==3){Build("MachineGunNest",20,7);Build("LandMine",18,10);}
        else if(completed==4){Build("MachineGunNest",21,12);Build("BarbedWire",17,8);}
        return "Purchased next-wave defenses using earned money only";
    }
    public static async Task<string> GameOver()
    {
        Find<BaseHealth>().TakeDamage(1);await Task.Delay(80);
        Check("Base HP bar follows gameplay",Find<PrototypeHud>().HealthText.Contains("99 / 100"));
        Find<BaseHealth>().TakeDamage(10000);await Task.Delay(80);
        Check("Game Over closes build and wave controls",Find<PrototypeHud>().GameOverVisible&&!Find<BuildHud>().BuildAvailable&&!Find<WaveHud>().StartAvailable);
        Check("Game Over clears pooled effects",Enum.GetValues(typeof(VfxChannel)).Cast<VfxChannel>().All(c=>Find<VfxService>().Pool(c).ActiveCount==0));
        Capture("game-over-1080");return "Game Over and health feedback verified";
    }
    public static string ResolutionCheck()
    {
        var input=Find<PlacementInput>();var grid=Find<BuildManager>().Battlefield.Grid;
        for(int y=0;y<grid.Height;y++)for(int x=0;x<grid.Width;x++)
        {
            var cell=new Vector2Int(x,y);var screen=Camera.main.WorldToScreenPoint(grid.CellToWorld(cell));
            Check("Cell "+cell+" accessible at "+Screen.width,input.TryScreenToCell(screen,out var actual)&&actual==cell&&!input.IsOverHud(screen));
        }
        var root=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement;
        var panel=new Vector2(root.resolvedStyle.width,root.resolvedStyle.height);
        var elements=new[]{"money-panel","wave-panel","health-panel","build-panel","start-wave"}.Select(n=>root.Q(n)).ToArray();
        foreach(var e in elements)Check(e.name+" in panel at "+Screen.width,e.worldBound.xMin>=0&&e.worldBound.yMin>=0&&e.worldBound.xMax<=panel.x+.5f&&e.worldBound.yMax<=panel.y+.5f);
        File.WriteAllText(Folder+"resolution-"+Screen.width+".json",JsonConvert.SerializeObject(new{Screen.width,Screen.height,panel=panel.ToString(),camera=Camera.main.orthographicSize,viewport=Camera.main.rect.ToString(),elements=elements.Select(e=>new{e.name,bounds=e.worldBound.ToString()})},Formatting.Indented));
        Capture("integrated-"+Screen.width);return "All 576 cells and HUD panels validated at actual "+Screen.width+"x"+Screen.height;
    }
}
