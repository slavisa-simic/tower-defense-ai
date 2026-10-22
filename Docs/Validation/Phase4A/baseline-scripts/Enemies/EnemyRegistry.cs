using System;
using System.Collections.Generic;
using UnityEngine;
using ShorelineDefense.Defenses;

namespace ShorelineDefense.Enemies
{
    /// <summary>Scene-owned live targets. Registration order resolves equal-distance ties.</summary>
    public sealed class EnemyRegistry : MonoBehaviour
    {
        private readonly List<EnemyController> enemies = new List<EnemyController>(128);
        public int Count => enemies.Count;
        public IReadOnlyList<EnemyController> Active => enemies;
        public event Action<EnemyController> EnemyKilled;
        public event Action<EnemyController> EnemyRemoved;
        public void Register(EnemyController enemy)
        {
            if (enemy != null && !enemies.Contains(enemy)) enemies.Add(enemy);
        }
        public void Unregister(EnemyController enemy)
        {
            if (enemies.Remove(enemy)) EnemyRemoved?.Invoke(enemy);
        }
        public void NotifyKilled(EnemyController enemy)
        {
            if (!enemies.Remove(enemy)) return;
            EnemyKilled?.Invoke(enemy);
            EnemyRemoved?.Invoke(enemy);
        }
        public EnemyController FindClosest(Vector3 position, float range) => FindTarget(position, range, TargetingMode.Closest);
        public EnemyController FindTarget(Vector3 position, float range, TargetingMode mode)
        {
            EnemyController best = null;
            float bestScore = float.PositiveInfinity;
            float rangeSquared = range * range;
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsTargetable) continue;
                float candidate = ((Vector2)(enemy.transform.position - position)).sqrMagnitude;
                if (candidate > rangeSquared) continue;
                float score = mode == TargetingMode.ClosestToBase ? enemy.Movement.RemainingPathDistance : candidate;
                if (score >= bestScore) continue;
                bestScore = score;
                best = enemy;
            }
            return best;
        }

        // Caller-owned buffer permits death to mutate the registry safely during AOE resolution.
        public void CollectInRadius(Vector3 position, float radius, List<EnemyController> result)
        {
            result.Clear();
            float squared = radius * radius;
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy != null && enemy.IsTargetable &&
                    ((Vector2)(enemy.transform.position - position)).sqrMagnitude <= squared) result.Add(enemy);
            }
        }
    }
}
