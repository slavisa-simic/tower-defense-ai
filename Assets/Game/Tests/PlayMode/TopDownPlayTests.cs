using System.Collections;
using NUnit.Framework;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ShorelineDefense.Tests
{
    // Run every Phase 1 gameplay regression against the persisted primary scene as well.
    public sealed class TopDownPlayTests : PrototypePlayTests
    {
        protected override string ScenePath => "Assets/Game/Scenes/CoastalDefense_TopDown.unity";
        protected override void PrepareSpawner() { var spawner = Object.FindFirstObjectByType<EnemySpawner>(); spawner.ConfigureBorder(null); spawner.gameObject.AddComponent<PrototypeSpawnLoop>(); Object.FindFirstObjectByType<ShorelineDefense.UI.PrototypeHud>().ConfigureLegacy(); }

        [UnityTest]
        public IEnumerator OccupiedAndBlockedSpawnPointsAreSkippedAndReopenedLaneResumes()
        {
            yield return SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Single);
            yield return null;
            PrepareSpawner();
            Time.timeScale = 4;
            var spawner = Object.FindFirstObjectByType<EnemySpawner>();
            var grid = Object.FindFirstObjectByType<GridManager>();
            float deadline = Time.realtimeSinceStartup + 5;
            while (spawner.TotalSpawned == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(spawner.TotalSpawned, Is.EqualTo(1));
            var first = Object.FindFirstObjectByType<EnemyController>();
            first.enabled = false;
            first.transform.position = spawner.SpawnPoints[0].position;
            for (int i = 1; i < spawner.SpawnPoints.Count; i++)
            {
                grid.Grid.TryWorldToCell(spawner.SpawnPoints[i].position, out var cell);
                grid.Grid.SetCell(cell, new GridCell(TerrainType.Blocked, false, false));
            }
            yield return new WaitForSeconds(3.2f);
            Assert.That(spawner.TotalSpawned, Is.EqualTo(1), "No spawn may overlap the parked enemy or use a blocked cell.");
            grid.Grid.TryWorldToCell(spawner.SpawnPoints[1].position, out var reopened);
            grid.Grid.SetCell(reopened, new GridCell(TerrainType.Water, true, false));
            yield return new WaitForSeconds(3.2f);
            Assert.That(spawner.TotalSpawned, Is.EqualTo(2), "A blocked lane must not stall the other coast points.");
            LogAssert.NoUnexpectedReceived();
        }
    }
}
