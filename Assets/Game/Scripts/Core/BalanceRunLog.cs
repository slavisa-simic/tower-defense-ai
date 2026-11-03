#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Core
{
    /// <summary>Opt-in local development observer. Never changes combat, saves, prices or profile data.</summary>
    public sealed class BalanceRunLog : IDisposable
    {
        private readonly RunSession session;
        private readonly BuildManager builder;
        private readonly WaveManager waves;
        private readonly RunStatistics statistics;
        private readonly EnemyRegistry enemies;
        private readonly string path;
        private readonly Dictionary<DefenseBase, Action[]> watched = new Dictionary<DefenseBase, Action[]>();
        private readonly Dictionary<EnemyController, double> bosses = new Dictionary<EnemyController, double>();
        private double clock;
        private long earned, spent, kills, breaches, homicides, repair, upgrade;
        private int spawned, destroyed, peak, writtenWave, pendingWave;
        private readonly List<string> bossTimes = new List<string>(2);
        private bool failed;
        private bool pendingFlush;

        public BalanceRunLog(RunSession run, BuildManager build, WaveManager manager, RunStatistics stats, EnemyRegistry registry)
        {
            session = run; builder = build; waves = manager; statistics = stats; enemies = registry;
            string folder = Path.Combine(LocalGameData.Files.Root, "BalanceLogs");
            path = Path.Combine(folder, session.RunId + ".csv");
            earned = builder.Economy.TotalEarned; spent = builder.Economy.TotalSpent;
            kills = stats.TotalKills; breaches = stats.TotalBreaches; homicides = stats.Homicides;
            try { Directory.CreateDirectory(folder); File.WriteAllText(path, "map,wave,result,spawned,killed,breaches,homicides,earned,spent,repair,upgrade,destroyed,peak,boss_ttk_seconds\n"); }
            catch (Exception e) { Fail(e); }
            builder.DefensePlaced += Observe; builder.ManagementCommitted += Management;
            enemies.EnemyRegistered += Registered; enemies.EnemyKilled += Killed; enemies.EnemyRemoved += Removed;
            waves.Changed += WaveChanged;
            foreach (var d in UnityEngine.Object.FindObjectsByType<DefenseBase>(FindObjectsSortMode.None)) Observe(d);
            foreach (var e in enemies.Active) Registered(e);
        }
        public void Tick(float dt)
        {
            if (builder.Game.IsPlaying) clock += dt;
            peak = Math.Max(peak, enemies.Count);
            if (pendingFlush) Flush("WaveComplete");
        }
        private void Registered(EnemyController e) { spawned++; if (e.Definition is BossDefinition) bosses[e] = clock; }
        private void Killed(EnemyController e) { if (bosses.TryGetValue(e, out double start)) bossTimes.Add((clock - start).ToString("0.00", CultureInfo.InvariantCulture)); }
        private void Removed(EnemyController e) => bosses.Remove(e);
        private void Management(DefenseBase d, StructureManagementAction action, int cost)
        { if (action == StructureManagementAction.Repair) repair += cost; else upgrade += cost; }
        private void Observe(DefenseBase d)
        {
            if (d == null || d.Owner != builder || watched.ContainsKey(d)) return;
            Action died = () => { if (!(d.GetComponent<LandMine>()?.Exploded ?? false)) destroyed++; Forget(d); };
            Action changed = () => { if (d.Dismantled) Forget(d); };
            watched.Add(d, new[] { died, changed }); d.Died += died; d.Changed += changed;
        }
        private void Forget(DefenseBase d)
        { if (!watched.TryGetValue(d, out var handlers)) return; d.Died -= handlers[0]; d.Changed -= handlers[1]; watched.Remove(d); }
        private void WaveChanged()
        {
            // Explicit simulation clocks can advance Preparation before the next Update.
            // Commit the completed wave before that transition's purchases can occur.
            if (pendingFlush && waves.State != WaveState.WaveComplete) Flush("WaveComplete");
            if (waves.State != WaveState.WaveComplete && waves.State != WaveState.GameOver ||
                writtenWave == waves.CurrentWaveNumber) return;
            // Wave completion notification precedes the completion payment; the run's next
            // Update flushes after that transaction, including the real completion reward.
            if (waves.State == WaveState.WaveComplete) { pendingFlush = true; pendingWave = waves.CurrentWaveNumber; }
            if (waves.State == WaveState.GameOver) Flush("GameOver");
        }
        public void Flush(string result = "Checkpoint")
        {
            int wave = pendingFlush ? pendingWave : waves.CurrentWaveNumber;
            pendingFlush = false;
            if (writtenWave == wave || wave < 1) return;
            writtenWave = wave;
            if (!failed) try
            {
                File.AppendAllText(path, string.Join(",", session.MapId, writtenWave, result, spawned,
                    statistics.TotalKills-kills, statistics.TotalBreaches-breaches, statistics.Homicides-homicides,
                    builder.Economy.TotalEarned-earned, builder.Economy.TotalSpent-spent, repair, upgrade,
                    destroyed, peak, string.Join(";", bossTimes)) + "\n");
            } catch (Exception e) { Fail(e); }
            earned = builder.Economy.TotalEarned; spent = builder.Economy.TotalSpent;
            kills = statistics.TotalKills; breaches = statistics.TotalBreaches; homicides = statistics.Homicides;
            spawned = destroyed = peak = 0; repair = upgrade = 0; bossTimes.Clear();
        }
        private void Fail(Exception e) { failed = true; Debug.LogWarning("Balance log unavailable: " + e.Message); }
        public void Dispose()
        {
            Flush(waves.State.ToString()); builder.DefensePlaced -= Observe; builder.ManagementCommitted -= Management;
            enemies.EnemyRegistered -= Registered; enemies.EnemyKilled -= Killed; enemies.EnemyRemoved -= Removed; waves.Changed -= WaveChanged;
            foreach (var pair in watched) if (pair.Key != null) { pair.Key.Died -= pair.Value[0]; pair.Key.Changed -= pair.Value[1]; }
            watched.Clear(); bosses.Clear();
        }
    }
}
#endif
