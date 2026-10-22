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
        private readonly List<EnemyController> spawnedEnemies = new List<EnemyController>();
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
                if (spawnedEnemies[i] == null || !spawnedEnemies[i].Health.IsAlive) spawnedEnemies.RemoveAt(i);
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
                if (spawnedEnemies[i] == null || !spawnedEnemies[i].Health.IsAlive) spawnedEnemies.RemoveAt(i);
            position = grid.Grid.ProjectToPlane(position);
            if (!HasSpace(position) || !grid.Grid.TryWorldToCell(playerBase.transform.position, out var goal) ||
                !grid.Paths.TryFindPath(cell, goal, unloadingPath)) return false;
            var enemy = Instantiate(requestedPrefab, position, Quaternion.identity, enemyParent);
            enemy.Initialize(grid, playerBase, game, registry);
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
