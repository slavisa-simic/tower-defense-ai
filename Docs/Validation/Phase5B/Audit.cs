using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using ShorelineDefense.Waves;
using ShorelineDefense.Building;
using ShorelineDefense.UI;
using ShorelineDefense.Presentation;

public static class Phase5BAudit
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Saved audit requires Edit Mode");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var w=UnityEngine.Object.FindFirstObjectByType<WaveManager>();var s=w.EndlessSettings;
        int missing=scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        if(scene.path!="Assets/Game/Scenes/CoastalDefense_TopDown.unity"||scene.isDirty||missing!=0||s.eventSettings.events.Length!=13)throw new Exception("Scene/catalog audit failed");
        if(s.enemyQuantityMultiplier!=5||s.globalEnemyHealthMultiplier!=1||s.bossHealthMultiplier!=1||s.maximumActiveEnemies!=400)throw new Exception("Baseline tuning changed");
        foreach(var e in s.eventSettings.events)if(e==null||e.uiIcon==null||!e.Eligible(Math.Max(24,e.minimumWave),false)||e.allowBossWave)throw new Exception("Invalid event asset");
        var defs=AssetDatabase.FindAssets("t:DefenseDefinition",new[]{"Assets/Game/Data/Defenses"}).Select(g=>AssetDatabase.GUIDToAssetPath(g)).ToArray();
        var receipt=new{scene=scene.path,scene.isDirty,missing,quantity=s.enemyQuantityMultiplier,enemyHP=s.globalEnemyHealthMultiplier,bossHP=s.bossHealthMultiplier,hardCap=s.maximumActiveEnemies,definitions=defs.Length,
            eventHuds=UnityEngine.Object.FindObjectsByType<EndlessEventHud>(FindObjectsSortMode.None).Length,eventPresentations=UnityEngine.Object.FindObjectsByType<EndlessEventPresentation>(FindObjectsSortMode.None).Length,
            events=s.eventSettings.events.Select(e=>new{e.id,e.displayName,e.minimumWave,e.maximumWave,e.cooldown,e.weight,category=e.category.ToString(),duration=e.durationType.ToString(),e.preparationEffects,icon=AssetDatabase.GetAssetPath(e.uiIcon),boat=AssetDatabase.GetAssetPath(e.modifiers.boatVisual)}).ToArray()};
        if(receipt.eventHuds!=1||receipt.eventPresentations!=1||defs.Length!=8)throw new Exception("Duplicate/missing scene observers");
        File.WriteAllText("Docs/Validation/Phase5B/integration-audit.json",JsonConvert.SerializeObject(receipt,Formatting.Indented));
        const string capture="Assets/Docs/Validation/Phase5B/baseline.png";
        if(File.Exists(capture)){File.Copy(capture,"Docs/Validation/Phase5B/baseline.png",true);AssetDatabase.DeleteAsset(capture);}
        return "Clean saved primary scene; 13 valid events; one HUD and presentation; baseline 5x quantity/1x HP preserved; zero missing scripts";
    }
    public static string History()
    {
        var settings=AssetDatabase.LoadAssetAtPath<EndlessEventSettings>("Assets/Game/Data/Events/EndlessEvents.asset");var m=new EndlessEventManager(settings,1977);
        for(int wave=1;wave<=30;wave++){m.Prepare(wave,wave%25==0,null);m.BeginWave();m.CompleteWave();}
        var rows=m.History.Select(h=>new{wave=h.Wave,id=h.Id}).ToArray();File.WriteAllText("Docs/Validation/Phase5B/seed-1977-selection-history.json",JsonConvert.SerializeObject(rows,Formatting.Indented));return JsonConvert.SerializeObject(rows);
    }
}
