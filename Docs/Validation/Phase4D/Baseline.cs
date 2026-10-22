using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using ShorelineDefense.Core;
public static class Phase4DBaseline {
 public static async Task<string> Observe() {
  var w=UnityEngine.Object.FindFirstObjectByType<WaveManager>();
  var b=UnityEngine.Object.FindFirstObjectByType<BoatLandingSystem>();
  var r=UnityEngine.Object.FindFirstObjectByType<EnemyRegistry>();
  var border=UnityEngine.Object.FindFirstObjectByType<BorderBreachService>();border.ConfigureThreshold(0);
  var samples=new System.Collections.Generic.List<object>();int peak=0;double sum=0;
  EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Show();Application.runInBackground=true;
  var counts=new[]{1,5,10,20,25,50}.Select(n=> {var d=new EndlessWaveGenerator(w.EndlessSettings,1977).Generate(n);int c=d.ScheduledCount;UnityEngine.Object.Destroy(d);return new{wave=n,count=c};}).ToArray();
  w.TryStartWave();
  for(int i=0;i<60;i++){await Task.Delay(500);peak=Math.Max(peak,r.Count);sum+=r.Count;samples.Add(new{seconds=(i+1)*.5,enemies=r.Count,boats=b.ActiveBoats,pending=w.PendingCount,state=w.State.ToString()});}
  File.WriteAllText("Docs/Validation/Phase4D/baseline.json",JsonConvert.SerializeObject(new{counts,peak,average=sum/60, b.MoveSpeed,b.Capacity,b.ConcurrencyLimit,b.ArrivalInterval,activeLimit=w.EndlessSettings.maximumActiveEnemies,health=w.CurrentWave.HealthMultiplier,frameTimeMs=Time.unscaledDeltaTime*1000,samples},Formatting.Indented));
  return "Baseline observed over 30 real seconds";
 }
}

