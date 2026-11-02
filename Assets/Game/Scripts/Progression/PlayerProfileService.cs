using System;
using System.Collections.Generic;
using System.IO;
using ShorelineDefense.Persistence;
using UnityEngine;

namespace ShorelineDefense.Progression
{
    /// <summary>Independent atomic profile transactions. Publish only after the primary write commits.</summary>
    public sealed class PlayerProfileService
    {
        private readonly LocalJsonStore store;
        private PlayerProfileData current;
        public bool Available { get; private set; }
        public string Error { get; private set; }
        public long ResearchPoints => current.researchPoints;
        public PlayerProfileData Current => Clone(current);
        public event Action Changed;
        public PlayerProfileService(LocalJsonStore files)
        {
            store = files;
            bool exists = File.Exists(store.PathFor("profile_save")) || File.Exists(store.PathFor("profile_save") + ".bak");
            Available = store.Read<PlayerProfileData>("profile_save", Valid, out var data, out string error) || !exists;
            current = data ?? new PlayerProfileData { profileSaveVersion = 1 }; Error = error;
            if (!exists) foreach (var record in new RunHistoryService(files).Load().runs)
            {
                string map = string.IsNullOrEmpty(record.mapId) ? ShorelineDefense.Grid.MapCatalog.OriginalId : record.mapId;
                var best = current.mapBestRecords.Find(r => r.mapId == map);
                if (best == null) current.mapBestRecords.Add(new MapBestRecord { mapId = map, highestWave = record.wave });
                else best.highestWave = Math.Max(best.highestWave, record.wave);
            }
        }
        private static PlayerProfileData Clone(PlayerProfileData d) => JsonUtility.FromJson<PlayerProfileData>(JsonUtility.ToJson(d));
        public int Rank(string id) => current.unlockedResearchNodes.Find(n => n.id == id)?.rank ?? 0;
        public bool HasCompleted(string runId) => current.completedRuns.Exists(r => r.runId == runId);
        public RunAwardReceipt Receipt(string runId)
        {
            var r = current.completedRuns.Find(a => a.runId == runId);
            return r == null ? null : new RunAwardReceipt { runId = r.runId, waveRP = r.waveRP, bossRP = r.bossRP, qualityRP = r.qualityRP, milestoneRP = r.milestoneRP };
        }
        public int BestWave(string mapId) => current.mapBestRecords.Find(r => r.mapId == mapId)?.highestWave ?? 0;
        private bool Commit(PlayerProfileData next, out string error)
        {
            error = null;
            if (!Available) { error = Error ?? "Profile unavailable. Existing files have been preserved."; return false; }
            if (!Valid(next)) { error = "Profile transaction contains invalid values."; return false; }
            if (!store.Write("profile_save", next, out error)) { Error = error; return false; }
            current = next;
            // Backup carries the same receipts, preventing replay after primary-file corruption.
            Error = store.Write("profile_save", next, out var backupError) ? null : "Profile saved; backup refresh failed: " + backupError;
            if (Changed != null) foreach (Action observer in Changed.GetInvocationList())
                try { observer(); } catch (Exception exception) { Debug.LogException(exception); }
            return true;
        }
        internal bool Purchase(ResearchNodeDefinition node, out string error)
        {
            var next = Clone(current);
            if (next.researchPoints < node.costRP) { error = "Insufficient RP."; return false; }
            next.researchPoints -= node.costRP; next.lifetimeStatistics.rpSpent = Add(next.lifetimeStatistics.rpSpent, node.costRP);
            var state = next.unlockedResearchNodes.Find(n => n.id == node.id);
            if (state == null) next.unlockedResearchNodes.Add(new ResearchNodeState { id = node.id, rank = 1 }); else state.rank++;
            return Commit(next, out error);
        }
        public bool CompleteRun(RunRecord run, ResearchRewardSettings tuning, out RunAwardReceipt award, out string error)
        {
            if (run == null) { award = null; error = "Run result unavailable."; return false; }
            award = Receipt(run.runId); error = null;
            if (award != null) return true;
            if (tuning == null || !Guid.TryParse(run.runId, out _) || run.wave < 0 || run.completedWaves < 0 || run.kills < 0 || run.homicides < 0 || run.bosses < 0 || run.dredgers < 0 || run.broodmothers < 0)
            { error = "Invalid run reward data."; return false; }
            var next = Clone(current);
            var calculated = tuning.Calculate(run, next.claimedMilestones, out var claims);
            next.researchPoints = Add(next.researchPoints, calculated.Total);
            next.claimedMilestones.AddRange(claims); next.completedRuns.Add(calculated);
            var s = next.lifetimeStatistics;
            s.totalRuns = Add(s.totalRuns, 1); s.wavesSurvived = Add(s.wavesSurvived, run.completedWaves);
            s.kills = Add(s.kills, run.kills); s.homicides = Add(s.homicides, run.homicides);
            s.bossesKilled = Add(s.bossesKilled, run.bosses); s.dredgersKilled = Add(s.dredgersKilled, run.dredgers); s.broodmothersKilled = Add(s.broodmothersKilled, run.broodmothers);
            s.rpEarned = Add(s.rpEarned, calculated.Total);
            var best = next.mapBestRecords.Find(r => r.mapId == run.mapId);
            if (best == null) next.mapBestRecords.Add(new MapBestRecord { mapId = run.mapId, highestWave = run.wave });
            else best.highestWave = Math.Max(best.highestWave, run.wave);
            if (!Commit(next, out error)) return false;
            award = Receipt(run.runId); return true;
        }
        private static long Add(long a, long b) => a > long.MaxValue - b ? long.MaxValue : a + b;
        public static bool Valid(PlayerProfileData d)
        {
            if (d == null || d.profileSaveVersion != 1 || d.researchPoints < 0 || d.unlockedResearchNodes == null || d.unlockedResearchNodes.Count > 128 || d.lifetimeStatistics == null ||
                d.mapBestRecords == null || d.mapBestRecords.Count > 128 || d.claimedMilestones == null || d.claimedMilestones.Count > 256 || d.completedRuns == null) return false;
            var ids = new HashSet<string>();
            foreach (var n in d.unlockedResearchNodes) if (n == null || string.IsNullOrEmpty(n.id) || n.rank < 1 || n.rank > 3 || !ids.Add(n.id)) return false;
            ids.Clear(); foreach (var id in d.claimedMilestones) if (string.IsNullOrEmpty(id) || !ids.Add(id)) return false;
            ids.Clear(); foreach (var r in d.completedRuns) if (r == null || !Guid.TryParse(r.runId, out _) || !ids.Add(r.runId) || r.waveRP < 0 || r.bossRP < 0 || r.qualityRP < 0 || r.milestoneRP < 0 || (long)r.waveRP + r.bossRP + r.qualityRP + r.milestoneRP > int.MaxValue) return false;
            ids.Clear(); foreach (var r in d.mapBestRecords) if (r == null || string.IsNullOrEmpty(r.mapId) || r.highestWave < 0 || !ids.Add(r.mapId)) return false;
            var s = d.lifetimeStatistics;
            return s.totalRuns >= 0 && s.wavesSurvived >= 0 && s.kills >= 0 && s.homicides >= 0 && s.bossesKilled >= 0 && s.dredgersKilled >= 0 && s.broodmothersKilled >= 0 && s.rpEarned >= 0 && s.rpSpent >= 0;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public bool DebugGrantRP(int amount, out string error)
        { var next = Clone(current); next.researchPoints = Add(next.researchPoints, Math.Max(0, amount)); return Commit(next, out error); }
        public bool DebugUnlock(ResearchCatalog catalog, bool all, string nodeId, out string error)
        {
            var next = Clone(current);
            foreach (var n in catalog.nodes) if (n != null && (all || n.id == nodeId) && !next.unlockedResearchNodes.Exists(s => s.id == n.id)) next.unlockedResearchNodes.Add(new ResearchNodeState { id = n.id, rank = n.maxRank });
            return Commit(next, out error);
        }
        public bool DebugResetResearch(out string error)
        { var next = Clone(current); next.unlockedResearchNodes.Clear(); return Commit(next, out error); }
        public bool DebugResetStatistics(out string error)
        { var next = Clone(current); next.lifetimeStatistics = new MetaStatistics(); return Commit(next, out error); }
#endif
    }
}
