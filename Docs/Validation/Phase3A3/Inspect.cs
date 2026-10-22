using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class DefenseArtInspect
{
    public static string Run()
    {
        var scene=SceneManager.GetActiveScene();
        var flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        var log=typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
        object[] counts={0,0,0};log.GetMethod("GetCountsByType",flags).Invoke(null,counts);
        var prefabs=Directory.GetFiles("Assets/Game/Prefabs/Defenses","*.prefab").Select(p=>{
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(p);
            return new{path=p,guid=AssetDatabase.AssetPathToGUID(p),components=root.GetComponents<Component>().Select(c=>c.GetType().Name).ToArray(),
                renderers=root.GetComponentsInChildren<SpriteRenderer>(true).Select(r=>new{r.name,path=AnimationUtility.CalculateTransformPath(r.transform,root.transform),
                    r.enabled,sprite=AssetDatabase.GetAssetPath(r.sprite),position=r.transform.localPosition.ToString(),scale=r.transform.localScale.ToString(),
                    color=r.color.ToString(),r.sortingLayerName,r.sortingOrder}).ToArray()};
        }).ToArray();
        var result=new{scene=scene.path,scene.isDirty,playing=Application.isPlaying,compilationFailed=EditorUtility.scriptCompilationFailed,
            version=Application.unityVersion,console=new{errors=counts[0],warnings=counts[1],logs=counts[2]},prefabs};
        File.WriteAllText("Docs/Validation/Phase3A3/editor-inspection.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return JsonConvert.SerializeObject(result);
    }
}
