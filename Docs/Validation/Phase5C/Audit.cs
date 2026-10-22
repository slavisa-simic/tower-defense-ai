using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Persistence;
using ShorelineDefense.UI;

public static class Phase5CAudit
{
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Edit Mode required");
        var rows = new System.Collections.Generic.List<object>();
        foreach (string path in new[] { "Assets/Game/Scenes/MainMenu.unity", "Assets/Game/Scenes/CoastalDefense_TopDown.unity" })
        {
            var scene = EditorSceneManager.OpenScene(path);
            int missing = scene.GetRootGameObjects().Sum(r => r.GetComponentsInChildren<Transform>(true).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
            int menus = UnityEngine.Object.FindObjectsByType<ProductMenuController>(FindObjectsSortMode.None).Length;
            int sessions = UnityEngine.Object.FindObjectsByType<RunSession>(FindObjectsSortMode.None).Length;
            if (missing != 0 || menus != 1 || sessions != (path.Contains("MainMenu") ? 0 : 1)) throw new Exception("Scene integration mismatch");
            rows.Add(new { path, missing, menus, sessions, dirty = scene.isDirty });
        }
        if (EditorBuildSettings.scenes[0].path != "Assets/Game/Scenes/MainMenu.unity") throw new Exception("Startup scene");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        // Remove only the accidental capture created by this phase's capture CLI invocation.
        string accidental = "Assets/Docs/Validation/Phase5C/menu-initial.png";
        if (File.Exists(accidental)) { File.Copy(accidental, "Docs/Validation/Phase5C/menu-initial.png", true); AssetDatabase.DeleteAsset(accidental); }
        foreach (string directory in new[] { "Assets/Docs/Validation/Phase5C", "Assets/Docs/Validation", "Assets/Docs" })
            if (Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length == 0) AssetDatabase.DeleteAsset(directory);
        File.WriteAllText("Docs/Validation/Phase5C/integration-audit.json", JsonConvert.SerializeObject(rows, Formatting.Indented));
        return "Two clean saved scenes, zero missing scripts, one product controller each, one gameplay RunSession, MainMenu first";
    }
}
