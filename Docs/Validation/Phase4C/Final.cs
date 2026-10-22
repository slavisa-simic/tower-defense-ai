using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class Phase4CFinal {
 public static string Audit(){
 if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
 var paths=Directory.GetFiles("Assets/Game/Art/Enemies/Special","*.png").Concat(new[]{"Assets/Game/Art/Props/MilitantBossTransport_v001.png","Assets/Game/Art/Props/PirateBossTransport_v001.png","Assets/Game/Art/UI/ui_icon_dismantle.png"}).ToArray();
 foreach(var p in paths){var imp=(TextureImporter)AssetImporter.GetAtPath(p);var settings=new TextureImporterSettings();imp.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;imp.SetTextureSettings(settings);imp.SaveAndReimport();}
 if(File.Exists("Assets/Docs/Validation/Phase4C/baseline.png")){File.Copy("Assets/Docs/Validation/Phase4C/baseline.png","Docs/Validation/Phase4C/baseline.png",true);AssetDatabase.DeleteAsset("Assets/Docs");}
 AssetDatabase.ImportAsset("Assets/Game/UI/TopDownHud.uss",ImportAssetOptions.ForceUpdate);
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();int missing=scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
 var imports=paths.Select(p=>{var i=(TextureImporter)AssetImporter.GetAtPath(p);var s=new TextureImporterSettings();i.ReadTextureSettings(s);return new{path=p,guid=AssetDatabase.AssetPathToGUID(p),filter=i.filterMode.ToString(),compression=i.textureCompression.ToString(),mips=i.mipmapEnabled,ppu=i.spritePixelsPerUnit,mesh=s.spriteMeshType.ToString(),sprites=AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>().Count()};}).ToArray();
 AssetDatabase.SaveAssets();File.WriteAllText("Docs/Validation/Phase4C/final-audit.json",JsonConvert.SerializeObject(new{scene=scene.path,scene.isDirty,missingScripts=missing,imports},Formatting.Indented));
 return "Audited "+paths.Length+" textures; missing scripts "+missing+"; scene dirty "+scene.isDirty;
 }
}
