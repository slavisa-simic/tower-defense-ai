using System;
using System.Collections.Generic;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    /// <summary>Retains a position, never a target reference. Visual arc is separate from radius/damage.</summary>
    public sealed class TimedAreaProjectile : MonoBehaviour
    {
        private EnemyRegistry registry;
        private GameStateController game;
        private Vector3 start;
        private float duration, elapsed, arc, radius;
        private int damage;
        private readonly List<EnemyController> victims = new List<EnemyController>(32);
        public Vector3 ImpactPoint { get; private set; }
        public bool Resolved { get; private set; }
        public event Action<Vector3, float> Impacted;
        public void Initialize(EnemyRegistry targets, GameStateController state, Vector3 origin, Vector3 impact,
            float travel, float height, float splash, int amount)
        {
            registry = targets; game = state; start = origin; ImpactPoint = impact;
            duration = Mathf.Max(.01f, travel); arc = height; radius = splash; damage = amount;
            transform.position = start;
        }
        private void Update() => Tick(Time.deltaTime);
        public void Tick(float dt)
        {
            if (Resolved || game == null) return;
            if (!game.IsPlaying) { Resolved = true; Destroy(gameObject); return; }
            if (dt <= 0) return;
            elapsed += dt; float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, ImpactPoint, t) + Vector3.up * (4 * arc * t * (1 - t));
            if (t < 1) return;
            Resolved = true;
            AreaDamage.Apply(registry, ImpactPoint, radius, damage, victims);
            Impacted?.Invoke(ImpactPoint, radius);
            Destroy(gameObject);
        }
    }
}
