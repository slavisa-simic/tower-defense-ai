using System.Collections.Generic;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    public static class AreaDamage
    {
        public static int Apply(EnemyRegistry registry, Vector3 point, float radius, int damage, List<EnemyController> buffer)
        {
            if (registry == null || radius <= 0 || damage <= 0) return 0;
            registry.CollectInRadius(point, radius, buffer);
            int hits = 0;
            for (int i = 0; i < buffer.Count; i++)
                if (buffer[i] != null && buffer[i].IsTargetable)
                { buffer[i].Health.TakeDefenseDamage(damage); hits++; }
            buffer.Clear();
            return hits;
        }
    }
}
