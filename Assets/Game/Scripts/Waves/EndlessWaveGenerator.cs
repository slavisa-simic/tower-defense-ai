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
        public int Budget(int wave) => Budget(wave, 0);
        public int Budget(int wave, int quantityOverride)
        {
            double age = Math.Max(0, (double)wave - 1);
            double budget = Math.Max(1, settings.baseBudget) + age * Safe(settings.linearGrowth, 3) + age * age * Safe(settings.nonlinearGrowth, .015f);
            return (int)Math.Min(100000000, Math.Max(1, (int)Math.Min(100000000, budget)) * (long)Mathf.Clamp(quantityOverride > 0 ? quantityOverride : settings.enemyQuantityMultiplier, 1, 20));
        }
        public WaveDefinition Generate(int number, EventModifiers modifiers = null, float spitterWeight = 1, float burrowerWeight = 1, int quantityOverride = 0)
        {
            if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
            var available = new List<EnemyController>();
            foreach (var prefab in settings.enemies)
                if (prefab != null && prefab.Definition != null && prefab.Definition.UnlockWave <= number) available.Add(prefab);
            if (available.Count == 0) throw new InvalidOperationException("No unlocked valid endless enemy prefab.");
            var random = new System.Random(unchecked(seed * 397 ^ number));
            int quantity = Mathf.Clamp(quantityOverride > 0 ? quantityOverride : settings.enemyQuantityMultiplier, 1, 20);
            int remaining = Budget(number, quantity) / quantity, cap = Mathf.Clamp(settings.maximumEnemiesPerWave / quantity, 1, 300);
            var groups = new List<WaveEnemyGroup>();
            int eventQuantitySoFar = 0;
            int baseQuantitySoFar = 0;
            // Small shuffled groups vary both boat cargo and successive landing pressure.
            for (int count = 0; count < cap; count++)
            {
                int totalWeight = 0;
                foreach (var p in available) if (p.Definition.DifficultyCost <= remaining) totalWeight += EffectiveWeight(p.Definition.Kind, number, modifiers, spitterWeight, burrowerWeight);
                if (totalWeight == 0) break;
                int roll = random.Next(totalWeight); EnemyController selected = null;
                foreach (var p in available)
                {
                    if (p.Definition.DifficultyCost > remaining) continue;
                    roll -= EffectiveWeight(p.Definition.Kind, number, modifiers, spitterWeight, burrowerWeight); if (roll < 0) { selected = p; break; }
                }
                remaining -= selected.Definition.DifficultyCost;
                baseQuantitySoFar += quantity * (selected.Definition.Kind == EnemyKind.Broodling ? 2 : 1);
                baseQuantitySoFar = Mathf.Min(baseQuantitySoFar, Mathf.Clamp(settings.maximumEnemiesPerWave, 1, 6000));
                int expanded = Mathf.Max(eventQuantitySoFar + 1, Mathf.RoundToInt(baseQuantitySoFar * (modifiers?.quantity ?? 1)));
                groups.Add(new WaveEnemyGroup(selected, expanded - eventQuantitySoFar)); eventQuantitySoFar = expanded;
                if (baseQuantitySoFar >= settings.maximumEnemiesPerWave) break;
            }
            if (groups.Count == 0) throw new InvalidOperationException("Wave budget cannot afford an unlocked enemy.");
            double age = Math.Max(0, (double)number - Math.Max(1, settings.scalingStartWave));
            float health = (float)Math.Min(Math.Max(1, Safe(settings.maximumHealthMultiplier, 20)), 1 + age * Safe(settings.healthGrowth, .0125f));
            float speed = (float)Math.Min(Math.Max(1, Safe(settings.maximumSpeedMultiplier, 1.6f)), 1 + age * Safe(settings.speedGrowth, .002f));
            int reward = settings.CompletionReward(number);
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
                    escortGroups.Add(new WaveEnemyGroup(escort.Prefab, escortCount)); encounterCost += Mathf.CeilToInt(escortCount * escort.Prefab.Definition.WaveCostHalfUnits / 2f);
                }
                groups.InsertRange(Math.Min(7, groups.Count), escortGroups);
                wave.Encounter = encounter; wave.EncounterCost = encounterCost;
            }
            wave.InitializeGenerated(number, groups.ToArray(), reward, health, speed); return wave;
        }
        private static int Weight(EnemyKind kind, int wave) {
            int themed = wave % 7;
            // Spend more of the existing late-wave budget on complementary threats.
            // The shift saturates at wave 60; base HP and formation quantity are unchanged.
            float late = Mathf.Clamp01((wave - 20f) / 40f);
            switch(kind) {
                case EnemyKind.Crawler: return 70 - Mathf.RoundToInt(20 * late);
                case EnemyKind.Skitter: return themed == 5 ? 38 : 25;
                case EnemyKind.Bulwark: return (themed == 3 ? 20 : Math.Min(14, 5 + wave / 5)) + Mathf.RoundToInt(25 * late);
                case EnemyKind.Spitter: return (themed == 2 ? 20 : 8) + Mathf.RoundToInt(12 * late);
                case EnemyKind.Broodling: return themed == 5 ? 50 : 25;
                case EnemyKind.Burrower: return (themed == 2 ? 16 : 6) + Mathf.RoundToInt(8 * late);
                default: return 0;
            }
        }
        private static int EffectiveWeight(EnemyKind kind, int wave, EventModifiers modifiers, float spitterWeight, float burrowerWeight) =>
            Mathf.RoundToInt(Weight(kind, wave) * Mathf.Clamp(modifiers?.Weight(kind) ?? 1, 0, 20) *
                Mathf.Clamp(kind == EnemyKind.Spitter ? spitterWeight : kind == EnemyKind.Burrower ? burrowerWeight : 1, 1, 4));
        internal static float Safe(float value, float fallback) => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(0, value);
    }
}
