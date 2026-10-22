using System.Collections;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class Phase2CPlayTests
    {
        private BuildManager builder;
        private BuildHud hud;
        private WaveManager waves;
        private EnemySpawner spawner;
        private EnemyRegistry registry;
        private BattlefieldGrid Grid => builder.Battlefield.Grid;
        private EconomyManager Money => builder.Economy;
        private DefenseDefinition Data(int index) => hud.Defenses[index];

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
            yield return null;
            builder = Object.FindFirstObjectByType<BuildManager>();
            hud = Object.FindFirstObjectByType<BuildHud>();
            waves = Object.FindFirstObjectByType<WaveManager>();
            spawner = Object.FindFirstObjectByType<EnemySpawner>();
            spawner.ConfigureBorder(null);
            registry = Object.FindFirstObjectByType<EnemyRegistry>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            var previous = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Phase 2C cleanup " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(previous);
            LogAssert.NoUnexpectedReceived();
        }
        private DefenseBase Build(int index, Vector2Int cell)
        {
            Assert.That(builder.TryPlaceDefense(Data(index), cell, out var defense), Is.EqualTo(PlacementResult.Valid));
            return defense;
        }
        private EnemyController Spawn(Vector2Int cell)
        {
            Assert.That(spawner.TrySpawn(spawner.DefaultPrefab, out var enemy), Is.True);
            enemy.transform.position = Grid.CellToWorld(cell);
            enemy.Movement.Initialize(Grid, builder.Battlefield.Paths, builder.Battlefield.Definition.BaseCell,
                enemy.Definition.MovementSpeed, enemy.Definition.AttackRange);
            return enemy;
        }
        private static void Click(Button button)
        {
            using (var click = NavigationSubmitEvent.GetPooled()) { click.target = button; button.SendEvent(click); }
        }

        [UnityTest]
        public IEnumerator FourButtonsSwitchOnePreviewAndEnforceCostsOccupancyAndTerrain()
        {
            Assert.That(Money.Money, Is.EqualTo(500));
            Assert.That(Object.FindFirstObjectByType<PrototypeSpawnLoop>(), Is.Null);
            waves.Tick(100);
            Assert.That(spawner.TotalSpawned, Is.Zero, "Preparation must never auto-spawn.");
            var root = hud.GetComponent<UIDocument>().rootVisualElement;
            var placement = Object.FindFirstObjectByType<PlacementController>();
            for (int i = 0; i < 4; i++)
            {
                var data = Data(i);
                var button = root.Q<Button>(i == 0 ? "build-mg" : "build-" + data.Id);
                Assert.That(button.text, Does.Contain(data.Cost + " RSD"));
                Click(button);
                Assert.That(placement.Selected, Is.SameAs(data));
                placement.HoverCell(new Vector2Int(14, 5));
                Assert.That(placement.PreviewVisible, Is.True);
                Assert.That(placement.Result, Is.EqualTo(PlacementResult.Valid));
                placement.HoverCell(new Vector2Int(4, 5));
                Assert.That(placement.Result, Is.EqualTo(PlacementResult.NotBuildable));
            }
            Build(3, new Vector2Int(14, 5));
            Assert.That(Money.Money, Is.EqualTo(200));
            Assert.That(root.Q<Button>("build-sniper-tower").enabledSelf, Is.False);
            Assert.That(root.Q<Button>("build-land-mine").enabledSelf, Is.True);
            foreach (var data in hud.Defenses)
                Assert.That(builder.TryPlaceDefense(data, new Vector2Int(14, 5), out _), Is.EqualTo(PlacementResult.Occupied));
            placement.Cancel();
            Assert.That(placement.PreviewVisible, Is.False);
            Assert.That(Money.Money, Is.EqualTo(200));
            yield return null;
        }

        [UnityTest]
        public IEnumerator WirePlacementSlowsRecoversAndCleansUpNavigation()
        {
            var cell = new Vector2Int(15, 9);
            int revision = Grid.Revision;
            var wire = Build(1, cell);
            Assert.That(Grid.OccupantAt(cell), Is.SameAs(wire));
            Assert.That(Grid.GetCell(cell).Walkable, Is.True);
            Assert.That(Grid.GetCell(cell).TraversalCost, Is.EqualTo(2));
            Assert.That(Grid.Revision, Is.EqualTo(revision + 1));
            var enemy = Spawn(cell);
            Assert.That(enemy.Movement.CurrentSpeed, Is.EqualTo(1.25f));
            Assert.That(enemy.Definition.MovementSpeed, Is.EqualTo(2.5f));
            var modifier = new object(); enemy.Movement.Modifiers.Set(modifier, 0.8f);
            Assert.That(enemy.Movement.CurrentSpeed, Is.EqualTo(1));
            enemy.Movement.Tick(1.5f);
            Assert.That(enemy.Movement.CurrentSpeed, Is.EqualTo(2));
            enemy.Movement.Modifiers.Remove(modifier);
            Assert.That(enemy.Movement.CurrentSpeed, Is.EqualTo(2.5f));
            int requests = builder.Battlefield.Paths.Requests;
            wire.TakeDamage(200);
            Assert.That(Grid.OccupantAt(cell), Is.Null);
            Assert.That(Grid.GetCell(cell).TraversalCost, Is.EqualTo(1));
            enemy.Movement.Tick(0.01f);
            Assert.That(builder.Battlefield.Paths.Requests, Is.EqualTo(requests + 1));
            enemy.Movement.Tick(0.01f);
            Assert.That(builder.Battlefield.Paths.Requests, Is.EqualTo(requests + 1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator MineArmsIgnoresDeadTargetsDamagesAreaOnceAndUsesGenericRewards()
        {
            var cell = new Vector2Int(15, 9);
            var defense = Build(2, cell);
            var mine = defense.GetComponent<LandMine>();
            Assert.That(Grid.GetCell(cell).Walkable, Is.True);
            Assert.That(Grid.OccupantAt(cell), Is.SameAs(defense));
            var dead = Spawn(cell); dead.Health.TakeDamage(100);
            mine.Tick(1);
            Assert.That(mine.Exploded, Is.False);
            var trigger = Spawn(cell);
            var inside = Spawn(cell + Vector2Int.right); inside.Health.Initialize(200);
            var outside = Spawn(cell + Vector2Int.right * 3);
            int detonations = 0;
            mine.Detonated += _ => { detonations++; mine.Tick(100); };
            int balance = Money.Money;
            mine.Tick(0.1f);
            Assert.That(mine.Exploded, Is.True);
            Assert.That(mine.enabled, Is.False);
            Assert.That(trigger.Health.IsAlive, Is.False);
            Assert.That(inside.Health.CurrentHealth, Is.EqualTo(50));
            Assert.That(outside.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(Money.Money, Is.EqualTo(balance + 25));
            Assert.That(detonations, Is.EqualTo(1));
            Assert.That(Grid.OccupantAt(cell), Is.Null);
            mine.Tick(100);
            Assert.That(inside.Health.CurrentHealth, Is.EqualTo(50));
            yield return null;
            Assert.That(defense == null, Is.True);
        }

        [UnityTest]
        public IEnumerator UnarmedMineWaitsAndGameOverStopsIt()
        {
            var cell = new Vector2Int(15, 9);
            var mine = Build(2, cell).GetComponent<LandMine>();
            var enemy = Spawn(cell);
            mine.Tick(0.1f);
            Assert.That(mine.Armed, Is.False);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(100));
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(1000);
            mine.Tick(10);
            Assert.That(mine.Exploded, Is.False);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(100));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SniperPrioritizesRouteProgressAndHonorsRangeCooldownAndDeadTargetRelease()
        {
            var defense = Build(3, new Vector2Int(18, 9));
            var turret = defense.GetComponent<TurretCombat>();
            var outside = Spawn(new Vector2Int(1, 9));
            turret.Tick(0.2f);
            Assert.That(turret.ShotsFired, Is.Zero);
            var closestToTower = Spawn(new Vector2Int(19, 9));
            var closestToBase = Spawn(new Vector2Int(25, 9));
            closestToBase.Health.Initialize(200);
            turret.Tick(0.2f);
            Assert.That(turret.Target, Is.SameAs(closestToBase));
            Assert.That(closestToBase.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(closestToTower.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(outside.Health.CurrentHealth, Is.EqualTo(100));
            turret.Tick(1.7f);
            Assert.That(turret.ShotsFired, Is.EqualTo(1));
            turret.Tick(0.31f);
            Assert.That(closestToBase.Health.IsAlive, Is.False);
            Assert.That(turret.Target, Is.Null);
            turret.Tick(0.2f);
            Assert.That(turret.Target, Is.SameAs(closestToTower));
            turret.Tick(1.8f);
            Assert.That(closestToTower.Health.IsAlive, Is.False);
            Assert.That(turret.ShotsFired, Is.EqualTo(3));
            Assert.That(Money.Money, Is.EqualTo(250));
            Assert.That(defense.transform.rotation, Is.EqualTo(Quaternion.identity));
            yield return null;
        }

        [UnityTest]
        public IEnumerator AllFiveWavesCountPendingAndLivingEnemiesAndRewardExactlyOnce()
        {
            // Preserve the historical direct-scheduler contract independently of transport timing.
            // Phase3C1PlayTests exercises the primary boat-backed wave lifecycle.
            waves.ConfigureEndless(null); waves.ConfigureLandings(null);
            var unrelated = Spawn(new Vector2Int(20, 1));
            var waveHud = Object.FindFirstObjectByType<WaveHud>();
            for (int wave = 0; wave < 5; wave++)
            {
                int count = 10 + 5 * wave;
                Assert.That(waves.CurrentWaveNumber, Is.EqualTo(wave + 1));
                Assert.That(waves.State, Is.EqualTo(WaveState.Preparation));
                Assert.That(waves.EnemiesRemaining, Is.EqualTo(count));
                Assert.That(waveHud.StartAvailable, Is.True);
                if (wave == 0) Click(hud.GetComponent<UIDocument>().rootVisualElement.Q<Button>("start-wave"));
                else Assert.That(waves.TryStartWave(), Is.True);
                Assert.That(waves.State, Is.EqualTo(WaveState.Spawning));
                Assert.That(waves.TryStartWave(), Is.False);
                Assert.That(waveHud.StartAvailable, Is.False);
                var spawned = new List<EnemyController>();
                var spawnRows = new HashSet<float>();
                for (int i = 0; i < count; i++)
                {
                    waves.Tick(0.8f);
                    var enemy = registry.Active[registry.Count - 1];
                    spawned.Add(enemy);
                    spawnRows.Add(enemy.transform.position.y);
                    enemy.transform.position = Grid.CellToWorld(new Vector2Int(15, 1));
                    Assert.That(waves.EnemiesRemaining, Is.EqualTo(count));
                    Assert.That(waves.PendingCount, Is.EqualTo(count - i - 1));
                }
                Assert.That(spawnRows.Count, Is.EqualTo(5));
                Assert.That(waves.State, Is.EqualTo(WaveState.Combat));
                int balance = Money.Money;
                for (int i = 0; i < count - 1; i++) spawned[i].Health.TakeDefenseDamage(100);
                Assert.That(waves.EnemiesRemaining, Is.EqualTo(1));
                Assert.That(waveHud.RemainingText, Is.EqualTo("Enemies Remaining: 1"));
                waves.Tick(100);
                Assert.That(waves.State, Is.EqualTo(WaveState.Combat), "Finished spawning is not completion.");
                spawned[count - 1].Health.TakeDefenseDamage(100);
                spawned[count - 1].Health.TakeDefenseDamage(100);
                Assert.That(waves.State, Is.EqualTo(WaveState.WaveComplete));
                Assert.That(waves.EnemiesRemaining, Is.Zero);
                Assert.That(Money.Money, Is.EqualTo(balance + count * 25 + 100));
                Assert.That(unrelated.Health.IsAlive, Is.True, "Unrelated enemies must not hold a wave open.");
                waves.Tick(2);
                Assert.That(waves.State, Is.EqualTo(wave == 4 ? WaveState.AllWavesComplete : WaveState.Preparation));
                Assert.That(Money.Money, Is.EqualTo(balance + count * 25 + 100));
                yield return null;
            }
            Assert.That(waves.CompletedWaves, Is.EqualTo(5));
            Assert.That(waves.TryStartWave(), Is.False);
            Assert.That(spawner.TotalSpawned, Is.EqualTo(101));
        }

        [UnityTest]
        public IEnumerator FailedSpawnRetriesWithoutLosingPendingEnemiesAndGameOverStopsEverything()
        {
            waves.ConfigureEndless(null); waves.ConfigureLandings(null);
            foreach (var point in spawner.SpawnPoints)
            {
                Grid.TryWorldToCell(point.position, out var cell);
                Grid.SetCell(cell, new GridCell(TerrainType.Blocked, false, false));
            }
            Assert.That(waves.TryStartWave(), Is.True);
            waves.Tick(10);
            Assert.That(waves.PendingCount, Is.EqualTo(10));
            Assert.That(waves.AliveCount, Is.Zero);
            Grid.TryWorldToCell(spawner.SpawnPoints[2].position, out var open);
            Grid.SetCell(open, new GridCell(TerrainType.Water, true, false));
            waves.Tick(0.2f);
            Assert.That(waves.PendingCount, Is.EqualTo(9));
            Assert.That(waves.AliveCount, Is.EqualTo(1));
            var turret = Build(3, new Vector2Int(10, 8)).GetComponent<TurretCombat>();
            int balance = Money.Money;
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(100);
            var enemy = registry.Active[0];
            var position = enemy.transform.position;
            waves.Tick(100); turret.Tick(100);
            Assert.That(waves.State, Is.EqualTo(WaveState.GameOver));
            Assert.That(waves.TryStartWave(), Is.False);
            Assert.That(spawner.TrySpawn(spawner.DefaultPrefab, out _), Is.False);
            Assert.That(spawner.TotalSpawned, Is.EqualTo(1));
            Assert.That(turret.ShotsFired, Is.Zero);
            Assert.That(builder.TryPlaceDefense(Data(1), new Vector2Int(13, 4), out _), Is.EqualTo(PlacementResult.NotPlaying));
            enemy.Health.TakeDefenseDamage(100);
            Assert.That(Money.Money, Is.EqualTo(balance));
            Assert.That(waves.CompletedWaves, Is.Zero);
            Assert.That(Object.FindFirstObjectByType<WaveHud>().StartAvailable, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneSmokeBuildFightEarnCompleteAndPrepareNextWave()
        {
            // Historical finite wave smoke test; endless pressure is exercised by Phase4C tests.
            waves.ConfigureEndless(null);
            Build(3, new Vector2Int(19, 9));
            Build(0, new Vector2Int(23, 9));
            Build(1, new Vector2Int(25, 9));
            Assert.That(Money.Money, Is.Zero);
            Assert.That(waves.TryStartWave(), Is.True);
            Time.timeScale = 4;
            float deadline = Time.realtimeSinceStartup + 35;
            bool sawSpawning = false, sawDamage = false;
            while (waves.CurrentWaveNumber == 1 && builder.Game.IsPlaying && Time.realtimeSinceStartup < deadline)
            {
                sawSpawning |= waves.PendingCount > 0 && waves.AliveCount > 0;
                foreach (var enemy in registry.Active) sawDamage |= enemy.Health.CurrentHealth < enemy.Health.MaxHealth;
                yield return null;
            }
            Assert.That(builder.Game.IsPlaying, Is.True);
            Assert.That(sawSpawning, Is.True);
            Assert.That(sawDamage || Object.FindObjectsByType<TurretCombat>(FindObjectsSortMode.None).Any(t=>t.ShotsFired>0), Is.True,"Combat must deal damage, including lethal one-hit Sniper shots.");
            Assert.That(waves.CurrentWaveNumber, Is.EqualTo(2));
            Assert.That(waves.State, Is.EqualTo(WaveState.Preparation));
            Assert.That(waves.CompletedWaves, Is.EqualTo(1));
            Assert.That(spawner.TotalSpawned, Is.EqualTo(10));
            Assert.That(Money.Money, Is.EqualTo(350));
            Assert.That(Object.FindFirstObjectByType<WaveHud>().StartAvailable, Is.True);
        }
    }
}
