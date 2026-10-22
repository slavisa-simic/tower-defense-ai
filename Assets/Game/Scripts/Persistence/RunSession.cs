using System;
using System.Collections.Generic;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Persistence
{
    /// <summary>Coordinates stable snapshots. Scene reload owns complete new-run reset.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class RunSession : MonoBehaviour
    {
        public static RunSaveData Pending { get; set; }
        public string RunId { get; private set; }
        public double Duration { get; private set; }
        public string Feedback { get; private set; }
        public bool Ready { get; private set; }
        public event Action FeedbackChanged;
        private BuildManager builder;
        private WaveManager waves;
        private GameStateController game;
        private RunStatistics statistics;
        private BorderBreachService border;
        private SaveGameService saves;
        private RunHistoryService history;
        private bool dirty, finalized;
        private float saveAfter;
        private WaveState lastWaveState;
        private RunRecord finished;
        public string MapId => FindFirstObjectByType<ShorelineDefense.Grid.MapRuntimeContext>()?.MapId ?? ShorelineDefense.Grid.MapCatalog.OriginalId;
        public bool CanSave => Ready && game.IsPlaying && waves.IsEndless && waves.State == WaveState.Preparation && waves.CompletedWaves == waves.PreparationWaveNumber - 1;
        private void Start()
        {
            builder = FindFirstObjectByType<BuildManager>(); waves = FindFirstObjectByType<WaveManager>();
            game = builder.Game; statistics = FindFirstObjectByType<RunStatistics>(); border = FindFirstObjectByType<BorderBreachService>();
            saves = new SaveGameService(LocalGameData.Files); history = new RunHistoryService(LocalGameData.Files);
            SettingsService.Instance.Apply();
            var data = Pending; Pending = null;
            try
            {
                if (data != null) Restore(data);
                else { RunId = Guid.NewGuid().ToString(); Duration = 0; }
                Ready = true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Continue restoration rejected: " + exception.Message);
                ProductMenuController.Notice = "The saved run could not be restored. Your save has been preserved.";
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); return;
            }
            lastWaveState = waves.State;
            waves.Changed += WaveChanged; waves.EventChanged += QueueSave; builder.StableActionCommitted += QueueSave; game.StateChanged += StateChanged;
            waves.PreparationEnding += BeforeWave;
            SaveNow();
        }
        private void Restore(RunSaveData data)
        {
            if (!SaveGameService.Valid(data)) throw new InvalidOperationException("Invalid snapshot.");
            if ((data.saveVersion == 1 ? ShorelineDefense.Grid.MapCatalog.OriginalId : data.mapId) != MapId) throw new InvalidOperationException("Wrong map for saved run.");
            var definitions = FindFirstObjectByType<BuildHud>().Defenses;
            if (!RunSaveCompatibility.Valid(data, definitions, builder.Battlefield.Definition, waves.EndlessSettings.eventSettings)) throw new InvalidOperationException("Save catalog or map is incompatible.");
            RunId = data.runId; Duration = data.duration; border.ConfigureThreshold(data.maxHomicides);
            builder.Economy.Restore(data.money, data.earned, data.spent, data.refunds, data.dismantled);
            waves.RestorePreparation(data);
            foreach (var item in data.structures) builder.RestoreDefense(FindDefinition(definitions, item.defenseId), item);
            // Wire OnEnable subscribes to grid occupancy and rebuilds each neighboring mask.
            statistics.Restore(data.statistics);
        }
        private static DefenseDefinition FindDefinition(IReadOnlyList<DefenseDefinition> catalog, string id)
        { foreach (var d in catalog) if (d.Id == id) return d; return null; }
        public RunSaveData Capture()
        {
            if (!CanSave) throw new InvalidOperationException("Progress is saved between waves.");
            var money = builder.Economy;
            var data = new RunSaveData { mapId = MapId, runId = RunId, duration = Duration, wave = waves.CurrentWaveNumber, completedWaves = waves.CompletedWaves,
                seed = waves.RunSeed, money = money.Money, earned = money.TotalEarned, spent = money.TotalSpent, refunds = money.RefundIncome,
                dismantled = money.StructuresDismantled, maxHomicides = border.MaxHomicides, statistics = statistics.Capture(), events = waves.Events.Capture() };
            foreach (var d in FindObjectsByType<DefenseBase>(FindObjectsSortMode.None))
                if (builder.CanDismantle(d)) data.structures.Add(new StructureSaveData { defenseId = d.Definition.Id, x = d.Anchor.x, y = d.Anchor.y,
                    level = d.Level, health = d.CurrentHealth, maxHealth = d.MaxHealth, constructionCost = d.ConstructionCost, investment = d.TotalInvestment,
                    mineArmingRemaining = d.GetComponent<LandMine>()?.ArmingRemaining ?? 0 });
            return data;
        }
        public void QueueSave() { if (!CanSave) return; dirty = true; saveAfter = Time.unscaledTime + .25f; }
        private void WaveChanged()
        { if (waves.State == lastWaveState) return; lastWaveState = waves.State; if (CanSave) SaveNow(); }
        private void BeforeWave() { if (CanSave) SaveNow(); }
        public bool SaveNow()
        {
            if (!CanSave) return false;
            dirty = false;
            bool ok = saves.Save(Capture(), out string error);
            Notify(ok ? "SAVED" : "SAVE FAILED — " + error);
            if (!ok) Debug.LogWarning("Run save failed: " + error);
            return ok;
        }
        private void Update()
        {
            if (!Ready) return;
            if (game.IsPlaying) Duration += Time.deltaTime;
            if (dirty && Time.unscaledTime >= saveAfter) SaveNow();
        }
        private void StateChanged(GameState state)
        {
            if (state != GameState.GameOver || finished != null) return;
            dirty = false;
            finished = new RunRecord { mapId = MapId, runId = RunId, endedUtc = DateTime.UtcNow.ToString("O"), wave = statistics.HighestWaveReached,
                kills = statistics.TotalKills, homicides = statistics.Homicides, bosses = statistics.BossesKilled,
                earned = statistics.TotalMoneyEarned, duration = Duration, peakEnemies = statistics.PeakActiveEnemies };
            FinalizeRun();
        }
        public bool FinalizeRun()
        {
            if (finalized || finished == null) return finalized;
            if (!history.Record(finished, out string error)) { saves.Delete(out _); Notify("RUN HISTORY SAVE FAILED — " + error); return false; }
            finalized = saves.Delete(out error);
            Notify(finalized ? "RUN RECORDED" : "Run recorded; active save cleanup failed: " + error);
            return finalized;
        }
        private void Notify(string text) { Feedback = text; FeedbackChanged?.Invoke(); }
        private void OnApplicationQuit() { if (CanSave) SaveNow(); else if (finished != null) FinalizeRun(); }
        private void OnApplicationPause(bool paused) { if (paused && CanSave) SaveNow(); }
        private void OnDestroy()
        {
            if (waves == null) return;
            waves.Changed -= WaveChanged; waves.EventChanged -= QueueSave;
            waves.PreparationEnding -= BeforeWave;
            builder.StableActionCommitted -= QueueSave; game.StateChanged -= StateChanged;
        }
    }
}
