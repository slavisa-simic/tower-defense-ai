using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using ShorelineDefense.Presentation;

public static class Phase3CAudit
{
    static string Hierarchy(Transform t)=>t.parent==null?t.name:Hierarchy(t.parent)+"/"+t.name;
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit Mode audit required");
        const string reference="Assets/Game/Art/MasterReferences/ref_gameplay_integrated_01.png";
        if(!File.Exists(reference))File.Copy("Docs/Validation/Phase3C/wave-5-firing.png",reference);
        AssetDatabase.ImportAsset(reference);
        var referenceImport=(TextureImporter)AssetImporter.GetAtPath(reference);
        referenceImport.textureType=TextureImporterType.Default;referenceImport.filterMode=FilterMode.Point;
        referenceImport.textureCompression=TextureImporterCompression.Uncompressed;referenceImport.mipmapEnabled=false;
        referenceImport.npotScale=TextureImporterNPOTScale.None;referenceImport.maxTextureSize=2048;referenceImport.SaveAndReimport();
        if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Game/Art/MasterReferences/p3c_review_temporary.png")!=null)
            AssetDatabase.DeleteAsset("Assets/Game/Art/MasterReferences/p3c_review_temporary.png");
        var imports=new List<object>();int count=0;
        foreach(string path in Directory.GetFiles("Assets/Game/Art","*.png",SearchOption.AllDirectories).Select(p=>p.Replace('\\','/')))
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(path);if(t==null||t.textureType!=TextureImporterType.Sprite||path.Contains("Placeholders"))continue;
            if(t.filterMode!=FilterMode.Point||t.mipmapEnabled||t.textureCompression!=TextureImporterCompression.Uncompressed||t.crunchedCompression||t.npotScale!=TextureImporterNPOTScale.None)
                throw new Exception("Runtime importer violation "+path);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if(sprites.Length==0)throw new Exception("Missing runtime sprites "+path);
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            float expected=path.StartsWith("Assets/Game/Art/UI/")?100:51.2f;
            if(Math.Abs(t.spritePixelsPerUnit-expected)>.001f)throw new Exception("PPU drift "+path);
            count+=sprites.Length;imports.Add(new{path,tex.width,tex.height,t.spritePixelsPerUnit,sprites=sprites.Length,filter=t.filterMode.ToString(),compression=t.textureCompression.ToString()});
        }
        File.WriteAllText("Docs/Validation/Phase3C/import-audit.json",JsonConvert.SerializeObject(imports,Formatting.Indented));
        var scene=SceneManager.GetActiveScene();var renderers=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
        var visible=renderers.Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&AssetDatabase.GetAssetPath(r.sprite).Contains("Placeholders")).ToArray();
        if(visible.Length>0)throw new Exception("Player-visible placeholders remain");
        var prefabs=new[]{"Enemies/Crawler_TopDown","Gameplay/PlayerBase_TopDown","Defenses/MachineGunNest","Defenses/SniperTower","Defenses/BarbedWire","Defenses/LandMine"}.Select(p=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/"+p+".prefab")).ToArray();
        File.WriteAllText("Docs/Validation/Phase3C/placeholder-audit.json",JsonConvert.SerializeObject(new{
            scene=scene.path,dirty=scene.isDirty,visible=visible.Length,
            retainedScene=renderers.Where(r=>AssetDatabase.GetAssetPath(r.sprite).Contains("Placeholders")).Select(r=>new{path=Hierarchy(r.transform),r.enabled,active=r.gameObject.activeInHierarchy}),
            prefabFallbacks=prefabs.SelectMany(p=>p.GetComponentsInChildren<SpriteRenderer>(true)).Where(r=>AssetDatabase.GetAssetPath(r.sprite).Contains("Placeholders")).Select(r=>new{path=Hierarchy(r.transform),r.enabled}),
            camera=new{Camera.main.orthographicSize,rect=Camera.main.rect.ToString(),rotation=Camera.main.transform.eulerAngles.ToString()},
            sortingLayers=SortingLayer.layers.Select(s=>s.name),rendererCount=renderers.Length,productionSprites=count
        },Formatting.Indented));
        AssetDatabase.SaveAssets();return "Audited "+imports.Count+" production textures / "+count+" sprites; zero visible placeholders";
    }
}
