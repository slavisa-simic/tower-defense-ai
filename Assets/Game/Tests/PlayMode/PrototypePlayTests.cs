using System.Collections;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ShorelineDefense.Tests
{
    public class PrototypePlayTests
    {
        protected virtual string ScenePath => "Assets/Game/Scenes/CoastalDefense_Prototype.unity";
        protected virtual void PrepareSpawner() { }
        private float originalTimeScale;

        [SetUp] public void SetUp() { originalTimeScale = Time.timeScale; }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = originalTimeScale;
            var empty = SceneManager.CreateScene("Test cleanup " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene != empty) yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        [UnityTest]
        public IEnumerator PersistedSceneRunsFromSeaToGameOverAndStopsSpawningAndMovement()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;
            PrepareSpawner();
            var playerBase = Object.FindFirstObjectByType<BaseHealth>();
            var game = Object.FindFirstObjectByType<GameStateController>();
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            var grid = Object.FindFirstObjectByType<GridManager>();
            var hud = Object.FindFirstObjectByType<PrototypeHud>();
            Assert.That(playerBase.CurrentHealth, Is.EqualTo(100));
            Assert.That(hud.HealthText, Is.EqualTo("BASE HP: 100 / 100"));
            Assert.That(game.CurrentState, Is.EqualTo(GameState.Playing));
            Assert.That(Camera.main.orthographic, Is.True);
            Assert.That(Object.FindObjectsByType<GridManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<GameStateController>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            bool topDown = grid.Definition.Plane == GameplayPlane.XY;
            if (topDown)
            {
                Assert.That(Camera.main.transform.forward, Is.EqualTo(Vector3.forward));
                Assert.That(spawner.SpawnPoints.Count, Is.EqualTo(5));
                var rows = new System.Collections.Generic.HashSet<int>();
                foreach (var point in spawner.SpawnPoints)
                {
                    Assert.That(grid.Grid.TryWorldToCell(point.position, out var cell), Is.True);
                    Assert.That(grid.Grid.GetCell(cell).Terrain, Is.EqualTo(TerrainType.Water));
                    rows.Add(cell.y);
                    Assert.That(point.position.x, Is.LessThan(playerBase.transform.position.x));
                }
                Assert.That(rows.Count, Is.EqualTo(5));
            }

            Time.timeScale = 4;
            float deadline = Time.realtimeSinceStartup + 20;
            EnemyController first = null;
            while (first == null && Time.realtimeSinceStartup < deadline)
            {
                first = Object.FindFirstObjectByType<EnemyController>();
                yield return null;
            }
            Assert.That(first, Is.Not.Null, "Spawner never produced an enemy.");
            Assert.That(first.Movement.HasPath, Is.True);
            Assert.That(grid.Grid.TryWorldToCell(first.transform.position, out var spawn), Is.True);
            Assert.That(grid.Grid.GetCell(spawn).Terrain, Is.EqualTo(TerrainType.Water));
            bool sawBeach = false;
            bool sawGround = false;
            Vector3 initialPosition = first.transform.position;
            while (playerBase.CurrentHealth == 100 && Time.realtimeSinceStartup < deadline)
            {
                grid.Grid.TryWorldToCell(first.transform.position, out var cell);
                var terrain = grid.Grid.GetCell(cell).Terrain;
                sawBeach |= terrain == TerrainType.Beach;
                sawGround |= terrain == TerrainType.Ground;
                yield return null;
            }
            Assert.That(sawBeach && sawGround, Is.True, "Enemy did not cross beach and ground.");
            Assert.That(first.transform.position, Is.Not.EqualTo(initialPosition));
            if (topDown)
            {
                Assert.That(first.transform.position.x, Is.GreaterThan(initialPosition.x));
                Assert.That(first.transform.position.z, Is.EqualTo(grid.Grid.Origin.z));
                var visual = first.transform.Find("Visual");
                Assert.That(Vector3.Dot(visual.right, first.Movement.FacingDirection), Is.GreaterThan(0.99f));
                Assert.That(first.transform.rotation, Is.EqualTo(Quaternion.identity), "Presentation must not rotate the gameplay root.");
            }
            Assert.That(playerBase.CurrentHealth, Is.LessThan(100));
            Assert.That(hud.HealthText, Is.EqualTo($"BASE HP: {playerBase.CurrentHealth} / 100"));

            while (game.IsPlaying && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(game.CurrentState, Is.EqualTo(GameState.GameOver));
            Assert.That(playerBase.CurrentHealth, Is.Zero);
            Assert.That(hud.HealthText, Is.EqualTo("BASE HP: 0 / 100"));
            Assert.That(hud.GameOverVisible, Is.True);
            Assert.That(spawner.TotalSpawned, Is.GreaterThan(1));
            Assert.That(first.GetComponent<EnemyCombat>().AttacksPerformed, Is.GreaterThan(1));
            int count = spawner.TotalSpawned;
            var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            var positions = new Vector3[enemies.Length];
            for (int i = 0; i < enemies.Length; i++) positions[i] = enemies[i].transform.position;
            yield return new WaitForSeconds(4);
            Assert.That(spawner.TotalSpawned, Is.EqualTo(count));
            for (int i = 0; i < enemies.Length; i++) Assert.That(enemies[i].transform.position, Is.EqualTo(positions[i]));
            playerBase.TakeDamage(100);
            Assert.That(playerBase.CurrentHealth, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LethalBaseDamageSignalsGameOverOnceAndSceneReloadResetsRuntimeState()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;
            var game = Object.FindFirstObjectByType<GameStateController>();
            var playerBase = Object.FindFirstObjectByType<BaseHealth>();
            int gameOvers = 0;
            game.StateChanged += state => { if (state == GameState.GameOver) gameOvers++; };
            playerBase.TakeDamage(1000);
            playerBase.TakeDamage(1000);
            Assert.That(game.CurrentState, Is.EqualTo(GameState.GameOver));
            Assert.That(gameOvers, Is.EqualTo(1));
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;
            Assert.That(Object.FindFirstObjectByType<BaseHealth>().CurrentHealth, Is.EqualTo(100));
            Assert.That(Object.FindFirstObjectByType<GameStateController>().IsPlaying, Is.True);
            Assert.That(Object.FindFirstObjectByType<PrototypeHud>().GameOverVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator MovementRepathsOnlyOnGridChangeAndResumesWhenRouteReopens()
        {
            var grid = new BattlefieldGrid(5, 3, 1, Vector3.zero);
            var paths = new PathfindingService(grid);
            var enemy = new GameObject("Movement test").AddComponent<EnemyMovement>();
            enemy.transform.position = grid.CellToWorld(new Vector2Int(0, 1));
            enemy.Initialize(grid, paths, new Vector2Int(4, 1), 1, 0.1f);
            enemy.Tick(0.25f);
            Assert.That(paths.Requests, Is.EqualTo(1));
            for (int y = 0; y < 3; y++) grid.SetCell(new Vector2Int(1, y), new GridCell(TerrainType.Blocked, false, false));
            Vector3 stopped = enemy.transform.position;
            enemy.Tick(1);
            Assert.That(enemy.HasPath, Is.False);
            Assert.That(enemy.transform.position, Is.EqualTo(stopped));
            Assert.That(paths.Requests, Is.EqualTo(2), "Changes in one frame must coalesce into one request.");
            enemy.Tick(1);
            Assert.That(paths.Requests, Is.EqualTo(2), "An unreachable enemy must wait, without retrying every frame.");
            grid.SetCell(new Vector2Int(1, 0), new GridCell(TerrainType.Ground, true, true));
            enemy.Tick(0.01f);
            Assert.That(enemy.HasPath, Is.True);
            Assert.That(enemy.Path, Has.Member(new Vector2Int(1, 0)));
            enemy.Tick(20);
            Assert.That(enemy.ReachedDestination, Is.True);
            Assert.That(Vector3.Distance(enemy.transform.position, grid.CellToWorld(new Vector2Int(4, 1))), Is.EqualTo(0.1f).Within(0.001f));
            Object.Destroy(enemy.gameObject);
            yield return null;
            grid.SetCell(Vector2Int.zero, new GridCell(TerrainType.Ground, true, true));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SpawnedEnemiesOwnIndependentHealthAndDeadEnemyIsRemoved()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            PrepareSpawner();
            Time.timeScale = 4;
            yield return new WaitForSeconds(4.2f);
            var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            Assert.That(enemies.Length, Is.GreaterThanOrEqualTo(2));
            var survivor = enemies[1].GetComponent<EnemyHealth>();
            var definition = enemies[0].Definition;
            enemies[0].GetComponent<EnemyHealth>().TakeDamage(1000);
            yield return null;
            Assert.That(enemies[0] == null || !enemies[0].gameObject.activeInHierarchy, Is.True);
            Assert.That(survivor.CurrentHealth, Is.EqualTo(100));
            Assert.That(definition.MaxHealth, Is.EqualTo(100));
        }
    }
}
