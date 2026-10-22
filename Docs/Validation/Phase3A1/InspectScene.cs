using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using Newtonsoft.Json;

public static class Phase3A1Inspect
{
    public static string Run()
    {
        var scene = SceneManager.GetActiveScene();
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        var grids = Object.FindObjectsByType<ShorelineDefense.Grid.GridManager>(FindObjectsSortMode.None);
        var renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        var report = new {
            unityVersion = Application.unityVersion, scene = scene.path, dirty = scene.isDirty,
            playing = Application.isPlaying, roots = scene.GetRootGameObjects().Select(o => o.name).ToArray(),
            cameras = cameras.Select(c => new { c.name, c.orthographic, c.orthographicSize, c.aspect,
                position = c.transform.position.ToString("F3"), rotation = c.transform.eulerAngles.ToString("F3") }).ToArray(),
            grids = grids.Select(g => new { g.name, definition = AssetDatabase.GetAssetPath(g.Definition),
                g.Definition.Width, g.Definition.Height, g.Definition.CellSize, plane = g.Definition.Plane.ToString() }).ToArray(),
            sortingLayers = SortingLayer.layers.Select(l => l.name).ToArray(),
            renderers = renderers.Select(r => new { r.name, sprite = AssetDatabase.GetAssetPath(r.sprite),
                r.sortingLayerName, r.sortingOrder, tint = r.color.ToString(),
                scale = r.transform.lossyScale.ToString("F3") }).ToArray()
        };
        var json = JsonConvert.SerializeObject(report, Formatting.Indented);
        System.IO.File.WriteAllText("Docs/Validation/Phase3A1/scene-inspection.json", json);
        return "Saved scene-inspection.json; scene=" + scene.path + "; sprite renderers=" + renderers.Length;
    }
}
