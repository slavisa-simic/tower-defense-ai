using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ShorelineDefense.Persistence;
using ShorelineDefense.Tutorial;

namespace ShorelineDefense.Tests
{
    public sealed class TutorialTests
    {
        private string directory;
        private LocalJsonStore files;
        private TutorialProfileService profile;
        [SetUp] public void Setup()
        { directory = Path.Combine(Path.GetTempPath(), "BorderTutorialTests", Guid.NewGuid().ToString()); files = new LocalJsonStore(directory); profile = new TutorialProfileService(files); }
        [TearDown] public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        [Test] public void OneTimeLessonsSurviveManagerRecreationAndIndependentProfileReload()
        {
            using (var queue = new TutorialManager(profile)) {
                Assert.IsTrue(queue.Enqueue("objective")); queue.Pump(p => true); queue.Dismiss(); Assert.IsFalse(queue.Enqueue("objective"));
            }
            var reloaded = new TutorialProfileService(files);
            using (var newRun = new TutorialManager(reloaded)) Assert.IsFalse(newRun.Enqueue("objective"));
            using (var continuedRun = new TutorialManager(reloaded)) Assert.IsFalse(continuedRun.Enqueue("objective"));
        }
        [Test] public void BatchPriorityDeduplicationAndCriticalPreemptionUseOneActiveCard()
        {
            using (var q = new TutorialManager(profile)) {
                q.Enqueue("speed"); q.Enqueue("repair"); q.Enqueue("boss"); Assert.IsFalse(q.Enqueue("boss"));
                Assert.That(q.Queue.Count, Is.EqualTo(3)); Assert.That(q.Pump(p => true).Definition.Id, Is.EqualTo("boss"));
                q.Dismiss(); Assert.That(q.Pump(p => true).Definition.Id, Is.EqualTo("repair"));
                q.Enqueue("structure-attack"); Assert.That(q.Pump(p => true).Definition.Id, Is.EqualTo("structure-attack"));
                q.Dismiss(); Assert.That(q.Pump(p => true).Definition.Id, Is.EqualTo("repair"));
            }
        }
        [Test] public void IneligibleHintsWaitWithoutBeingMarkedShown()
        {
            using (var q = new TutorialManager(profile)) {
                q.Enqueue("repair"); q.Enqueue("speed");
                Assert.That(q.Pump(p => p.Definition.Id != "repair").Definition.Id, Is.EqualTo("speed"));
                Assert.IsFalse(profile.HasSeen("repair")); q.Dismiss(); Assert.That(q.Pump(p => true).Definition.Id, Is.EqualTo("repair"));
            }
        }
        [Test] public void DisabledGuidanceClearsActiveAndPendingWithoutChangingSeenProgress()
        {
            using (var q = new TutorialManager(profile)) {
                q.Enqueue("objective"); q.Pump(p => true); q.Enqueue("build"); Assert.IsTrue(profile.SetEnabled(false));
                Assert.IsNull(q.Active); Assert.IsEmpty(q.Queue); Assert.IsFalse(q.Enqueue("boss")); Assert.IsNull(q.Pump(p => true));
                Assert.IsTrue(profile.SetEnabled(true)); Assert.IsFalse(q.Enqueue("objective")); Assert.IsTrue(q.Enqueue("build"));
            }
            Assert.IsTrue(new TutorialProfileService(files).Enabled);
        }
        [Test] public void ResetOnlyTouchesTutorialFileAndPreservesOffSetting()
        {
            foreach (string file in new[] { "profile_save", "run_history", "active_run", "settings" }) { Directory.CreateDirectory(directory); File.WriteAllText(files.PathFor(file), "sentinel:" + file); }
            profile.MarkShown("repair"); profile.RecordRunCompleted(); profile.SetEnabled(false);
            Assert.IsTrue(profile.ResetProgress()); Assert.IsFalse(profile.HasSeen("repair")); Assert.IsFalse(profile.RunCompleted); Assert.IsFalse(profile.Enabled);
            foreach (string file in new[] { "profile_save", "run_history", "active_run", "settings" }) Assert.That(File.ReadAllText(files.PathFor(file)), Is.EqualTo("sentinel:" + file));
            profile.SetEnabled(true); using (var q = new TutorialManager(profile)) Assert.IsTrue(q.Enqueue("repair"));
        }
        [Test] public void BasicsDoNotRequireAllCreatureDiscoveries()
        {
            foreach (string id in new[] { "objective", "build", "start-wave", "repair", "upgrade" }) profile.MarkShown(id);
            Assert.IsTrue(profile.BasicsCompleted); Assert.IsFalse(profile.HasSeen("creature-Burrower"));
        }
        [Test] public void ProfileRecoversFromBackupAndReportsPersistenceFailure()
        {
            profile.MarkShown("objective"); profile.MarkShown("repair");
            File.WriteAllText(files.PathFor("tutorial_profile"), "broken json");
            Assert.IsTrue(new TutorialProfileService(files).HasSeen("objective"));
            string obstruction = Path.Combine(directory, "file"); File.WriteAllText(obstruction, "obstruction");
            var blocked = new TutorialProfileService(new LocalJsonStore(obstruction)); Assert.IsFalse(blocked.MarkShown("objective")); Assert.IsFalse(blocked.HasSeen("objective")); Assert.IsNotNull(blocked.Error);
        }
        [Test] public void CatalogHasUniqueKeysAndConciseCentralizedCopy()
        {
            Assert.That(TutorialCatalog.All.Select(d => d.Id).Distinct().Count(), Is.EqualTo(TutorialCatalog.All.Length));
            foreach (var d in TutorialCatalog.All) { Assert.Less(d.Body.Length, 230, d.Id); Assert.IsNotEmpty(d.Title); }
            Assert.That(TutorialCatalog.Find("boss").Priority, Is.GreaterThan(TutorialCatalog.Find("speed").Priority));
        }
    }
}
