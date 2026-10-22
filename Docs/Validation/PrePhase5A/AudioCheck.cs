using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
public static class PrePhase5AAudioCheck
{
    public static async Task<string> Run()
    {
        string path = "Assets/Game/Audio/Processed/Environment/sfx_TEST_pipeline_noise_01.wav";
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = true;
        importer.SaveAndReimport();
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null || !clip.LoadAudioData()) throw new Exception("Clip failed to load");
        var data = new float[clip.samples * clip.channels];
        if (!clip.GetData(data, 0) || data.Any(v => float.IsNaN(v) || float.IsInfinity(v))) throw new Exception("Invalid clip samples");
        int sourcesBefore = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length;
        int listenersBefore = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
        var go = new GameObject("TEST audio pipeline (temporary)");
        bool playing = false; double outputRms = 0;
        float previousTimeScale = Time.timeScale;
        try
        {
            if (listenersBefore == 0) go.AddComponent<AudioListener>();
            var source = go.AddComponent<AudioSource>();
            source.clip = clip; source.spatialBlend = 0; source.volume = .1f; source.loop = true;
            Time.timeScale = 1;
            await Task.Delay(150);
            source.Play();
            var output = new float[1024];
            for (int i = 0; i < 30; i++) {
                await Task.Delay(20); playing |= source.isPlaying;
                source.GetOutputData(output, 0);
                outputRms = Math.Max(outputRms, Math.Sqrt(output.Average(v => (double)v * v)));
            }
        }
        finally { Time.timeScale = previousTimeScale; UnityEngine.Object.Destroy(go); }
        var receipt = new { testOnly = true, generator = "FFmpeg deterministic procedural white noise; NOT generative AI", path, clip.length, clip.frequency, clip.channels, clip.samples, clip.loadState, sourceRms = Math.Sqrt(data.Average(v => (double)v*v)), playing, outputRms, sourcesBefore, listenersBefore, mixerAssets = AssetDatabase.FindAssets("t:AudioMixer").Length, deviceSampleRate = AudioSettings.outputSampleRate, audibleHumanListening = "not performed" };
        File.WriteAllText("Docs/Validation/PrePhase5A/audio-import.json", Newtonsoft.Json.JsonConvert.SerializeObject(receipt, Newtonsoft.Json.Formatting.Indented));
        if (!playing || outputRms <= 0) throw new Exception("Playback/output verification failed; inspect receipt");
        return "Valid PCM AudioClip, nonzero samples and real AudioSource output; temporary objects removed";
    }
}
