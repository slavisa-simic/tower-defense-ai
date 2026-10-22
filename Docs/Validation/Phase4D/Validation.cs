using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using ShorelineDefense.Core;
using ShorelineDefense.Building;
using ShorelineDefense.UI;
using ShorelineDefense.Presentation;
public static class Phase4DValidation {
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 public static async Task<string> DismantleUnderLoad(){
  var build=Find<BuildManager>();var registry=Find<EnemyRegistry>();if(registry.Count<100)throw new Exception("Need 100 live enemies");
  Find<WaveManager>().TryStartWave();int active=registry.Count;var rows=new List<object>();var hud=Find<BuildHud>();
  var wire=hud.Defenses.First(d=>d.Id=="barbed-wire");build.TryPlaceDefense(wire,new Vector2Int(10,3),out var neighbor);
  foreach(string id in new[]{"barbed-wire","machine-gun-nest","rocket"}){
   var d=hud.Defenses.First(v=>v.Id==id);var defense=UnityEngine.Object.FindObjectsByType<ShorelineDefense.Defenses.DefenseBase>(FindObjectsSortMode.None).First(v=>v.Definition==d && v!=neighbor && (id!="barbed-wire" || v.Anchor==new Vector2Int(11,3)));
   int before=build.Economy.Money,revision=build.Battlefield.Grid.Revision;var point=defense.Anchor;
   if(!build.TryDismantle(defense,out int refund)||refund!=d.Cost/2||build.TryDismantle(defense,out _)||build.Economy.Money-before!=refund||build.Battlefield.Grid.OccupantAt(point)!=null)throw new Exception("Dismantle invariants failed for "+id);
   if(id=="barbed-wire" && neighbor.GetComponent<ConnectedWirePresentation>().Mask!=0)throw new Exception("Wire adjacency not cleared");rows.Add(new{id,refund,revisionChanged=build.Battlefield.Grid.Revision>revision,adjacency=neighbor.GetComponent<ConnectedWirePresentation>().Mask});
  }
  Time.timeScale=1;await Task.Delay(100);Time.timeScale=0;
  File.WriteAllText("Docs/Validation/Phase4D/dismantle-under-load.json",JsonConvert.SerializeObject(new{activeBefore=active,activeAfter=registry.Count,state=Find<WaveManager>().State.ToString(),rows},Formatting.Indented));return "Wire, MG, Rocket dismantled once with exact refunds under 100+ live enemies";
 }
 public static string Audit(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();int missing=scene.GetRootGameObjects().Sum(root=>root.GetComponentsInChildren<Transform>(true).Sum(t=>UnityEditor.GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
  File.WriteAllText("Docs/Validation/Phase4D/final-audit.json",JsonConvert.SerializeObject(new{scene=scene.path,scene.isDirty,missing,editorPlaying=EditorApplication.isPlaying},Formatting.Indented));return "Saved scene missing scripts: "+missing;
 }
}


