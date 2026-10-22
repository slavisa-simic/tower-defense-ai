using System;
using System.Collections.Generic;
using ShorelineDefense.Enemies;
using UnityEngine;
namespace ShorelineDefense.Waves
{
    /// <summary>Pure seeded composition rules. One bounded transient definition is owned by WaveManager.</summary>
    public sealed class EndlessWaveGenerator
    {
        private readonly EndlessWaveSettings settings;
        private readonly int seed;
        public EndlessWaveGenerator(EndlessWaveSettings tuning, int runSeed) { settings = tuning; seed = runSeed; }
        public int Budget(int wave)
        {
            double age = Math.Max(0, (double)wave - 1);
            double budget = Math.Max(1, settings.baseBudget) + age * Safe(settings.linearGrowth, 3) + age * age * Safe(settings.nonlinearGrowth, .015f);
            return (int)Math.Min(100000000, Math.Max(1, (int)Math.Min(100000000, budget)) * (long)Mathf.Clamp(settings.enemyQuantityMultiplier, 1, 20));
        }
        public WaveDefinition Generate(int number)
        {
            if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
            var available = new List<EnemyController>();
            foreach (var prefab in settings.enemies)
                if (prefab != null && prefab.Definition != null && prefab.Definition.UnlockWave <= number) available.Add(prefab);
            if (available.Count == 0) throw new InvalidOperationException("No unlocked valid endless enemy prefab.");
            var random = new System.Random(unchecked(seed * 397 ^ number));
            int quantity = Mathf.Clamp(settings.enemyQuantityMultiplier, 1, 20);
            int remaining = Budget(number) / quantity, cap = Mathf.Clamp(settings.maximumEnemiesPerWave / quantity, 1, 300);
            var groups = new List<WaveEnemyGroup>();
            // Small shuffled groups vary both boat cargo and successive landing pressure.
            for (int count = 0; count < cap; count++)
            {
                int totalWeight = 0;
                foreach (var p in available) if (p.Definition.DifficultyCost <= remaining) totalWeight += Weight(p.Definition.Kind, number);
                if (totalWeight == 0) break;
                int roll = random.Next(totalWeight); EnemyController selected = null;
                foreach (var p in available)
                {
                    if (p.Definition.DifficultyCost > remaining) continue;
                    roll -= Weight(p.Definition.Kind, number); if (roll < 0) { selected = p; break; }
                }
                remaining -= selected.Definition.DifficultyCost;
                groups.Add(new WaveEnemyGroup(selected, quantity));
            }
            if (groups.Count == 0) throw new InvalidOperationException("Wave budget cannot afford an unlocked enemy.");
            double age = Math.Max(0, (double)number - Math.Max(1, settings.scalingStartWave));
            float health = (float)Math.Min(Math.Max(1, Safe(settings.maximumHealthMultiplier, 20)), 1 + age * Safe(settings.healthGrowth, .0125f));
            float speed = (float)Math.Min(Math.Max(1, Safe(settings.maximumSpeedMultiplier, 1.6f)), 1 + age * Safe(settings.speedGrowth, .002f));
            int reward = (int)Math.Min(int.MaxValue, Math.Max(0, settings.baseReward) + ((long)number - 1) * Math.Max(0, settings.rewardGrowth));
            var wave = ScriptableObject.CreateInstance<WaveDefinition>(); wave.hideFlags = HideFlags.DontSave;
            var boss = settings.bossEncounters != null ? settings.bossEncounters.ForWave(number) : null;
            if (boss != null && boss.Definition is BossDefinition encounter) {
                // Additive encounter budget: never displaces the normal wave assault.
                var escortGroups = new List<WaveEnemyGroup>();
                escortGroups.Add(new WaveEnemyGroup(boss, 1));
                int encounterCost = boss.Definition.DifficultyCost;
                foreach (var escort in encounter.Escorts) {
                    if (escort == null || !escort.IsValid) continue;
                    int escortCount = escort.Count * quantity;
                    escortGroups.Add(new WaveEnemyGroup(escort.Prefab, escortCount)); encounterCost += escortCount * escort.Prefab.Definition.DifficultyCost;
                }
                groups.InsertRange(Math.Min(7, groups.Count), escortGroups);
                wave.Encounter = encounter; wave.EncounterCost = encounterCost;
            }
            wave.InitializeGenerated(number, groups.ToArray(), reward, health, speed); return wave;
        }
        private static int Weight(EnemyKind kind, int wave) {
            int themed = wave % 7;
            switch(kind) {
                case EnemyKind.Basic: return 70;
                case EnemyKind.Runner: return 30;
                case EnemyKind.Brute: return Math.Min(14, 5 + wave / 5);
                case EnemyKind.IsisBreacher: return themed == 3 ? 30 : 10;
                case EnemyKind.IsisHeavy: return themed == 3 ? 12 : 4;
                case EnemyKind.PirateRunner: return themed == 5 ? 28 : 12;
                case EnemyKind.PirateRaider: return themed == 5 ? 20 : 8;
                default: return 0;
            }
        }
        internal static float Safe(float value, float fallback) => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(0, value);
    }
}
