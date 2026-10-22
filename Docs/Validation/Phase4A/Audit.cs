using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using ShorelineDefense.Core;
using ShorelineDefense.UI;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
public static class Phase4AAudit
{
    public static string Run()
    {
        var paths=new[]{"Assets/Game/Art/UI/ui_icon_money_rsd.png","Assets/Game/Art/UI/ui_icon_money_bundle_rsd.png","Assets/Game/Art/UI/ui_money_panel_rsd.png"};
        var imports=paths.Select(path=>{var i=(TextureImporter)AssetImporter.GetAtPath(path);var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);return new{path,tex.width,tex.height,filter=i.filterMode.ToString(),compression=i.textureCompression.ToString(),i.mipmapEnabled,wrap=i.wrapMode.ToString(),npot=i.npotScale.ToString(),i.spritePixelsPerUnit,i.alphaIsTransparency};}).ToArray();
        var scene=SceneManager.GetActiveScene();int missing=scene.GetRootGameObjects().Sum(root=>root.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        bool sceneCorrect=scene.path=="Assets/Game/Scenes/CoastalDefense_TopDown.unity";
        var result=new{sceneCorrect,missing,imports,scene.isDirty,sourceHashRecord=File.ReadAllText("Docs/Art/Batches/p4a_money_source_hashes.json")};
        if(!sceneCorrect||missing!=0||imports.Any(i=>i.filter!="Point"||i.compression!="Uncompressed"||i.mipmapEnabled))throw new Exception("Asset audit failed");
        File.WriteAllText("Docs/Validation/Phase4A/asset-audit.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        if(File.Exists("Assets/Docs/Validation/Phase4A/before.png"))
        {
            File.Copy("Assets/Docs/Validation/Phase4A/before.png","Docs/Validation/Phase4A/before.png",true);
            AssetDatabase.DeleteAsset("Assets/Docs/Validation/Phase4A/before.png");
            foreach(var p in new[]{"Assets/Docs/Validation/Phase4A","Assets/Docs/Validation","Assets/Docs"})
                if(Directory.Exists(p)&&!Directory.EnumerateFileSystemEntries(p).Any())AssetDatabase.DeleteAsset(p);
        }
        return JsonConvert.SerializeObject(new{sceneCorrect,missing,scene.isDirty,imports});
    }
}

