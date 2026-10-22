using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using ShorelineDefense.Defenses;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;

public static class Phase4BAudit
{
    public static string Run()
    {
        var hud=UnityEngine.Object.FindFirstObjectByType<BuildHud>();var defs=hud.Defenses;
        var paths=Directory.GetFiles("Assets/Game/Art/Defenses","defense_*.png",SearchOption.AllDirectories)
            .Concat(Directory.GetFiles("Assets/Game/Art/Effects/Heavy","*.png")).Concat(Directory.GetFiles("Assets/Game/Art/UI","ui_icon_*.png")).ToArray();
        var imports=paths.Select(path=>{var i=(TextureImporter)AssetImporter.GetAtPath(path);var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);return new{path,tex.width,tex.height,
            filter=i.filterMode.ToString(),compression=i.textureCompression.ToString(),i.mipmapEnabled,wrap=i.wrapMode.ToString(),i.spritePixelsPerUnit,frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count()};}).ToArray();
        var scene=SceneManager.GetActiveScene();int missing=scene.GetRootGameObjects().Sum(root=>root.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        var catalog=UnityEngine.Object.FindFirstObjectByType<VfxService>().Catalog;
        var result=new{scene=scene.path,scene.isDirty,missing,definitions=defs.Select(d=>new{d.Id,d.IsValid,d.Cost,d.MaxHealth,d.Damage,d.RangeInCells,d.FireRate,footprint=d.Footprint.ToString(),target=d.Targeting.ToString(),prefab=AssetDatabase.GetAssetPath(d.Prefab)}),
            imports,vfx=catalog.sequences.Where(s=>(int)s.id>=(int)VfxId.MortarMuzzle).Select(s=>new{id=s.id.ToString(),frames=s.frames.Length,s.fps,s.lifetime})};
        if(missing!=0||defs.Count!=8||defs.Any(d=>!d.IsValid)||imports.Any(i=>i.filter!="Point"||i.compression!="Uncompressed"||i.mipmapEnabled))throw new Exception("Asset audit failed");
        File.WriteAllText("Docs/Validation/Phase4B/asset-audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return JsonConvert.SerializeObject(new{missing,count=defs.Count,imports=imports.Length,scene.isDirty});
    }
    public static string Console()
    {
        var type=typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");int errors=0,warnings=0,logs=0;
        object[] args={errors,warnings,logs};type.GetMethod("GetCountsByType",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).Invoke(null,args);
        var result=new{errors=args[0],warnings=args[1],logs=args[2]};
        File.WriteAllText("Docs/Validation/Phase4B/console.json",JsonConvert.SerializeObject(result,Formatting.Indented));return JsonConvert.SerializeObject(result);
    }
}
