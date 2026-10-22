using System;
using UnityEngine;

namespace ShorelineDefense.Persistence
{
    public sealed class SaveGameService
    {
        private readonly LocalJsonStore store;
        private readonly RunHistoryService history;
        private readonly Func<RunSaveData, bool> compatibility;
        public SaveGameService(LocalJsonStore files, Func<RunSaveData, bool> preflight = null) { store = files; history = new RunHistoryService(files); compatibility = preflight; }
        public bool Save(RunSaveData data, out string error)
        {
            if (!Valid(data)) { error = "Run is not a valid Preparation snapshot."; return false; }
            data.savedUtc = DateTime.UtcNow.ToString("O");
            return store.Write("active_run", data, out error);
        }
        public bool Load(out RunSaveData data, out string error) => store.Read("active_run", d => Valid(d) && (compatibility == null || compatibility(d)) && !history.Contains(d.runId), out data, out error);
        public bool Delete(out string error) => store.Delete("active_run", out error);
        public static bool Valid(RunSaveData d)
        {
            if (d == null || (d.saveVersion != 1 && d.saveVersion != 2) || (d.saveVersion == 2 && string.IsNullOrEmpty(d.mapId)) || !Guid.TryParse(d.runId, out _) || d.wave < 1 || d.wave > 1000000 ||
                d.completedWaves != d.wave - 1 || d.money < 0 || d.maxHomicides < 0 || double.IsNaN(d.duration) || double.IsInfinity(d.duration) || d.duration < 0 ||
                d.earned < 0 || d.spent < 0 || d.refunds < 0 || d.dismantled < 0 || d.statistics?.kills?.Length != 9 || d.events == null || d.structures == null || d.structures.Count > 2048) return false;
            var s = d.statistics;
            if (s.homicides < 0 || s.breaches < 0 || s.bossesBreached < 0 || s.highestWave < 0 || s.built < 0 || s.upgraded < 0 || s.repaired < 0 ||
                s.peakEnemies < 0 || s.peakBoats < 0 || d.maxHomicides > 0 && s.homicides >= d.maxHomicides) return false;
            foreach (long k in s.kills) if (k < 0) return false;
            foreach (var structure in d.structures)
                if (structure == null || string.IsNullOrEmpty(structure.defenseId) || structure.level < 1 || structure.level > 3 || structure.health <= 0 ||
                    structure.health > structure.maxHealth || structure.investment < structure.constructionCost || structure.constructionCost < 0 ||
                    !float.IsFinite(structure.mineArmingRemaining) || structure.mineArmingRemaining < 0) return false;
            var e = d.events;
            if (e.recent == null || e.cooldowns == null || e.counts == null || e.history == null || e.history.Count > 128 || e.recent.Count > 128 ||
                e.cooldowns.Count > 128 || e.counts.Count > 128 || e.experienced < 0 || e.nextWave < 0 || e.sector < 0 || e.sector > 2 ||
                !float.IsFinite(e.warningRemaining) || !float.IsFinite(e.secondsRemaining) || e.warningRemaining < 0 || e.secondsRemaining < 0) return false;
            foreach (var list in new[] { e.cooldowns, e.counts, e.history }) foreach (var item in list) if (item == null || string.IsNullOrEmpty(item.id) || item.value < 0) return false;
            return true;
        }
    }
    public sealed class RunHistoryService
    {
        private readonly LocalJsonStore store;
        public string Error { get; private set; }
        public RunHistoryService(LocalJsonStore files) { store = files; }
        public HistoryData Load()
        {
            bool ok = store.Read<HistoryData>("run_history", d => d.saveVersion == 1 && d.runs != null && d.runs.Count <= 20 && d.runs.TrueForAll(r => r != null && Guid.TryParse(r.runId, out _) && r.wave >= 0 && r.kills >= 0 && double.IsFinite(r.duration) && r.duration >= 0), out var data, out string error);
            Error = error; return ok ? data : new HistoryData();
        }
        public bool Contains(string id) => Load().runs.Exists(r => r.runId == id);
        public bool Record(RunRecord record, out string error)
        {
            var data = Load();
            if (data.runs.Exists(r => r.runId == record.runId)) { error = null; return true; }
            data.runs.Insert(0, record);
            if (data.runs.Count > 20) data.runs.RemoveRange(20, data.runs.Count - 20);
            // Two durable copies ensure a recovered history also excludes the latest dead run.
            if (!store.Write("run_history", data, out error)) return false;
            return store.Write("run_history", data, out error);
        }
    }
}
