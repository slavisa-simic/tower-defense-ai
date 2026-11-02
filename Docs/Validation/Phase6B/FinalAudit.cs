using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;
using ShorelineDefense.Waves;
using ShorelineDefense.Enemies;
using Object=UnityEngine.Object;
public static class Phase6BFinalAudit
{
    public static string RefreshBorderPreview()
    {
        if(Application.isPlaying)throw new Exception("Edit mode only");
        string path="Assets/Game/Art/UI/map_preview_danube_border.png";
        string before="Docs/Validation/Phase6B/map-preview-before.png";if(!File.Exists(before))File.Copy(path,before);
        EditorSceneManager.OpenScene("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
        var context=Object.FindFirstObjectByType<MapRuntimeContext>();var grid=context.definition.battlefield.CreateGrid(context.grid.transform.position);var camera=Camera.main;
        var originalPosition=camera.transform.position;var originalRect=camera.rect;float originalSize=camera.orthographicSize;var originalTarget=camera.targetTexture;var originalActive=RenderTexture.active;
        var target=new RenderTexture(960,540,24);var image=new Texture2D(960,540,TextureFormat.RGB24,false);
        try
        {
            camera.rect=new Rect(0,0,1,1);camera.aspect=960f/540;camera.orthographicSize=Mathf.Max(grid.Height*grid.CellSize/2,grid.Width*grid.CellSize/(2*camera.aspect))+.25f;
            var center=grid.Origin+new Vector3(grid.Width*grid.CellSize/2,grid.Height*grid.CellSize/2,0);camera.transform.position=new Vector3(center.x,center.y,originalPosition.z);camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        }
        finally{camera.transform.position=originalPosition;camera.rect=originalRect;camera.orthographicSize=originalSize;camera.targetTexture=originalTarget;camera.ResetAspect();RenderTexture.active=originalActive;Object.DestroyImmediate(image);Object.DestroyImmediate(target);}
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        return "Replaced obsolete boat-bearing map-card preview with an actual saved aquatic-era scene capture; original texture GUID/importer retained and old image archived as validation provenance.";
    }
    public static string Run()
    {
        if(Application.isPlaying)throw new Exception("Edit mode only");
        PlayerSettings.runInBackground=false;
        var rows=new List<object>();
        foreach(string name in new[]{"MainMenu","CoastalDefense_TopDown","DanubeIndustrialPort"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Game/Scenes/"+name+".unity");
            int missing=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            var legacy=AssetDatabase.GetDependencies(scene.path,true).Where(p=>p.Contains("/Legacy/")).ToArray();
            if(missing!=0||legacy.Length!=0)throw new Exception("Production dependency failure "+name+" "+string.Join(",",legacy));
            var system=Object.FindFirstObjectByType<AssaultCarrierSystem>();
            if(system!=null&&(system.Carriers.Count!=4||system.Carriers.Any(c=>c==null||c.visualPrefab==null)))throw new Exception("Missing carrier "+name);
            rows.Add(new{scene=name,missing,legacyDependencies=legacy,carriers=system?.Carriers.Count??0});
        }
        var settings=AssetDatabase.LoadAssetAtPath<EndlessWaveSettings>("Assets/Game/Data/Waves/EndlessSurvival.asset");
        if(settings.enemies.Length!=6||settings.enemies.Any(e=>e==null||e.Definition.Kind==EnemyKind.LegacyUnclassified))throw new Exception("Invalid active roster");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");AssetDatabase.SaveAssets();
        string json=JsonConvert.SerializeObject(new{runInBackground=PlayerSettings.runInBackground,rows,previews=AssetDatabase.FindAssets("t:MapDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<MapDefinition>(AssetDatabase.GUIDToAssetPath(g))).Select(m=>new{m.id,preview=AssetDatabase.GetAssetPath(m.preview)})},Formatting.Indented);
        File.WriteAllText("Docs/Validation/Phase6B/final-asset-audit.json",json);return json;
    }
}
