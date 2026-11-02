using System;
using System.Collections.Generic;

namespace ShorelineDefense.Progression
{
    [Serializable] public sealed class ResearchNodeState { public string id; public int rank; }
    [Serializable] public sealed class MetaStatistics
    {
        public long totalRuns, wavesSurvived, kills, homicides, bossesKilled, dredgersKilled, broodmothersKilled, rpEarned, rpSpent;
    }
    [Serializable] public sealed class MapBestRecord { public string mapId; public int highestWave; }
    [Serializable] public sealed class RunAwardReceipt
    {
        public string runId;
        public int waveRP, bossRP, qualityRP, milestoneRP;
        public int Total => waveRP + bossRP + qualityRP + milestoneRP;
    }
    [Serializable] public sealed class PlayerProfileData
    {
        // Zero is invalid: a truncated '{}' file must not be mistaken for a new profile.
        public int profileSaveVersion;
        public long researchPoints;
        public List<ResearchNodeState> unlockedResearchNodes = new List<ResearchNodeState>();
        public MetaStatistics lifetimeStatistics = new MetaStatistics();
        public List<MapBestRecord> mapBestRecords = new List<MapBestRecord>();
        public List<string> claimedMilestones = new List<string>();
        // Independent of the 20-entry run history: evicting history must never enable an RP replay.
        public List<RunAwardReceipt> completedRuns = new List<RunAwardReceipt>();
    }
}
