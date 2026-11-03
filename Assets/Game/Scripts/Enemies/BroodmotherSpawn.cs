using System.Collections.Generic;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class BroodmotherSpawn : MonoBehaviour
    {
        [SerializeField] private EnemyController broodling;
        [SerializeField, Min(1)] private float spawnInterval = 8;
        [SerializeField, Min(1)] private float initialSpawnDelay = 3;
        [SerializeField, Range(1, 12)] private int broodlingsPerSpawn = 6;
        [SerializeField, Range(0, 120)] private int maxTotalBroodlingsSpawned = 60;
        [SerializeField, Range(1, 40)] private int maxConcurrentSpawnedBroodlings = 18;
        private EnemyController enemy;
        private EnemySpawner spawner;
        private GridManager grid;
        private EnemyRegistry registry;
        private float timer;
        private readonly HashSet<EnemyController> children = new HashSet<EnemyController>();
        public int TotalSpawned { get; private set; }
        public int Concurrent => children.Count;
        public void Initialize(EnemyController owner, GridManager battlefield)
        {
            if (registry != null) registry.EnemyRemoved -= ChildRemoved;
            enemy = owner; grid = battlefield; spawner = FindFirstObjectByType<EnemySpawner>();
            registry = FindFirstObjectByType<EnemyRegistry>();
            if (isActiveAndEnabled && registry != null) registry.EnemyRemoved += ChildRemoved;
            children.Clear(); timer = initialSpawnDelay; TotalSpawned = 0;
        }
        public void Tick(float dt)
        {
            if (dt <= 0 || enemy == null || !enemy.IsCombatActive || broodling == null || spawner == null || TotalSpawned >= maxTotalBroodlingsSpawned) return;
            children.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy || !e.Health.IsAlive);
            timer -= dt;
            if (timer > 0 || children.Count >= maxConcurrentSpawnedBroodlings || enemy.Movement.RemainingPathDistance < grid.Grid.CellSize * 3) return;
            timer = spawnInterval;
            int count = Mathf.Min(broodlingsPerSpawn, Mathf.Min(maxTotalBroodlingsSpawned - TotalSpawned, maxConcurrentSpawnedBroodlings - children.Count));
            if (!grid.Grid.TryWorldToCell(transform.position, out var center)) return;
            int produced = 0;
            for (int attempt = 0; attempt < 25 && produced < count; attempt++)
            {
                var cell = center + new Vector2Int(attempt % 5 - 2, attempt / 5 - 2);
                if (!grid.Grid.Contains(cell) || !spawner.TrySpawnReinforcement(broodling, grid.Grid.CellToWorld(cell), out var child)) continue;
                children.Add(child); TotalSpawned++; produced++;
            }
            if (produced > 0) enemy.NotifyAttack();
        }
        private void ChildRemoved(EnemyController child) => children.Remove(child);
        private void OnEnable() { if (registry != null) registry.EnemyRemoved += ChildRemoved; }
        private void OnDisable() { if (registry != null) registry.EnemyRemoved -= ChildRemoved; children.Clear(); }
    }
}
