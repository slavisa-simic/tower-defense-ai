using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using Object=UnityEngine.Object;
namespace ShorelineDefense.Tests
{
    public sealed class AquaticPlayTests
    {
        static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
        EnemySpawner Spawn=>Find<EnemySpawner>();WaveManager Waves=>Find<WaveManager>();
        EnemyController Prefab(EnemyKind kind)=>Waves.EndlessSettings.enemies.Concat(Waves.EndlessSettings.bossEncounters.bosses).Single(e=>e.Definition.Kind==kind);
        Vector3 At(int x,int y)=>Spawn.Battlefield.Grid.CellToWorld(new Vector2Int(x,y));
        IEnumerator Load(string map){RunSession.Pending=null;yield return SceneManager.LoadSceneAsync(map);yield return null;Time.timeScale=0;Find<BorderBreachService>().ConfigureThreshold(0);}
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;yield return SceneManager.LoadSceneAsync("CoastalDefense_TopDown");yield return null;}
        [UnityTest]public IEnumerator AllArrivalTypesFitBothMapsAndResolveWithoutTargetableWaterCargo()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            {
                yield return Load(map);var system=Find<AssaultCarrierSystem>();var registry=Find<EnemyRegistry>();
                foreach(var definition in system.Carriers)
                {
                    Assert.IsTrue(system.LandingPoints.Any(p=>system.IsValidApproach(p,definition)),map+" "+definition.method+" clearance");
                    system.ForcedArrival=definition.method;
                    var prefab=Prefab(definition.method==ArrivalMethod.LeviathanCarrier?EnemyKind.Bulwark:EnemyKind.Crawler);
                    Assert.IsTrue(system.TryDispatch(prefab,1),definition.method.ToString());Assert.Zero(registry.Count,"Water cargo must not register");
                    int spawned=Spawn.TotalSpawned;
                    for(int i=0;i<250&&(system.ActiveCarriers>0||registry.Count>0);i++)
                    {system.Tick(.2f);foreach(var e in registry.Active.ToArray())e.Tick(100);yield return null;}
                    Assert.AreEqual(spawned+1,Spawn.TotalSpawned);Assert.Zero(system.IncomingPassengers);Assert.Zero(system.ActiveCarriers);Assert.Zero(registry.Count);
                }
            }
        }
        [UnityTest]public IEnumerator SpitterDamagesNearbyStructureThroughVisibleProjectileAndRepairStillWorks()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            {
                yield return Load(map);var build=Find<BuildManager>();build.Economy.AddMoney(2000);var mg=Find<BuildHud>().Defenses.Single(d=>d.Id=="machine-gun-nest");
                Assert.AreEqual(PlacementResult.Valid,build.TryPlaceDefense(mg,new Vector2Int(13,5),out var defense));
                Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Spitter),At(7,5),out var far));
                far.transform.position=defense.transform.position+Vector3.left*(far.Definition.StructureRangeCells+1)*defense.Grid.CellSize;
                far.GetComponent<SpitterAttack>().Tick(2);Assert.Zero(far.GetComponent<SpitterAttack>().ShotsFired);far.Health.TakeDefenseDamage(int.MaxValue);yield return null;
                Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Spitter),At(9,5),out var enemy));var attack=enemy.GetComponent<SpitterAttack>();int hp=defense.CurrentHealth;
                attack.Tick(2);Assert.AreEqual(1,attack.ShotsFired);var shot=Find<CorrosiveSpit>();Assert.NotNull(shot);Assert.NotNull(shot.GetComponent<SpriteRenderer>().sprite);Assert.AreEqual(hp,defense.CurrentHealth);
                shot.Tick(.7f);Assert.AreEqual(hp-24,defense.CurrentHealth);shot.Tick(10);Assert.AreEqual(hp-24,defense.CurrentHealth,"No double impact");Assert.IsTrue(build.TryRepair(defense));Assert.AreEqual(hp,defense.CurrentHealth);
            }
        }
        [UnityTest]public IEnumerator BurrowerIsVisibleLimitedIgnoresWireThenEmergesBeforeBorderAndBreachesOnce()
        {
            foreach(string map in new[]{"CoastalDefense_TopDown","DanubeIndustrialPort"})
            {
                yield return Load(map);var registry=Find<EnemyRegistry>();Assert.IsTrue(Spawn.TrySpawnAt(Prefab(EnemyKind.Burrower),At(8,5),out var enemy));var ability=enemy.GetComponent<BurrowerTraversal>();
                var burn=enemy.GetComponent<EnemyDamageOverTime>();burn.ApplyBurn(5,10);int hp=enemy.Health.CurrentHealth;
                ability.Move(1.1f);Assert.IsTrue(ability.IsBurrowed);Assert.IsFalse(enemy.IsTargetable);Assert.IsTrue(enemy.Movement.IgnoreWireSlow);Assert.IsNull(registry.FindClosest(enemy.transform.position,20));burn.Tick(1);Assert.AreEqual(hp-10,enemy.Health.CurrentHealth,"Surface burn persists underground");yield return null;
                var renderer=enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();Assert.IsTrue(renderer.enabled);Assert.That(renderer.sprite.name,Does.Contain("special").Or.Contain("underground"));
                for(int i=0;i<400&&ability.IsBurrowed;i++)ability.Move(.02f);
                Assert.IsFalse(ability.IsBurrowed);Assert.IsTrue(enemy.IsTargetable);Assert.IsFalse(enemy.Movement.IgnoreWireSlow);Assert.LessOrEqual(ability.DistanceBurrowed,Spawn.Battlefield.Grid.CellSize*5+.01f);Assert.GreaterOrEqual(enemy.Movement.RemainingPathDistance,Spawn.Battlefield.Grid.CellSize*8-.01f);
                long before=Find<RunStatistics>().TotalBreaches;enemy.Tick(100);Assert.AreEqual(before+1,Find<RunStatistics>().TotalBreaches);Assert.IsFalse(Find<BorderBreachService>().TryBreach(enemy));
            }
        }
        [UnityTest]public IEnumerator BroodmotherSpawnsAreCappedRegisteredAndIncludedInWaveAliveCount()
        {
            yield return Load("CoastalDefense_TopDown");Assert.IsTrue(Waves.TryStartWave());Assert.IsTrue(Spawn.TrySpawnReinforcement(Prefab(EnemyKind.Broodmother),At(12,9),out var mother));
            var ability=mother.GetComponent<BroodmotherSpawn>();var registry=Find<EnemyRegistry>();
            for(int i=0;i<4;i++)ability.Tick(8);Assert.AreEqual(18,ability.Concurrent);Assert.AreEqual(18,ability.TotalSpawned);Assert.AreEqual(19,Waves.AliveCount);Assert.AreEqual(registry.Count,Waves.AliveCount);
            for(int iteration=0;iteration<20;iteration++)
            {foreach(var child in registry.Active.Where(e=>e!=mother).ToArray())child.Health.TakeDefenseDamage(int.MaxValue);Assert.Zero(ability.Concurrent,"Removal updates ownership before pooled reuse");yield return null;ability.Tick(8);}
            Assert.AreEqual(60,ability.TotalSpawned);foreach(var child in registry.Active.Where(e=>e!=mother).ToArray())child.Health.TakeDefenseDamage(int.MaxValue);mother.Health.TakeDefenseDamage(int.MaxValue);yield return null;Assert.Zero(Waves.AliveCount);
        }
        [UnityTest]public IEnumerator ThreeHundredFiftyBroodlingsRespectRegistryCapRewardAndPooling()
        {
            yield return Load("CoastalDefense_TopDown");Spawn.ConfigureWave(400,1,1);Spawn.ConfigureDeployment(.1f);var prefab=Prefab(EnemyKind.Broodling);var registry=Find<EnemyRegistry>();
            int count=0;for(int x=6;x<30&&count<350;x++)for(int y=0;y<18&&count<350;y++)if(Spawn.TrySpawnAt(prefab,At(x,y),out _))count++;
            Assert.AreEqual(350,count);Assert.AreEqual(350,registry.Count);Assert.AreEqual(350,Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Count(e=>e.IsTargetable));
            long income=Find<RunStatistics>().TotalMoneyEarned;foreach(var enemy in registry.Active.ToArray())enemy.Health.TakeDefenseDamage(int.MaxValue);yield return null;
            Assert.Zero(registry.Count);Assert.AreEqual(350*prefab.Definition.KillReward,Find<RunStatistics>().TotalMoneyEarned-income);Assert.LessOrEqual(Spawn.PooledEnemies,400);
            Assert.IsTrue(Spawn.TrySpawnAt(prefab,At(8,5),out var reused));Assert.IsTrue(reused.IsTargetable);Assert.Greater(Spawn.ReusedEnemies,0);Assert.IsFalse(reused.GetComponent<EnemyDamageOverTime>().Burning);
        }
    }
}
