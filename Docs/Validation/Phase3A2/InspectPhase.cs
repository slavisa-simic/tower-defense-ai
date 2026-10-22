using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Newtonsoft.Json;

public static class Phase3A2Inspect
{
    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        var camera = Camera.main;
        var grid = UnityEngine.Object.FindFirstObjectByType<ShorelineDefense.Grid.GridManager>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Enemies/Crawler_TopDown.prefab");
        var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        var logType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
        var countMethod = logType.GetMethod("GetCountsByType",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        object[] counts = {0,0,0};
        countMethod.Invoke(null,counts);
        var entryType=typeof(Editor).Assembly.GetType("UnityEditor.LogEntry");
        var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        var messages=new System.Collections.Generic.List<object>();
        int total=(int)logType.GetMethod("GetCount",flags).Invoke(null,null);
        logType.GetMethod("StartGettingEntries",flags).Invoke(null,null);
        try {
            for(int i=0;i<total;i++){
                var entry=Activator.CreateInstance(entryType);
                logType.GetMethod("GetEntryInternal",flags).Invoke(null,new object[]{i,entry});
                var fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
                messages.Add(new{message=entryType.GetField("message",fields)?.GetValue(entry),mode=entryType.GetField("mode",fields)?.GetValue(entry)});
            }
        } finally {logType.GetMethod("EndGettingEntries",flags).Invoke(null,null);}
        var result = new {
            version=Application.unityVersion, scene=scene.path, scene.isDirty, playing=Application.isPlaying,
            compiling=EditorApplication.isCompiling, compilationFailed=EditorUtility.scriptCompilationFailed,
            camera=camera == null ? null : new {camera.orthographic,camera.orthographicSize,camera.aspect,
                position=camera.transform.position.ToString(),rotation=camera.transform.eulerAngles.ToString()},
            grid=grid == null ? null : new {grid.Definition.Width,grid.Definition.Height,grid.Definition.CellSize},
            placeholderRenderers=renderers.Select(r=>new {r.name,scale=r.transform.localScale.ToString(),
                bounds=r.sprite.bounds.size.ToString(),r.sortingLayerName,r.sortingOrder}).ToArray(),
            console=new {errors=counts[0],warnings=counts[1],logs=counts[2],messages}
        };
        File.WriteAllText("Docs/Validation/Phase3A2/editor-inspection.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return JsonConvert.SerializeObject(result);
    }
}
