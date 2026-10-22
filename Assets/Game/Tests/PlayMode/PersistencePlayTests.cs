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
using ShorelineDefense.Presentation;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    [SetUpFixture] public sealed class PlayModeLocalDataIsolation
    {
        private string directory;
        [OneTimeSetUp] public void Begin() { directory = Path.Combine(Path.GetTempPath(), "BorderDefendersRegression", Guid.NewGuid().ToString()); LocalGameData.UseDirectory(directory); }
        [OneTimeTearDown] public void End() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
    public sealed class PersistencePlayTests
    {
        private static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();
        private int transitionIndex;
        private static VisualElement Root => Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
        private static void Click(string id) { var b = Root.Q<Button>(id); Assert.That(b, Is.Not.Null, id); Assert.IsTrue(b.enabledSelf, id); b.Focus(); using (var e = NavigationSubmitEvent.GetPooled()) { e.target = b; b.SendEvent(e); } }
        private IEnumerator Scene(string name) { yield return SceneManager.LoadSceneAsync(name); yield return new WaitForSecondsRealtime(.15f); yield return null; }
        private IEnumerator Transition(string name) { int index = ++transitionIndex; float until = Time.realtimeSinceStartup + 10; while (Time.realtimeSinceStartup < until && (SceneManager.GetActiveScene().name != name || Find<ProductMenuController>() == null || Find<ProductMenuController>().IsLoading)) yield return null; yield return new WaitForSecondsRealtime(.15f); yield return null; Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(name), "Transition " + index + "; pending=" + (RunSession.Pending != null) + "; loading=" + Find<ProductMenuController>()?.IsLoading + "; title=" + Root.Q<Label>(className: "product-title")?.text); }
        [UnitySetUp] public IEnumerator Setup()
        { transitionIndex = 0; new SaveGameService(LocalGameData.Files).Delete(out _); LocalGameData.Files.Delete("run_history", out _); LocalGameData.Files.Delete("settings", out _); RunSession.Pending = null; Time.timeScale = 1; yield return Scene("MainMenu"); }
        [UnityTearDown] public IEnumerator Cleanup()
        { var scene = SceneManager.GetActiveScene(); SceneManager.SetActiveScene(SceneManager.CreateScene("Persistence cleanup")); yield return SceneManager.UnloadSceneAsync(scene); Time.timeScale = 1; LogAssert.NoUnexpectedReceived(); }
        private static DefenseBase Place(string id, int x, int y)
        { var b = Find<BuildManager>(); Assert.That(b.TryPlaceDefense(Find<BuildHud>().Defenses.Single(d => d.Id == id), new Vector2Int(x, y), out var defense), Is.EqualTo(PlacementResult.Valid)); return defense; }
        private static void SeedPreview(int wave, string eventId)
        {
            var w = Find<WaveManager>(); var d = Find<RunSession>().Capture(); d.wave = wave; d.completedWaves = wave - 1; d.statistics.highestWave = wave - 1;
            d.events = new EventSaveData { nextWave = wave }; w.RestorePreparation(d);
            Assert.IsTrue(w.ForceEvent(eventId));
        }
        [UnityTest] public IEnumerator StructuresEconomyWireEventsAndStatisticsRestoreWithoutRerollOrPayment()
        {
            Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown");
            Find<BuildManager>().Economy.AddMoney(20000); SeedPreview(24, "emergency-supply");
            foreach (var pos in new[] { new Vector2Int(13,8), new Vector2Int(14,8), new Vector2Int(15,8), new Vector2Int(14,7), new Vector2Int(14,9), new Vector2Int(13,7) }) Place("barbed-wire", pos.x, pos.y);
            foreach (var pos in new[] { new Vector2Int(9,8), new Vector2Int(10,8), new Vector2Int(11,8), new Vector2Int(16,11), new Vector2Int(15,11), new Vector2Int(17,11), new Vector2Int(16,12) }) Place("barbed-wire", pos.x, pos.y);
            Place("mortar",7,4); Place("flamethrower",10,4); Place("autocannon",17,3); Place("rocket",21,3); Place("sniper-tower",25,8);
            var mg = Place("machine-gun-nest", 18, 8); var b = Find<BuildManager>(); b.TryUpgrade(mg); b.TryUpgrade(mg); mg.TakeDamage(120);
            var repaired = Place("sniper-tower", 20, 8); repaired.TakeDamage(100); b.TryRepair(repaired); b.TryDismantle(repaired, out _);
            var mine = Place("land-mine", 19, 7); b.TryUpgrade(mine); mine.GetComponent<LandMine>().Tick(1); var used = Place("land-mine", 19, 9); used.TakeDamage(1);
            Find<RunStatistics>().RecordBreach(100); yield return null;
            var session = Find<RunSession>(); var expected = session.Capture();
            var masks = Object.FindObjectsByType<ConnectedWirePresentation>(FindObjectsSortMode.None).ToDictionary(w => w.GetComponent<DefenseBase>().Anchor, w => w.Mask);
            Assert.IsTrue(session.SaveNow()); Assert.IsTrue(new SaveGameService(LocalGameData.Files).Load(out var loaded, out _));
            Find<ProductMenuController>().ReturnToMenu(); yield return Transition("MainMenu"); Click("menu-continue"); yield return Transition("CoastalDefense_TopDown");
            var actual = Find<RunSession>().Capture();
            Assert.That(actual.wave, Is.EqualTo(24)); Assert.That(actual.money, Is.EqualTo(expected.money)); Assert.That(actual.seed, Is.EqualTo(expected.seed));
            Assert.That(actual.earned, Is.EqualTo(expected.earned)); Assert.That(actual.spent, Is.EqualTo(expected.spent)); Assert.That(actual.refunds, Is.EqualTo(expected.refunds)); Assert.That(actual.dismantled, Is.EqualTo(1));
            Assert.That(actual.statistics.homicides, Is.EqualTo(100)); Assert.That(actual.statistics.built, Is.EqualTo(expected.statistics.built)); Assert.That(actual.statistics.upgraded, Is.EqualTo(3)); Assert.That(actual.statistics.repaired, Is.EqualTo(1));
            Assert.That(actual.events.currentId, Is.EqualTo("emergency-supply")); Assert.That(actual.events.supplyReceived, Is.EqualTo(expected.events.supplyReceived)); Assert.That(actual.events.experienced, Is.EqualTo(expected.events.experienced));
            Assert.That(actual.structures.Count, Is.EqualTo(expected.structures.Count));
            foreach (var item in expected.structures) {
                var d = Find<BuildManager>().Battlefield.Grid.OccupantAt(new Vector2Int(item.x, item.y)) as DefenseBase;
                Assert.That(d, Is.Not.Null); Assert.That(d.Definition.Id, Is.EqualTo(item.defenseId)); Assert.That(d.Level, Is.EqualTo(item.level)); Assert.That(d.CurrentHealth, Is.EqualTo(item.health)); Assert.That(d.TotalInvestment, Is.EqualTo(item.investment));
                if (d.GetComponent<LandMine>() != null) Assert.That(d.GetComponent<LandMine>().ArmingRemaining, Is.EqualTo(item.mineArmingRemaining).Within(.01));
                for (int x = 0; x < d.Definition.Footprint.x; x++) for (int y = 0; y < d.Definition.Footprint.y; y++) Assert.That(d.Grid.OccupantAt(d.Anchor + new Vector2Int(x,y)), Is.SameAs(d));
            }
            foreach (var w in Object.FindObjectsByType<ConnectedWirePresentation>(FindObjectsSortMode.None)) Assert.That(w.Mask, Is.EqualTo(masks[w.GetComponent<DefenseBase>().Anchor]));
            var resumedEvent = Find<WaveManager>().Events; var future = new EndlessEventManager(Find<WaveManager>().EndlessSettings.eventSettings, expected.seed); future.Restore(expected.events);
            Assert.That(resumedEvent.Select(30,false)?.id, Is.EqualTo(future.Select(30,false)?.id));
            Assert.That(Find<WaveManager>().CurrentWave.Encounter, Is.Null);
        }
        [UnityTest] public IEnumerator FullMenuBuildWaveAutosaveContinueDeathHistoryAndFreshRunJourney()
        {
            Assert.IsFalse(Root.Q<Button>("menu-continue").enabledSelf); Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown");
            Place("machine-gun-nest",12,8); Assert.IsTrue(Find<RunSession>().SaveNow()); Click("start-wave");
            var w = Find<WaveManager>(); var registry = Find<EnemyRegistry>(); Find<BorderBreachService>().ConfigureThreshold(0); Time.timeScale = 0;
            // Explicit accelerated combat fixture: real boats/unloads; lethal test damage drains Wave 1.
            for (int i = 0; i < 1500 && w.State != WaveState.WaveComplete; i++) { w.Tick(.5f); foreach (var enemy in registry.Active.ToArray()) enemy.Health.TakeDefenseDamage(int.MaxValue); yield return null; }
            Assert.That(w.State, Is.EqualTo(WaveState.WaveComplete)); w.Tick(2); yield return new WaitForSecondsRealtime(.4f);
            Assert.That(w.CurrentWaveNumber, Is.EqualTo(2)); Assert.IsTrue(new SaveGameService(LocalGameData.Files).Load(out var checkpoint, out _)); Assert.That(checkpoint.wave, Is.EqualTo(2));
            Find<ProductMenuController>().ReturnToMenu(); yield return Transition("MainMenu");
            Click("menu-new"); Click("map-river_border"); Click("map-start"); Assert.That(Root.Q<Button>("confirm-cancel"), Is.Not.Null); Click("confirm-cancel"); Click("menu-continue"); yield return Transition("CoastalDefense_TopDown");
            Assert.That(Find<WaveManager>().CurrentWaveNumber, Is.EqualTo(2)); Assert.That(Find<BuildManager>().Economy.Money, Is.EqualTo(checkpoint.money)); Assert.That(Find<RunStatistics>().TotalKills, Is.GreaterThan(0));
            Click("start-wave"); Find<GameStateController>().EndRun(); Find<GameStateController>().EndRun();
            Assert.That(Root.Q<Button>("over-main"), Is.Not.Null); Assert.That(new RunHistoryService(LocalGameData.Files).Load().runs.Count, Is.EqualTo(1)); Assert.IsFalse(new SaveGameService(LocalGameData.Files).Load(out _, out _));
            Click("over-main"); yield return Transition("MainMenu"); Click("menu-history"); Assert.That(Root.Q<Button>("history-back"), Is.Not.Null); Click("history-back"); Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown");
            var clean = Find<RunSession>().Capture(); Assert.That(clean.wave, Is.EqualTo(1)); Assert.That(clean.money, Is.EqualTo(500)); Assert.That(clean.statistics.homicides, Is.Zero); Assert.That(clean.statistics.kills.Sum(), Is.Zero); Assert.IsEmpty(clean.structures); Assert.That(clean.events.experienced, Is.Zero); Assert.That(new RunHistoryService(LocalGameData.Files).Load().runs.Count, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator PausePreservesDoubleSpeedAndActiveQuitUsesPreparationCheckpoint()
        {
            Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown"); Click("start-wave"); var speed = Find<GameSpeedController>(); speed.SetSpeed(GameSpeed.Double);
            yield return new WaitForSecondsRealtime(.7f); Find<ProductMenuController>().ShowPause();
            var projectile = new GameObject("Pause verification projectile").AddComponent<TimedAreaProjectile>();
            projectile.Initialize(Find<EnemyRegistry>(),Find<GameStateController>(),Vector3.zero,new Vector3(10,5,0),2,1,1,1);
            Vector3 projectilePosition = projectile.transform.position;
            var boatTransforms = Find<WaveManager>().Landings.GetComponentsInChildren<Transform>(); var boatPositions = boatTransforms.Select(t=>t.position).ToArray();
            float clock = Time.time; var enemies = Find<EnemyRegistry>().Active.Select(e => e.transform.position).ToArray(); var boats = Find<WaveManager>().Landings.ActiveBoats;
            yield return new WaitForSecondsRealtime(.2f); Assert.That(Time.time, Is.EqualTo(clock).Within(.01)); Assert.That(Find<EnemyRegistry>().Active.Select(e => e.transform.position).ToArray(), Is.EqualTo(enemies)); Assert.That(Find<WaveManager>().Landings.ActiveBoats, Is.EqualTo(boats));
            Assert.That(projectile.transform.position,Is.EqualTo(projectilePosition)); Assert.That(boatTransforms.Select(t=>t.position).ToArray(),Is.EqualTo(boatPositions));
            Click("pause-settings"); Click("settings-back"); Click("pause-resume"); Assert.That(speed.Speed, Is.EqualTo(GameSpeed.Double)); Assert.That(Time.timeScale, Is.EqualTo(2));
            Find<ProductMenuController>().ShowPause(); Click("pause-main"); Assert.That(Root.Q<Button>("confirm-cancel"), Is.Not.Null); Click("confirm-accept"); yield return Transition("MainMenu"); Click("menu-continue"); yield return Transition("CoastalDefense_TopDown"); Assert.That(Find<WaveManager>().State, Is.EqualTo(WaveState.Preparation)); Assert.That(Find<WaveManager>().CurrentWaveNumber, Is.EqualTo(1)); Assert.That(Find<RunStatistics>().TotalKills, Is.Zero);
        }
        [UnityTest] public IEnumerator SettingsApplyLivePresentationCapsAndPersistAcrossScene()
        {
            Click("menu-settings"); Root.Q<DropdownField>("settings-vfx").index = 0; Root.Q<Toggle>("settings-rewards").value = false; Root.Q<Toggle>("settings-vsync").value = false; Click("settings-apply");
            Assert.That(SettingsService.Instance.Current.vfxDensity, Is.Zero); Assert.That(QualitySettings.vSyncCount, Is.Zero); Click("settings-back"); Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown");
            Assert.That(Find<VfxService>().Pool(VfxChannel.Decal).PlaybackLimit, Is.EqualTo(12)); Assert.That(new SettingsService(LocalGameData.Files).Current.rewardPopups, Is.False);
            Find<ProductMenuController>().ShowPause(); Click("pause-restart"); Click("confirm-accept"); yield return Transition("CoastalDefense_TopDown"); Assert.That(SettingsService.Instance.Current.vfxDensity, Is.Zero);
            // Restore standard presentation for the remaining regression fixtures.
            SettingsService.Instance.Save(new GameSettings());
        }
        [UnityTest] public IEnumerator CorruptAndUnsupportedSavesKeepMenuUsableAndBackupRecoverable()
        {
            Directory.CreateDirectory(LocalGameData.Files.Root); File.WriteAllText(LocalGameData.Files.PathFor("active_run"), "broken");
            Find<ProductMenuController>().ShowMain(); Assert.IsFalse(Root.Q<Button>("menu-continue").enabledSelf); Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown");
            var snapshot = Find<RunSession>().Capture(); var saves = new SaveGameService(LocalGameData.Files); saves.Save(snapshot,out _); saves.Save(snapshot,out _);
            yield return Scene("MainMenu"); File.WriteAllText(LocalGameData.Files.PathFor("active_run"), "broken"); Find<ProductMenuController>().ShowMain(); Assert.IsTrue(Root.Q<Button>("menu-continue").enabledSelf);
            Click("menu-continue"); yield return Transition("CoastalDefense_TopDown"); Assert.That(Find<WaveManager>().CurrentWaveNumber,Is.EqualTo(1));
            yield return Scene("MainMenu"); snapshot.saveVersion=99; LocalGameData.Files.Write("active_run",snapshot,out _); LocalGameData.Files.Write("active_run",snapshot,out _);
            Find<ProductMenuController>().ShowMain(); Assert.IsFalse(Root.Q<Button>("menu-continue").enabledSelf); Click("menu-settings"); Assert.That(Root.Q<Button>("settings-back"),Is.Not.Null);
            snapshot.saveVersion=1; snapshot.structures.Add(new StructureSaveData { defenseId="removed-defense", x=18, y=8, level=1, health=100, maxHealth=100, investment=100, constructionCost=100 });
            LocalGameData.Files.Write("active_run",snapshot,out _); LocalGameData.Files.Write("active_run",snapshot,out _); Find<ProductMenuController>().ShowMain();
            Assert.IsFalse(Root.Q<Button>("menu-continue").enabledSelf,"Unavailable defense must disable Continue before scene load");
        }
        [UnityTest] public IEnumerator AnnouncedFogAndNextBossPlanSurviveReloadWithSameSeed()
        {
            Click("menu-new"); Click("map-river_border"); Click("map-start"); yield return Transition("CoastalDefense_TopDown"); SeedPreview(24,"fog");
            var session=Find<RunSession>(); var snapshot=session.Capture(); Assert.IsTrue(session.SaveNow());
            var baseline=new EndlessWaveGenerator(Find<WaveManager>().EndlessSettings,snapshot.seed).Generate(25);
            int planned=baseline.ScheduledCount; string boss=baseline.Encounter.name; Object.Destroy(baseline);
            Find<ProductMenuController>().ReturnToMenu(); yield return Transition("MainMenu"); Click("menu-continue"); yield return Transition("CoastalDefense_TopDown");
            Assert.That(Find<WaveManager>().Events.Current.Definition.id,Is.EqualTo("fog")); Assert.That(Find<WaveManager>().Events.Capture().nextWave,Is.EqualTo(snapshot.events.nextWave));
            var next=Find<RunSession>().Capture(); next.wave=25; next.completedWaves=24; next.events.currentId=null;
            Find<WaveManager>().RestorePreparation(next); Assert.That(Find<WaveManager>().CurrentWave.ScheduledCount,Is.EqualTo(planned)); Assert.That(Find<WaveManager>().CurrentWave.Encounter.name,Is.EqualTo(boss));
        }
    }
}
