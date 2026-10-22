using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.U2D;
using UnityEditor.U2D;
using ShorelineDefense.Grid;

public static class Phase6AFinalize
{
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit mode required.");
        const string portRaw="Assets/Game/Art/SourceSheets/p6a_port_001/map_preview_industrial_port_raw_v001.png";
        const string portPreview="Assets/Game/Art/UI/map_preview_industrial_port.png";
        if(!File.Exists(portPreview))File.Copy(portRaw,portPreview);
        AssetDatabase.ImportAsset(portPreview);Sprite port=Import(portPreview);
        var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
        var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
        var cameraRect=camera.rect;camera.rect=new Rect(0,0,1,1);UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Presentation.TopDownCamera>().FrameBattlefield();
        var rt=new RenderTexture(960,540,24);var old=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();
        const string riverPreview="Assets/Game/Art/UI/map_preview_danube_border.png";File.WriteAllBytes(riverPreview,image.EncodeToPNG());camera.targetTexture=old;camera.rect=cameraRect;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        AssetDatabase.ImportAsset(riverPreview);Sprite river=Import(riverPreview);
        var catalog=AssetDatabase.LoadAssetAtPath<MapCatalog>("Assets/Game/Data/Maps/MapCatalog.asset");catalog.Find("river_border").preview=river;catalog.Find("industrial_port").preview=port;
        foreach(var map in catalog.maps)EditorUtility.SetDirty(map);AssetDatabase.SaveAssets();
        string atlasPath="Assets/Game/Art/Environment/PortEnvironment.spriteatlas";
        var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);if(atlas==null){atlas=new SpriteAtlas();AssetDatabase.CreateAsset(atlas,atlasPath);}
        SpriteAtlasExtensions.Remove(atlas,SpriteAtlasExtensions.GetPackables(atlas));
        var sprites=AssetDatabase.FindAssets("bd_prop_port_ t:Texture2D",new[]{"Assets/Game/Art/Props"}).Concat(AssetDatabase.FindAssets("bd_env_port_ t:Texture2D",new[]{"Assets/Game/Art/Environment"})).Select(g=>AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        SpriteAtlasExtensions.Add(atlas,sprites);var packing=SpriteAtlasExtensions.GetPackingSettings(atlas);packing.enableRotation=false;packing.enableTightPacking=false;SpriteAtlasExtensions.SetPackingSettings(atlas,packing);
        var texture=SpriteAtlasExtensions.GetTextureSettings(atlas);texture.filterMode=FilterMode.Point;texture.generateMipMaps=false;SpriteAtlasExtensions.SetTextureSettings(atlas,texture);EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssets();SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget);
        int missing=0;var receipts=new System.Collections.Generic.List<string>();
        foreach(string name in new[]{"MainMenu","CoastalDefense_TopDown","DanubeIndustrialPort"})
        {scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");int count=0;foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))count+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);missing+=count;receipts.Add(name+": missing="+count+", roots="+scene.rootCount);}
        File.WriteAllText("Docs/Validation/Phase6A/scene-audit.txt",string.Join("\n",receipts)+"\nAtlas textures="+sprites.Length);
        return "Two previews imported, port sprite atlas packed; missing scripts="+missing+". "+string.Join(" | ",receipts);
    }
    static Sprite Import(string path)
    {var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
}
