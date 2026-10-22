using System.Collections;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Waves;
using ShorelineDefense.Economy;
using ShorelineDefense.Building;
using ShorelineDefense.Grid;
using ShorelineDefense.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace ShorelineDefense.Tests
{
    public sealed class SurvivalPlayTests
    {
        private EnemySpawner spawner;private EnemyRegistry registry;private WaveManager waves;private BorderBreachService border;private RunStatistics stats;private EconomyManager money;
        [UnitySetUp] public IEnumerator Setup()
        {Time.timeScale=0;yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity");yield return null;spawner=Object.FindFirstObjectByType<EnemySpawner>();registry=Object.FindFirstObjectByType<EnemyRegistry>();waves=Object.FindFirstObjectByType<WaveManager>();border=Object.FindFirstObjectByType<BorderBreachService>();stats=Object.FindFirstObjectByType<RunStatistics>();money=Object.FindFirstObjectByType<EconomyManager>();}
        [UnityTearDown] public IEnumerator Cleanup()
        {Time.timeScale=1;var previous=SceneManager.GetActiveScene();SceneManager.SetActiveScene(SceneManager.CreateScene("Survival cleanup"));yield return SceneManager.UnloadSceneAsync(previous);LogAssert.NoUnexpectedReceived();}
        private EnemyController Spawn(EnemyController prefab)
        {Assert.IsTrue(spawner.TrySpawnAt(prefab,spawner.Battlefield.Grid.CellToWorld(new Vector2Int(27,4)),out var enemy));return enemy;}
        [UnityTest] public IEnumerator EachBreachResolvesOnceWithoutRewardAndThresholdStopsBoats()
        {
            int events=0;stats.OnHomicidesChanged+=_=>events++;int previousMoney=money.Money;
            foreach(var prefab in waves.EndlessSettings.enemies.Take(3))
            {var enemy=Spawn(prefab);enemy.Tick(100);Assert.IsFalse(border.TryBreach(enemy));Assert.IsFalse(enemy.IsTargetable);Assert.That(registry.Count,Is.Zero);enemy.Health.TakeDefenseDamage(10000);yield return null;}
            Assert.That(stats.Homicides,Is.EqualTo(175));Assert.That(stats.TotalBreaches,Is.EqualTo(3));Assert.That(events,Is.EqualTo(3));Assert.That(stats.TotalKills,Is.Zero);Assert.That(money.Money,Is.EqualTo(previousMoney));
            border.ConfigureThreshold(200);Assert.IsTrue(waves.TryStartWave());waves.Tick(.3f);Assert.Greater(Object.FindFirstObjectByType<BoatLandingSystem>().ActiveBoats,0);
            Spawn(spawner.DefaultPrefab).Tick(100);Assert.IsFalse(Object.FindFirstObjectByType<GameStateController>().IsPlaying);Assert.That(waves.State,Is.EqualTo(WaveState.GameOver));Assert.That(Object.FindFirstObjectByType<BoatLandingSystem>().ActiveBoats,Is.Zero);
            Assert.That(stats.Report,Does.Contain("HOMICIDES 200"));Assert.That(Object.FindFirstObjectByType<PrototypeHud>().GameOverVisible,Is.True);Assert.That(Object.FindFirstObjectByType<PrototypeHud>().HealthText,Does.Contain("200 / 200"));
            var feedback=Object.FindFirstObjectByType<CombatFeedbackHud>();feedback.enabled=false;feedback.enabled=true;
            Assert.That(feedback.GetComponent<UIDocument>().rootVisualElement.Q<Label>("game-over").text,Is.EqualTo(stats.Report));yield return null;
        }
        [UnityTest] public IEnumerator UnlimitedModeKillsMoneyAndActiveCapRemainCorrect()
        {
            border.ConfigureThreshold(0);spawner.ConfigureWave(1,1.25f,1.1f);var enemy=Spawn(spawner.DefaultPrefab);Assert.That(enemy.Health.MaxHealth,Is.EqualTo(125));
            Assert.IsFalse(spawner.TrySpawnAt(spawner.DefaultPrefab,spawner.Battlefield.Grid.CellToWorld(new Vector2Int(20,9)),out _));
            enemy.Health.TakeDefenseDamage(10000);enemy.Health.TakeDefenseDamage(10000);Assert.That(stats.TotalKills,Is.EqualTo(1));Assert.That(stats.BasicKills,Is.EqualTo(1));Assert.That(stats.TotalMoneyEarned,Is.EqualTo(25));
            foreach(var prefab in waves.EndlessSettings.enemies.Take(3).Skip(1)){var killed=Spawn(prefab);killed.Health.TakeDefenseDamage(10000);yield return null;}
            Assert.That(stats.RunnerKills,Is.EqualTo(1));Assert.That(stats.BruteKills,Is.EqualTo(1));Assert.That(stats.TotalMoneyEarned,Is.EqualTo(135));
            Assert.IsTrue(money.TrySpend(50));Assert.That(stats.TotalMoneySpent,Is.EqualTo(50));Assert.IsFalse(money.TrySpend(int.MaxValue));Assert.That(stats.TotalMoneySpent,Is.EqualTo(50));
            for(int i=0;i<45;i++){Spawn(spawner.DefaultPrefab).Tick(100);yield return null;}
            Assert.That(stats.Homicides,Is.EqualTo(1125));Assert.IsTrue(Object.FindFirstObjectByType<GameStateController>().IsPlaying);Assert.That(Object.FindFirstObjectByType<PrototypeHud>().HealthText,Does.Not.Contain("/ 0"));
        }
        [UnityTest, Timeout(360000)] public IEnumerator HundredAcceleratedBoatWavesCompleteAndReleaseTransientObjects()
        {
            border.ConfigureThreshold(0);var boats=Object.FindFirstObjectByType<BoatLandingSystem>();int completed=0;int peakEnemies=0,peakBoats=0;
            for(int number=1;number<=100;number++)
            {
                Assert.That(waves.CurrentWaveNumber,Is.EqualTo(number));Assert.That(waves.State,Is.EqualTo(WaveState.Preparation));
                waves.Tick(waves.Events?.Current?.WarningRemaining ?? 0);Assert.IsTrue(waves.TryStartWave());
                int count=waves.CurrentWave.ScheduledCount;long breaches=stats.TotalBreaches;int reward=waves.CurrentWave.CompletionReward;int balance=money.Money;
                for(int tick=0;tick<2000&&waves.State!=WaveState.WaveComplete;tick++)
                {
                    waves.Tick(.5f);peakEnemies=Mathf.Max(peakEnemies,registry.Count);peakBoats=Mathf.Max(peakBoats,boats.ActiveBoats);
                    foreach(var e in registry.Active.ToArray())e.Tick(100);
                    // Let Unity destroy enemy/transport objects and effects between accelerated steps.
                    yield return null;
                }
                Assert.That(waves.State,Is.EqualTo(WaveState.WaveComplete),"Wave "+number);Assert.That(stats.TotalBreaches-breaches,Is.EqualTo(count));Assert.That(money.Money,Is.EqualTo(balance+reward));Assert.That(registry.Count,Is.Zero);
                waves.Tick(2);for(int t=0;t<40&&boats.ActiveBoats>0;t++)boats.Tick(1);yield return null;
                Assert.That(boats.ActiveBoats,Is.Zero);Assert.That(boats.IncomingPassengers,Is.Zero);Assert.That(Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length,Is.Zero);Assert.LessOrEqual(spawner.PooledEnemies,400);completed++;
                Assert.LessOrEqual(Object.FindObjectsByType<WaveDefinition>(FindObjectsSortMode.None).Count(w=>w.hideFlags==HideFlags.DontSave),1);
            }
            Assert.That(completed,Is.EqualTo(100));Assert.That(waves.CurrentWaveNumber,Is.EqualTo(101));Assert.That(stats.HighestWaveReached,Is.EqualTo(100));Assert.LessOrEqual(peakEnemies,waves.EndlessSettings.maximumActiveEnemies);
            float eventBoatFactor = waves.EndlessSettings.eventSettings != null ? waves.EndlessSettings.eventSettings.events.Max(e=>e.modifiers.boatConcurrency) : 1;
            Assert.LessOrEqual(peakBoats,Mathf.Clamp(Mathf.CeilToInt(waves.EndlessSettings.maximumConcurrentBoats*eventBoatFactor)+waves.EndlessSettings.surgeExtraBoats,1,32));
            Assert.That(Object.FindFirstObjectByType<WaveHud>().GetComponent<UIDocument>().rootVisualElement.Q<Label>("wave-number").text, Is.EqualTo("WAVE 101"));
            Debug.Log("Phase4A 100 real boat waves: breaches="+stats.TotalBreaches+", homicides="+stats.Homicides+", earned="+stats.TotalMoneyEarned+", peak enemies="+peakEnemies+", peak boats="+peakBoats);
        }
        [UnityTest] public IEnumerator ThresholdOnFinalWaveMemberNeverPaysCompletionReward()
        {
            border.ConfigureThreshold(waves.CurrentWave.ScheduledCount * 25);Assert.IsTrue(waves.TryStartWave());
            for(int tick=0;tick<250&&Object.FindFirstObjectByType<GameStateController>().IsPlaying;tick++)
            { waves.Tick(.5f);foreach(var e in registry.Active.ToArray())e.Tick(100);yield return null; }
            Assert.That(stats.Homicides,Is.EqualTo(waves.CurrentWave.ScheduledCount * 25));Assert.That(stats.TotalBreaches,Is.EqualTo(waves.CurrentWave.ScheduledCount));Assert.That(waves.PendingCount,Is.Zero);Assert.That(waves.AliveCount,Is.Zero);
            Assert.That(waves.State,Is.EqualTo(WaveState.GameOver));Assert.That(waves.CompletedWaves,Is.Zero);Assert.That(money.Money,Is.EqualTo(500));Assert.That(stats.TotalMoneyEarned,Is.Zero);
        }
        [UnityTest] public IEnumerator BothTurretsTargetEveryEnemyKindAndPresentationUsesItsOwnArt()
        {
            money.AddMoney(1000);var builder=Object.FindFirstObjectByType<BuildManager>();var hud=Object.FindFirstObjectByType<BuildHud>();
            foreach(int index in new[]{0,3})
            {
                var cell=new Vector2Int(18,index==0?5:12);Assert.That(builder.TryPlaceDefense(hud.Defenses[index],cell,out var defense),Is.EqualTo(PlacementResult.Valid));var turret=defense.GetComponent<ShorelineDefense.Defenses.TurretCombat>();
                foreach(var prefab in waves.EndlessSettings.enemies.Take(3))
                {
                    var enemy=Spawn(prefab);enemy.transform.position=spawner.Battlefield.Grid.CellToWorld(cell+Vector2Int.right);enemy.Health.Initialize(1000);int shots=turret.ShotsFired;
                    turret.Tick(2.1f);Assert.Greater(turret.ShotsFired,shots,"Turret "+index+" kind "+prefab.Definition.Kind+" targetable "+enemy.IsTargetable+" registered "+registry.Count+" range "+turret.IsValidTarget(enemy));Assert.Less(enemy.Health.CurrentHealth,1000,"Turret "+index+" kind "+prefab.Definition.Kind+" target "+turret.Target?.Definition.Kind+" registry "+registry.Count);
                    var body=enemy.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();Assert.IsNotNull(body.sprite);Assert.AreEqual(prefab.transform.Find("Visual/Body").GetComponent<SpriteRenderer>().sprite.texture,body.sprite.texture);
                    enemy.Health.TakeDamage(int.MaxValue);yield return null;
                }
            }
        }
        [UnityTest] public IEnumerator MixedCargoCapacityAndAllTypesUseDefenseRules()
        {
            var boats=Object.FindFirstObjectByType<BoatLandingSystem>();var roster=waves.EndlessSettings.enemies;Assert.IsFalse(boats.TryDispatch(new[]{roster[2],roster[2],roster[2]}));Assert.IsTrue(boats.TryDispatch(new[]{roster[2],roster[1],roster[0]}));
            for(int i=0;i<80&&boats.IncomingPassengers>0;i++){boats.Tick(.5f);foreach(var e in registry.Active)e.Movement.Tick(.5f);yield return null;}
            Assert.That(registry.Count,Is.EqualTo(3));CollectionAssert.AreEquivalent(roster.Take(3).Select(p=>p.Definition.Kind),registry.Active.Select(e=>e.Definition.Kind));
            var builder=Object.FindFirstObjectByType<BuildManager>();var hud=Object.FindFirstObjectByType<BuildHud>();money.AddMoney(1000);
            var cell=new Vector2Int(18,9);Assert.That(builder.TryPlaceDefense(hud.Defenses[1],cell,out var wire),Is.EqualTo(PlacementResult.Valid));
            foreach(var e in registry.Active){e.transform.position=spawner.Battlefield.Grid.CellToWorld(cell);Assert.That(e.Movement.CurrentSpeed,Is.EqualTo(e.Movement.BaseSpeed*.5f).Within(.001));}
            Assert.That(builder.TryPlaceDefense(hud.Defenses[2],cell+Vector2Int.right,out var mineDefense),Is.EqualTo(PlacementResult.Valid));
            foreach(var e in registry.Active)e.transform.position=mineDefense.transform.position;
            var mine=mineDefense.GetComponent<ShorelineDefense.Defenses.LandMine>();mine.Tick(1);Assert.That(stats.BasicKills,Is.EqualTo(1));Assert.That(stats.RunnerKills,Is.EqualTo(1));Assert.That(registry.Count,Is.EqualTo(1));Assert.That(registry.Active[0].Health.CurrentHealth,Is.EqualTo(450));
            yield return null;
        }
    }
}
