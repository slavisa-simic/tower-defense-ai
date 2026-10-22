using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ShorelineDefense.Waves;
using ShorelineDefense.UI;
using ShorelineDefense.Core;
public static class Phase4DAuthor {
 public static string Apply() {
  var s=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
  s.enemyQuantityMultiplier=10;s.globalEnemyHealthMultiplier=3;s.bossHealthMultiplier=3;
  s.maximumEnemiesPerWave=2400;s.maximumActiveEnemies=400;s.pressureDirectorEnabled=true;
  s.earlyArrivalInterval=1;s.minimumArrivalInterval=.3f;s.arrivalGrowth=.08f;
  s.initialConcurrentBoats=6;s.wavesPerBoatConcurrency=2;s.maximumConcurrentBoats=28;
  s.earlyPressureTarget=50;s.pressureGrowthPerWave=8;s.maximumPressureTarget=320;
  s.boatSpeed=9;s.boatCapacity=12;s.unloadInterval=.07f;s.departureDelay=.15f;
  EditorUtility.SetDirty(s);AssetDatabase.SaveAssets();
  var w=UnityEngine.Object.FindFirstObjectByType<WaveManager>();
  var hud=UnityEngine.Object.FindFirstObjectByType<BuildHud>();
  var debug=hud.GetComponent<AssaultPressureDebugHud>()??hud.gameObject.AddComponent<AssaultPressureDebugHud>();
  debug.Configure(w,UnityEngine.Object.FindFirstObjectByType<RunStatistics>());
  EditorUtility.SetDirty(debug);EditorSceneManager.MarkSceneDirty(w.gameObject.scene);EditorSceneManager.SaveScene(w.gameObject.scene);
  var rows=new[]{1,3,5,10,20,25,50}.Select(n=>{var d=new EndlessWaveGenerator(s,1977).Generate(n);var row=new{wave=n,count=d.ScheduledCount,health=s.RuntimeHealthMultiplier(d.HealthMultiplier,false),bossHealth=s.RuntimeHealthMultiplier(d.HealthMultiplier,true),target=s.PressureTarget(n),boats=s.BoatConcurrency(n),interval=s.ArrivalInterval(n)};UnityEngine.Object.DestroyImmediate(d);return row;}).ToArray();
  File.WriteAllText("Docs/Validation/Phase4D/tuning.json",Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented));return "Extreme assault tuning saved with existing asset GUIDs";
 }
}

