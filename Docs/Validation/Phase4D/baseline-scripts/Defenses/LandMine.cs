using System;
using System.Collections.Generic;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class LandMine : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float proximityInterval = 0.05f;
        private readonly List<EnemyController> victims = new List<EnemyController>(32);
        private DefenseBase defense;
        private float elapsed;
        private float proximityTimer;
        public bool Exploded { get; private set; }
        public bool Armed => defense != null && defense.Definition != null && elapsed >= defense.Definition.ArmingDelay && !Exploded;
        public event Action<float> Detonated;
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float deltaTime)
        {
            if (Exploded || defense == null || !defense.IsAlive || defense.Game == null || !defense.Game.IsPlaying || deltaTime <= 0) return;
            elapsed += deltaTime;
            proximityTimer -= deltaTime;
            if (!Armed || proximityTimer > 0) return;
            proximityTimer = proximityInterval;
            if (defense.Enemies.FindClosest(transform.position, defense.Definition.TriggerRadiusInCells * defense.Grid.CellSize) == null) return;
            // Latch before callbacks/damage, including reentrant ticks from observers.
            Exploded = true;
            AreaDamage.Apply(defense.Enemies, transform.position, defense.Range, defense.Definition.Damage, victims);
            Detonated?.Invoke(defense.Range);
            defense.TakeDamage(defense.CurrentHealth);
            enabled = false;
        }
    }
}
