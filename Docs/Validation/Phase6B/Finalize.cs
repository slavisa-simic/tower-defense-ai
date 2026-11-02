using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Presentation;
using Object=UnityEngine.Object;
public static class Phase6BFinalize
{
    static void Move(string from,string to)
    {if(AssetDatabase.LoadMainAssetAtPath(from)==null)return;Directory.CreateDirectory(Path.GetDirectoryName(to));AssetDatabase.Refresh();var error=AssetDatabase.MoveAsset(from,to);if(error!="")throw new Exception(error);}
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        foreach(string name in new[]{"Crawler","Skitter","Bulwark","Spitter","Broodling","Burrower","Dredger","Broodmother"})
        {
            string path="Assets/Game/Prefabs/Enemies/"+name+"_TopDown.prefab";var enemy=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var visual=enemy.transform.Find("Visual");var body=visual.Find("Body").GetComponent<SpriteRenderer>();
                var shadow=visual.Find("Contact Shadow");if(shadow==null){shadow=new GameObject("Contact Shadow").transform;shadow.SetParent(visual,false);shadow.gameObject.AddComponent<SpriteRenderer>();}
                shadow.localPosition=new Vector3(.04f,-.04f,.02f);shadow.localScale=Vector3.one;var renderer=shadow.GetComponent<SpriteRenderer>();renderer.sprite=body.sprite;renderer.sharedMaterial=body.sharedMaterial;renderer.color=new Color(.09f,.13f,.12f,.24f);renderer.sortingLayerName="Enemies";renderer.sortingOrder=-1;
                var so=new SerializedObject(enemy.GetComponent<EnemySpritePresentation>());so.FindProperty("contactShadow").objectReferenceValue=renderer;so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(enemy,path);
            }finally{PrefabUtility.UnloadPrefabContents(enemy);}
        }
        foreach(var pair in new[]{("PirateRaid","SwarmSurge"),("MilitantAssault","ArmoredMigration"),("HeavyAssault","DeepWaterDisturbance"),("RunnerSwarm","BroodTide"),("MassLanding","MassEmergence"),("ConcentratedLanding","ConcentratedEmergence")})Move("Assets/Game/Data/Events/"+pair.Item1+".asset","Assets/Game/Data/Events/"+pair.Item2+".asset");
        var catalog=AssetDatabase.FindAssets("t:VfxCatalog").Select(g=>AssetDatabase.LoadAssetAtPath<VfxCatalog>(AssetDatabase.GUIDToAssetPath(g))).Single();
        var ids=new[]{VfxId.CorrosiveGlob,VfxId.SlimeImpact,VfxId.PodRupture,VfxId.BroodRelease,VfxId.CreatureFluids,VfxId.Bubbles};
        var action=new[]{"idle","move","attack","hit","death","emergence"};
        var sequences=catalog.sequences.Where(s=>!ids.Contains(s.id)).ToList();
        for(int i=0;i<6;i++)
        {
            string from="Assets/Game/Art/Effects/Aquatic/bd_enemy_biovfx_"+action[i]+"_sheet_v001.png";
            string to="Assets/Game/Art/Effects/Aquatic/bd_fx_biological_"+ids[i].ToString().ToLowerInvariant()+"_sheet_v001.png";Move(from,to);
            var frames=AssetDatabase.LoadAllAssetsAtPath(to).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            sequences.Add(new VfxSequence{id=ids[i],frames=frames,fps=10,channel=VfxChannel.Burst,sortingLayer="Effects",sortingOrder=6,lifetime=.4f,opacity=.8f,fadePortion=.35f});
        }
        catalog.sequences=sequences.ToArray();EditorUtility.SetDirty(catalog);
        string mother="Assets/Game/Prefabs/Enemies/Broodmother_TopDown.prefab";var root=PrefabUtility.LoadPrefabContents(mother);
        try{var so=new SerializedObject(root.GetComponent<EnemySpritePresentation>());var p=so.FindProperty("special");var frames=AssetDatabase.LoadAllAssetsAtPath("Assets/Game/Art/Enemies/Aquatic/bd_enemy_broodmother_special_sheet_v001.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();p.arraySize=frames.Length;for(int i=0;i<frames.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=frames[i];so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,mother);}finally{PrefabUtility.UnloadPrefabContents(root);}
        foreach(string directory in new[]{"BasicInfected","Brute","Runner","Special"})Move("Assets/Game/Art/Enemies/"+directory,"Assets/Game/Art/Legacy/PreAquatic/"+directory);
        foreach(string name in new[]{"RubberLandingBoat","MilitantBossTransport","PirateBossTransport"})Move("Assets/Game/Prefabs/Presentation/"+name+".prefab","Assets/Game/Legacy/Phase4C/"+name+".prefab");
        foreach(string name in new[]{"bd_prop_rubber_boat_v001","bd_prop_destroyed_rubber_boat_v001","MilitantBossTransport_v001","PirateBossTransport_v001"})Move("Assets/Game/Art/Props/"+name+".png","Assets/Game/Art/Legacy/Props/"+name+".png");
        AssetDatabase.SaveAssets();var audit=new List<object>();
        foreach(string name in new[]{"MainMenu","CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
            int disabled=0;
            foreach(var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if(renderer.sprite!=null&&AssetDatabase.GetAssetPath(renderer.sprite).Contains("/Art/Legacy/")){renderer.enabled=false;renderer.sprite=null;EditorUtility.SetDirty(renderer);disabled++;}
            var system=Object.FindFirstObjectByType<AssaultCarrierSystem>();
            if(system!=null)
            {
                var so=new SerializedObject(system);var p=so.FindProperty("carriers");var names=new[]{"direct_emergence","drift_pod","brood_carrier","leviathan_carrier"};p.arraySize=4;
                for(int i=0;i<4;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<CarrierDefinition>("Assets/Game/Data/Carriers/"+names[i]+".asset");
                so.FindProperty("boatPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<CarrierDefinition>("Assets/Game/Data/Carriers/drift_pod.asset").visualPrefab;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(system);
            }
            int missing=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            if(missing!=0)throw new Exception("Missing scripts "+name);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var legacy=AssetDatabase.GetDependencies(scene.path,true).Where(p=>p.Contains("/Legacy/")&&p.EndsWith(".prefab")).ToArray();
            audit.Add(new{scene=name,missing,disabled,legacyPrefabDependencies=legacy});
        }
        AssetDatabase.SaveAssets();string json=JsonConvert.SerializeObject(audit,Formatting.Indented);File.WriteAllText("Docs/Validation/Phase6B/asset-audit.json",json);return json;
    }
}
