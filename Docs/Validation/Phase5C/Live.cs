using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Persistence;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEditor;

public static class Phase5CLive
{
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    static T Find<T>() where T : UnityEngine.Object => UnityEngine.Object.FindFirstObjectByType<T>();
    static VisualElement Root => Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
    static void Check(bool condition, string why) { if (!condition) throw new Exception(why); }
    static void Click(string name) { var b = Root.Q<Button>(name); Check(b != null && b.enabledSelf, name); b.Focus(); using (var e = NavigationSubmitEvent.GetPooled()) { e.target = b; b.SendEvent(e); } }
    static void Capture(string name)
    {
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands")).First(t => t != null);
        var encode = type.GetMethod("EncodeScreenToPng", BindingFlags.NonPublic | BindingFlags.Static);
        File.WriteAllBytes("Docs/Validation/Phase5C/" + name + ".png", (byte[])encode.Invoke(null, new object[] { Screen.width, Screen.height }));
    }
    public static async Task<string> Begin()
    {
        Check(Application.isPlaying, "Play Mode required"); Application.runInBackground = true;
        LocalGameData.UseDirectory(Path.Combine(Path.GetTempPath(), "BorderDefendersLive5C", "review"));
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        var op = SceneManager.LoadSceneAsync("MainMenu"); while (!op.isDone) await Task.Delay(20); await Task.Delay(300);
        return "Live review isolated at " + LocalGameData.Files.Root;
    }
    public static async Task<string> RefreshMenus()
    {
        foreach (var size in new[] { new Vector2Int(1366,768), new Vector2Int(1600,900), new Vector2Int(1920,1080), new Vector2Int(2560,1440) })
            await MenuResolution(size.x, size.y);
        return "Final Main Menu/settings captures and bounds passed at all four resolutions";
    }
    public static async Task<string> RefreshGameplay()
    {
        await NewRun();
        foreach (var size in new[] { new Vector2Int(1366,768), new Vector2Int(1600,900), new Vector2Int(1920,1080), new Vector2Int(2560,1440) })
            await HudResolution(size.x, size.y);
        await Finish();
        return "Final HUD/event/selection/tooltip/pause captures passed at all four resolutions; summary/history checked";
    }
    static async Task SetSize(int width, int height)
    {
        var type = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        var group = type.GetProperty("currentGroup", Flags).GetValue(sizes);
        var view = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        string label = "Phase5C " + width + "x" + height;
        var labels = (string[])group.GetType().GetMethod("GetDisplayTexts", Flags).Invoke(group, null);
        int index = Array.FindIndex(labels, s => s.Contains("(" + width + "x" + height + ")") || s.StartsWith(label));
        if (index < 0) {
            var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var constructor = sizeType.GetConstructors(Flags).First(c => c.GetParameters().Length == 4);
            var size = constructor.Invoke(new[] { Enum.Parse(constructor.GetParameters()[0].ParameterType, "FixedResolution"), (object)width, height, label });
            group.GetType().GetMethod("AddCustomSize", Flags).Invoke(group, new[] { size });
            index = (int)group.GetType().GetMethod("GetTotalCount", Flags).Invoke(group, null) - 1;
        }
        view.GetType().GetProperty("selectedSizeIndex", Flags).SetValue(view, index); view.Focus(); view.Repaint(); await Task.Delay(400);
        Check(Screen.width == width && Screen.height == height, "Actual render dimensions");
    }
    public static async Task<string> MenuResolution(int width, int height)
    {
        Check(SceneManager.GetActiveScene().name == "MainMenu", "Main Menu required"); await SetSize(width, height); Find<ProductMenuController>().ShowMain(); await Task.Delay(100); Capture("menu-" + height);
        var card = Root.Q("product-card"); Check(card.worldBound.xMin >= 0 && card.worldBound.xMax <= Root.worldBound.xMax && card.worldBound.yMax <= Root.worldBound.yMax, "Menu card bounds");
        Find<ProductMenuController>().ShowSettings(); await Task.Delay(100); Capture("settings-" + height);
        Check(Root.Q<Button>("settings-back").worldBound.yMax <= card.worldBound.yMax, "Settings Back accessible");
        File.WriteAllText("Docs/Validation/Phase5C/menu-layout-" + height + ".json", JsonConvert.SerializeObject(new { width = Screen.width, height = Screen.height, card = card.worldBound.ToString() }));
        Find<ProductMenuController>().ShowMain(); return "Main Menu and settings checked at actual " + width + "x" + height;
    }
    public static async Task<string> NewRun()
    {
        new SaveGameService(LocalGameData.Files).Delete(out _); Find<ProductMenuController>().ShowMain(); await Task.Delay(100); Click("menu-new");
        for (int i = 0; i < 200 && (SceneManager.GetActiveScene().name != "CoastalDefense_TopDown" || Find<RunSession>() == null || !Find<RunSession>().Ready); i++) await Task.Delay(20);
        Check(Find<RunSession>() != null && Find<RunSession>().Ready, "New run ready"); await Task.Delay(150); return "New run started via real UI button";
    }
    public static async Task<string> Diagnose()
    {
        var save = new SaveGameService(LocalGameData.Files); bool deleted = save.Delete(out string error);
        Find<ProductMenuController>().NewGame(); await Task.Delay(300);
        return JsonConvert.SerializeObject(new { deleted, error, scene = SceneManager.GetActiveScene().name,
            loading = Find<ProductMenuController>().IsLoading, labels = Root.Query<Label>().ToList().Select(l => l.text).ToArray(), directory = LocalGameData.Files.Root });
    }
    public static async Task<string> HudResolution(int width, int height)
    {
        await SetSize(width, height); var w = Find<WaveManager>(); var b = Find<BuildManager>(); var menu = Find<ProductMenuController>();
        Find<GameSpeedController>().SetSpeed(GameSpeed.Normal);
        b.Economy.AddMoney(20000);
        var d = Find<BuildHud>().Defenses.Single(x => x.Id == "mortar");
        var defense = UnityEngine.Object.FindObjectsByType<ShorelineDefense.Defenses.DefenseBase>(FindObjectsSortMode.None).FirstOrDefault(x => x.Definition.Id == "mortar");
        if (defense == null) { Check(b.TryPlaceDefense(d, new Vector2Int(20,7), out defense) == PlacementResult.Valid, "Mortar"); b.TryUpgrade(defense); }
        Find<StructureSelection>().Select(defense);
        var data = Find<RunSession>().Capture(); data.wave = 24; data.completedWaves = 23; data.events = new EventSaveData { nextWave = 24 }; w.RestorePreparation(data); Check(w.ForceEvent("fog"), "Fog preview");
        await Task.Delay(150); Capture("hud-" + height);
        var root = Root; var panel = root.Q("structure-panel"); var build = root.Q("build-panel");
        Check(panel.worldBound.yMax < build.worldBound.yMin, "Selection/build overlap");
        foreach (string id in new[] { "money-panel", "health-panel", "wave-panel", "speed-controls", "build-panel" }) { var e = root.Q(id); Check(e.worldBound.xMin >= 0 && e.worldBound.xMax <= root.worldBound.xMax && e.worldBound.yMax <= root.worldBound.yMax, "HUD bounds " + id); }
        var button = root.Q<Button>("structure-upgrade");
        using (var e = PointerOverEvent.GetPooled()) { e.target = button; button.SendEvent(e); }
        await Task.Delay(100); Capture("tooltip-" + height);
        Check(root.Q<Label>("runtime-tooltip").text.Contains("Damage"), "Upgrade tooltip actual preview");
        using (var e = PointerOutEvent.GetPooled()) { e.target = button; button.SendEvent(e); }
        menu.ShowPause(); await Task.Delay(100); Capture("pause-" + height); Click("pause-resume");
        File.WriteAllText("Docs/Validation/Phase5C/hud-layout-" + height + ".json", JsonConvert.SerializeObject(new { width = Screen.width, height = Screen.height, panel = panel.worldBound.ToString(), build = build.worldBound.ToString() }));
        return "HUD, event, selection, tooltip and pause checked at actual " + width + "x" + height;
    }
    public static async Task<string> Finish()
    {
        Find<RunSession>().SaveNow(); Find<GameStateController>().EndRun(); await Task.Delay(150); Capture("run-summary");
        Check(new RunHistoryService(LocalGameData.Files).Load().runs.Count > 0, "History"); Check(!new SaveGameService(LocalGameData.Files).Load(out _, out _), "Dead Continue");
        Click("over-main"); await Task.Delay(400); Click("menu-history"); await Task.Delay(120); Capture("history");
        return "Run summary/history verified, dead Continue disabled";
    }
}
