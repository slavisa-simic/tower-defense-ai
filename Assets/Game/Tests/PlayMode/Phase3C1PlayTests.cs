using System.Collections;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Base;
using ShorelineDefense.Building;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ShorelineDefense.Tests
{
    public sealed class Phase3C1PlayTests
    {
        private BuildManager builder;
        private WaveManager waves;
        private BoatLandingSystem boats;
        private EnemyRegistry enemies;
        private EnemySpawner spawner;
        [UnitySetUp] public IEnumerator SetUp()
        {
            Time.timeScale=0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");
            yield return null;
            builder=Object.FindFirstObjectByType<BuildManager>();waves=Object.FindFirstObjectByType<WaveManager>();
            boats=Object.FindFirstObjectByType<BoatLandingSystem>();enemies=Object.FindFirstObjectByType<EnemyRegistry>();spawner=Object.FindFirstObjectByType<EnemySpawner>();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Time.timeScale=1;var previous=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("Phase3C1 cleanup"));
            yield return SceneManager.UnloadSceneAsync(previous);LogAssert.NoUnexpectedReceived();
        }
        private DefenseBase Wire(Vector2Int cell)
        {
            var data=Object.FindFirstObjectByType<BuildHud>().Defenses.First(d=>d.Prefab.GetComponent<BarbedWire>()!=null);
            Assert.That(builder.TryPlaceDefense(data,cell,out var wire),Is.EqualTo(PlacementResult.Valid));return wire;
        }
        [UnityTest] public IEnumerator WirePlacementRemovalAndPreviewKeepTopologyAndSlowdown()
        {
            var cell=new Vector2Int(15,8);var center=Wire(cell);var visual=center.GetComponent<ConnectedWirePresentation>();
            Assert.That(visual.Mask,Is.Zero);
            var west=Wire(cell+Vector2Int.left);Assert.That(visual.Mask,Is.EqualTo(8));Assert.That(west.GetComponent<ConnectedWirePresentation>().Mask,Is.EqualTo(2));
            var east=Wire(cell+Vector2Int.right);Assert.That(visual.Mask,Is.EqualTo(10));
            var north=Wire(cell+Vector2Int.up);Assert.That(visual.Mask,Is.EqualTo(11));
            var south=Wire(cell+Vector2Int.down);Assert.That(visual.Mask,Is.EqualTo(15));
            var placement=Object.FindFirstObjectByType<PlacementController>();placement.Select(center.Definition);placement.HoverCell(cell+Vector2Int.up*2);
            Assert.That(GameObject.Find("Placement Preview").GetComponent<SpriteRenderer>().sprite,Is.SameAs(visual.SpriteFor(4)));
            Assert.That(north.GetComponent<ConnectedWirePresentation>().Mask,Is.EqualTo(4),"Hover must not mutate neighbors");
            for(int i=0;i<16;i++)Assert.That(visual.SpriteFor(i),Is.Not.Null);
            Assert.That(center.Grid.GetCell(cell).Walkable,Is.True);Assert.That(center.Grid.GetCell(cell).TraversalCost,Is.EqualTo(2));
            Assert.That(center.Grid.MovementMultiplierAt(center.transform.position),Is.EqualTo(.5f));
            north.TakeDamage(10000);Assert.That(visual.Mask,Is.EqualTo(14));
            east.TakeDamage(10000);Assert.That(visual.Mask,Is.EqualTo(12));
            south.TakeDamage(10000);Assert.That(visual.Mask,Is.EqualTo(8));
            west.TakeDamage(10000);Assert.That(visual.Mask,Is.Zero);
            Assert.That(center.Grid.MovementMultiplierAt(center.transform.position),Is.EqualTo(.5f));
            center.TakeDamage(10000);Assert.That(builder.Battlefield.Grid.MovementMultiplierAt(builder.Battlefield.Grid.CellToWorld(cell)),Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator BoatsReserveDistinctRiverLanesAndUnloadThroughNormalSpawner()
        {
            Assert.That(boats.LandingPoints.Count,Is.EqualTo(5));Assert.That(boats.PassengerLimit(spawner.DefaultPrefab),Is.EqualTo(7));
            Assert.That(boats.TryDispatch(spawner.DefaultPrefab,8),Is.False);
            Assert.That(boats.TryDispatch(spawner.DefaultPrefab,2),Is.True);Assert.That(boats.TryDispatch(spawner.DefaultPrefab,2),Is.True);
            Assert.That(boats.ActiveBoats,Is.EqualTo(2));Assert.That(enemies.Count,Is.Zero);
            var positions=boats.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="Rubber Boat").Select(r=>r.transform.position.y).ToArray();
            Assert.That(positions.Distinct().Count(),Is.EqualTo(2));
            boats.Tick(10);Assert.That(enemies.Count,Is.EqualTo(2));Assert.That(boats.IncomingPassengers,Is.EqualTo(2));
            foreach(var enemy in enemies.Active)
            {
                Assert.That(enemy.Movement.HasPath,Is.True);Assert.That(Object.FindFirstObjectByType<ShorelineDefense.Core.BorderBreachService>().BreachCells,Has.Member(enemy.Movement.Path.Last()));
                builder.Battlefield.Grid.TryWorldToCell(enemy.transform.position,out var cell);Assert.That(builder.Battlefield.Grid.GetCell(cell).Terrain,Is.EqualTo(TerrainType.Beach));
                enemy.Movement.Tick(1);
            }
            boats.Tick(.3f);Assert.That(enemies.Count,Is.EqualTo(4));Assert.That(spawner.TotalSpawned,Is.EqualTo(4));Assert.That(boats.IncomingPassengers,Is.Zero);
            boats.Tick(2);boats.Tick(10);Assert.That(boats.ActiveBoats,Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator WaveKeepsIncomingPassengersPendingAndCompletesOnlyAfterLastKill()
        {
            Assert.That(waves.TryStartWave(),Is.True);waves.Tick(.3f);
            Assert.That(waves.PendingCount,Is.EqualTo(waves.CurrentWave.ScheduledCount));Assert.That(waves.AliveCount,Is.Zero);Assert.That(boats.ActiveBoats,Is.EqualTo(1));
            waves.Tick(.1f);Assert.That(waves.State,Is.EqualTo(WaveState.Spawning));Assert.That(waves.CompletedWaves,Is.Zero);
            int before=builder.Economy.Money;int count=waves.CurrentWave.ScheduledCount;int reward=waves.CurrentWave.CompletionReward;
            for(int i=0;i<200&&waves.PendingCount>0;i++)
            {
                waves.Tick(.5f);
                foreach(var enemy in enemies.Active.ToArray())enemy.Health.TakeDefenseDamage(10000);
                if(waves.PendingCount>0)Assert.That(waves.CompletedWaves,Is.Zero);
            }
            Assert.That(spawner.TotalSpawned,Is.EqualTo(waves.CurrentWave.ScheduledCount));Assert.That(boats.TotalArrivals,Is.EqualTo(Mathf.CeilToInt(waves.CurrentWave.ScheduledCount/(float)boats.Capacity)));
            Assert.That(waves.CompletedWaves,Is.EqualTo(1));Assert.That(waves.PendingCount,Is.Zero);Assert.That(waves.AliveCount,Is.Zero);
            Assert.That(builder.Economy.Money,Is.EqualTo(before+count*25+reward));waves.Tick(2);Assert.That(builder.Economy.Money,Is.EqualTo(before+count*25+reward));
            yield return null;
        }
        [UnityTest] public IEnumerator BlockedLandingWaitsForValidRouteAndGameOverCancelsUnloading()
        {
            Assert.That(boats.TryDispatch(spawner.DefaultPrefab,3),Is.True);
            foreach(var position in boats.LandingPoints[0].unloadingPositions)
            {builder.Battlefield.Grid.TryWorldToCell(position,out var cell);builder.Battlefield.Grid.SetCell(cell,new GridCell(TerrainType.Blocked,false,false));}
            boats.Tick(20);Assert.That(enemies.Count,Is.Zero);Assert.That(boats.IncomingPassengers,Is.EqualTo(3));
            var restored=boats.LandingPoints[0].unloadingPositions[0];builder.Battlefield.Grid.TryWorldToCell(restored,out var open);
            builder.Battlefield.Grid.SetCell(open,new GridCell(TerrainType.Beach,true,false));boats.Tick(.2f);
            Assert.That(enemies.Count,Is.EqualTo(1));Assert.That(enemies.Active[0].Movement.HasPath,Is.True);
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(10000);boats.Tick(100);waves.Tick(100);
            Assert.That(boats.ActiveBoats,Is.Zero);Assert.That(boats.IncomingPassengers,Is.Zero);Assert.That(spawner.TotalSpawned,Is.EqualTo(1));
            Assert.That(waves.State,Is.EqualTo(WaveState.GameOver));Assert.That(boats.TryDispatch(spawner.DefaultPrefab,1),Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator IdleWeaponsStartLeftAndCameraAndVegetationStayOverhead()
        {
            var data=Object.FindFirstObjectByType<BuildHud>().Defenses;
            builder.Economy.AddMoney(5000);int column=8;
            foreach(var definition in data.Where(d=>d.Prefab.GetComponent<TurretCombat>()!=null||d.Prefab.GetComponent<AreaProjectileWeapon>()!=null||d.Prefab.GetComponent<FlameWeapon>()!=null))
            {
                Assert.That(builder.TryPlaceDefense(definition,new Vector2Int(column,8),out var defense),Is.EqualTo(PlacementResult.Valid));column+=3;
                var weapon=defense.GetComponentsInChildren<SpriteRenderer>().First(r=>r.transform.parent.name=="Rotating Weapon").transform.parent;
                Assert.That(Vector3.Dot(weapon.right,Vector3.left),Is.GreaterThan(.99f));Assert.That(defense.transform.rotation,Is.EqualTo(Quaternion.identity));
                weapon.rotation=Quaternion.identity;
            }
            Assert.That(Camera.main.orthographic,Is.True);Assert.That(Camera.main.transform.rotation,Is.EqualTo(Quaternion.identity));
            var vegetation=GameObject.Find("Riverbank Vegetation");Assert.That(vegetation.GetComponentsInChildren<MonoBehaviour>().Length,Is.Zero);
            Assert.That(vegetation.GetComponentsInChildren<Component>().Any(c=>c.GetType().Name.Contains("Collider")),Is.False);
            // .75s idle delay plus a 180-degree turn at 180 degrees/s, with frame margin.
            Time.timeScale=1;yield return new WaitForSeconds(2.2f);
            foreach(var weapon in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Rotating Weapon"))
                Assert.That(Vector3.Dot(weapon.right,Vector3.left),Is.GreaterThan(.99f),"Idle weapon must return left after the configured delay");
        }
        [UnityTest] public IEnumerator InvalidRiverLanesAndActiveLimitPreventBadArrivalsAndGameOverCancelsIncoming()
        {
            foreach(var point in boats.LandingPoints)point.berth+=Vector3.right*5;
            Assert.That(boats.TryDispatch(spawner.DefaultPrefab,1),Is.False,"Hull must never cross onto land");
            foreach(var point in boats.LandingPoints)point.berth-=Vector3.right*5;
            for(int i=0;i<3;i++)Assert.That(boats.TryDispatch(spawner.DefaultPrefab,5),Is.True);
            Assert.That(boats.TryDispatch(spawner.DefaultPrefab,5),Is.False,"Bound concurrent arrivals");
            Assert.That(boats.IncomingPassengers,Is.EqualTo(15));Assert.That(spawner.TotalSpawned,Is.Zero);
            Object.FindFirstObjectByType<BaseHealth>().TakeDamage(10000);boats.Tick(100);
            Assert.That(boats.ActiveBoats,Is.Zero);Assert.That(spawner.TotalSpawned,Is.Zero);Assert.That(waves.CompletedWaves,Is.Zero);
            yield return null;
        }
    }
}
