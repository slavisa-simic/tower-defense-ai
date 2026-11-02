using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Presentation;
using Object=UnityEngine.Object;

// Explicit bounded authoring, never a scene-load hook or runtime manager.
public static class ShadowAuthor
{
    const string Folder="Docs/Validation/ShadowAudit";
    static Material low, medium, tall;
    static readonly List<object> receipt=new List<object>();
    static Material Style(string name,float pixels,float opacity)
    {
        string path="Assets/Game/Materials/PixelShadow"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("BorderDefenders/PixelShadow"));AssetDatabase.CreateAsset(material,path);}
        material.SetColor("_Color",new Color(31/255f,38/255f,41/255f,opacity));
        material.SetVector("_OffsetPixels",new Vector4(pixels,-pixels*.75f,0,0));EditorUtility.SetDirty(material);return material;
    }
    static SpriteRenderer Shadow(SpriteRenderer source,SpriteRenderer existing,Material style)
    {
        var shadow=existing;
        if(shadow==null){var go=new GameObject("Contact Shadow");go.isStatic=source.gameObject.isStatic;go.transform.SetParent(source.transform,false);shadow=go.AddComponent<SpriteRenderer>();}
        shadow.sprite=source.sprite;shadow.sharedMaterial=style;shadow.color=Color.white;
        shadow.sortingLayerName="GroundObjects";shadow.sortingOrder=-20;
        shadow.flipX=source.flipX;shadow.flipY=source.flipY;
        shadow.drawMode=source.drawMode;shadow.size=source.size;
        shadow.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
        var scale=source.transform.lossyScale;var parent=shadow.transform.parent!=null?shadow.transform.parent.lossyScale:Vector3.one;
        shadow.transform.localScale=new Vector3(scale.x/parent.x,scale.y/parent.y,scale.z/parent.z);
        shadow.enabled=source.enabled;
        return shadow;
    }
    static SpriteRenderer ChildShadow(SpriteRenderer source) => source.transform.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r=>r!=source&&r.name.Contains("Shadow"));
    static void Prefab(string path, bool enemy)
    {
        var root=PrefabUtility.LoadPrefabContents(path);
        try {
            var renderers=root.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer body;
            if(enemy)body=root.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
            else {
                var presentation=root.GetComponent<DefenseSpritePresentation>();
                body=presentation!=null?new SerializedObject(presentation).FindProperty("baseRenderer").objectReferenceValue as SpriteRenderer:null;
                if(body==null)body=renderers.First(r=>r.sprite!=null&&!r.name.Contains("Shadow")&&r.enabled);
            }
            var shadows=renderers.Where(r=>r.name.Contains("Shadow")).ToArray();
            var keep=shadows.FirstOrDefault(r=>r.transform.parent==body.transform.parent)??shadows.FirstOrDefault();
            foreach(var duplicate in shadows.Where(r=>r!=keep))Object.DestroyImmediate(duplicate.gameObject);
            Material style=enemy?(root.name.Contains("Dredger")||root.name.Contains("Broodmother")?tall:low):root.name.Contains("PlayerBase")?tall:root.name=="LandMine"||root.name=="BarbedWire"?low:medium;
            var shadow=Shadow(body,keep,style);
            if(enemy){var so=new SerializedObject(root.GetComponent<EnemySpritePresentation>());so.FindProperty("contactShadow").objectReferenceValue=shadow;so.ApplyModifiedPropertiesWithoutUndo();}
            var wire=root.GetComponent<ConnectedWirePresentation>();if(wire!=null)wire.ConfigureShadow(shadow);
            PrefabUtility.SaveAsPrefabAsset(root,path);
            receipt.Add(new {prefab=path,removedDuplicates=Math.Max(0,shadows.Length-1),body=body.name,material=AssetDatabase.GetAssetPath(style)});
        } finally {PrefabUtility.UnloadPrefabContents(root);}
    }
    static bool Tall(SpriteRenderer r) => new[]{"crane","roof","shed","booth","Command Post","military_tent","watch_post","antenna","floodlight"}.Any(s=>r.name.IndexOf(s,StringComparison.OrdinalIgnoreCase)>=0);
    static int Orphans(UnityEngine.SceneManagement.Scene scene)
    {
        int count=0;
        foreach(var shadow in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).Where(r=>r.name.Contains("Shadow"))) {
            var owner=shadow.transform.parent!=null?shadow.transform.parent.GetComponent<SpriteRenderer>():null;
            if(owner!=null&&(!owner.enabled||owner.sprite==null)){
                shadow.enabled=false;shadow.sharedMaterial=low;shadow.color=Color.white;count++;
            }
        }
        return count;
    }
    public static string DisableOrphans()
    {
        low=AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/PixelShadowLow.mat");
        var setup=EditorSceneManager.GetSceneManagerSetup();int count=0;
        try{foreach(string name in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"}){
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");count+=Orphans(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        return "Disabled "+count+" orphaned shadows of hidden/null retired props";
    }
    static void Scene(string name)
    {
        var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
        int count=0, floors=0;
        foreach(string rootName in new[]{"Danube Quarantine Dressing","Riverbank Vegetation","Port Environment"}) {
            var go=GameObject.Find(rootName);if(go==null)continue;
            // Port Phase 6A shadows were anonymous siblings, not children of their owners.
            if(rootName=="Port Environment")foreach(var old in go.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.name.Contains("Shadow")).ToArray())Object.DestroyImmediate(old.gameObject);
            foreach(var r in go.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.sprite!=null&&!r.name.Contains("Shadow")).ToArray()) {
                if(r.sortingLayerName!="GroundObjects")continue;
                bool floor=new[]{"Blocked cargo footprint","Quarantine service compound","Dock hazard markings","Exit road marking"}.Contains(r.name);
                if(floor){r.sortingLayerName="Environment";r.sortingOrder+=2;floors++;continue;}
                // Grass is ground cover, not a raised structure. Retain its authored silhouette only.
                if(r.name.StartsWith("Grass "))continue;
                var style=Tall(r)?tall:Mathf.Max(r.bounds.size.x,r.bounds.size.y)<=1.5f?low:medium;
                Shadow(r,ChildShadow(r),style);count++;
            }
        }
        // Preserve prefab instance hierarchy and any scene-authored command-post sprite binding.
        var player=GameObject.Find("Player Base");
        if(player!=null){var renderers=player.GetComponentsInChildren<SpriteRenderer>(true);var body=renderers.FirstOrDefault(r=>r.name=="Command Post");if(body!=null)Shadow(body,renderers.FirstOrDefault(r=>r.name.Contains("Shadow")),tall);}
        int orphans=Orphans(scene);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        receipt.Add(new {scene=name,staticOwners=count,terrainRenderersMovedToEnvironment=floors,orphanedShadowsDisabled=orphans});
    }
    public static string Apply()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required");
        var setup=EditorSceneManager.GetSceneManagerSetup();receipt.Clear();AssetDatabase.Refresh();
        low=Style("Low",4,.36f);medium=Style("Medium",8,.40f);tall=Style("Tall",12,.40f);
        try {
            foreach(var path in Directory.GetFiles("Assets/Game/Prefabs/Enemies","*_TopDown.prefab"))Prefab(path.Replace('\\','/'),true);
            foreach(var path in Directory.GetFiles("Assets/Game/Prefabs/Defenses","*.prefab"))Prefab(path.Replace('\\','/'),false);
            Prefab("Assets/Game/Prefabs/Gameplay/PlayerBase_TopDown.prefab",false);
            foreach(string scene in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})Scene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Folder+"/authoring-receipt.json",JsonConvert.SerializeObject(receipt,Formatting.Indented));
        } finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
        return "Applied stronger 4/8/12px lower-right alpha-only shadows at 36/40/40%; saved both maps";
    }
}
