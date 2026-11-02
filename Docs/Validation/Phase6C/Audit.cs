using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
using ShorelineDefense.UI;
using Object=UnityEngine.Object;
public static class Phase6CAudit
{
    public static string SavedAssets()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first.");
        var scenes=new List<object>();
        foreach(string name in new[]{"MainMenu","CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
            int missing=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            if(missing!=0)throw new Exception("Missing scripts: "+name);
            var menus=Object.FindObjectsByType<ProductMenuController>(FindObjectsSortMode.None);var sessions=Object.FindObjectsByType<RunSession>(FindObjectsSortMode.None);var intel=Object.FindObjectsByType<PreparationIntelHud>(FindObjectsSortMode.None);
            if(menus.Length!=1||sessions.Length!=(name=="MainMenu"?0:1)||intel.Length!=(name=="MainMenu"?0:1)||scene.isDirty)throw new Exception("Owner count or dirty scene: "+name);
            scenes.Add(new{name,missing,menus=menus.Length,sessions=sessions.Length,intelligence=intel.Length,scene.isDirty});
        }
        var catalog=ResearchCatalog.Load();var imports=new List<object>();var alpha=new List<object>();
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Game/Art/UI/Research"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            var raw=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(raw,File.ReadAllBytes(path));var pixels=raw.GetPixels32();
            int transparent=pixels.Count(p=>p.a==0),opaque=pixels.Count(p=>p.a==255),partial=pixels.Length-transparent-opaque;
            if(transparent==0||opaque==0)throw new Exception("Missing real alpha or opaque silhouette: "+path);
            alpha.Add(new{path,raw.width,raw.height,transparentPixels=transparent,opaquePixels=opaque,partialAlphaPixels=partial});Object.DestroyImmediate(raw);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(importer.filterMode!=FilterMode.Point||importer.mipmapEnabled||importer.textureCompression!=TextureImporterCompression.Uncompressed)throw new Exception("Pixel import mismatch");
            imports.Add(new{path,texture.width,texture.height,filter=importer.filterMode.ToString(),sprites=sprites.Select(s=>new{name=s.name,rect=s.rect.ToString(),pivot=s.pivot.ToString()}).ToArray()});
        }
        if(catalog.nodes.Length!=20||catalog.nodes.Select(n=>n.icon).Distinct().Count()!=20||catalog.researchPointIcon==null)throw new Exception("Catalog assets");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        string json=JsonConvert.SerializeObject(new{scenes,imports,nodes=catalog.nodes.Select(n=>new{n.id,n.displayName,branch=n.branch.ToString(),n.costRP,n.tier,n.maxRank,prerequisites=n.prerequisites.Select(p=>p.id),effects=n.effects.Select(e=>new{kind=e.kind.ToString(),e.amount})})},Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase6C/saved-assets.json",json);File.WriteAllText("Docs/Validation/Phase6C/alpha-audit.json",JsonConvert.SerializeObject(alpha,Formatting.Indented));return "Three clean scenes; 20 unique node icons plus RP; Point/uncompressed/no-mipmap imports; no missing scripts or duplicate owners.";
    }
}
