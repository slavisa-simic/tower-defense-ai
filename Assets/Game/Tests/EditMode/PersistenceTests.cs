using System;
using System.IO;
using NUnit.Framework;
using ShorelineDefense.Persistence;

namespace ShorelineDefense.Tests
{
    public sealed class PersistenceTests
    {
        private string directory;
        private LocalJsonStore files;
        private SaveGameService saves;
        [SetUp] public void Setup() { directory = Path.Combine(Path.GetTempPath(), "BorderDefendersTests", Guid.NewGuid().ToString()); files = new LocalJsonStore(directory); saves = new SaveGameService(files); }
        [TearDown] public void Cleanup() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private static RunSaveData Snapshot() => new RunSaveData { runId = Guid.NewGuid().ToString(), wave = 7, completedWaves = 6, seed = 1977, money = 421, maxHomicides = 1000, duration = 84.5,
            statistics = new StatisticsSaveData { homicides = 50, highestWave = 6 }, events = new EventSaveData { nextWave = 8 } };
        [Test] public void RoundTripKeepsExactLongValuesAndVersion()
        {
            var data = Snapshot(); data.spent = 9007199254740993; data.statistics.kills[5] = 4294967297;
            Assert.IsTrue(saves.Save(data, out _)); Assert.IsTrue(saves.Load(out var loaded, out _));
            Assert.That(loaded.spent, Is.EqualTo(data.spent)); Assert.That(loaded.statistics.kills[5], Is.EqualTo(data.statistics.kills[5]));
            Assert.That(loaded.wave, Is.EqualTo(7)); Assert.That(loaded.duration, Is.EqualTo(84.5)); Assert.That(loaded.saveVersion, Is.EqualTo(2));
        }
        [Test] public void NewInstallationCanDeleteAnAbsentRunBeforeItsDirectoryExists()
        { Assert.IsFalse(Directory.Exists(directory)); Assert.IsTrue(saves.Delete(out string error), error); Assert.IsFalse(saves.Load(out _, out _)); }
        [Test] public void CorruptPrimaryRecoversBackupAndBothCorruptDisableContinue()
        {
            var data = Snapshot(); Assert.IsTrue(saves.Save(data, out _)); data.money = 111; Assert.IsTrue(saves.Save(data, out _));
            File.WriteAllText(files.PathFor("active_run"), "{broken");
            Assert.IsTrue(saves.Load(out var loaded, out string message)); Assert.That(loaded.money, Is.EqualTo(421)); Assert.That(message, Does.Contain("backup"));
            File.WriteAllText(files.PathFor("active_run") + ".bak", "{broken"); Assert.IsFalse(saves.Load(out _, out _));
        }
        [Test] public void UnsupportedVersionAndInvalidHealthAreRejected()
        {
            var data = Snapshot(); data.saveVersion = 99; Assert.IsFalse(saves.Save(data, out _));
            files.Write("active_run", data, out _); Assert.IsFalse(saves.Load(out _, out _));
            data.saveVersion = 1; data.structures.Add(new StructureSaveData { defenseId = "machine-gun-nest", level = 1, health = 301, maxHealth = 300, constructionCost = 150, investment = 150 });
            Assert.IsFalse(saves.Save(data, out _));
        }
        [Test] public void FailureDoesNotReplaceThePreviousValidSave()
        {
            var data = Snapshot(); saves.Save(data, out _); Directory.CreateDirectory(files.PathFor("active_run") + ".tmp");
            data.money = 123; Assert.IsFalse(saves.Save(data, out _)); Assert.IsTrue(saves.Load(out var loaded, out _)); Assert.That(loaded.money, Is.EqualTo(421));
        }
        [Test] public void HistoryFinalizationIsIdempotentBoundedAndRejectsDeadBackup()
        {
            var history = new RunHistoryService(files); var data = Snapshot(); saves.Save(data, out _); saves.Save(data, out _);
            var record = new RunRecord { runId = data.runId, wave = 6, duration = 80, kills = 500, endedUtc = DateTime.UtcNow.ToString("O") };
            Assert.IsTrue(history.Record(record, out _)); Assert.IsTrue(history.Record(record, out _)); Assert.That(history.Load().runs.Count, Is.EqualTo(1));
            Assert.IsFalse(saves.Load(out _, out _));
            File.WriteAllText(files.PathFor("run_history"), "corrupt"); Assert.IsFalse(saves.Load(out _, out _), "Recovered history must still exclude the dead backup");
            for (int i = 0; i < 25; i++) history.Record(new RunRecord { runId = Guid.NewGuid().ToString(), wave = i }, out _);
            Assert.That(history.Load().runs.Count, Is.EqualTo(20)); Assert.That(history.Load().runs[0].wave, Is.EqualTo(24));
            Assert.IsTrue(saves.Delete(out _)); Assert.IsFalse(File.Exists(files.PathFor("active_run") + ".bak"));
        }
        [Test] public void SettingsSurviveRunDeletionAndRejectInvalidDimensions()
        {
            var settings = new SettingsService(files);
            Assert.IsTrue(settings.Save(new GameSettings { width = 1600, height = 900, fullscreen = false, vsync = false, vfxDensity = 0, rewardPopups = false }));
            saves.Save(Snapshot(), out _); saves.Delete(out _);
            var reopened = new SettingsService(files).Current;
            Assert.That(reopened.width, Is.EqualTo(1600)); Assert.IsFalse(reopened.fullscreen); Assert.IsFalse(reopened.vsync); Assert.That(reopened.vfxDensity, Is.Zero); Assert.IsFalse(reopened.rewardPopups);
            Assert.IsFalse(settings.Save(new GameSettings { width = -1 }));
        }
    }
}
