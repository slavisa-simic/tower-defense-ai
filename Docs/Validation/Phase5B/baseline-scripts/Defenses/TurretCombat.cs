using System;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    [RequireComponent(typeof(DefenseBase))]
    public class TurretCombat : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float acquisitionInterval = 0.15f;
        private DefenseBase defense;
        private double cooldown;
        private float acquisitionTimer;
        public EnemyController Target { get; private set; }
        public int ShotsFired { get; private set; }
        public event Action<Vector3> Fired;
        protected void Awake() => defense = GetComponent<DefenseBase>();
        protected void Update() => Tick(Time.deltaTime);
        protected void OnEnable() => defense.Upgraded += RefreshUpgrade;
        protected void OnDisable() => defense.Upgraded -= RefreshUpgrade;
        private void RefreshUpgrade() { cooldown = Math.Min(cooldown, 1.0 / defense.FireRate); acquisitionTimer = 0; }

        public bool IsValidTarget(EnemyController enemy) => enemy != null && enemy.IsTargetable &&
            ((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude <= defense.Range * defense.Range;

        public void Tick(float deltaTime)
        {
            if (defense == null) defense = GetComponent<DefenseBase>();
            if (!defense.IsAlive || defense.Game == null || !defense.Game.IsPlaying)
            { Target = null; return; }
            if (deltaTime <= 0) return;
            if (!IsValidTarget(Target)) Target = null;
            acquisitionTimer -= deltaTime;
            if (acquisitionTimer <= 0)
            {
                Target = defense.Enemies.FindTarget(transform.position, defense.Range, defense.Definition.Targeting);
                acquisitionTimer = acquisitionInterval;
            }
            if (Target == null)
            {
                // Cool down while idle, but never accumulate a burst of banked shots.
                cooldown = Math.Max(0, cooldown - deltaTime);
                return;
            }
            cooldown -= deltaTime;
            double interval = 1.0 / defense.FireRate;
            while (cooldown <= 0 && IsValidTarget(Target) && defense.Game.IsPlaying)
            {
                cooldown += interval;
                Vector3 impact = Target.transform.position;
                Target.Health.TakeDefenseDamage(defense.Damage);
                ShotsFired++;
                Fired?.Invoke(impact);
            }
            if (!IsValidTarget(Target))
            {
                Target = null;
                acquisitionTimer = 0;
                cooldown = Math.Max(0, cooldown);
            }
        }

        protected void OnDrawGizmosSelected()
        {
            if (defense == null || defense.Definition == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, defense.Range);
            if (Target != null) Gizmos.DrawLine(transform.position, Target.transform.position);
        }
    }
}
