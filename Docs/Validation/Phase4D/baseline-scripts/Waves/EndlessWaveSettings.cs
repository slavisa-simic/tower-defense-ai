using System;
using UnityEngine;
using ShorelineDefense.Enemies;
namespace ShorelineDefense.Waves
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Endless Wave Settings")]
    public sealed class EndlessWaveSettings : ScriptableObject
    {
        public EnemyController[] enemies = Array.Empty<EnemyController>();
        [Min(1)] public int baseBudget = 16;
        [Min(0)] public float linearGrowth = 8;
        [Min(0)] public float nonlinearGrowth = .08f;
        [Min(1)] public int maximumEnemiesPerWave = 240;
        [Min(1)] public int maximumActiveEnemies = 200;
        public int seed = 1977;
        public bool randomizeSeed = true;
        [Min(0)] public int baseReward = 100;
        [Min(0)] public int rewardGrowth = 5;
        [Min(1)] public int scalingStartWave = 20;
        [Min(0)] public float healthGrowth = .0125f;
        [Min(0)] public float speedGrowth = .002f;
        [Min(1)] public float maximumHealthMultiplier = 20;
        [Min(1)] public float maximumSpeedMultiplier = 1.6f;
        [Min(.05f)] public float earlyArrivalInterval = 2.2f;
        [Min(.05f)] public float minimumArrivalInterval = .65f;
        [Min(0)] public float arrivalGrowth = .045f;
        [Min(1)] public int wavesPerBoatConcurrency = 4;
        [Range(1,12)] public int maximumConcurrentBoats = 8;
        [Min(1)] public int initialConcurrentBoats = 2;
        public BossEncounterDefinition bossEncounters;
        public float ArrivalInterval(int wave) => Mathf.Max(.05f, Mathf.Max(minimumArrivalInterval, earlyArrivalInterval / (1 + Mathf.Max(0, arrivalGrowth) * (Mathf.Max(1, wave) - 1))));
        public int BoatConcurrency(int wave) => Mathf.Clamp((int)Math.Min(12L, Math.Max(1, initialConcurrentBoats) + ((long)Math.Max(1, wave) - 1) / Math.Max(1, wavesPerBoatConcurrency)), 1, Mathf.Clamp(maximumConcurrentBoats, 1, 12));
    }
}

