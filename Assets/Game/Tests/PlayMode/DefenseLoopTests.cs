using System.Collections;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class FailedCreationBuilder : BuildManager
    {
        protected override DefenseBase CreateDefense(DefenseDefinition definition, Vector3 position, Transform parent) => null;
    }

    public sealed class DefenseLoopTests
    {
        private BuildManager builder;
        private BuildHud hud;
        private PlacementController placement;
        private EnemySpawner spawner;
        private EnemyRegistry enemies;
        private DefenseDefinition Data => hud.MachineGun;
        private EconomyManager Economy => builder.Economy;
        private BattlefieldGrid Grid => builder.Battlefield.Grid;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
            yield return null;
            builder = Object.FindFirstObjectByType<BuildManager>();
            hud = Object.FindFirstObjectByType<BuildHud>();
            placement = Object.FindFirstObjectByType<PlacementController>();
            spawner = Object.FindFirstObjectByType<EnemySpawner>();
            // Existing Phase 2B fixture explicitly opts into its historical spawn cadence.
            // Normal gameplay uses WaveManager and has no automatic spawn component.
            spawner.ConfigureBorder(null); spawner.gameObject.AddComponent<PrototypeSpawnLoop>();
            enemies = Object.FindFirstObjectByType<EnemyRegistry>();
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            var previous = SceneManager.GetActiveScene();
            var empty = SceneManager.CreateScene("Defense cleanup " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync(previous);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator SpawnPlaceShootKillRewardAndBuyAgain()
        {
            Assert.That(Economy.Money, Is.EqualTo(500));
            Assert.That(hud.MoneyText, Is.EqualTo("RSD 500"));
            Assert.That(Data.Cost, Is.EqualTo(150));
            Assert.That(Data.Damage, Is.EqualTo(20));
            Assert.That(Data.FireRate, Is.EqualTo(4));
            Assert.That(Data.MaxHealth, Is.EqualTo(300));
            var button = hud.GetComponent<UIDocument>().rootVisualElement.Q<Button>("build-mg");
            using (var click = NavigationSubmitEvent.GetPooled()) { click.target = button; button.SendEvent(click); }
            Assert.That(placement.IsPlacing, Is.True, "The real build button must select the defense.");
            placement.HoverCell(new Vector2Int(8, 3));
            Assert.That(placement.PreviewVisible, Is.True);
            Assert.That(placement.TryPlaceCurrent(out var defense), Is.EqualTo(PlacementResult.Valid));
            var combat = defense.GetComponent<MachineGunCombat>();
            Assert.That(Economy.Money, Is.EqualTo(350));
            Assert.That(Grid.OccupantAt(new Vector2Int(8, 3)), Is.SameAs(defense));
            Assert.That(placement.IsPlacing, Is.True);
            float deadline = Time.realtimeSinceStartup + 8;
            EnemyController first = null;
            while (first == null && Time.realtimeSinceStartup < deadline)
            { first = Object.FindFirstObjectByType<EnemyController>(); yield return null; }
            Assert.That(first, Is.Not.Null);
            spawner.enabled = false;
            bool sawDamage = false;
            while (first != null && first.IsTargetable && Time.realtimeSinceStartup < deadline)
            {
                sawDamage |= first.Health.CurrentHealth < 100;
                yield return null;
            }
            Assert.That(first == null || !first.gameObject.activeInHierarchy, Is.True);
            Assert.That(sawDamage, Is.True);
            Assert.That(combat.ShotsFired, Is.EqualTo(5));
            Assert.That(enemies.Count, Is.Zero);
            Assert.That(Economy.Money, Is.EqualTo(375));
            Assert.That(hud.MoneyText, Is.EqualTo("RSD 375"));
            Assert.That(combat.Target == null, Is.True);
            Assert.That(builder.TryPlaceDefense(Data, new Vector2Int(8, 6), out _), Is.EqualTo(PlacementResult.Valid));
            Assert.That(Economy.Money, Is.EqualTo(225));
        }

        [UnityTest]
        public IEnumerator RejectedPlacementsCancelPreviewAndOccupancyCleanupNeverSpend()
        {
            spawner.enabled = false;
            foreach (var cell in new[] { new Vector2Int(1, 3), new Vector2Int(4, 3), new Vector2Int(29, 9), new Vector2Int(28, 8), new Vector2Int(-1, 3), new Vector2Int(32, 3) })
                Assert.That(builder.TryPlaceDefense(Data, cell, out _), Is.Not.EqualTo(PlacementResult.Valid));
            Assert.That(Economy.Money, Is.EqualTo(500));
            placement.Select(Data);
            placement.HoverCell(new Vector2Int(9, 4));
            placement.Cancel();
            Assert.That(placement.PreviewVisible, Is.False);
            Assert.That(Economy.Money, Is.EqualTo(500));
            var cellToBuild = new Vector2Int(9, 4);
            int navigation = Grid.Revision;
            Assert.That(builder.TryPlaceDefense(Data, cellToBuild, out var defense), Is.EqualTo(PlacementResult.Valid));
            Assert.That(builder.TryPlaceDefense(Data, cellToBuild, out _), Is.EqualTo(PlacementResult.Occupied));
            Assert.That(Economy.Money, Is.EqualTo(350));
            Assert.That(Grid.Revision, Is.EqualTo(navigation));
            defense.TakeDamage(300);
            Assert.That(Grid.OccupantAt(cellToBuild), Is.Null);
            yield return null;
            Assert.That(defense == null, Is.True);
            Assert.That(builder.TryPlaceDefense(Data, cellToBuild, out var replacement), Is.EqualTo(PlacementResult.Valid));
            Object.Destroy(replacement.gameObject);
            yield return null;
            Assert.That(Grid.OccupantAt(cellToBuild), Is.Null);
            Economy.TrySpend(200);
            Assert.That(builder.TryPlaceDefense(Data, cellToBuild, out _), Is.EqualTo(PlacementResult.InsufficientMoney));
            Assert.That(hud.BuildAvailable, Is.False);
            Assert.That(hud.MoneyText, Is.EqualTo("RSD 0"));
            Economy.AddMoney(150);
            Assert.That(hud.BuildAvailable, Is.True);
        }

        [UnityTest]
        public IEnumerator DeathRewardOnceAndNoRewardForCleanupOrPostGameDamage()
        {
            yield return new WaitForSeconds(1.1f);
            spawner.enabled = false;
            var enemy = Object.FindFirstObjectByType<EnemyController>();
            int balance = Economy.Money;
            int deaths = 0;
            enemy.Health.Died += () => deaths++;
            enemy.Health.TakeDefenseDamage(100);
            enemy.Health.TakeDefenseDamage(100);
            Assert.That(deaths, Is.EqualTo(1));
            Assert.That(enemy.IsTargetable, Is.False);
            Assert.That(Economy.Money, Is.EqualTo(balance + 25));
            yield return null;
            Assert.That(enemy == null || !enemy.gameObject.activeInHierarchy, Is.True);
            spawner.enabled = true;
            yield return new WaitForSeconds(3.1f);
            spawner.enabled = false;
            enemy = Object.FindFirstObjectByType<EnemyController>();
            enemy.Health.TakeDamage(100);
            Assert.That(Economy.Money, Is.EqualTo(balance + 25), "Unattributed damage/despawn must not produce income.");
            yield return null;
            spawner.enabled = true;
            yield return new WaitForSeconds(3.1f);
            spawner.enabled = false;
            enemy = Object.FindFirstObjectByType<EnemyController>();
            placement.Select(Data);
            builder.TryPlaceDefense(Data, new Vector2Int(20, 9), out var defense);
            var combat = defense.GetComponent<MachineGunCombat>();
            int shots = combat.ShotsFired;
            int finalMoney = Economy.Money;
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(1000);
            enemy.Health.TakeDefenseDamage(1000);
            combat.Tick(10);
            Assert.That(Economy.Money, Is.EqualTo(finalMoney));
            Assert.That(combat.ShotsFired, Is.EqualTo(shots));
            Assert.That(placement.IsPlacing, Is.False);
            Assert.That(hud.BuildAvailable, Is.False);
            Assert.That(builder.TryPlaceDefense(Data, new Vector2Int(15, 8), out _), Is.EqualTo(PlacementResult.NotPlaying));
            Assert.That(Object.FindFirstObjectByType<PrototypeHud>().GameOverVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator CreationFailureAndMultiCellPlacementKeepTransactionsConsistent()
        {
            spawner.enabled = false;
            var failure = new GameObject("Failed creation test").AddComponent<FailedCreationBuilder>();
            failure.Configure(builder.Battlefield, Economy, builder.Game, enemies, null);
            var anchor = new Vector2Int(10, 5);
            Assert.That(failure.TryPlaceDefense(Data, anchor, out _), Is.EqualTo(PlacementResult.CreationFailed));
            Assert.That(Economy.Money, Is.EqualTo(500));
            Assert.That(Grid.OccupantAt(anchor), Is.Null);
            var larger = Object.Instantiate(Data);
            JsonUtility.FromJsonOverwrite("{\"footprint\":{\"x\":2,\"y\":3}}", larger);
            Assert.That(builder.TryPlaceDefense(larger, anchor, out var defense), Is.EqualTo(PlacementResult.Valid));
            Assert.That(defense.transform.position, Is.EqualTo(Grid.FootprintCenter(anchor, larger.Footprint)));
            for (int y = 0; y < 3; y++)
            for (int x = 0; x < 2; x++)
                Assert.That(Grid.OccupantAt(anchor + new Vector2Int(x, y)), Is.SameAs(defense));
            Assert.That(Economy.Money, Is.EqualTo(350));
            Object.Destroy(defense.gameObject);
            yield return null;
            Assert.That(Grid.CanOccupy(anchor, larger.Footprint), Is.True);
            Object.Destroy(larger);
        }

        [UnityTest]
        public IEnumerator TargetingRangeDisabledTargetsAimAndStableCooldown()
        {
            yield return new WaitForSeconds(1.1f);
            spawner.enabled = false;
            var enemy = Object.FindFirstObjectByType<EnemyController>();
            builder.TryPlaceDefense(Data, new Vector2Int(12, 8), out var defense);
            var combat = defense.GetComponent<MachineGunCombat>();
            combat.enabled = false;
            enemy.transform.position = defense.transform.position + Vector3.right * (defense.Range + 0.2f);
            combat.Tick(0.2f);
            Assert.That(combat.ShotsFired, Is.Zero);
            enemy.transform.position = defense.transform.position + Vector3.up * 3;
            combat.Tick(0.01f);
            // Allow the acquisition interval to expire; no scene-wide scan each frame.
            combat.Tick(0.15f);
            Assert.That(enemy.Health.CurrentHealth, Is.EqualTo(80));
            Assert.That(combat.ShotsFired, Is.EqualTo(1));
            Assert.That(defense.transform.rotation, Is.EqualTo(Quaternion.identity));
            Assert.That(Vector3.Dot(defense.transform.Find("Rotating Weapon").right, Vector3.up), Is.GreaterThan(0.99f));
            combat.Tick(0.05f);
            Assert.That(combat.ShotsFired, Is.EqualTo(1));
            combat.Tick(0.21f);
            Assert.That(combat.ShotsFired, Is.EqualTo(2));
            enemy.enabled = false;
            Assert.That(enemies.Count, Is.Zero);
            combat.Tick(1);
            Assert.That(combat.Target == null, Is.True);
            Assert.That(combat.ShotsFired, Is.EqualTo(2));
            enemy.enabled = true;
            Assert.That(enemies.Count, Is.EqualTo(1));
            enemy.Health.Initialize(1000);
            int before = combat.ShotsFired;
            combat.Tick(1);
            Assert.That(combat.ShotsFired - before, Is.EqualTo(5), "Immediate shot plus four quarter-second boundaries, independent of frame count.");
        }
    }
}
