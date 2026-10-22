using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ShorelineDefense.Building;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Enemies;
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace ShorelineDefense.Tests
{
    public sealed class EndlessEventPlayTests
    {
        private WaveManager waves;
        private BuildManager build;
        private EnemyRegistry registry;
        private BuildHud hud;
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale = 0;
            yield return SceneManager.LoadSceneAsync("Assets/Game/Scenes/CoastalDefense_TopDown.unity"); yield return null;
            waves = Object.FindFirstObjectByType<WaveManager>(); build = Object.FindFirstObjectByType<BuildManager>();
            registry = Object.FindFirstObjectByType<EnemyRegistry>(); hud = Object.FindFirstObjectByType<BuildHud>();
            Object.FindFirstObjectByType<BorderBreachService>().ConfigureThreshold(0);
            build.Economy.AddMoney(20000);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var previous = SceneManager.GetActiveScene(); SceneManager.SetActiveScene(SceneManager.CreateScene("Event cleanup"));
            yield return SceneManager.UnloadSceneAsync(previous); Time.timeScale = 1; LogAssert.NoUnexpectedReceived();
        }
        private void Jump(int number)
        {
            Assert.That(waves.State, Is.EqualTo(WaveState.Preparation));
            typeof(WaveManager).GetMethod("ReleaseGenerated", Private).Invoke(waves, null);
            typeof(WaveManager).GetField("waveIndex", Private).SetValue(waves, number - 1);
            typeof(WaveManager).GetMethod("EnsureGenerated", Private).Invoke(waves, null);
        }
        private DefenseBase Place(string id, int x = 18, int y = 8)
        {
            Assert.That(build.TryPlaceDefense(hud.Defenses.Single(d => d.Id == id), new Vector2Int(x, y), out var defense), Is.EqualTo(PlacementResult.Valid));
            return defense;
        }
        private IEnumerator Drain()
        {
            for (int tick = 0; tick < 1000 && waves.State != WaveState.WaveComplete; tick++) {
                waves.Tick(.5f); foreach (var enemy in registry.Active.ToArray()) enemy.Health.TakeDefenseDamage(int.MaxValue); yield return null;
            }
            Assert.That(waves.State, Is.EqualTo(WaveState.WaveComplete));
            Assert.That(waves.Events.Modifiers.defenseRange, Is.EqualTo(1));
            Assert.That(waves.Events.Modifiers.boatSpeed, Is.EqualTo(1));
            Assert.That(waves.Events.Modifiers.upgradeCost, Is.EqualTo(1)); Assert.That(waves.Events.Modifiers.floodedColumns, Is.Zero);
            waves.Tick(2); yield return null;
        }
        [UnityTest] public IEnumerator EveryEventCanBeForcedTelegraphedAppliedAndClearedThroughRealBoats()
        {
            var root = hud.GetComponent<UIDocument>().rootVisualElement;
            var presentation = Object.FindFirstObjectByType<EndlessEventPresentation>();
            foreach (var e in waves.EndlessSettings.eventSettings.events) {
                Jump(24); Assert.IsTrue(waves.ForceEvent(e.id));
                Assert.That(root.Q<Label>("event-title").text, Does.Contain(e.displayName));
                Assert.That(root.Q("event-panel").style.display.value, Is.EqualTo(DisplayStyle.Flex));
                if (e.preparationWarningSeconds > 0) { Assert.IsFalse(waves.TryStartWave()); waves.Tick(8); }
                Assert.IsTrue(waves.TryStartWave());
                if (e.durationType == EventDuration.Preparation) Assert.That(waves.Events.Current, Is.Null);
                else { Assert.That(waves.Events.Modifiers, Is.SameAs(e.modifiers)); Assert.That(root.Q<Label>("event-title").text, Does.StartWith("ACTIVE:")); }
                Assert.That(presentation.FogVisible, Is.EqualTo(e.modifiers.fog));
                Assert.That(presentation.FloodVisible, Is.EqualTo(e.modifiers.floodedColumns > 0));
                yield return Drain(); Assert.IsFalse(presentation.FogVisible); Assert.IsFalse(presentation.FloodVisible);
                Debug.Log("Phase5B forced event passed: " + e.id);
            }
        }
        [UnityTest] public IEnumerator FogRestoresUpgradedRangeAndFloodNeverChangesTerrainOrExistingStructures()
        {
            var defense = Place("sniper-tower", 6, 8); float baseRange = defense.Definition.RangeInCells;
            Jump(24);
            var baseline = new EndlessWaveGenerator(waves.EndlessSettings, waves.RunSeed).Generate(24); int normalCount = baseline.ScheduledCount; Object.Destroy(baseline);
            waves.ForceEvent("mass-landing"); Assert.That(waves.CurrentWave.ScheduledCount, Is.GreaterThan(normalCount));
            waves.Events.Cancel(); Assert.That(waves.CurrentWave.ScheduledCount, Is.EqualTo(normalCount));
            Assert.That(waves.Landings.ConcurrencyLimit, Is.EqualTo(waves.EndlessSettings.BoatConcurrency(24)));
            waves.ForceEvent("fog"); Assert.IsTrue(waves.TryStartWave());
            Assert.That(defense.Range, Is.EqualTo(baseRange * defense.Grid.CellSize * .8f).Within(.001));
            build.TryUpgrade(defense); build.TryUpgrade(defense);
            float upgraded = baseRange * defense.Definition.LevelData(3).range * defense.Grid.CellSize;
            Assert.That(defense.Range, Is.EqualTo(upgraded * .8f).Within(.001));
            waves.Events.Cancel(); Assert.That(defense.Range, Is.EqualTo(upgraded).Within(.001));
            Assert.That(defense.Definition.RangeInCells, Is.EqualTo(baseRange)); yield return Drain();
            int revision = defense.Grid.Revision; Jump(24); waves.ForceEvent("flooded-shore");
            Assert.That(build.Validate(defense.Definition, new Vector2Int(6, 12)), Is.EqualTo(PlacementResult.NotBuildable));
            Assert.IsTrue(defense.IsAlive); Assert.That(defense.Grid.OccupantAt(defense.Anchor), Is.SameAs(defense));
            Assert.That(defense.Grid.Revision, Is.EqualTo(revision));
            waves.Events.Cancel(); Assert.That(build.Validate(defense.Definition, new Vector2Int(6, 12)), Is.EqualTo(PlacementResult.Valid));
            Assert.That(defense.Grid.Revision, Is.EqualTo(revision)); yield return null;
        }
        [UnityTest] public IEnumerator CostEventsRefreshPanelAndDeductExactlyWithoutChangingRefundRules()
        {
            var d = Place("sniper-tower"); var selection = Object.FindFirstObjectByType<StructureSelection>(); selection.Select(d);
            d.TakeDamage(150); int upgrade = build.UpgradeCost(d), repair = build.RepairCost(d), refund = build.RefundFor(d);
            Jump(24); waves.ForceEvent("ammunition-shortage");
            Assert.That(build.UpgradeCost(d), Is.EqualTo(System.Math.Ceiling(upgrade * 1.2))); // includes exact 225 -> 270 rounding
            Assert.That(build.RepairCost(d), Is.EqualTo(repair)); Assert.That(build.RefundFor(d), Is.EqualTo(refund));
            var root = hud.GetComponent<UIDocument>().rootVisualElement;
            Assert.That(root.Q<Button>("structure-upgrade").text, Does.Contain("270"));
            int balance = build.Economy.Money; Assert.IsTrue(build.TryUpgrade(d)); Assert.That(build.Economy.Money, Is.EqualTo(balance - 270));
            Assert.IsTrue(waves.TryStartWave()); int combatRepair = build.RepairCost(d); Assert.That(build.RepairMultiplier, Is.EqualTo(1.5f));
            Assert.That(root.Q<Button>("structure-repair").text, Does.Contain(combatRepair.ToString()));
            balance = build.Economy.Money; Assert.IsTrue(build.TryRepair(d)); Assert.That(build.Economy.Money, Is.EqualTo(balance - combatRepair));
            yield return Drain(); d.TakeDamage(150); repair = build.RepairCost(d); refund = build.RefundFor(d);
            Jump(24); waves.ForceEvent("field-engineers"); Assert.That(build.RepairCost(d), Is.LessThan(repair)); Assert.That(build.RefundFor(d), Is.EqualTo(refund));
            waves.Events.Cancel(); Assert.That(build.RepairCost(d), Is.EqualTo(repair));
            balance = build.Economy.Money; waves.ForceEvent("emergency-supply"); Assert.That(build.Economy.Money - balance, Is.EqualTo(292));
            Assert.That(root.Q<Label>("event-effect").text, Does.Contain("+292 RSD RECEIVED")); yield return null;
        }
        [UnityTest] public IEnumerator LandingPatternsChangeActualDispatchSectorsAndMassConcurrency()
        {
            foreach (string id in new[] { "multi-sector", "concentrated-landing", "mass-landing", "pirate-raid" }) {
                Jump(24); Assert.IsTrue(waves.ForceEvent(id)); var sector = waves.Events.Current.Sector;
                int[] before = waves.Landings.SectorArrivals.ToArray(); Assert.IsTrue(waves.TryStartWave());
                if (id == "mass-landing") Assert.That(waves.Landings.ConcurrencyLimit, Is.EqualTo(Mathf.CeilToInt(waves.EndlessSettings.BoatConcurrency(24) * 1.5f)));
                yield return Drain(); int[] counts = waves.Landings.SectorArrivals.Select((n, i) => n - before[i]).ToArray();
                if (id == "concentrated-landing") Assert.That(counts[(int)sector], Is.GreaterThan(counts.Sum() / 2));
                else Assert.That(counts.Count(n => n > 0), Is.EqualTo(3));
            }
        }
        [UnityTest] public IEnumerator TimedEventsAndCalmWarningsUsePauseAndDoubleSpeedAndGameOverClears()
        {
            Jump(24); waves.ForceEvent("calm-before-storm"); var speed = Object.FindFirstObjectByType<GameSpeedController>();
            speed.SetSpeed(GameSpeed.Pause); float remaining = waves.Events.Current.WarningRemaining;
            yield return new WaitForSecondsRealtime(.15f); Assert.That(waves.Events.Current.WarningRemaining, Is.EqualTo(remaining));
            speed.SetSpeed(GameSpeed.Double); yield return new WaitForSecondsRealtime(.2f);
            Assert.That(waves.Events.Current.WarningRemaining, Is.LessThan(remaining)); speed.SetSpeed(GameSpeed.Pause);
            waves.Tick(8); Assert.IsTrue(waves.TryStartWave());
            var timed = ScriptableObject.CreateInstance<EndlessEventDefinition>(); timed.id = "timed-fixture"; timed.durationType = EventDuration.Timed; timed.duration = 2; timed.modifiers.defenseRange = .8f;
            yield return Drain(); Jump(24); Assert.IsTrue(waves.Events.Force(timed, 24, false, build.Economy)); Assert.IsTrue(waves.TryStartWave());
            remaining = waves.Events.Current.SecondsRemaining; yield return new WaitForSecondsRealtime(.15f); Assert.That(waves.Events.Current.SecondsRemaining, Is.EqualTo(remaining));
            speed.SetSpeed(GameSpeed.Double); yield return new WaitForSecondsRealtime(.2f); Assert.That(waves.Events.Current.SecondsRemaining, Is.LessThan(remaining));
            speed.SetSpeed(GameSpeed.Pause); build.Game.EndRun(); Assert.That(waves.Events.Current, Is.Null); Assert.That(waves.Landings.ActiveBoats, Is.Zero);
            Object.Destroy(timed); yield return null;
        }
        [UnityTest, Timeout(180000)] public IEnumerator ThirtyNaturalEventWavesCompleteWithCooldownsBossSuppressionAndNoLeakage()
        {
            var settings = Object.Instantiate(waves.EndlessSettings); settings.randomizeSeed = false; settings.seed = 1977; waves.ConfigureEndless(settings);
            for (int number = 1; number <= 30; number++) {
                Assert.That(waves.CurrentWaveNumber, Is.EqualTo(number));
                if (number < 5 || number == 25) Assert.That(waves.Events.Current, Is.Null);
                if (waves.Events.Current != null) Debug.Log("Phase5B natural " + number + ": " + waves.Events.Current.Definition.id);
                waves.Tick(8); Assert.IsTrue(waves.TryStartWave()); yield return Drain();
                var next = waves.Events.Current;
                Assert.That(waves.Events.Modifiers.upgradeCost, Is.EqualTo(next != null && next.Active ? next.Definition.modifiers.upgradeCost : 1));
            }
            Assert.That(waves.Events.EventsExperienced, Is.InRange(6, 14));
            Assert.That(waves.Events.History.Select(h => h.Id).Distinct().Count(), Is.GreaterThanOrEqualTo(5));
            Debug.Log("Phase5B natural 30 waves: " + string.Join(", ", waves.Events.History.Select(h => h.ToString())));
            Object.Destroy(settings);
        }
    }
}
