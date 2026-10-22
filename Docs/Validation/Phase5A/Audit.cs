using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;

public static class Phase5AAudit
{
    public static string Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Audit saved consumers in Edit Mode");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var defs=AssetDatabase.FindAssets("t:DefenseDefinition",new[]{"Assets/Game/Data/Defenses"}).Select(g=>AssetDatabase.LoadAssetAtPath<DefenseDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        var rows=defs.Select(d=>new{d.Id,valid=d.IsValid,levelII=d.LevelData(2),levelIII=d.LevelData(3),prefab=AssetDatabase.GetAssetPath(d.Prefab),guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(d.Prefab)),upgradeObserver=d.Prefab.GetComponent<DefenseUpgradePresentation>()!=null}).ToArray();
        int missing=scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        var import=(TextureImporter)AssetImporter.GetAtPath("Assets/Game/Art/Defenses/bd_defense_upgrade_overlays_sheet_v001.png");
        var sprites=AssetDatabase.LoadAllAssetsAtPath(import.assetPath).OfType<Sprite>().Select(s=>new{s.name,s.rect,s.pixelsPerUnit}).ToArray();
        // Serialize data explicitly: Unity object graphs contain cycles and are not audit records.
        var data=new{scene=scene.path,scene.isDirty,missing,selection=UnityEngine.Object.FindObjectsByType<StructureSelection>(FindObjectsSortMode.None).Length,
            speed=UnityEngine.Object.FindObjectsByType<GameSpeedController>(FindObjectsSortMode.None).Length,hud=UnityEngine.Object.FindObjectsByType<StructureHud>(FindObjectsSortMode.None).Length,
            defenses=rows.Select(r=>new{r.Id,r.valid,r.prefab,r.guid,r.upgradeObserver,ii=r.levelII.cost,iii=r.levelIII.cost,iiOverlay=r.levelII.overlay?.name,iiiOverlay=r.levelIII.overlay?.name}),
            texture=new{import.spritePixelsPerUnit,filter=import.filterMode.ToString(),compression=import.textureCompression.ToString(),import.mipmapEnabled,wrap=import.wrapMode.ToString(),import.npotScale,import.maxTextureSize},
            sprites=sprites.Select(s=>new{s.name,x=s.rect.x,y=s.rect.y,w=s.rect.width,h=s.rect.height,s.pixelsPerUnit})};
        if(missing!=0 || scene.isDirty || defs.Length!=8 || rows.Any(r=>!r.valid||!r.upgradeObserver||r.levelII.overlay==null||r.levelIII.overlay==null)||sprites.Length!=16)throw new Exception("Saved integration audit failed");
        string json=JsonConvert.SerializeObject(data,Formatting.Indented);File.WriteAllText("Docs/Validation/Phase5A/integration-audit.json",json);return "Clean primary scene, eight valid upgraded defense prefabs, sixteen bound sprites, zero missing scripts";
    }
    public static string RawImportsAndCaptureCleanup()
    {
        if(EditorApplication.isPlaying)throw new Exception("Requires Edit Mode");
        foreach(string path in Directory.GetFiles("Assets/Game/Art/SourceSheets/p5a_upgrades_001","*.png"))
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);
            i.textureType=TextureImporterType.Default;i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.wrapMode=TextureWrapMode.Clamp;i.maxTextureSize=2048;i.SaveAndReimport();
        }
        foreach(string name in new[]{"baseline","live-current"})
        {
            string path="Assets/Docs/Validation/Phase5A/"+name+".png";
            if(!File.Exists(path))continue;
            File.Copy(path,"Docs/Validation/Phase5A/"+name+".png",true);AssetDatabase.DeleteAsset(path);
        }
        foreach(string path in new[]{"Assets/Docs/Validation/Phase5A","Assets/Docs/Validation","Assets/Docs"})
            if(Directory.Exists(path)&&Directory.GetFileSystemEntries(path).Length==0)AssetDatabase.DeleteAsset(path);
        return "Raw references remain non-runtime; tool captures preserved outside Assets";
    }
}
