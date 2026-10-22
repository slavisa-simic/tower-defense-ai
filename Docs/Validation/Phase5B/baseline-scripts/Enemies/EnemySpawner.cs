using System.Collections.Generic;
using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyController prefab;
        [SerializeField] private GridManager grid;
        [SerializeField] private BaseHealth playerBase;
        [SerializeField] private GameStateController game;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform enemyParent;
        [SerializeField, Min(0)] private float minimumSpawnSeparation = 1f;
        [SerializeField] private EnemyRegistry registry;
        [SerializeField] private BorderBreachService border;
        private int activeLimit = int.MaxValue;
        private float healthMultiplier = 1, speedMultiplier = 1, bossHealthMultiplier = 1;
        public void ConfigureBorder(BorderBreachService service) => border = service;
        public void ConfigureDeployment(float separation) => minimumSpawnSeparation = Mathf.Clamp(separation, .1f, 2);
        public void ConfigureWave(int maximumActive, float health, float speed, float bossHealth = -1)
        { activeLimit = Mathf.Clamp(maximumActive, 1, 400); healthMultiplier = Mathf.Clamp(health, 1, 10000); bossHealthMultiplier = bossHealth < 0 ? healthMultiplier : Mathf.Clamp(bossHealth, 1, 10000); speedMultiplier = Mathf.Clamp(speed, 1, 3); }
        private readonly List<EnemyController> spawnedEnemies = new List<EnemyController>();
        private readonly Dictionary<EnemyController, Stack<EnemyController>> pools = new Dictionary<EnemyController, Stack<EnemyController>>();
        private readonly Dictionary<EnemyController, EnemyController> sources = new Dictionary<EnemyController, EnemyController>();
        private readonly List<EnemyController> returning = new List<EnemyController>();
        private int pooledCount;
        public int CreatedEnemies { get; private set; }
        public int ReusedEnemies { get; private set; }
        public int PooledEnemies => pooledCount;
        private void QueueReturn(EnemyController enemy) => returning.Add(enemy);
        private void LateUpdate()
        {
            // Defer reuse until death/reward/presentation callbacks have all returned.
            foreach (var enemy in returning) {
                if (enemy == null) continue;
                spawnedEnemies.Remove(enemy);
                if (pooledCount >= 400) { sources.Remove(enemy); Destroy(enemy.gameObject); continue; }
                pools[sources[enemy]].Push(enemy); pooledCount++;
            }
            returning.Clear();
        }
        private int nextSpawnPoint;
        private readonly List<Vector2Int> unloadingPath = new List<Vector2Int>(64);
        public int TotalSpawned { get; private set; }
        public IReadOnlyList<Transform> SpawnPoints => spawnPoints;
        public EnemyController DefaultPrefab => prefab;
        public GridManager Battlefield => grid;
        public void ConfigureRegistry(EnemyRegistry enemies) => registry = enemies;

        public void Configure(EnemyController crawler, GridManager battlefield, BaseHealth target,
            GameStateController state, Transform[] points, Transform parent)
        {
            prefab = crawler;
            grid = battlefield;
            playerBase = target;
            game = state;
            spawnPoints = points;
            enemyParent = parent;
        }

        private void Start()
        {
            if (prefab == null || prefab.Definition == null || grid == null || grid.Grid == null || playerBase == null ||
                game == null || spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("EnemySpawner is missing a prefab, configuration, target, grid, state, or spawn points.", this);
                enabled = false;
                return;
            }
            foreach (Transform point in spawnPoints)
            {
                if (point == null || !grid.Grid.TryWorldToCell(point.position, out var cell) || !grid.Grid.GetCell(cell).Walkable)
                {
                    Debug.LogError("Each spawn point must be on a walkable grid cell.", this);
                    enabled = false;
                    return;
                }
            }
        }

        // Request executor only. WaveManager (or the historical scene's explicit test loop) schedules calls.
        public bool TrySpawn(EnemyController requestedPrefab, out EnemyController spawned)
        {
            spawned = null;
            if (!isActiveAndEnabled || game == null || !game.IsPlaying || requestedPrefab == null ||
                requestedPrefab.Definition == null || grid?.Grid == null || playerBase == null || !playerBase.IsAlive ||
                spawnPoints == null || spawnPoints.Length == 0) return false;
            // Scan the configured coast points; one occupied/blocked point must not stall every lane.
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
                if (spawnedEnemies[i] == null || !spawnedEnemies[i].gameObject.activeInHierarchy || !spawnedEnemies[i].Health.IsAlive) spawnedEnemies.RemoveAt(i);
            for (int attempt = 0; attempt < spawnPoints.Length; attempt++)
            {
                Transform point = spawnPoints[nextSpawnPoint];
                nextSpawnPoint = (nextSpawnPoint + 1) % spawnPoints.Length;
                if (point == null || !grid.Grid.TryWorldToCell(point.position, out var cell) || !grid.Grid.GetCell(cell).Walkable) continue;
                Vector3 position = grid.Grid.CellToWorld(cell);
                if (!HasSpace(position)) continue;
                if (TrySpawnAt(requestedPrefab, position, out spawned)) return true;
            }
            return false;
        }

        // Boats request a land position; initialization and registration stay here.
        public bool TrySpawnAt(EnemyController requestedPrefab, Vector3 position, out EnemyController spawned)
        {
            spawned = null;
            if (!isActiveAndEnabled || game == null || !game.IsPlaying || requestedPrefab == null ||
                requestedPrefab.Definition == null || grid?.Grid == null || playerBase == null || !playerBase.IsAlive ||
                !grid.Grid.TryWorldToCell(position, out var cell) || !grid.Grid.GetCell(cell).Walkable) return false;
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
                if (spawnedEnemies[i] == null || !spawnedEnemies[i].gameObject.activeInHierarchy || !spawnedEnemies[i].Health.IsAlive) spawnedEnemies.RemoveAt(i);
            if ((registry != null ? registry.Count : spawnedEnemies.Count) >= activeLimit) return false;
            position = grid.Grid.ProjectToPlane(position);
            if (!HasSpace(position) || !grid.Grid.TryWorldToCell(playerBase.transform.position, out var goal)) return false;
            if (border != null && !border.TryGetDestination(cell, out goal)) return false;
            if (
                !grid.Paths.TryFindPath(cell, goal, unloadingPath)) return false;
            if (!pools.TryGetValue(requestedPrefab, out var pool)) pools[requestedPrefab] = pool = new Stack<EnemyController>();
            EnemyController enemy;
            if (pool.Count > 0) { enemy = pool.Pop(); pooledCount--; ReusedEnemies++; enemy.transform.SetPositionAndRotation(position, Quaternion.identity); }
            else { enemy = Instantiate(requestedPrefab, position, Quaternion.identity, enemyParent); sources[enemy] = requestedPrefab; enemy.ConfigurePool(QueueReturn); CreatedEnemies++; }
            enemy.Initialize(grid, playerBase, game, registry, border, border != null ? goal : (Vector2Int?)null,
                requestedPrefab.Definition is ShorelineDefense.Waves.BossDefinition ? bossHealthMultiplier : healthMultiplier, speedMultiplier);
            enemy.gameObject.SetActive(true);
            spawnedEnemies.Add(enemy);
            TotalSpawned++;
            spawned = enemy;
            return true;
        }

        private bool HasSpace(Vector3 position)
        {
            float separationSquared = minimumSpawnSeparation * minimumSpawnSeparation;
            foreach (var enemy in spawnedEnemies)
                if ((enemy.transform.position - position).sqrMagnitude < separationSquared) return false;
            return true;
        }

        private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            Gizmos.color = Color.magenta;
            foreach (Transform point in spawnPoints)
                if (point != null) Gizmos.DrawWireSphere(point.position, 0.45f);
        }
    }
}
