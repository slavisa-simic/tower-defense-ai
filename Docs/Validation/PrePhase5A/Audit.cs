using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class PrePhase5AAudit
{
    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        int missing = scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren<Transform>(true).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        File.WriteAllText("Docs/Validation/PrePhase5A/final-audit.json", Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene.path, scene.isDirty, missing, playing = EditorApplication.isPlaying, sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length, listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, testSourceReferences = scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren<AudioSource>(true).Count(a => a.clip != null && a.clip.name.Contains("TEST"))) }, Newtonsoft.Json.Formatting.Indented));
        return "Saved-scene audit: " + missing + " missing scripts";
    }
}
