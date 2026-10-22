using System;
using System.Collections.Generic;
using System.Linq;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ShorelineDefense.UI
{
    /// <summary>UI observes services; simulation and persistence remain in their owners.</summary>
    [DefaultExecutionOrder(1100), RequireComponent(typeof(UIDocument))]
    public sealed class ProductMenuController : MonoBehaviour
    {
        [SerializeField] private bool mainMenu;
        [SerializeField] private StyleSheet menuStyle;
        [SerializeField] private Font pixelFont;
        [SerializeField] private ShorelineDefense.Defenses.DefenseDefinition[] defenseCatalog;
        [SerializeField] private ShorelineDefense.Grid.BattlefieldDefinition battlefield;
        [SerializeField] private ShorelineDefense.Waves.EndlessEventSettings eventCatalog;
        private SaveGameService Saves => new SaveGameService(LocalGameData.Files, d => RunSaveCompatibility.Valid(d, defenseCatalog, battlefield, eventCatalog));
        public static string Notice;
        public bool ModalOpen => overlay != null && overlay.style.display != DisplayStyle.None;
        public bool IsLoading => loading;
        private VisualElement root, overlay, card, content;
        private Label feedback;
        private GameSpeedController speed;
        private RunSession session;
        private GameStateController game;
        private PlacementController placement;
        private StructureSelection selection;
        private string page;
        private Action cancelDialog;
        private bool loading;
        public void Configure(bool menu, StyleSheet style, Font font) { mainMenu = menu; menuStyle = style; pixelFont = font; }
        public void ConfigureCatalog(ShorelineDefense.Defenses.DefenseDefinition[] defenses, ShorelineDefense.Grid.BattlefieldDefinition map, ShorelineDefense.Waves.EndlessEventSettings events)
        { defenseCatalog = defenses; battlefield = map; eventCatalog = events; }
        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            if (menuStyle != null) root.styleSheets.Add(menuStyle);
            if (pixelFont != null) root.style.unityFontDefinition = FontDefinition.FromFont(pixelFont);
            overlay = new VisualElement { name = "product-overlay" }; overlay.AddToClassList("product-overlay");
            card = new VisualElement { name = "product-card" }; card.AddToClassList("product-card");
            var scroll = new ScrollView(ScrollViewMode.Vertical); scroll.AddToClassList("product-scroll");
            content = scroll.contentContainer; card.Add(scroll); overlay.Add(card); root.Add(overlay);
            feedback = new Label { name = "save-feedback", pickingMode = PickingMode.Ignore }; feedback.AddToClassList("save-feedback"); root.Add(feedback);
            SettingsService.Instance.Apply();
            if (mainMenu) ShowMain();
            else
            {
                speed = FindFirstObjectByType<GameSpeedController>(); session = FindFirstObjectByType<RunSession>(); game = FindFirstObjectByType<GameStateController>();
                placement = FindFirstObjectByType<PlacementController>(); selection = FindFirstObjectByType<StructureSelection>();
                speed.Changed += SpeedChanged; game.StateChanged += StateChanged; session.FeedbackChanged += FeedbackChanged;
                Hide(); FeedbackChanged();
            }
            gameObject.AddComponent<RuntimeTooltips>();
        }
        private void OnDestroy()
        { if (speed != null) speed.Changed -= SpeedChanged; if (game != null) game.StateChanged -= StateChanged; if (session != null) session.FeedbackChanged -= FeedbackChanged; }
        private void FeedbackChanged() { if (feedback != null) feedback.text = session.Feedback; }
        private void Page(string title, string id)
        {
            page = id; content.Clear(); cancelDialog = null; overlay.style.display = DisplayStyle.Flex; overlay.BringToFront();
            var heading = new Label(title); heading.AddToClassList("product-title"); content.Add(heading);
        }
        private void Copy(string text) { var label = new Label(text); label.AddToClassList("product-copy"); content.Add(label); }
        private Button ActionButton(string id, string text, Action action)
        { var button = new Button(action) { name = id, text = text }; button.AddToClassList("product-button"); content.Add(button); return button; }
        public void ShowMain()
        {
            Page("BORDER DEFENDERS", "main"); Copy("DANUBE QUARANTINE LINE\nHold the river. Defend the border.");
            var saves = Saves;
            bool valid = saves.Load(out _, out string error);
            ActionButton("menu-continue", "CONTINUE", Continue).SetEnabled(valid);
            ActionButton("menu-new", "NEW GAME", NewGame);
            ActionButton("menu-history", "RUN HISTORY", ShowHistory);
            ActionButton("menu-settings", "SETTINGS", ShowSettings);
            ActionButton("menu-quit", "QUIT", () => Confirm("QUIT GAME?", "Your last Preparation save will be kept.", "QUIT", Application.Quit, ShowMain));
            if (Notice != null) { Copy(Notice); Notice = null; }
            else if (error != null) Copy(valid ? error : "The active save could not be loaded. You can start a new run.");
        }
        public void NewGame()
        {
            if (Saves.Load(out _, out _))
                Confirm("START NEW RUN?", "Your current active run will be replaced. Completed run history will be kept.", "START NEW RUN", StartNew, Back);
            else StartNew();
        }
        private void StartNew()
        {
            if (!Saves.Delete(out string error)) { Copy("Could not replace active save: " + error); return; }
            RunSession.Pending = null; Load("CoastalDefense_TopDown");
        }
        public void Continue()
        {
            if (!Saves.Load(out var data, out _)) { ShowMain(); return; }
            RunSession.Pending = data; Load("CoastalDefense_TopDown");
        }
        private void Load(string scene)
        { if (loading) return; loading = true; Time.timeScale = 1; Page("LOADING...", "loading"); SceneManager.LoadSceneAsync(scene); }
        private void Hide() { overlay.style.display = DisplayStyle.None; page = null; }
        private void Back() { if (mainMenu) ShowMain(); else ShowPause(); }
        public void ShowPause()
        {
            if (game == null || !game.IsPlaying) return;
            if (speed.Speed != GameSpeed.Pause) speed.SetSpeed(GameSpeed.Pause);
            Page("PAUSED", "pause");
            Copy("Progress is saved between waves.\n" + (session.CanSave ? "Preparation progress can be saved now." : "Leaving now returns to the last Preparation save."));
            ActionButton("pause-resume", "RESUME", () => { Hide(); speed.Resume(); });
            ActionButton("pause-settings", "SETTINGS", ShowSettings);
            ActionButton("pause-restart", "RESTART RUN", () => Confirm("RESTART RUN?", "The current active run will be replaced.", "RESTART", StartNew, ShowPause));
            ActionButton("pause-main", "MAIN MENU", ReturnToMenu);
        }
        public void ReturnToMenu()
        {
            if (game.CurrentState == GameState.GameOver) { if (session.FinalizeRun()) Load("MainMenu"); else Copy(session.Feedback); return; }
            if (session.CanSave) { if (session.SaveNow()) Load("MainMenu"); else Copy("Progress could not be saved. Resume to try again."); }
            else Confirm("PROGRESS IS SAVED BETWEEN WAVES", "Current wave progress will be lost. Continue will return to your last Preparation save.", "QUIT CURRENT WAVE", () => Load("MainMenu"), ShowPause);
        }
        public void Confirm(string title, string message, string accept, Action yes, Action no)
        {
            Page(title, "confirm"); Copy(message); cancelDialog = no;
            ActionButton("confirm-cancel", "CANCEL", no); ActionButton("confirm-accept", accept, yes);
        }
        public void ShowHistory()
        {
            Page("RUN HISTORY", "history");
            var history = new RunHistoryService(LocalGameData.Files); var data = history.Load();
            Copy(data.runs.Count == 0 ? "No completed runs yet." : "PERSONAL BEST — WAVE " + data.runs.Max(r => r.wave));
            foreach (var r in data.runs)
            {
                string date = DateTime.TryParse(r.endedUtc, out var ended) ? ended.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "Unknown date";
                Copy($"{date}\nWave {r.wave}  /  Kills {r.kills:N0}\nHomicides {r.homicides:N0}  /  Bosses {r.bosses:N0}\nEarned {r.earned:N0} RSD  /  {FormatDuration(r.duration)}");
            }
            if (history.Error != null) Copy("Some run history could not be loaded: " + history.Error);
            ActionButton("history-back", "BACK", Back);
        }
        public static string FormatDuration(double seconds) => TimeSpan.FromSeconds(seconds).ToString(seconds >= 3600 ? @"h\:mm\:ss" : @"mm\:ss");
        public void ShowSettings()
        {
            Page("SETTINGS", "settings"); var current = SettingsService.Instance.Current;
            var resolutions = new List<Vector2Int>();
            foreach (var r in Screen.resolutions) if (r.width >= 800 && r.height >= 600 && !resolutions.Contains(new Vector2Int(r.width, r.height))) resolutions.Add(new Vector2Int(r.width, r.height));
            if (!resolutions.Contains(new Vector2Int(current.width, current.height))) resolutions.Add(new Vector2Int(current.width, current.height));
            resolutions.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            var resolution = new DropdownField("Resolution", resolutions.Select(r => r.x + " x " + r.y).ToList(), resolutions.IndexOf(new Vector2Int(current.width, current.height))) { name = "settings-resolution" }; content.Add(resolution);
            var fullscreen = new Toggle("Fullscreen") { name = "settings-fullscreen", value = current.fullscreen }; content.Add(fullscreen);
            var vsync = new Toggle("VSync") { name = "settings-vsync", value = current.vsync }; content.Add(vsync);
            var density = new DropdownField("VFX density", new List<string> { "Low", "Medium", "High" }, current.vfxDensity) { name = "settings-vfx" }; content.Add(density);
            var rewards = new Toggle("Show RSD reward popups") { name = "settings-rewards", value = current.rewardPopups }; content.Add(rewards);
            Copy("VFX density limits secondary effects and blood decals.\nGameplay damage and enemy counts stay the same.");
            if (Application.isEditor) Copy("Display mode applies in the standalone game. Editor Game View is controlled separately.");
            var result = new Label { name = "settings-result" }; result.AddToClassList("product-copy"); content.Add(result);
            ActionButton("settings-apply", "APPLY & SAVE", () => {
                var size = resolutions[resolution.index];
                bool ok = SettingsService.Instance.Save(new GameSettings { width = size.x, height = size.y, fullscreen = fullscreen.value, vsync = vsync.value, vfxDensity = density.index, rewardPopups = rewards.value });
                result.text = ok ? "SETTINGS SAVED" : "Settings could not be saved: " + SettingsService.Instance.Error;
            });
            ActionButton("settings-back", "BACK", Back);
        }
        private void SpeedChanged()
        { if (game.IsPlaying && speed.Speed == GameSpeed.Pause && !ModalOpen) ShowPause(); else if (speed.Speed != GameSpeed.Pause && page == "pause") Hide(); }
        private void StateChanged(GameState state)
        {
            if (state != GameState.GameOver) return;
            Page("BORDER DEFENSE COLLAPSED", "over");
            var stats = FindFirstObjectByType<RunStatistics>();
            Copy($"WAVE REACHED {stats.HighestWaveReached}\nTOTAL KILLS {stats.TotalKills:N0}\nHOMICIDES {stats.Homicides:N0}\nBOSSES KILLED {stats.BossesKilled:N0}\nRSD EARNED {stats.TotalMoneyEarned:N0}\nSTRUCTURES BUILT {stats.StructuresBuilt}\nDURATION {FormatDuration(session.Duration)}");
            // Existing game-over label remains for legacy scene/tests; product overlay owns this view.
            root.Q<Label>("game-over").style.visibility = Visibility.Hidden;
            ActionButton("over-new", "NEW RUN", () => { if (session.FinalizeRun()) StartNew(); else Copy(session.Feedback); });
            ActionButton("over-main", "MAIN MENU", ReturnToMenu);
        }
        private void Update()
        {
            if (loading) return;
            if (mainMenu) { if (Input.GetKeyDown(KeyCode.Escape)) { if (page == "confirm") cancelDialog?.Invoke(); else if (page != "main") ShowMain(); } return; }
            if (game == null || !game.IsPlaying) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (placement.IsInteracting) placement.Cancel();
                else if (page == "confirm") cancelDialog?.Invoke();
                else if (page == "settings" || page == "history") Back();
                else if (page == "pause") { Hide(); speed.Resume(); }
                else if (selection.Selected != null) selection.Clear();
                else ShowPause();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Space) && (page == null || page == "pause"))
            { if (speed.Speed == GameSpeed.Pause) { Hide(); speed.Resume(); } else ShowPause(); }
            if (ModalOpen) return;
            var buttons = root.Q("defense-buttons");
            if (buttons != null) for (int i = 0; i < 8; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { var b = buttons.Children().OfType<Button>().ElementAtOrDefault(i); if (b != null && b.enabledSelf) using (var e = NavigationSubmitEvent.GetPooled()) { e.target = b; b.SendEvent(e); } }
            if (Input.GetKeyDown(KeyCode.Delete) && selection.Selected != null)
            {
                var selected = selection.Selected; var builder = selected.Owner;
                speed.SetSpeed(GameSpeed.Pause);
                Confirm("DISMANTLE STRUCTURE?", $"Refund: {builder.RefundFor(selected):N0} RSD", "DISMANTLE", () => { builder.TryDismantle(selected, out _); Hide(); speed.Resume(); }, () => { Hide(); speed.Resume(); });
            }
        }
    }
}
