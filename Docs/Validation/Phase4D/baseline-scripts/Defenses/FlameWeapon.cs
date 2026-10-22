using System.Collections.Generic;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class FlameWeapon : MonoBehaviour
    {
        private DefenseBase defense;
        private float timer;
        private readonly List<EnemyController> victims = new List<EnemyController>(32);
        public Vector2 AimDirection { get; private set; } = Vector2.left;
        public bool Firing { get; private set; }
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void Update() => Tick(Time.deltaTime);
        public static bool InCone(Vector2 offset, Vector2 direction, float range, float angle) =>
            offset.sqrMagnitude <= range * range && (offset.sqrMagnitude < .0001f ||
            Vector2.Dot(offset.normalized, direction.normalized) >= Mathf.Cos(angle * .5f * Mathf.Deg2Rad));
        public void Tick(float dt)
        {
            if (dt <= 0 || !defense.IsAlive || !defense.Game.IsPlaying) { Firing = false; return; }
            timer -= dt;
            if (timer > 0) return;
            var data = defense.Definition.HeavyWeapon;
            timer = data.damageTick;
            var target = defense.Enemies.FindTarget(transform.position, defense.Range, defense.Definition.Targeting);
            Firing = target != null;
            if (!Firing) { AimDirection = Vector2.left; return; }
            AimDirection = ((Vector2)(target.transform.position - transform.position)).normalized;
            defense.Enemies.CollectInRadius(transform.position, defense.Range, victims);
            for (int i = 0; i < victims.Count; i++)
            {
                var enemy = victims[i];
                if (enemy == null || !enemy.IsTargetable || !InCone(enemy.transform.position - transform.position, AimDirection, defense.Range, data.coneAngle)) continue;
                enemy.Health.TakeDefenseDamage(defense.Definition.Damage);
                if (!enemy.IsTargetable) continue;
                var burn = enemy.GetComponent<EnemyDamageOverTime>();
                if (burn == null) burn = enemy.gameObject.AddComponent<EnemyDamageOverTime>();
                burn.ApplyBurn(data.burnDuration, data.burnDps);
            }
            victims.Clear();
        }
    }
}
