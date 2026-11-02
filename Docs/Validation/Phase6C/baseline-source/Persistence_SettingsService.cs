using System;
using UnityEngine;

namespace ShorelineDefense.Persistence
{
    [Serializable] public sealed class GameSettings
    {
        public int saveVersion = 1, width = 1920, height = 1080, vfxDensity = 2;
        public bool fullscreen = true, vsync = true, rewardPopups = true;
    }
    public sealed class SettingsService
    {
        private readonly LocalJsonStore store;
        public GameSettings Current { get; private set; }
        public string Error { get; private set; }
        public static SettingsService Instance => LocalGameData.Settings;
        public event Action Changed;
        public SettingsService(LocalJsonStore files)
        {
            store = files;
            store.Read<GameSettings>("settings", Valid, out var data, out string error);
            var displayDefaults = new GameSettings { width = Screen.width, height = Screen.height, fullscreen = Screen.fullScreen };
            // A small Editor Game View must not create defaults that Save rejects.
            Current = data ?? (Valid(displayDefaults) ? displayDefaults : new GameSettings { fullscreen = Screen.fullScreen });
            Error = error;
        }
        public static bool Valid(GameSettings d) => d != null && d.saveVersion == 1 && d.width >= 800 && d.width <= 16384 && d.height >= 600 && d.height <= 16384 && d.vfxDensity >= 0 && d.vfxDensity <= 2;
        public bool Save(GameSettings data)
        {
            if (!Valid(data)) { Error = "Invalid display settings."; return false; }
            if (!store.Write("settings", data, out string error)) { Error = error; return false; }
            Current = data; Error = null; Apply(); Changed?.Invoke(); return true;
        }
        public void Apply()
        {
            QualitySettings.vSyncCount = Current.vsync ? 1 : 0;
            if (!Application.isEditor) Screen.SetResolution(Current.width, Current.height, Current.fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed);
        }
    }
    public static class LocalGameData
    {
        private static LocalJsonStore files;
        private static SettingsService settings;
        public static LocalJsonStore Files => files ?? (files = new LocalJsonStore(System.IO.Path.Combine(Application.persistentDataPath, "BorderDefenders")));
        public static SettingsService Settings => settings ?? (settings = new SettingsService(Files));
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { files = null; settings = null; RunSession.Pending = null; }
        // Test fixtures redirect local storage before loading any product scene.
        public static void UseDirectory(string directory) { files = new LocalJsonStore(directory); settings = null; }
    }
}
