using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using ShorelineDefense.Core;
using ShorelineDefense.UI;
public static class Phase4CAuthor {
 const string Data="Assets/Game/Data/Enemies/", Prefabs="Assets/Game/Prefabs/Enemies/";
 public static void Set(UnityEngine.Object o,string key,object value){var s=new SerializedObject(o);var p=s.FindProperty(key);if(p==null)throw new Exception("Missing "+key);if(value is int n)p.intValue=n;else if(value is float f)p.floatValue=f;else if(value is string t)p.stringValue=t;else p.objectReferenceValue=value as UnityEngine.Object;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
 static T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>();
 static EnemyController Enemy(string id,string name,EnemyKind kind,int hp,float speed,int reward,int penalty,int cost,int unlock,int units,float armor=0,bool boss=false){
  string path=Data+id+".asset";var d=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
  if(d==null){d=boss?(EnemyDefinition)ScriptableObject.CreateInstance<BossDefinition>():ScriptableObject.CreateInstance<EnemyDefinition>();AssetDatabase.CreateAsset(d,path);}
  Set(d,"displayName",name);Set(d,"kind",(int)kind);Set(d,"maxHealth",hp);Set(d,"movementSpeed",speed);Set(d,"killReward",reward);Set(d,"breachPenalty",penalty);Set(d,"difficultyCost",cost);Set(d,"unlockWave",unlock);Set(d,"transportCapacityUnits",units);Set(d,"armor",armor);
  string prefabPath=Prefabs+id+"_TopDown.prefab";
  if(!File.Exists(prefabPath))AssetDatabase.CopyAsset(Prefabs+(boss||units==3?"Brute":"Runner")+"_TopDown.prefab",prefabPath);
  var root=PrefabUtility.LoadPrefabContents(prefabPath);try{root.name=id;root.GetComponent<EnemyController>().Configure(d);PrefabUtility.SaveAsPrefabAsset(root,prefabPath);}finally{PrefabUtility.UnloadPrefabContents(root);}
  return AssetDatabase.LoadAssetAtPath<EnemyController>(prefabPath);
 }
 static GameObject Boat(string id){
  string path="Assets/Game/Prefabs/Presentation/"+id+".prefab";
  if(!File.Exists(path))AssetDatabase.CopyAsset("Assets/Game/Prefabs/Presentation/RubberLandingBoat.prefab",path);
  return AssetDatabase.LoadAssetAtPath<GameObject>(path);
 }
 static void Escorts(BossDefinition boss,params WaveEnemyGroup[] groups){var s=new SerializedObject(boss);var p=s.FindProperty("escorts");p.arraySize=groups.Length;for(int i=0;i<groups.Length;i++){var e=p.GetArrayElementAtIndex(i);e.FindPropertyRelative("prefab").objectReferenceValue=groups[i].Prefab;e.FindPropertyRelative("count").intValue=groups[i].Count;e.FindPropertyRelative("spawnInterval").floatValue=.15f;e.FindPropertyRelative("delayBeforeGroup").floatValue=0;}s.ApplyModifiedPropertiesWithoutUndo();}
 public static string Apply(){
  if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
  if(SceneManager.GetActiveScene().path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity")throw new Exception("Wrong scene");
  var s=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
  var basic=s.enemies[0];var runner=s.enemies[1];var brute=s.enemies[2];
  var breacher=Enemy("IsisBreacher","ISIS INFECTED BREACHER",EnemyKind.IsisBreacher,180,3.2f,55,80,3,10,1);
  var heavy=Enemy("IsisHeavy","ISIS HEAVY INFECTED",EnemyKind.IsisHeavy,900,1.4f,120,160,8,18,3);
  var pirate=Enemy("PirateRunner","SOMALI PIRATE RUNNER",EnemyKind.PirateRunner,70,5.5f,45,90,3,12,1);
  var raider=Enemy("PirateRaider","SOMALI PIRATE RAIDER",EnemyKind.PirateRaider,220,3.7f,65,100,4,20,2);
  var osama=Enemy("UndeadOsama","UNDEAD OSAMA BIN LADEN",EnemyKind.UndeadOsama,8000,1.15f,2500,1000,20,25,3,.35f,true);
  var warlord=Enemy("PirateWarlord","SOMALI PIRATE WARLORD",EnemyKind.PirateWarlord,6000,2.6f,2200,900,18,25,3,.2f,true);
  var o=(BossDefinition)osama.Definition;var p=(BossDefinition)warlord.Definition;
  Set(o,"bossBoat",Boat("MilitantBossTransport"));Set(p,"bossBoat",Boat("PirateBossTransport"));Set(p,"arrivalMultiplier",.7f);Set(p,"boatSpeedMultiplier",1.3f);
  Escorts(o,new WaveEnemyGroup(breacher,12),new WaveEnemyGroup(heavy,4),new WaveEnemyGroup(basic,14),new WaveEnemyGroup(brute,4));
  Escorts(p,new WaveEnemyGroup(pirate,20),new WaveEnemyGroup(raider,12),new WaveEnemyGroup(runner,16));
  string encounterPath="Assets/Game/Data/Waves/BossEncounters.asset";var encounters=AssetDatabase.LoadAssetAtPath<BossEncounterDefinition>(encounterPath);if(encounters==null){encounters=ScriptableObject.CreateInstance<BossEncounterDefinition>();AssetDatabase.CreateAsset(encounters,encounterPath);}encounters.bosses=new[]{osama,warlord};EditorUtility.SetDirty(encounters);
  s.enemies=new[]{basic,runner,brute,breacher,heavy,pirate,raider};s.baseBudget=16;s.linearGrowth=8;s.nonlinearGrowth=.08f;s.maximumEnemiesPerWave=240;s.maximumActiveEnemies=200;s.earlyArrivalInterval=2.2f;s.arrivalGrowth=.045f;s.initialConcurrentBoats=2;s.wavesPerBoatConcurrency=4;s.maximumConcurrentBoats=8;s.bossEncounters=encounters;EditorUtility.SetDirty(s);
  var boat=Find<BoatLandingSystem>();boat.ConfigureMotion(7,5,.15f,.2f);foreach(var point in boat.LandingPoints)point.riverEntry.x=-23.5f;EditorUtility.SetDirty(boat);
  // Seven seats fit within the existing hull's footprint. Keep gameplay lanes and boat GUID.
  foreach(var path in new[]{"RubberLandingBoat","MilitantBossTransport","PirateBossTransport"}.Select(n=>"Assets/Game/Prefabs/Presentation/"+n+".prefab")){
   var root=PrefabUtility.LoadPrefabContents(path);try{
    var seats=root.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.name.StartsWith("Passenger ")).ToArray();var parent=seats[0].transform.parent;
    for(int i=0;i<7;i++){var seat=seats.FirstOrDefault(r=>r.name=="Passenger "+i);if(seat==null){var go=UnityEngine.Object.Instantiate(seats[0].gameObject,parent);go.name="Passenger "+i;seat=go.GetComponent<SpriteRenderer>();}seat.transform.localPosition=new Vector3(-.8f+(i/2)*.48f,(i%2==0?-.22f:.22f),seat.transform.localPosition.z);}
    PrefabUtility.SaveAsPrefabAsset(root,path);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  var manager=Find<BossEncounterManager>();if(manager==null)manager=Find<WaveManager>().gameObject.AddComponent<BossEncounterManager>();Set(manager,"waves",Find<WaveManager>());Set(manager,"registry",Find<EnemyRegistry>());Set(manager,"boats",boat);Set(manager,"border",Find<BorderBreachService>());
  var hud=Find<BuildHud>().GetComponent<BossHUDController>();if(hud==null)hud=Find<BuildHud>().gameObject.AddComponent<BossHUDController>();Set(hud,"encounters",manager);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
  var rows=new[]{1,3,5,10,20,25,30,50,100}.Select(n=>{var w=new EndlessWaveGenerator(s,1977).Generate(n);var result=new{wave=n,count=w.ScheduledCount,budget=new EndlessWaveGenerator(s,1977).Budget(n),boss=w.Encounter?.DisplayName,interval=s.ArrivalInterval(n),concurrency=s.BoatConcurrency(n)};UnityEngine.Object.DestroyImmediate(w);return result;});
  File.WriteAllText("Docs/Validation/Phase4C/pressure-settings.json",JsonConvert.SerializeObject(rows,Formatting.Indented));return "Phase4C data and primary scene integrated";
 }
}


