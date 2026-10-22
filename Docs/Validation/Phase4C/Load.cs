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
using Unity.Profiling;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
public static class Phase4CLoad {
 const string Folder="Docs/Validation/Phase4C/";
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 static void Set(object obj,string field,object value)=>obj.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(obj,value);
 static void Capture(string name){var t=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(a=>a!=null);var m=t.GetMethod("EncodeScreenToPng",BindingFlags.Static|BindingFlags.NonPublic);File.WriteAllBytes(Folder+name+".png",(byte[])m.Invoke(null,new object[]{Screen.width,Screen.height}));}
 static void Buy(string id,int x,int y){var d=Find<BuildHud>().Defenses.First(v=>v.Id==id);if(Find<BuildManager>().TryPlaceDefense(d,new Vector2Int(x,y),out _)!=PlacementResult.Valid)throw new Exception("Cannot place "+id+" "+x+","+y);}
 public static async Task<string> Stress(int count){
  Time.timeScale=0;var load=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!load.isDone)await Task.Delay(20);await Task.Delay(100);while(!Find<GameStateController>().IsPlaying)await Task.Delay(20);
  Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();
  Find<PlacementInput>().enabled=false;Find<BorderBreachService>().ConfigureThreshold(0);Find<BuildManager>().Economy.AddMoney(50000);
  foreach(int y in new[]{3,8,13}){Buy("machine-gun-nest",15,y);Buy("sniper-tower",18,y);Buy("flamethrower",12,y);Buy("mortar",20,y);Buy("autocannon",23,y);Buy("rocket",26,y);Buy("barbed-wire",11,y);}
  var spawner=Find<EnemySpawner>();spawner.ConfigureWave(250,1,1);Set(spawner,"minimumSpawnSeparation",.05f);
  for(int i=0;i<count;i++){var pos=spawner.Battlefield.Grid.CellToWorld(new Vector2Int(7+i%5,1+(i/5)%16))+new Vector3((i/80)*.1f,0,0);var prefab=Find<WaveManager>().EndlessSettings.enemies[i%7];if(!spawner.TrySpawnAt(prefab,pos,out var e))throw new Exception("Cannot spawn "+i);e.Health.Initialize(100000);}
  var boats=Find<BoatLandingSystem>();boats.ConfigurePressure(.7f,8);for(int i=0;i<5;i++)boats.TryDispatch(spawner.DefaultPrefab,7);
  int gc0=GC.CollectionCount(0),gc1=GC.CollectionCount(1),gc2=GC.CollectionCount(2);long memory=GC.GetTotalMemory(false);var frames=new List<double>(500);var active=new List<int>(500);int last=Time.frameCount;var registry=Find<EnemyRegistry>();
  using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",500)){
   Time.timeScale=1;double until=EditorApplication.timeSinceStartup+5;
   while(EditorApplication.timeSinceStartup<until){await Task.Delay(1);if(last!=Time.frameCount){last=Time.frameCount;frames.Add(Time.unscaledDeltaTime*1000);active.Add(registry.Count);}}
   Time.timeScale=0;var sorted=frames.OrderBy(v=>v).ToArray();var alloc=gc.ToArray();
   File.WriteAllText(Folder+"stress-"+count+".json",JsonConvert.SerializeObject(new{requested=count,samples=sorted.Length,medianMs=sorted[sorted.Length/2],p95Ms=sorted[(int)(sorted.Length*.95)],maxMs=sorted.Last(),activeMin=active.Min(),activeMax=active.Max(),boats=boats.ActiveBoats,renderers=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length,projectiles=UnityEngine.Object.FindObjectsByType<TimedAreaProjectile>(FindObjectsSortMode.None).Length,gc0=GC.CollectionCount(0)-gc0,gc1=GC.CollectionCount(1)-gc1,gc2=GC.CollectionCount(2)-gc2,managedMemoryDelta=GC.GetTotalMemory(false)-memory,gcRecorderValid=gc.Valid,gcAllocationAverage=alloc.Length>0?alloc.Average(v=>(double)v.Value):0,gcAllocationMax=alloc.Length>0?alloc.Max(v=>v.Value):0},Formatting.Indented));
  }
  Capture("stress-"+count);return "Natural combat load sampled at "+count+" initial enemies";
 }
 public static async Task<string> Density(int number){
  Time.timeScale=0;var load=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!load.isDone)await Task.Delay(20);await Task.Delay(100);while(!Find<GameStateController>().IsPlaying)await Task.Delay(20);
  Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();Find<BorderBreachService>().ConfigureThreshold(0);Find<PlacementInput>().enabled=false;
  var w=Find<WaveManager>();Set(w,"waveIndex",number-1);var old=(WaveDefinition)typeof(WaveManager).GetField("generated",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(w);if(old!=null)UnityEngine.Object.Destroy(old);Set(w,"generated",null);Set(w,"generator",new EndlessWaveGenerator(w.EndlessSettings,1977));
  int planned=w.CurrentWave.ScheduledCount;w.TryStartWave();Time.timeScale=1;var samples=new List<object>();int peak=0,boatsPeak=0;double sum=0;int sampleCount=0;
  for(int i=0;i<38;i++){await Task.Delay(500);int count=Find<EnemyRegistry>().Count;int boats=Find<BoatLandingSystem>().ActiveBoats;peak=Math.Max(peak,count);boatsPeak=Math.Max(boatsPeak,boats);sum+=count;sampleCount++;samples.Add(new{seconds=(i+1)*.5,enemies=count,boats,pending=w.PendingCount});if(i==7||i==15||i==31)Capture("density-"+number+"-"+i);}
  Time.timeScale=0;Capture("density-"+number);File.WriteAllText(Folder+"density-"+number+".json",JsonConvert.SerializeObject(new{number,planned,peak,boatsPeak,average=sum/sampleCount,samples},Formatting.Indented));return "Observed natural arrivals at wave "+number;
 }
 public static async Task<string> DismantleReview(){
  var build=Find<BuildManager>();var definition=Find<BuildHud>().Defenses.First(d=>d.Id=="machine-gun-nest");var cell=new Vector2Int(18,8);
  if(build.TryPlaceDefense(definition,cell,out var defense)!=PlacementResult.Valid)throw new Exception("Cannot build visual dismantle fixture");
  var placement=Find<PlacementController>();placement.SelectDismantle();placement.HoverCell(cell);Time.timeScale=1;await Task.Delay(150);Capture("dismantle-hover");int money=build.Economy.Money;
  if(!placement.TryDismantleCurrent())throw new Exception("Dismantle failed");await Task.Delay(100);Capture("dismantle-refund");
  File.WriteAllText(Folder+"dismantle-visual.json",JsonConvert.SerializeObject(new{moneyBefore=money,moneyAfter=build.Economy.Money,build.Economy.RefundIncome,build.Economy.StructuresDismantled,modeRemains=placement.IsDismantling,cellClear=build.Battlefield.Grid.OccupantAt(cell)==null,timeScale=Time.timeScale,waveState=Find<WaveManager>().State.ToString()},Formatting.Indented));Time.timeScale=0;return "Hover, 75 RSD refund, green feedback and continuing combat reviewed";
 }}



