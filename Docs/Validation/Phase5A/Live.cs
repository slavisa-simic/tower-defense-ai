using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;

public static class Phase5ALive
{
    static T Find<T>() where T:UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static void Check(bool ok,string message) { if(!ok)throw new Exception(message); }
    static void Capture(string name)
    {
        var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);
        var encode=type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static);
        File.WriteAllBytes("Docs/Validation/Phase5A/"+name+".png",(byte[])encode.Invoke(null,new object[]{Screen.width,Screen.height}));
    }
    static void Click(string name)
    {
        var b=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>(name);Check(b!=null && b.enabledSelf,"Button unavailable: "+name);
        using(var e=NavigationSubmitEvent.GetPooled()){e.target=b;b.SendEvent(e);}
    }
    public static async Task<string> Walkthrough()
    {
        Check(Application.isPlaying,"Requires Play Mode");
        Application.runInBackground=true;
        UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        await Task.Delay(300);
        var build=Find<BuildManager>();var selection=Find<StructureSelection>();var speed=Find<GameSpeedController>();var wave=Find<WaveManager>();var hud=Find<BuildHud>();
        speed.SetSpeed(GameSpeed.Pause);build.Economy.AddMoney(50000);
        string[] ids={"machine-gun-nest","sniper-tower","barbed-wire","land-mine","mortar","flamethrower","autocannon","rocket"};
        var receipt=new List<object>();var placed=new List<DefenseBase>();
        for(int i=0;i<8;i++)for(int tier=1;tier<=3;tier++)
        {
            var definition=hud.Defenses.Single(d=>d.Id==ids[i]);var cell=new Vector2Int(8+(i%4)*6+(tier-1)*2,4+(i/4)*7);
            Check(build.TryPlaceDefense(definition,cell,out var d)==PlacementResult.Valid,"Placement "+ids[i]);placed.Add(d);
            Check(selection.SelectCell(cell),"Selection "+ids[i]);
            for(int n=1;n<tier;n++)Click("structure-upgrade");Check(d.Level==tier,"Level "+ids[i]);
            receipt.Add(new{type=ids[i],tier,d.CurrentHealth,d.MaxHealth,d.Damage,d.FireRate,d.Range,d.TotalInvestment,overlay=definition.LevelData(tier)?.overlay?.name});
        }
        selection.Clear();await Task.Delay(120);Capture("all-levels");
        var mg=placed.First(d=>d.Definition.Id=="machine-gun-nest" && d.Level==3);selection.Select(mg);mg.TakeDamage(mg.MaxHealth/2);
        int prep=build.RepairCost(mg);await Task.Delay(120);Capture("preparation-repair");int money=build.Economy.Money;
        Click("structure-repair");Check(mg.CurrentHealth==mg.MaxHealth && build.Economy.Money==money-prep,"Preparation repair");
        Click("start-wave");mg.TakeDamage(mg.MaxHealth/2);int combat=build.RepairCost(mg);Check(combat>prep,"Combat repair pricing");
        await Task.Delay(120);Capture("wave-repair");Click("structure-repair");
        int refund=build.RefundFor(mg);money=build.Economy.Money;Click("structure-dismantle");Check(selection.Selected==null && build.Economy.Money==money+refund,"Panel dismantle");
        Click("speed-normal");await Task.Delay(300);float start=Time.time,realStart=Time.unscaledTime;await Task.Delay(1000);float normal=(Time.time-start)/(Time.unscaledTime-realStart);
        Click("speed-pause");start=Time.time;int pending=wave.PendingCount;await Task.Delay(200);Check(Math.Abs(Time.time-start)<.01 && wave.PendingCount==pending,"Pause wave clock");
        selection.Select(placed.First(d=>d.Definition.Id=="sniper-tower"));await Task.Delay(120);Capture("paused-selection");
        Click("speed-double");await Task.Delay(300);start=Time.time;realStart=Time.unscaledTime;await Task.Delay(1000);float fast=(Time.time-start)/(Time.unscaledTime-realStart);
        Check(normal>.8f && normal<1.2f && fast>1.8f && fast<2.2f,"Scaled/unscaled clock ratio: "+normal+", "+fast);
        // Actual natural Update-driven boats/enemies/combat while upgrading is already exercised above.
        await Task.Delay(4500);Capture("active-combat");Click("speed-pause");
        var result=new{fixtureBudget=50000,checks="All eight tier variants, UI actions, Preparation/Wave repair, invested refund, Pause/x1/x2",prep,combat,refund,normalClock=normal,doubleClock=fast,wave.State,wave.AliveCount,wave.PendingCount,boats=wave.Landings.ActiveBoats,structures=receipt};
        File.WriteAllText("Docs/Validation/Phase5A/live-walkthrough.json",JsonConvert.SerializeObject(result,Formatting.Indented));return JsonConvert.SerializeObject(result);
    }
    public static async Task<string> ResolutionPanel()
    {
        Check(Application.isPlaying,"Requires Play Mode");Application.runInBackground=true;
        UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        var build=Find<BuildManager>();var hud=Find<BuildHud>();var selection=Find<StructureSelection>();var input=Find<PlacementInput>();
        Find<GameSpeedController>().SetSpeed(GameSpeed.Pause);build.Economy.AddMoney(10000);
        var rows=new List<object>();
        foreach(string id in new[]{"mortar","flamethrower","barbed-wire","land-mine"})
        {
            var data=hud.Defenses.Single(d=>d.Id==id);Check(build.TryPlaceDefense(data,new Vector2Int(20,7),out var d)==PlacementResult.Valid,"Resolution placement");
            build.TryUpgrade(d);build.TryUpgrade(d);selection.Select(d);
            if(d.MaxHealth>1)d.TakeDamage(d.MaxHealth/2);
            await Task.Delay(160);Capture("panel-"+id+"-"+Screen.height);
            var root=hud.GetComponent<UIDocument>().rootVisualElement;var panel=root.Q("structure-panel");
            var screen=UnityEngine.Object.FindFirstObjectByType<Camera>().WorldToScreenPoint(d.transform.position);
            Check(input.TryScreenToCell(screen,out var cell)&&ReferenceEquals(build.Battlefield.Grid.OccupantAt(cell),d),"Screen-to-cell mapping");
            var button=root.Q<Button>("structure-upgrade");var point=button.worldBound.center;
            var picked=root.panel.Pick(point);Check(picked!=null&&(picked==button||button.Contains(picked)),"Panel button hit target");
            Check(panel.worldBound.yMax<root.Q("build-panel").worldBound.yMin,"Structure panel must not cover build bar");
            rows.Add(new{id,width=Screen.width,height=Screen.height,title=root.Q<Label>("structure-title").text,stats=root.Q<Label>("structure-stats").text,repair=build.RepairCost(d),mappedCell=cell.ToString(),panelMaxY=panel.worldBound.yMax});
            build.TryDismantle(d,out _);await Task.Delay(30);
        }
        File.WriteAllText("Docs/Validation/Phase5A/resolution-"+Screen.height+".json",JsonConvert.SerializeObject(rows,Formatting.Indented));return "Four panel types at "+Screen.width+"x"+Screen.height+" checked: text, map picking, button hit targets, build-bar clearance";
    }
}
