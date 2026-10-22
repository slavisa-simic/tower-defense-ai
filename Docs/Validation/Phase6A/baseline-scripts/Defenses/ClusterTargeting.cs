using System;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    /// <summary>Bounded O(candidates * N) query over one caller-owned snapshot; registration order breaks final ties.</summary>
    public static class ClusterTargeting
    {
        internal struct Sample
        {
            public Vector3 position, predicted;
            public float progress;
            public int health;
            public bool inRange;
        }
        public sealed class Workspace
        {
            internal Sample[] samples = new Sample[300];
        }
        public static bool TrySelect(EnemyRegistry registry, Vector3 origin, float minimum, float maximum,
            float radius, float leadTime, int candidateLimit, Workspace workspace, out Vector3 impact)
        {
            impact = default;
            var enemies = registry.Active;
            if (workspace.samples.Length < enemies.Count)
                Array.Resize(ref workspace.samples, Mathf.NextPowerOfTwo(enemies.Count));
            var samples = workspace.samples;
            int count = 0, valid = 0, limit = Mathf.Clamp(candidateLimit, 1, 32);
            float min2 = minimum * minimum, max2 = maximum * maximum, radius2 = radius * radius;
            // Unity/native reads occur once per enemy, never inside the candidate score loop.
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null || !e.IsTargetable) continue;
                Vector3 position = e.transform.position;
                var movement = e.Movement;
                float progress = movement.RemainingPathDistance;
                Vector3 predicted = position + (movement.HasPath && !movement.ReachedDestination ?
                    movement.FacingDirection * Mathf.Min(movement.CurrentSpeed * leadTime, progress) : Vector3.zero);
                float distance = ((Vector2)(position - origin)).sqrMagnitude;
                bool inRange = distance >= min2 && distance <= max2;
                samples[count++] = new Sample { position = position, predicted = predicted,
                    progress = progress, health = e.Health.CurrentHealth, inRange = inRange };
                if (inRange) valid++;
            }
            if (valid == 0) return false;
            int stride = Mathf.Max(1, Mathf.CeilToInt(valid / (float)limit));
            int index = 0, tested = 0, bestCount = -1;
            long bestUseful = -1;
            float bestProgress = float.PositiveInfinity;
            for (int i = 0; i < count && tested < limit; i++)
            {
                var sample = samples[i];
                if (!sample.inRange) continue;
                if (index++ % stride != 0) continue;
                tested++;
                Vector3 point = sample.predicted;
                float distance = ((Vector2)(point - origin)).sqrMagnitude;
                if (distance < min2 || distance > max2) point = sample.position;
                int nearby = 0; long useful = 0;
                for (int j = 0; j < count; j++)
                {
                    if (((Vector2)(samples[j].predicted - point)).sqrMagnitude > radius2) continue;
                    nearby++; useful += samples[j].health;
                }
                if (nearby < bestCount || nearby == bestCount && (useful < bestUseful ||
                    useful == bestUseful && sample.progress >= bestProgress)) continue;
                bestCount = nearby; bestUseful = useful; bestProgress = sample.progress; impact = point;
            }
            return bestCount >= 0;
        }
    }
}
