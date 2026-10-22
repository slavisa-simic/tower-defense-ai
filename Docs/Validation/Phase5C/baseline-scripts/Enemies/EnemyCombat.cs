using ShorelineDefense.Combat;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class EnemyCombat : MonoBehaviour
    {
        private IDamageable target;
        private int damage;
        private float interval;
        private float elapsed;
        public int AttacksPerformed { get; private set; }

        public void Initialize(IDamageable damageTarget, EnemyDefinition definition)
        {
            target = damageTarget;
            damage = definition.AttackDamage;
            interval = definition.AttackInterval;
            elapsed = 0;
            AttacksPerformed = 0;
        }

        public void Tick(float deltaTime, bool inRange)
        {
            if (target == null || !target.IsAlive || deltaTime <= 0) return;
            if (!inRange) { elapsed = 0; return; }
            elapsed += deltaTime;
            while (elapsed >= interval && target.IsAlive)
            {
                elapsed -= interval;
                AttacksPerformed++;
                target.TakeDamage(damage);
            }
        }
    }
}
