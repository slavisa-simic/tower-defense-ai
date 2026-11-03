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
using ShorelineDefense.Tutorial;
using ShorelineDefense.UI;
using ShorelineDefense.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ShorelineDefense.Tests
{
    public sealed class TutorialPlayTests
    {
        private string directory, previousDirectory;
        private static T Find<T>() where T : Object => Object.FindFirstObjectByType<T>();
        private static TutorialRuntime Tutorial => Find<TutorialRuntime>();
        private static VisualElement Root => Find<ProductMenuController>().GetComponent<UIDocument>().rootVisualElement;
        private static IEnumerator Load(string scene)
        { yield return SceneManager.LoadSceneAsync(scene); yield return null; yield return null; Time.timeScale = 0; }
        private static void Click(string id)
        { var b = Root.Q<Button>(id); Assert.IsNotNull(b, id); using (var e = NavigationSubmitEvent.GetPooled()) { e.target = b; b.SendEvent(e); } }
        private static DefenseBase Place(string id, int x, int y)
        { var definition = Find<BuildHud>().Defenses.Single(d => d.Id == id); Assert.That(Find<BuildManager>().TryPlaceDefense(definition, new Vector2Int(x, y), out var defense), Is.EqualTo(PlacementResult.Valid)); return defense; }
        private static void CompleteExcept(params string[] ids)
        { foreach (var d in TutorialCatalog.All) if (!ids.Contains(d.Id)) LocalGameData.Tutorial.MarkShown(d.Id); Tutorial.Manager.Clear(); }
        [UnitySetUp] public IEnumerator Setup()
        { previousDirectory = LocalGameData.Files.Root; directory = Path.Combine(Path.GetTempPath(), "BorderTutorialPlay", Guid.NewGuid().ToString()); LocalGameData.UseDirectory(directory); RunSession.Pending = null; yield return Load("MainMenu"); }
        [UnityTearDown] public IEnumerator Cleanup()
        { var scene = SceneManager.GetActiveScene(); SceneManager.SetActiveScene(SceneManager.CreateScene("Tutorial cleanup")); yield return SceneManager.UnloadSceneAsync(scene); Time.timeScale = 1; LocalGameData.UseDirectory(previousDirectory); if (Directory.Exists(directory)) Directory.Delete(directory, true); LogAssert.NoUnexpectedReceived(); }
        [UnityTest] public IEnumerator FreshRunIntroPlacementAndContinuePersistWithoutAffectingClock()
        {
            yield return Load("CoastalDefense_TopDown");
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("intro"));
            Assert.That(Find<GameSpeedController>().Speed, Is.EqualTo(GameSpeed.Normal));
            Assert.That(Root.Q<Label>("tutorial-copy").text, Does.Contain("Danube"));
            Tutorial.View.Dismiss(); yield return new WaitForSecondsRealtime(.4f); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("objective"));
            var placement = Find<PlacementController>(); Assert.IsTrue(placement.Select(Find<BuildHud>().MachineGun));
            Place("machine-gun-nest", 14, 8); Assert.IsTrue(Tutorial.Manager.Queue.Any(p => p.Definition.Id == "placement"));
            Find<RunSession>().SaveNow(); Assert.IsTrue(new SaveGameService(LocalGameData.Files).Load(out var data, out _));
            RunSession.Pending = data; yield return Load("CoastalDefense_TopDown");
            Assert.IsTrue(Find<RunSession>().RestoredRun); Assert.IsFalse(Tutorial.Manager.Queue.Any(p => p.Definition.Id == "intro" || p.Definition.Id == "objective"));
            yield return Load("CoastalDefense_TopDown"); Assert.IsFalse(Tutorial.Manager.Queue.Any(p => p.Definition.Id == "intro" || p.Definition.Id == "objective"));
        }
        [UnityTest] public IEnumerator DamageRepairUpgradeAndDismantleUseRealSelectedControlsAndRefund()
        {
            yield return Load("CoastalDefense_TopDown"); CompleteExcept("damage", "repair", "upgrade", "dismantle");
            var b = Find<BuildManager>(); var d = Place("machine-gun-nest", 14, 8); d.TakeDamage(d.MaxHealth / 2);
            Find<StructureSelection>().Select(d); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("repair")); Assert.That(Root.Q<Button>("structure-repair").ClassListContains("tutorial-highlight"), Is.True);
            Assert.That(Root.Q<Label>("tutorial-copy").text, Does.Contain("Preparation is cheaper"));
            Tutorial.View.Dismiss(); b.TryRepair(d); yield return new WaitForSecondsRealtime(.4f); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("upgrade")); Assert.That(Root.Q<Label>("tutorial-detail").text, Does.Contain("→"));
            Tutorial.View.Dismiss(); CompleteExcept("dismantle");
            Place("barbed-wire", 18, 5); Place("barbed-wire", 18, 6);
            var snapshot = Find<RunSession>().Capture(); snapshot.wave = 2; snapshot.completedWaves = 1; Find<WaveManager>().RestorePreparation(snapshot);
            Find<StructureSelection>().Select(d); yield return new WaitForSecondsRealtime(.4f); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("dismantle")); Assert.That(Root.Q<Label>("tutorial-detail").text, Does.Contain(b.RefundFor(d).ToString()));
            Assert.IsTrue(d.IsAlive); Assert.That(Root.Q("structure-panel").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
        }
        [UnityTest] public IEnumerator EveryCreatureDiscoveryAndActualBlockadeAttackTriggerOnce()
        {
            yield return Load("CoastalDefense_TopDown"); CompleteExcept("structure-attack", "blockade", "creature-Crawler", "creature-Skitter", "creature-Bulwark", "creature-Spitter", "creature-Broodling", "creature-Burrower");
            var b = Find<BuildManager>(); b.Economy.AddMoney(50000); var grid = b.Battlefield.Grid;
            for (int y = 0; y < grid.Height; y++) if (grid.GetCell(new Vector2Int(14, y)).Walkable) {
                var wall = Place("machine-gun-nest", 14, y); foreach (var c in wall.GetComponents<MonoBehaviour>()) if (c is TurretCombat) c.enabled = false;
            }
            var spawn = Find<EnemySpawner>(); spawn.ConfigureDeployment(.01f); spawn.ConfigureWave(100, 1, 1);
            var settings = Find<WaveManager>().EndlessSettings;
            int serial = 0;
            foreach (var prefab in settings.enemies) {
                int count = prefab.Definition.Kind == EnemyKind.Broodling ? 6 : 1;
                for (int i = 0; i < count; i++) {
                    var offset = new Vector3(-.3f + serial % 4 * .2f, -.3f + serial / 4 * .2f, 0); serial++;
                    Assert.IsTrue(spawn.TrySpawnAt(prefab, grid.CellToWorld(new Vector2Int(13, 8)) + offset, out _), prefab.Definition.Kind.ToString());
                }
            }
            for (int tick = 0; tick < 120; tick++) foreach (var enemy in Find<EnemyRegistry>().Active.ToArray()) enemy.Tick(.1f);
            Assert.IsTrue(Tutorial.Manager.Queue.Any(p => p.Definition.Id == "structure-attack"));
            foreach (string id in new[] { "creature-Crawler", "creature-Skitter", "creature-Bulwark", "creature-Spitter", "creature-Broodling", "creature-Burrower" }) {
                Assert.That(Tutorial.Manager.Queue.Count(p => p.Definition.Id == id), Is.EqualTo(1), id);
            }
            yield return null; Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("structure-attack"));
            Tutorial.View.Dismiss(); Assert.IsFalse(Tutorial.Manager.Enqueue("structure-attack"));
        }
        [UnityTest] public IEnumerator DisabledGuidanceSkipConfirmationAndResetKeepGameAndResearch()
        {
            yield return Load("CoastalDefense_TopDown"); Click("tutorial-skip"); yield return null;
            Assert.IsNotNull(Root.Q("tutorial-skip-confirm")); Assert.IsTrue(LocalGameData.Tutorial.Enabled);
            Click("tutorial-skip-no"); yield return null; Click("tutorial-skip"); Click("tutorial-skip-yes"); yield return null;
            Assert.IsFalse(LocalGameData.Tutorial.Enabled); Assert.IsNull(Tutorial.Manager.Active); Assert.IsTrue(Find<BuildManager>().Game.IsPlaying);
            Place("machine-gun-nest", 14, 8); Assert.IsTrue(Find<WaveManager>().TryStartWave()); yield return null; Assert.IsEmpty(Tutorial.Manager.Queue);
            Find<ProductMenuController>().ShowSettings(); Click("settings-reset-tutorial"); Assert.IsTrue(LocalGameData.Tutorial.HasSeen("intro"));
            Click("confirm-cancel"); Assert.IsTrue(LocalGameData.Tutorial.HasSeen("intro")); Click("settings-reset-tutorial"); Click("confirm-accept");
            Assert.IsFalse(LocalGameData.Tutorial.HasSeen("intro")); Assert.IsFalse(LocalGameData.Tutorial.Enabled);
        }
        [UnityTest] public IEnumerator SummaryResearchAndPortHintsAreContextualAndPersistAcrossLoss()
        {
            yield return Load("CoastalDefense_TopDown"); var s = Find<RunStatistics>().Capture(); s.highestWave = 5; Find<RunStatistics>().Restore(s);
            Assert.IsFalse(LocalGameData.Tutorial.HasSeen("rp")); Find<GameStateController>().EndRun(); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("rp")); Assert.That(Root.Q<Label>("tutorial-detail").text, Does.Contain("+10 RP"));
            Assert.IsTrue(LocalGameData.Tutorial.RunCompleted); yield return Load("MainMenu");
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("research-intro"));
            Click("menu-research"); yield return null; Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("research-tree"));
            Click("research-node-emergency-fund"); Click("research-purchase"); Tutorial.View.Dismiss(); yield return new WaitForSecondsRealtime(.4f); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("research-applied"));
            CompleteExcept("port"); yield return Load("DanubeIndustrialPort"); Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("port"));
        }
        [UnityTest] public IEnumerator BossPreemptsSpeedAndNeverOverlapsBossOrCoreHud()
        {
            yield return Load("CoastalDefense_TopDown"); CompleteExcept("boss", "creature-Dredger", "speed");
            var waves = Find<WaveManager>(); var data = Find<RunSession>().Capture(); data.wave = 25; data.completedWaves = 24; waves.RestorePreparation(data);
            waves.Events.Tick(100); Assert.IsTrue(waves.TryStartWave()); yield return null; yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("boss"));
            Assert.That(Root.Q("boss-panel").resolvedStyle.display, Is.EqualTo(DisplayStyle.Flex));
            foreach (string id in new[] { "health-panel", "money-panel", "event-panel", "boss-panel", "build-panel", "structure-panel" }) {
                var target = Root.Q(id); if (target != null && target.resolvedStyle.display != DisplayStyle.None) Assert.IsFalse(Tutorial.View.Bounds.Overlaps(target.worldBound), id);
            }
        }
        [UnityTest] public IEnumerator MenuGuidanceDoesNotMoveTargetsOnTimeoutAndCombatTimingIgnoresX2()
        {
            Find<ProductMenuController>().ShowMapSelect(); yield return null; yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("maps"));
            Rect before = Root.Q<Button>("map-river_border").worldBound;
            Tutorial.View.Tick(30, Tutorial.Eligible); yield return null;
            Assert.That(Tutorial.Manager.Active.Definition.Id, Is.EqualTo("maps"));
            Assert.That(Root.Q<Button>("map-river_border").worldBound, Is.EqualTo(before));
            yield return Load("CoastalDefense_TopDown"); CompleteExcept("creature-Crawler");
            Find<GameSpeedController>().SetSpeed(GameSpeed.Double); Tutorial.Manager.Enqueue("creature-Crawler"); yield return null;
            Tutorial.View.Tick(5, Tutorial.Eligible); Assert.IsNotNull(Tutorial.Manager.Active);
            Tutorial.View.Tick(5, Tutorial.Eligible); Assert.IsNull(Tutorial.Manager.Active);
            Assert.That(Find<GameSpeedController>().Speed, Is.EqualTo(GameSpeed.Double));
        }
    }
}
