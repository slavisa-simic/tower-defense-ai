using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class Phase3A1Terrain
{
    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Game/Scenes/CoastalDefense_TopDown.unity" || scene.isDirty)
            throw new InvalidOperationException("Requires the clean saved primary scene in Edit Mode.");
        string[] names = { "Ocean", "Beach", "Grass Combat Field", "Shallow Water", "Shoreline" };
        string[] subjects = { "deep_danube_water_tile", "muddy_shore_tile", "grass_tile", "shallow_water_tile", "wet_bank_transition_tile" };
        var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        var targets = names.Select(n => renderers.Single(r => r.name == n)).ToArray();
        var sprites = subjects.Select(n => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Art/Environment/bd_env_" + n + "_v001.png")).ToArray();
        if (sprites.Any(s => s == null)) throw new InvalidOperationException("Missing terrain sprite.");
        var records = new List<object>();
        for (int i=0;i<targets.Length;i++)
        {
            var r = targets[i];
            var oldSize = Vector2.Scale(r.sprite.bounds.size, r.transform.localScale);
            var old = new { sprite=AssetDatabase.GetAssetPath(r.sprite), scale=r.transform.localScale.ToString("F3"),
                tint=r.color.ToString(), drawMode=r.drawMode.ToString(), size=oldSize.ToString("F3") };
            Undo.RecordObjects(new UnityEngine.Object[] { r, r.transform }, "Phase 3A-1 terrain art");
            r.sprite = sprites[i];
            r.color = Color.white;
            r.drawMode = SpriteDrawMode.Tiled;
            r.tileMode = SpriteTileMode.Continuous;
            r.size = i == 4 ? new Vector2(1.25f, oldSize.y) : oldSize;
            // Unity adjusts scale when switching draw mode; normalize AFTER that switch.
            r.transform.localScale = Vector3.one;
            records.Add(new { r.name, before=old, after=new { sprite=AssetDatabase.GetAssetPath(r.sprite),
                size=r.size.ToString("F3"), scale=r.transform.localScale.ToString("F3"),
                r.sortingLayerName, r.sortingOrder, position=r.transform.position.ToString("F3") } });
            EditorUtility.SetDirty(r);
            EditorUtility.SetDirty(r.transform);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        File.WriteAllText("Docs/Validation/Phase3A1/terrain-integration.json",JsonConvert.SerializeObject(records,Formatting.Indented));
        return "Saved five presentation-only terrain replacements; no new objects/components or gameplay changes.";
    }

    public static string FixScale()
    {
        var scene = SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != "Assets/Game/Scenes/CoastalDefense_TopDown.unity")
            throw new InvalidOperationException("Wrong scene/state");
        var rows = new List<object>();
        foreach (var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (!AssetDatabase.GetAssetPath(r.sprite).StartsWith("Assets/Game/Art/Environment/")) continue;
            Undo.RecordObject(r.transform,"Normalize tiled presentation scale");
            r.transform.localScale = Vector3.one;
            rows.Add(new {r.name, size=r.size.ToString("F3"),scale=r.transform.lossyScale.ToString("F3"),bounds=r.bounds.size.ToString("F3")});
            EditorUtility.SetDirty(r.transform);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.WriteAllText("Docs/Validation/Phase3A1/terrain-scale-correction.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
        return "Normalized " + rows.Count + " tiled renderer transforms after Unity's automatic draw-mode adjustment.";
    }
}
