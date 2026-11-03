using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Persistence;
using ShorelineDefense.Progression;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class BalancePlayTests
    {
        private string previous, directory;
        static T Find<T>()where T:Object=>Object.FindFirstObjectByType<T>();
        [UnitySetUp] public IEnumerator Setup()
        {
            previous=LocalGameData.Files.Root;directory=Path.Combine(Path.GetTempPath(),"BorderDefendersBalance",Guid.NewGuid().ToString());LocalGameData.UseDirectory(directory);LocalGameData.Tutorial.SetEnabled(false);
            yield return SceneManager.LoadSceneAsync("CoastalDefense_TopDown");yield return null;Time.timeScale=0;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var scene=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("Balance cleanup"));yield return SceneManager.UnloadSceneAsync(scene);
            LocalGameData.UseDirectory(previous);Time.timeScale=1;if(Directory.Exists(directory))Directory.Delete(directory,true);LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator PrecisionFireSelectsHeavyOverAdvancedLarvaAndActuallyPiercesArmor()
        {
            var b=Find<BuildManager>();var sniper=Find<BuildHud>().Defenses.Single(d=>d.Id=="sniper-tower");
            Assert.That(b.TryPlaceDefense(sniper,new Vector2Int(18,8),out var defense),Is.EqualTo(PlacementResult.Valid));
            var sp=Find<EnemySpawner>();var roster=Find<WaveManager>().EndlessSettings.enemies;
            Assert.IsTrue(sp.TrySpawnAt(roster.Single(p=>p.Definition.Kind==EnemyKind.Broodling),b.Battlefield.Grid.CellToWorld(new Vector2Int(23,8)),out var larva));
            Assert.IsTrue(sp.TrySpawnAt(roster.Single(p=>p.Definition.Kind==EnemyKind.Bulwark),b.Battlefield.Grid.CellToWorld(new Vector2Int(14,8)),out var heavy));
            int hp=heavy.Health.CurrentHealth;var weapon=defense.GetComponent<TurretCombat>();weapon.Tick(.01f);
            Assert.That(weapon.Target,Is.SameAs(heavy));Assert.That(hp-heavy.Health.CurrentHealth,Is.EqualTo(defense.Damage));Assert.That(larva.Health.CurrentHealth,Is.EqualTo(larva.Definition.MaxHealth));
            heavy.Health.TakeDefenseDamage(heavy.Health.CurrentHealth-10,1);
            weapon.Tick(.2f);Assert.That(weapon.Target,Is.SameAs(heavy),"Precision priority must not abandon a wounded heavy for healthy larvae");yield return null;
        }
        [UnityTest] public IEnumerator EarlyCargoAdmissionCannotOvershootWithAFullDriftPod()
        {
            var w=Find<WaveManager>();var registry=Find<EnemyRegistry>();w.Landings.ForcedArrival=ArrivalMethod.DriftPod;Assert.IsTrue(w.TryStartWave());
            int upper=Mathf.CeilToInt(w.EndlessSettings.PressureTarget(1)*(1+w.EndlessSettings.pressureHysteresis));
            for(int i=0;i<60;i++)
            {
                w.Tick(.1f);Assert.That(registry.Count+w.Landings.IncomingPassengers,Is.LessThanOrEqualTo(upper));
                foreach(var e in registry.Active.ToArray())e.Tick(.1f);yield return null;
            }
            Assert.That(Find<EnemySpawner>().TotalSpawned,Is.GreaterThan(0));
        }
        [UnityTest] public IEnumerator SpitterHoldsRangeWhileReloadingAndResumesAfterTargetRemoval()
        {
            var b=Find<BuildManager>();var mg=Find<BuildHud>().Defenses.Single(d=>d.Id=="machine-gun-nest");
            Assert.That(b.TryPlaceDefense(mg,new Vector2Int(18,8),out var defense),Is.EqualTo(PlacementResult.Valid));
            var sp=Find<EnemySpawner>();var prefab=Find<WaveManager>().EndlessSettings.enemies.Single(p=>p.Definition.Kind==EnemyKind.Spitter);
            Assert.IsTrue(sp.TrySpawnAt(prefab,b.Battlefield.Grid.CellToWorld(new Vector2Int(13,8)),out var enemy));
            Vector3 position=enemy.transform.position;enemy.Tick(2);enemy.Tick(1);
            Assert.That(enemy.transform.position,Is.EqualTo(position));Assert.That(enemy.GetComponent<SpitterAttack>().ShotsFired,Is.EqualTo(1));
            Assert.IsTrue(b.TryDismantle(defense,out _));enemy.Tick(.6f);Assert.That(enemy.Movement.DistanceTravelled,Is.GreaterThan(0));yield return null;
        }
        [UnityTest] public IEnumerator ArmoredBurnDamageDoesNotDependOnTickSize()
        {
            var sp=Find<EnemySpawner>();var prefab=Find<WaveManager>().EndlessSettings.enemies.Single(p=>p.Definition.Kind==EnemyKind.Bulwark);
            Assert.IsTrue(sp.TrySpawnAt(prefab,sp.Battlefield.Grid.CellToWorld(new Vector2Int(12,8)),out var a));Assert.IsTrue(sp.TrySpawnAt(prefab,sp.Battlefield.Grid.CellToWorld(new Vector2Int(14,8)),out var b));
            int hp=a.Health.CurrentHealth;var big=a.GetComponent<EnemyDamageOverTime>();var small=b.GetComponent<EnemyDamageOverTime>();big.ApplyBurn(2,15);small.ApplyBurn(2,15);big.Tick(2);
            for(int i=0;i<100;i++)small.Tick(.02f);
            Assert.That(hp-a.Health.CurrentHealth,Is.EqualTo(15));Assert.That(a.Health.CurrentHealth,Is.EqualTo(b.Health.CurrentHealth));yield return null;
        }
        [UnityTest] public IEnumerator FlameResearchCadenceSurvivesFixedSmallTicks()
        {
            var b=Find<BuildManager>();b.ConfigureMeta(new MetaModifierService(new RunMetaSnapshot{fireRate=1.05f}));
            var definition=Find<BuildHud>().Defenses.Single(d=>d.Id=="flamethrower");Assert.That(b.TryPlaceDefense(definition,new Vector2Int(18,8),out var flame),Is.EqualTo(PlacementResult.Valid));
            var sp=Find<EnemySpawner>();Assert.IsTrue(sp.TrySpawnAt(sp.DefaultPrefab,flame.transform.position+Vector3.left*2,out var enemy));enemy.Health.Initialize(100000);
            int pulses=0;enemy.Health.Damaged+=_=>pulses++;for(int i=0;i<400;i++)flame.GetComponent<FlameWeapon>().Tick(.05f);
            Assert.That(pulses,Is.InRange(104,106),"20s at researched 5.25 pulses/s; reset-to-period incorrectly yields only 100");yield return null;
        }
        [UnityTest] public IEnumerator BalanceLogIncludesCommittedManagementAndCompletionRewardOnlyOnce()
        {
            var b=Find<BuildManager>();var w=Find<WaveManager>();var registry=Find<EnemyRegistry>();b.Economy.AddMoney(1000);
            using(var log=new BalanceRunLog(Find<RunSession>(),b,w,Find<RunStatistics>(),registry))
            {
                var d=Find<BuildHud>().Defenses.Single(n=>n.Id=="machine-gun-nest");Assert.That(b.TryPlaceDefense(d,new Vector2Int(14,8),out var mg),Is.EqualTo(PlacementResult.Valid));
                int cost=b.UpgradeCost(mg);Assert.IsTrue(b.TryUpgrade(mg));mg.TakeDamage(100);int repair=b.RepairCost(mg);Assert.IsTrue(b.TryRepair(mg));
                long before=b.Economy.TotalEarned;Assert.IsTrue(w.TryStartWave());
                for(int i=0;i<2000&&w.State!=WaveState.WaveComplete;i++){w.Tick(.2f);foreach(var e in registry.Active.ToArray())e.Health.TakeDefenseDamage(int.MaxValue);if(w.State!=WaveState.WaveComplete)log.Tick(.2f);yield return null;}
                Assert.That(w.State,Is.EqualTo(WaveState.WaveComplete));w.Tick(2);Assert.That(w.CurrentWaveNumber,Is.EqualTo(2));log.Tick(0);
                var lines=File.ReadAllLines(Path.Combine(directory,"BalanceLogs",Find<RunSession>().RunId+".csv"));Assert.That(lines.Length,Is.EqualTo(2));
                var cells=lines[1].Split(',');Assert.That(cells[1],Is.EqualTo("1"));Assert.That(cells[2],Is.EqualTo("WaveComplete"));Assert.That(long.Parse(cells[7]),Is.EqualTo(b.Economy.TotalEarned-before));Assert.That(int.Parse(cells[9]),Is.EqualTo(repair));Assert.That(int.Parse(cells[10]),Is.EqualTo(cost));
            }
        }
        [UnityTest] public IEnumerator BroodmotherFirstBroodArrivesEarlyAndStillRespectsTheConcurrentCap()
        {
            var w=Find<WaveManager>();Assert.IsTrue(w.TryStartWave());var sp=Find<EnemySpawner>();
            Assert.IsTrue(sp.TrySpawnReinforcement(w.EndlessSettings.bossEncounters.bosses.Single(p=>p.Definition.Kind==EnemyKind.Broodmother),sp.Battlefield.Grid.CellToWorld(new Vector2Int(12,9)),out var boss));
            var ability=boss.GetComponent<BroodmotherSpawn>();ability.Tick(2.9f);Assert.That(ability.TotalSpawned,Is.Zero);ability.Tick(.11f);Assert.That(ability.TotalSpawned,Is.EqualTo(6));
            for(int i=0;i<20;i++)ability.Tick(8);Assert.That(ability.Concurrent,Is.EqualTo(18));Assert.That(w.AliveCount,Is.EqualTo(19));yield return null;
        }
    }
}
