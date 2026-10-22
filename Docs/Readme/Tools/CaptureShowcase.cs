using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Persistence;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Documentation capture only. Compile with Pipeline run_script; never import into Assets.
public static class CaptureShowcase
{
    const int Width = 960, Height = 540, Fps = 10, Frames = 120;
    static string Root => Directory.GetCurrentDirectory();
    static string Media => Path.Combine(Root, "Docs/Readme/Media");
    static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();

    public static string Prepare()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        EditorSceneManager.OpenScene("Assets/Game/Scenes/MainMenu.unity");
        return "Main menu ready; enter Play Mode before recording.";
    }

    public static Task<string> Border() => Record("CoastalDefense_TopDown", "danube-border");
    public static Task<string> Port() => Record("DanubeIndustrialPort", "industrial-port");

    static async Task<string> Record(string scene, string stem)
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Capture runs only in Play Mode.");
        // The main menu has no RunSession. Redirect storage before loading gameplay.
        LocalGameData.UseDirectory(Path.Combine(Root, "TestResults/Readme/LocalData"));
        RunSession.Pending = null;
        Time.timeScale = 0;
        var operation = SceneManager.LoadSceneAsync(scene);
        while (!operation.isDone) await Task.Delay(20);
        double deadline = Time.realtimeSinceStartupAsDouble + 8;
        while ((Find<RunSession>() == null || !Find<RunSession>().Ready) && Time.realtimeSinceStartupAsDouble < deadline)
            await Task.Delay(20);
        if (Find<RunSession>() == null || !Find<RunSession>().Ready) throw new Exception("Scene initialization timed out.");

        var builder = Find<BuildManager>();
        var waves = Find<WaveManager>();
        var statistics = Find<RunStatistics>();
        var gun = Find<BuildHud>().Defenses.Single(d => d.Id == "machine-gun-nest");
        var desired = new[] { new Vector2Int(10, 8), new Vector2Int(16, 12), new Vector2Int(21, 6) };
        var placed = new Vector2Int[desired.Length];
        int startMoney = builder.Economy.Money;
        for (int i = 0; i < desired.Length; i++)
        {
            var candidates = Enumerable.Range(0, builder.Battlefield.Grid.Width)
                .SelectMany(x => Enumerable.Range(0, builder.Battlefield.Grid.Height).Select(y => new Vector2Int(x, y)))
                .OrderBy(cell => (cell - desired[i]).sqrMagnitude);
            bool done = false;
            foreach (var cell in candidates)
                if (builder.Validate(gun, cell) == PlacementResult.Valid)
                {
                    if (builder.TryPlaceDefense(gun, cell, out _) != PlacementResult.Valid) throw new Exception("Placement changed.");
                    placed[i] = cell; done = true; break;
                }
            if (!done) throw new Exception("No valid MG placement.");
        }

        Directory.CreateDirectory(Media);
        string frames = Path.Combine(Root, "TestResults/Readme/Frames", stem);
        Directory.CreateDirectory(frames);
        var camera = Camera.main;
        var oldRect = camera.rect;
        var oldTarget = camera.targetTexture;
        var target = new RenderTexture(Width, Height, 24) { filterMode = FilterMode.Point };
        var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        target.Create();
        camera.targetTexture = target;
        camera.rect = new Rect(0, 0, 1, 1);
        Find<TopDownCamera>().FrameBattlefield();
        try
        {
            Capture(camera, target, image, Path.Combine(Media, stem + ".png"));
            if (!waves.TryStartWave()) throw new Exception("Wave did not start.");
            Time.timeScale = 1;
            double start = Time.realtimeSinceStartupAsDouble;
            for (int i = 0; i < Frames; i++)
            {
                while (Time.realtimeSinceStartupAsDouble < start + (double)i / Fps) await Task.Delay(5);
                Capture(camera, target, image, Path.Combine(frames, i.ToString("D4") + ".png"));
                if (i == 60) Capture(camera, target, image, Path.Combine(Media, stem + "-poster.png"));
            }
            string receipt = "scene=" + scene + "\nseed=" + waves.RunSeed + "\nstartMoney=" + startMoney +
                "\nplacements=" + string.Join("; ", placed.Select(p => p.ToString())) +
                "\nframes=" + Frames + "\nfps=" + Fps + "\nkills=" + statistics.TotalKills +
                "\nhomicides=" + statistics.Homicides + "\nwaveState=" + waves.State +
                "\nWorld-camera capture; UI is shown in separate screenshots. No stats or budgets changed.\n";
            File.WriteAllText(Path.Combine(Root, "TestResults/Readme", stem + "-capture.txt"), receipt);
            return receipt;
        }
        finally
        {
            Time.timeScale = 0;
            camera.targetTexture = oldTarget;
            camera.rect = oldRect;
            RenderTexture.active = null;
            target.Release();
            Object.Destroy(target);
            Object.Destroy(image);
        }
    }

    static void Capture(Camera camera, RenderTexture target, Texture2D image, string path)
    {
        var previous = RenderTexture.active;
        camera.Render();
        RenderTexture.active = target;
        image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        image.Apply();
        File.WriteAllBytes(path, image.EncodeToPNG());
        RenderTexture.active = previous;
    }
}
