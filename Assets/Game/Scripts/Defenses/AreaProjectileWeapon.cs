using System;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    [RequireComponent(typeof(DefenseBase))]
    public sealed class AreaProjectileWeapon : MonoBehaviour
    {
        private DefenseBase defense;
        private float cooldown;
        private float retry;
        private readonly ClusterTargeting.Workspace targeting = new ClusterTargeting.Workspace();
        public int ShotsFired { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public bool HasSolution { get; private set; }
        public event Action<Vector3, TimedAreaProjectile> Fired;
        private void Awake() => defense = GetComponent<DefenseBase>();
        private void Update() => Tick(Time.deltaTime);
        private void OnEnable() => defense.Upgraded += RefreshUpgrade;
        private void OnDisable() => defense.Upgraded -= RefreshUpgrade;
        private void RefreshUpgrade() { cooldown = Mathf.Min(cooldown, 1 / defense.FireRate); retry = 0; }
        public void Tick(float dt)
        {
            if (dt <= 0 || !defense.IsAlive || !defense.Game.IsPlaying) return;
            cooldown = Mathf.Max(0, cooldown - dt); retry -= dt;
            if (cooldown > 0 || retry > 0) return;
            var data = defense.Definition.HeavyWeapon;
            HasSolution = ClusterTargeting.TrySelect(defense.Enemies, transform.position,
                data.minimumRangeInCells * defense.Grid.CellSize, defense.Range,
                defense.SplashRadiusInCells * defense.Grid.CellSize, data.travelTime, data.clusterCandidates, targeting, out var point);
            if (!HasSolution) { retry = .25f; return; }
            AimPoint = point; cooldown = 1 / defense.FireRate; ShotsFired++;
            var shot = new GameObject("Timed area projectile").AddComponent<TimedAreaProjectile>();
            shot.Initialize(defense.Enemies, defense.Game, transform.position, point, data.travelTime,
                data.arcHeight, defense.SplashRadiusInCells * defense.Grid.CellSize, defense.Damage);
            Fired?.Invoke(point, shot);
        }
    }
}
