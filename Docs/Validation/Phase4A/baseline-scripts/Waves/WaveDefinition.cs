using System;
using System.Collections.Generic;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    [Serializable]
    public sealed class WaveEnemyGroup
    {
        [SerializeField] private EnemyController prefab;
        [SerializeField, Min(1)] private int count = 10;
        [SerializeField, Min(0.05f)] private float spawnInterval = 0.75f;
        [SerializeField, Min(0)] private float delayBeforeGroup;
        public EnemyController Prefab => prefab;
        public int Count => count;
        public float SpawnInterval => spawnInterval;
        public float DelayBeforeGroup => delayBeforeGroup;
        public bool IsValid => prefab != null && prefab.Definition != null && count > 0 &&
            spawnInterval >= 0.05f && !float.IsInfinity(spawnInterval) && delayBeforeGroup >= 0 && !float.IsInfinity(delayBeforeGroup);
    }

    [CreateAssetMenu(menuName = "Shoreline Defense/Wave Definition")]
    public sealed class WaveDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int waveNumber = 1;
        [SerializeField] private WaveEnemyGroup[] groups = Array.Empty<WaveEnemyGroup>();
        [SerializeField, Min(0)] private int completionReward = 100;
        public int WaveNumber => waveNumber;
        public int CompletionReward => completionReward;
        public IReadOnlyList<WaveEnemyGroup> Groups => groups;
        public int ScheduledCount
        {
            get
            {
                int total = 0;
                if (groups != null) foreach (var group in groups) if (group != null) total = checked(total + group.Count);
                return total;
            }
        }
        public bool IsValid
        {
            get
            {
                if (waveNumber < 1 || completionReward < 0 || groups == null || groups.Length == 0) return false;
                long total = 0;
                foreach (var group in groups)
                {
                    if (group == null || !group.IsValid) return false;
                    total += group.Count;
                }
                return total <= int.MaxValue;
            }
        }
    }
}
