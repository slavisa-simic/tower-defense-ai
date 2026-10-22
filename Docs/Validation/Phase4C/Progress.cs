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
public static class Phase4CProgress {
 const string Folder="Docs/Validation/Phase4C/";
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 static void Capture(string name){var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t=>t!=null);File.WriteAllBytes(Folder+name+".png",(byte[])type.GetMethod("EncodeScreenToPng",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{Screen.width,Screen.height}));}
 static bool Buy(string id,int x,int y){var d=Find<BuildHud>().Defenses.First(v=>v.Id==id);return Find<BuildManager>().TryPlaceDefense(d,new Vector2Int(x,y),out _)==PlacementResult.Valid;}
 static void Reinforce(int n){
  if(n==1){Buy("sniper-tower",23,9);Buy("machine-gun-nest",23,4);Buy("barbed-wire",27,4);}
  if(n==2){Buy("machine-gun-nest",23,14);Buy("barbed-wire",27,14);}
  if(n==3){Buy("machine-gun-nest",23,7);Buy("barbed-wire",27,9);}
  if(n==4)Buy("mortar",20,11);
  if(n==5){Buy("flamethrower",25,9);Buy("barbed-wire",26,9);Buy("barbed-wire",26,8);Buy("barbed-wire",26,10);}
  if(n==6)Buy("autocannon",20,5);
  if(n==7)Buy("rocket",20,14);
  if(n==8){Buy("sniper-tower",23,13);Buy("machine-gun-nest",23,11);}
  if(n==9){Buy("flamethrower",25,4);Buy("barbed-wire",26,4);Buy("flamethrower",25,14);Buy("barbed-wire",26,14);}
  if(n==10){Buy("mortar",19,8);Buy("autocannon",22,16);}
  if(n==11){Buy("rocket",18,3);Buy("barbed-wire",26,3);Buy("barbed-wire",26,5);}
  if(n==12){Buy("mortar",18,13);Buy("barbed-wire",26,13);Buy("barbed-wire",26,15);Buy("land-mine",27,8);}
 }
 public static async Task<string> Fresh(){Application.runInBackground=true;var view=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));view.Show();view.Focus();Time.timeScale=0;var op=SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");while(!op.isDone)await Task.Delay(20);while(!Find<GameStateController>().IsPlaying)await Task.Delay(20);Find<PlacementInput>().enabled=false;var w=Find<WaveManager>();typeof(WaveManager).GetField("generator",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(w,new EndlessWaveGenerator(w.EndlessSettings,1977));typeof(WaveManager).GetMethod("ReleaseGenerated",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(w,null);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();Capture("preparation");return "Fresh 500-RSD progression with saved loss threshold";}
 public static async Task<string> Through(int last){var w=Find<WaveManager>();var game=Find<GameStateController>();var stats=Find<RunStatistics>();var registry=Find<EnemyRegistry>();var boats=Find<BoatLandingSystem>();int first=w.CurrentWaveNumber;var rows=new List<object>();
  for(int n=first;n<=last && game.IsPlaying;n++){
   Reinforce(n);int planned=w.CurrentWave.ScheduledCount,peak=0,peakBoats=0,balance=Find<BuildManager>().Economy.Money;long h=stats.Homicides,k=stats.TotalKills;
   var button=Find<BuildHud>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave");using(var click=NavigationSubmitEvent.GetPooled()){click.target=button;button.SendEvent(click);}if(w.State!=WaveState.Spawning)throw new Exception("UI Start failed");
   Time.timeScale=4;double start=EditorApplication.timeSinceStartup;bool captured=false;
   while(w.State!=WaveState.Preparation && game.IsPlaying && EditorApplication.timeSinceStartup-start<25){await Task.Delay(20);peak=Math.Max(peak,registry.Count);peakBoats=Math.Max(peakBoats,boats.ActiveBoats);if(!captured && EditorApplication.timeSinceStartup-start>2){Capture("progress-wave-"+n);captured=true;}}
   Time.timeScale=0;rows.Add(new{wave=n,planned,peak,peakBoats,startBalance=balance,endBalance=Find<BuildManager>().Economy.Money,kills=stats.TotalKills-k,newHomicides=stats.Homicides-h,homicides=stats.Homicides,state=w.State.ToString(),realSeconds=EditorApplication.timeSinceStartup-start});
   if(w.State!=WaveState.Preparation || !game.IsPlaying)break;
  }
  Capture("progress-end-"+last);File.WriteAllText(Folder+"progress-through-"+last+".json",JsonConvert.SerializeObject(new{rows,externalMoney=0,totalEarned=Find<BuildManager>().Economy.TotalEarned,totalSpent=Find<BuildManager>().Economy.TotalSpent,stats.HighestWaveReached,stats.Homicides,gameOver=!game.IsPlaying},Formatting.Indented));return game.CurrentState==GameState.Initializing?"Fixture has not started; no play result":game.IsPlaying?"Natural earned-money progression reached wave "+w.CurrentWaveNumber:"Defense collapsed at wave "+stats.HighestWaveReached;
 }
}


