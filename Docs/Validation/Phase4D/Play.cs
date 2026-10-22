using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
public static class Phase4DPlay {
 const string Folder="Docs/Validation/Phase4D/";
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 static void Set(object obj,string field,object value)=>obj.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value);
 static void Capture(string name){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);File.WriteAllBytes(Folder+name+".png",(byte[])type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{Screen.width,Screen.height}));}
 static bool Buy(string id,int x,int y){var d=Find<BuildHud>().Defenses.First(v=>v.Id==id);return Find<BuildManager>().TryPlaceDefense(d,new Vector2Int(x,y),out _)==PlacementResult.Valid;}
 static async Task Fresh(int wave){Time.timeScale=0;var op=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!op.isDone)await Task.Delay(20);await Task.Delay(100);Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();Find<PlacementInput>().enabled=false;var w=Find<WaveManager>();Set(w,"waveIndex",wave-1);typeof(WaveManager).GetMethod("ReleaseGenerated",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(w,null);Set(w,"generator",new EndlessWaveGenerator(w.EndlessSettings,1977));}
 static void StartWave(){var button=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave");using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}if(Find<WaveManager>().State!=WaveState.Spawning)throw new Exception("Start Wave UI failed");}
 public static async Task<string> Early(int layout){await Fresh(1);
  if(layout==0){Buy("sniper-tower",23,9);Buy("machine-gun-nest",23,4);Buy("barbed-wire",27,4);}
  if(layout==1){Buy("flamethrower",10,9);Buy("machine-gun-nest",10,4);Buy("barbed-wire",9,9);}
  if(layout==2){Buy("mortar",15,9);Buy("barbed-wire",9,4);Buy("barbed-wire",9,14);}
  if(layout==3){Buy("flamethrower",26,9);Buy("machine-gun-nest",24,9);Buy("barbed-wire",25,9);}if(layout==4){Buy("flamethrower",26,9);Buy("land-mine",24,9);Buy("land-mine",25,9);Buy("barbed-wire",23,9);}if(layout==5 || layout==6){Buy("mortar",15,9);Buy("land-mine",24,9);}var w=Find<WaveManager>();var stats=Find<RunStatistics>();var registry=Find<EnemyRegistry>();var boats=Find<BoatLandingSystem>();var rows=new List<object>();float nextMineTime=0;
  for(int n=1;n<=5 && Find<GameStateController>().IsPlaying;n++){
   int planned=w.CurrentWave.ScheduledCount,peak=0,bpeak=0;long h=stats.Homicides,k=stats.TotalKills;long income=Find<BuildManager>().Economy.TotalEarned;
   StartWave();Time.timeScale=3;double start=EditorApplication.timeSinceStartup;bool capture=false;
   while(w.State!=WaveState.Preparation && Find<GameStateController>().IsPlaying && EditorApplication.timeSinceStartup-start<24){await Task.Delay(30);peak=Math.Max(peak,registry.Count);bpeak=Math.Max(bpeak,boats.ActiveBoats);
    if(layout>=7){var mine=Find<BuildHud>().Defenses.First(d=>d.Id=="land-mine");Vector2Int bestCell=default;int bestCount=3;foreach(var enemy in registry.Active){if(!Find<BuildManager>().Battlefield.Grid.TryWorldToCell(enemy.transform.position+enemy.Movement.FacingDirection*enemy.Movement.CurrentSpeed*mine.ArmingDelay,out var cell)||Find<BuildManager>().Battlefield.Grid.OccupantAt(cell)!=null||cell.x<6||cell.x>27)continue;var center=Find<BuildManager>().Battlefield.Grid.CellToWorld(cell);int count=registry.Active.Where(e=>(e.transform.position+e.Movement.FacingDirection*e.Movement.CurrentSpeed*mine.ArmingDelay-center).sqrMagnitude<Mathf.Pow(mine.RangeInCells*Find<BuildManager>().Battlefield.Grid.CellSize,2)).Sum(e=>e.Health.CurrentHealth<=mine.Damage?4:1);if(count>bestCount){bestCount=count;bestCell=cell;}}if(bestCount>3 && (layout==7 || Time.time>=nextMineTime)){if(Buy("land-mine",bestCell.x,bestCell.y))nextMineTime=Time.time+.4f;}if(layout==8 && stats.TotalKills>=20){if(Find<BuildManager>().Economy.Money>=1400)Buy("rocket",19,9);foreach(int y in new[]{9,4,14})if(Find<BuildManager>().Economy.Money>=625)Buy("mortar",17,y);foreach(int y in new[]{9,4,14})if(Find<BuildManager>().Economy.Money>=525)Buy("flamethrower",24,y);}}else if(layout==6){foreach(int x in new[]{17,19,21})Buy("mortar",x,9);foreach(int y in new[]{4,14})Buy("flamethrower",24,y);}else foreach(int y in new[]{4,14,9}) Buy("flamethrower",24,y);if(layout<7 && Find<BuildManager>().Economy.Money>=500) foreach(int y in new[]{4,14,9}){Buy("barbed-wire",23,y);Buy("barbed-wire",23,y+1);Buy("barbed-wire",23,y-1);} 
    if(!capture && registry.Count>=50){Capture("early-"+layout+"-wave-"+n);capture=true;}
   }
   Time.timeScale=0;rows.Add(new{wave=n,planned,peak,bpeak,kills=stats.TotalKills-k,homicides=stats.Homicides-h,income=Find<BuildManager>().Economy.TotalEarned-income,seconds=(EditorApplication.timeSinceStartup-start)*3,state=w.State.ToString()});if(w.State!=WaveState.Preparation)break;
  }
  Capture("early-"+layout+"-end");File.WriteAllText(Folder+"early-"+layout+".json",JsonConvert.SerializeObject(new{externalMoney=0,startingMoney=500,rows,stats.Homicides,stats.HighestWaveReached,balance=Find<BuildManager>().Economy.Money},Formatting.Indented));return "Earned-income attempt "+layout+" reached wave "+stats.HighestWaveReached;
 }
 public static async Task<string> Prepared(int number){await Fresh(number);Find<BuildManager>().Economy.AddMoney(18000);
  foreach(int y in new[]{4,9,14}){Buy("machine-gun-nest",20,y);Buy("sniper-tower",22,y);Buy("flamethrower",24,y);Buy("mortar",17,y);Buy("autocannon",26,y);Buy("rocket",19,y);for(int dy=-2;dy<=2;dy++)Buy("barbed-wire",23,y+dy);}
  var w=Find<WaveManager>();var registry=Find<EnemyRegistry>();var stats=Find<RunStatistics>();var boats=Find<BoatLandingSystem>();int planned=w.CurrentWave.ScheduledCount;StartWave();Time.timeScale=3;double start=EditorApplication.timeSinceStartup;int peak=0,bpeak=0;bool capture=false;var samples=new List<object>();
  while(w.State!=WaveState.Preparation && Find<GameStateController>().IsPlaying && EditorApplication.timeSinceStartup-start<25){await Task.Delay(100);peak=Math.Max(peak,registry.Count);bpeak=Math.Max(bpeak,boats.ActiveBoats);samples.Add(new{enemies=registry.Count,boats=boats.ActiveBoats,pending=w.PendingCount,boss=Find<BossEncounterManager>().ActiveBoss?.Health.CurrentHealth});if(!capture && registry.Count>100){Capture("prepared-"+number);capture=true;}}
  Time.timeScale=0;Capture("prepared-"+number+"-end");File.WriteAllText(Folder+"prepared-"+number+".json",JsonConvert.SerializeObject(new{number,externalMoney=18000,planned,peak,bpeak,stats.Homicides,stats.TotalKills,stats.BossesKilled,stats.BossesBreached,income=Find<BuildManager>().Economy.TotalEarned-18000,seconds=(EditorApplication.timeSinceStartup-start)*3,state=w.State.ToString(),samples},Formatting.Indented));return "Prepared live combat wave "+number+": "+w.State;
 }
}








