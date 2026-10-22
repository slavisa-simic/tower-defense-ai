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
        [Range(1,32)] public int maximumConcurrentBoats = 8;
        [Min(1)] public int initialConcurrentBoats = 2;
        public BossEncounterDefinition bossEncounters;
        public EndlessEventSettings eventSettings;
        [Header("Assault difficulty (legacy formations are the reference)")]
        [Range(1, 20)] public int enemyQuantityMultiplier = 1;
        [Min(1)] public float globalEnemyHealthMultiplier = 1;
        [Min(1)] public float bossHealthMultiplier = 1;
        public bool pressureDirectorEnabled;
        [Min(1)] public int earlyPressureTarget = 60;
        [Min(1)] public int maximumPressureTarget = 320;
        [Min(0)] public float pressureGrowthPerWave = 6;
        [Range(.05f, .4f)] public float pressureHysteresis = .2f;
        [Range(.1f, .25f)] public float directorEvaluationInterval = .2f;
        [Min(.05f)] public float acceleratedArrivalInterval = .15f;
        [Min(0)] public float firstAttackDelay = .25f;
        [Min(1)] public int boatCapacity = 12;
        [Min(.1f)] public float boatSpeed = 9;
        [Min(.05f)] public float unloadInterval = .07f;
        [Min(.1f)] public float disembarkSeparation = .4f;
        [Min(0)] public float departureDelay = .15f;
        public bool assaultSurgesEnabled = true;
        [Min(1)] public float surgePeriod = 24;
        [Min(1)] public float surgeDuration = 7;
        [Range(0, 8)] public int surgeExtraBoats = 3;
        public bool showPressureDebug;
        public int PressureTarget(int wave) => (int)Math.Min(Mathf.Clamp(maximumPressureTarget, 1, 400),
            Math.Max(1, earlyPressureTarget) + Math.Max(0, (double)wave - 1) * EndlessWaveGenerator.Safe(pressureGrowthPerWave, 6));
        // Boss tuning replaces the ordinary global factor, rather than multiplying it again.
        public float RuntimeHealthMultiplier(float waveHealth, bool boss) => Mathf.Clamp(waveHealth, 1, 100) *
            Mathf.Clamp(EndlessWaveGenerator.Safe(boss ? bossHealthMultiplier : globalEnemyHealthMultiplier, 1), 1, 100);
        public float ArrivalInterval(int wave) => Mathf.Max(.05f, Mathf.Max(minimumArrivalInterval, earlyArrivalInterval / (1 + Mathf.Max(0, arrivalGrowth) * (Mathf.Max(1, wave) - 1))));
        public int BoatConcurrency(int wave) => Mathf.Clamp((int)Math.Min(32L, Math.Max(1, initialConcurrentBoats) + ((long)Math.Max(1, wave) - 1) / Math.Max(1, wavesPerBoatConcurrency)), 1, Mathf.Clamp(maximumConcurrentBoats, 1, 32));
    }
}

