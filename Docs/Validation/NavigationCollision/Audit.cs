using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ShorelineDefense.Grid;
using ShorelineDefense.Core;
using ShorelineDefense.Waves;

public static class NavigationAudit
{
    public static string Run()
    {
        if (Application.isPlaying) throw new Exception("Edit mode only");
        var report = new StringBuilder();
        foreach (string map in new[] {"CoastalDefense_TopDown", "DanubeIndustrialPort"})
        {
            EditorSceneManager.OpenScene("Assets/Game/Scenes/" + map + ".unity");
            var manager = UnityEngine.Object.FindFirstObjectByType<GridManager>();
            var grid = manager.Definition.CreateGrid(manager.transform.position);
            report.AppendLine(map + " origin=" + grid.Origin + " size=" + grid.Width + "x" + grid.Height + " blocked=" + manager.Definition.blockedCells.Length);
            var border = UnityEngine.Object.FindFirstObjectByType<BorderBreachService>();
            report.AppendLine("Breach: " + string.Join(", ", border.BreachCells));
            foreach (var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || r.sprite == null || r.name.Contains("Shadow") || r.sortingLayerName != "GroundObjects") continue;
                if (!System.Text.RegularExpressions.Regex.IsMatch(r.name, "[Cc]ontainer|[Cc]rane|[Ww]arehouse|[Ss]hed|[Bb]ooth|[Bb]arrier|[Cc]ommand|[Ff]ort|[Vv]ehicle|[Tt]ruck|[Ff]orklift|[Ss]andbag|[Ff]ence|[Bb]unker")) continue;
                grid.TryWorldToCell(r.bounds.min + Vector3.one * .001f, out var min);
                grid.TryWorldToCell(r.bounds.max - Vector3.one * .001f, out var max);
                int blocked = 0, total = 0;
                for (int x = min.x; x <= max.x; x++) for (int y = min.y; y <= max.y; y++)
                { var c = new Vector2Int(x,y); if (!grid.Contains(c)) continue; total++; if (!grid.GetCell(c).Walkable) blocked++; }
                report.AppendLine(r.name + " pos=" + r.transform.position + " cells=" + min + ".." + max + " blocked=" + blocked + "/" + total + " colliders=" + r.GetComponentsInChildren<Collider2D>().Length);
            }
        }
        Directory.CreateDirectory("Docs/Validation/NavigationCollision");
        File.WriteAllText("Docs/Validation/NavigationCollision/original-map-audit.txt", report.ToString());
        return report.ToString();
    }
}
