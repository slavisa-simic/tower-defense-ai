using System;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using UnityEngine;

namespace ShorelineDefense.Core
{
    /// <summary>Run-local counters. No persistence or gameplay ownership in presentation.</summary>
    public sealed class RunStatistics : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry enemies;
        [SerializeField] private WaveManager waves;
        [SerializeField] private EconomyManager economy;
        private readonly long[] kills = new long[9];
        public long Homicides { get; private set; }
        public long TotalBreaches { get; private set; }
        public long TotalKills { get { long total = 0; foreach (long count in kills) total = SaturatingAdd(total, count); return total; } }
        public long BossesKilled => SaturatingAdd(kills[7], kills[8]);
        public long OsamaKills => kills[7];
        public long SomaliWarlordKills => kills[8];
        public long BossesBreached { get; private set; }
        public void RecordBossBreach() => BossesBreached = SaturatingAdd(BossesBreached, 1);
        public int PeakActiveEnemies { get; private set; }
        public int PeakConcurrentBoats { get; private set; }
        private double pressureSum, frameTimeSum;
        private long pressureSamples, waveIncomeStart;
        private int incomeWave;
        public double AverageActiveEnemies => pressureSamples == 0 ? 0 : pressureSum / pressureSamples;
        public double AverageFrameTimeMs => pressureSamples == 0 ? 0 : frameTimeSum / pressureSamples;
        public long CurrentWaveIncome => economy == null ? 0 : Math.Max(0, economy.TotalEarned - waveIncomeStart);
        public long LastCompletedWaveIncome { get; private set; }
        public void SamplePressure(int active, float frameSeconds) { pressureSum += active; frameTimeSum += frameSeconds * 1000; pressureSamples++; }
        public void RecordPressure(int active, int boats) { PeakActiveEnemies = Math.Max(PeakActiveEnemies, active); PeakConcurrentBoats = Math.Max(PeakConcurrentBoats, boats); }
        public long BasicKills => kills[0];
        public long RunnerKills => kills[1];
        public long BruteKills => kills[2];
        public int CurrentWave => waves != null ? waves.CurrentWaveNumber : 0;
        public long EventsExperienced => waves?.Events?.EventsExperienced ?? 0;
        public string CurrentEvent => waves?.Events?.Current?.Definition.id;
        public string MostRecentEvent => waves?.Events?.MostRecentEvent;
        public int EventCount(string id) => waves?.Events?.Count(id) ?? 0;
        public int HighestWaveReached { get; private set; }
        public int StructuresBuilt { get; private set; }
        public int StructuresUpgraded { get; private set; }
        public int StructuresRepaired { get; private set; }
        public void RecordBuild() => StructuresBuilt++;
        public void RecordUpgrade() => StructuresUpgraded++;
        public void RecordRepair() => StructuresRepaired++;
        public Persistence.StatisticsSaveData Capture() => new Persistence.StatisticsSaveData {
            kills = (long[])kills.Clone(), homicides = Homicides, breaches = TotalBreaches, bossesBreached = BossesBreached,
            highestWave = HighestWaveReached, peakEnemies = PeakActiveEnemies, peakBoats = PeakConcurrentBoats,
            built = StructuresBuilt, upgraded = StructuresUpgraded, repaired = StructuresRepaired };
        public void Restore(Persistence.StatisticsSaveData data)
        {
            Array.Copy(data.kills, kills, kills.Length); Homicides = data.homicides; TotalBreaches = data.breaches; BossesBreached = data.bossesBreached;
            HighestWaveReached = data.highestWave; PeakActiveEnemies = data.peakEnemies; PeakConcurrentBoats = data.peakBoats;
            StructuresBuilt = data.built; StructuresUpgraded = data.upgraded; StructuresRepaired = data.repaired;
            OnHomicidesChanged?.Invoke(Homicides);
        }
        public long TotalMoneyEarned => economy != null ? economy.TotalEarned : 0;
        public long TotalMoneySpent => economy != null ? economy.TotalSpent : 0;
        public event Action<long> OnHomicidesChanged;
        public void Configure(EnemyRegistry registry, WaveManager manager, EconomyManager money)
        { Unsubscribe(); enemies = registry; waves = manager; economy = money; if (isActiveAndEnabled) Subscribe(); }
        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();
        private void Subscribe() { if (enemies != null) enemies.EnemyKilled += OnKilled; if (waves != null) waves.Changed += OnWave; }
        private void Unsubscribe() { if (enemies != null) enemies.EnemyKilled -= OnKilled; if (waves != null) waves.Changed -= OnWave; }
        private void OnKilled(EnemyController enemy)
        { int index = (int)enemy.Definition.Kind; if (index >= 0 && index < kills.Length) kills[index] = SaturatingAdd(kills[index], 1); }
        private void OnWave()
        {
            if (waves.State == WaveState.Spawning || waves.State == WaveState.Combat) HighestWaveReached = Math.Max(HighestWaveReached, CurrentWave);
            if (waves.State == WaveState.Spawning && incomeWave != CurrentWave) { incomeWave = CurrentWave; waveIncomeStart = economy != null ? economy.TotalEarned : 0; }
            if (waves.State == WaveState.Preparation && incomeWave != CurrentWave && incomeWave > 0) LastCompletedWaveIncome = CurrentWaveIncome;
        }
        public void RecordBreach(int penalty)
        {
            TotalBreaches = SaturatingAdd(TotalBreaches, 1); Homicides = SaturatingAdd(Homicides, Math.Max(0, penalty));
            if (OnHomicidesChanged == null) return;
            foreach (Action<long> observer in OnHomicidesChanged.GetInvocationList())
            { try { observer(Homicides); } catch (Exception exception) { Debug.LogException(exception, this); } }
        }
        private static long SaturatingAdd(long value, long amount) => value > long.MaxValue - amount ? long.MaxValue : value + amount;
        public string Report => $"BORDER DEFENSE COLLAPSED\nWAVE REACHED {HighestWaveReached}\nHOMICIDES {Homicides:N0}    TOTAL KILLS {TotalKills:N0}\nBASIC {BasicKills:N0}    RUNNERS {RunnerKills:N0}    BRUTES {BruteKills:N0}\nTOTAL MONEY EARNED {TotalMoneyEarned:N0} RSD";
    }
}
