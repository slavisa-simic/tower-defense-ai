using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using ShorelineDefense.Editor;
using ShorelineDefense.Tutorial;
using ShorelineDefense.Persistence;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine.UIElements;

public static class Phase6DWalkthrough
{
    public static async Task<string> Size(int width, int height)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var type = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
        var group = type.GetProperty("currentGroup", flags).GetValue(sizes);
        var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        string label = "Phase6D " + width + "x" + height;
        var labels = (string[])group.GetType().GetMethod("GetDisplayTexts", flags).Invoke(group, null);
        int index = Array.FindIndex(labels, s => s.Contains("(" + width + "x" + height + ")") || s.StartsWith(label));
        if (index < 0) {
            var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var ctor = sizeType.GetConstructors(flags).First(c => c.GetParameters().Length == 4);
            var size = ctor.Invoke(new[] { Enum.Parse(ctor.GetParameters()[0].ParameterType, "FixedResolution"), (object)width, height, label });
            group.GetType().GetMethod("AddCustomSize", flags).Invoke(group, new[] { size });
            index = (int)group.GetType().GetMethod("GetTotalCount", flags).Invoke(group, null) - 1;
        }
        view.GetType().GetProperty("selectedSizeIndex", flags).SetValue(view, index); view.Focus(); view.Repaint();
        await Task.Delay(500);
        if (Screen.width != width || Screen.height != height) throw new Exception("Game View resolution did not apply.");
        return Save("layout-" + width + "x" + height);
    }
    public static string Save(string label)
    {
        string json = TutorialValidationTools.Snapshot();
        File.WriteAllText("Docs/Validation/Phase6D/" + label + ".json", json);
        return json;
    }
    public static async Task<string> Capture(string label)
    {
        await Task.Delay(100);
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t => t != null);
        var encode = type.GetMethod("EncodeScreenToPng", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        string path = "Docs/Validation/Phase6D/" + label + ".png";
        File.WriteAllBytes(path, (byte[])encode.Invoke(null, new object[] { Screen.width, Screen.height }));
        Save(label); return Path.GetFullPath(path);
    }
    private static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    private static void Only(string id)
    {
        LocalGameData.Tutorial.ResetProgress();
        foreach (var d in TutorialCatalog.All) if (d.Id != id) LocalGameData.Tutorial.MarkShown(d.Id);
        Find<TutorialRuntime>().Manager.Clear();
    }
    private static void CheckLayout(string expected)
    {
        var t = Find<TutorialRuntime>(); var root = t.GetComponent<UIDocument>().rootVisualElement;
        if (t.Manager.Active?.Definition.Id != expected) throw new Exception("Expected tutorial " + expected + ", found " + t.Manager.Active?.Definition.Id);
        Rect bounds = t.View.Bounds;
        if (bounds.xMin < 0 || bounds.yMin < 0 || bounds.xMax > root.resolvedStyle.width || bounds.yMax > root.resolvedStyle.height) throw new Exception("Tutorial outside viewport");
        if (!Find<ProductMenuController>().ModalOpen) foreach (string id in new[] { "health-panel", "money-panel", "event-panel", "boss-panel", "build-panel", "structure-panel", "preparation-intelligence" }) {
            var e = root.Q(id); if (e != null && e.resolvedStyle.display != DisplayStyle.None && bounds.Overlaps(e.worldBound)) throw new Exception("Overlaps " + id);
        }
        foreach (string id in new[] { "tutorial-title", "tutorial-copy", "tutorial-detail" }) {
            var label = root.Q<Label>(id); if (label.resolvedStyle.display == DisplayStyle.None) continue;
            var measured = label.MeasureTextSize(label.text, label.contentRect.width, VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined);
            if (measured.y > label.contentRect.height + 2) throw new Exception("Clipped tutorial text: " + id);
        }
    }
    private static async Task SetAndCapture(string context)
    {
        foreach (var size in new[] { new Vector2Int(1366, 768), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) }) {
            await Size(size.x, size.y);
            // Each capture is deliberately kept on its current lesson to inspect layout, not timing.
            CheckLayout(context); await Capture(context + "-" + size.x + "x" + size.y);
        }
    }
    public static async Task<string> ResolutionAudit()
    {
        LocalGameData.UseDirectory(Path.GetFullPath(Path.Combine("Temp", "Phase6DResolutionProfiles", Guid.NewGuid().ToString("N"))));
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CoastalDefense_TopDown"); await Task.Delay(250); Time.timeScale = 0;
        await SetAndCapture("intro");
        Only("repair");
        var builder = Find<BuildManager>(); var def = Find<BuildHud>().MachineGun;
        if (builder.TryPlaceDefense(def, new Vector2Int(14, 8), out var defense) != PlacementResult.Valid) throw new Exception("QA placement failed");
        defense.TakeDamage(defense.MaxHealth / 2); Find<StructureSelection>().Select(defense); await Task.Delay(200);
        await SetAndCapture("repair");
        Only("boss");
        var waves = Find<WaveManager>(); var data = Find<RunSession>().Capture(); data.wave = 25; data.completedWaves = 24;
        waves.RestorePreparation(data); waves.Events.Tick(100); waves.TryStartWave(); await Task.Delay(200);
        await SetAndCapture("boss");
        Only("rp"); Find<GameStateController>().EndRun(); await Task.Delay(200); await SetAndCapture("rp");
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu"); await Task.Delay(250);
        Only("research-tree"); LocalGameData.Tutorial.RecordRunCompleted(); Find<ProductMenuController>().ShowResearch(); await Task.Delay(200);
        await SetAndCapture("research-tree");
        Only("port"); await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("DanubeIndustrialPort"); await Task.Delay(250); Time.timeScale = 0;
        await SetAndCapture("port");
        return "18 resolution/context captures passed bounds, text height and critical HUD separation checks.";
    }
}
