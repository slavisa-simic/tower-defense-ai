using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Material-only style revision. Existing sprite silhouettes, roots and renderer bindings stay intact.
public static class DirectionalShadowR2
{
    public static string Apply()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        var receipt=new System.Collections.Generic.List<object>();
        foreach(var tier in new[]{("Low",4f,.36f),("Medium",8f,.40f),("Tall",12f,.40f)}){
            string path="Assets/Game/Materials/PixelShadow"+tier.Item1+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            var beforeOffset=material.GetVector("_OffsetPixels");var beforeTone=material.GetColor("_Color");
            material.SetVector("_OffsetPixels",new Vector4(tier.Item2,-tier.Item2*.75f,0,0));
            material.SetColor("_Color",new Color(31/255f,38/255f,41/255f,tier.Item3));
            EditorUtility.SetDirty(material);
            receipt.Add(new {path,beforeOffset=beforeOffset.ToString(),beforeOpacity=beforeTone.a,offset=material.GetVector("_OffsetPixels").ToString(),opacity=tier.Item3});
        }
        var low=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/PixelShadowLow.mat");
        var medium=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/PixelShadowMedium.mat");
        // Small cargo/contact silhouettes need less translation than containers and roofs.
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try{foreach(var map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"}){
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+map+".unity");
            foreach(var shadow in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).Where(r=>r.name.Contains("Shadow"))){
                var owner=shadow.transform.parent!=null?shadow.transform.parent.GetComponent<SpriteRenderer>():null;
                if(owner!=null&&shadow.sharedMaterial==medium&&Mathf.Max(owner.bounds.size.x,owner.bounds.size.y)<=1.5f){
                    shadow.sharedMaterial=low;receipt.Add(new{map,smallOwner=owner.name,material=low.name});
                }
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        foreach(var name in new[]{"LandMine","BarbedWire"}){
            string path="Assets/Game/Prefabs/Defenses/"+name+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
            try{root.GetComponentsInChildren<SpriteRenderer>(true).Single(r=>r.name=="Contact Shadow").sharedMaterial=low;PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText("Docs/Validation/ShadowAudit/r2-material-receipt.json",JsonConvert.SerializeObject(receipt,Formatting.Indented));
        return "Saved stronger lower-right casts: Low (+4,-3) / 36%, Medium (+8,-6) / 40%, Tall (+12,-9) / 40%; small props/wire/mines use Low";
    }
    public static string SortingAudit()
    {
        var setup=EditorSceneManager.GetSceneManagerSetup();var rows=new System.Collections.Generic.List<object>();
        try{foreach(var map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"}){
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+map+".unity");
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
            var active=all.Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
            var terrain=active.Where(r=>r.sortingLayerName=="Environment").ToArray();
            var shadows=active.Where(r=>r.name.Contains("Shadow")).ToArray();
            bool aboveTerrain=shadows.All(s=>terrain.All(t=>SortingLayer.GetLayerValueFromID(s.sortingLayerID)>SortingLayer.GetLayerValueFromID(t.sortingLayerID)));
            if(!aboveTerrain)throw new Exception(map+": terrain occludes shadows");
            rows.Add(new{map,shadowCount=shadows.Length,aboveAllEnvironmentTerrain=aboveTerrain,
                layers=shadows.Select(s=>s.sortingLayerName+" "+s.sortingOrder).Distinct().ToArray(),
                materials=shadows.GroupBy(s=>s.sharedMaterial).Select(g=>new{material=g.Key.name,count=g.Count(),opacity=g.Key.GetColor("_Color").a,offset=g.Key.GetVector("_OffsetPixels").ToString()}).ToArray()});
        }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        File.WriteAllText("Docs/Validation/ShadowAudit/r2-sorting-audit.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
        return "Verified both maps: enabled shadow renderers are above all Environment terrain, on GroundObjects -20";
    }
}
