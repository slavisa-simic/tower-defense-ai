using System;
using System.Collections.Generic;
using ShorelineDefense.Persistence;
using UnityEngine;

namespace ShorelineDefense.Progression
{
    public enum MilestoneCondition { WaveReached, FirstBoss, Dredger, Broodmother, LowHomicides, MapWave }
    [Serializable] public sealed class ResearchMilestone
    {
        public string id, mapId;
        public MilestoneCondition condition;
        public int wave, reward;
        public long maxHomicides = 250;
        public float maxHomicidesPerWave;
        public bool Matches(RunRecord r) => condition switch
        {
            MilestoneCondition.WaveReached => r.wave >= wave,
            MilestoneCondition.FirstBoss => r.bosses > 0,
            MilestoneCondition.Dredger => r.dredgers > 0,
            MilestoneCondition.Broodmother => r.broodmothers > 0,
            MilestoneCondition.LowHomicides => r.wave >= wave && r.homicides <= Math.Max(maxHomicides, r.wave * (double)maxHomicidesPerWave),
            MilestoneCondition.MapWave => r.mapId == mapId && r.wave >= wave,
            _ => false
        };
    }
    [CreateAssetMenu(menuName = "Shoreline Defense/Research Rewards")]
    public sealed class ResearchRewardSettings : ScriptableObject
    {
        public int minimumWave = 5, bossReward = 10, qualityWaveStep = 20, qualityPerStep = 5, qualityMaximum = 20;
        public float waveFactor = .5f;
        public long qualityHomicidesMaximum = 250;
        // Normal loss occurs at a fixed Homicides threshold. Scale the allowance by run
        // length so a long, well-defended failed run can still earn a quality bonus.
        public float qualityHomicidesPerWaveMaximum = 25;
        public int maximumStandardReward = 10000;
        public ResearchMilestone[] milestones = Array.Empty<ResearchMilestone>();
        public RunAwardReceipt Calculate(RunRecord r, ICollection<string> claimed, out List<string> newClaims)
        {
            var a = new RunAwardReceipt { runId = r.runId }; newClaims = new List<string>();
            if (r.wave >= minimumWave)
            {
                a.waveRP = (int)Math.Min(maximumStandardReward, Math.Max(0, Math.Floor(r.wave * (double)waveFactor)));
                a.bossRP = (int)Math.Min(maximumStandardReward, (double)r.bosses * Math.Max(0, bossReward));
                if (r.homicides <= Math.Max(qualityHomicidesMaximum, r.wave * (double)qualityHomicidesPerWaveMaximum))
                    a.qualityRP = Math.Min(qualityMaximum, r.wave / Math.Max(1, qualityWaveStep) * Math.Max(0, qualityPerStep));
            }
            foreach (var m in milestones)
                if (!claimed.Contains(m.id) && !newClaims.Contains(m.id) && m.Matches(r)) { newClaims.Add(m.id); a.milestoneRP += Math.Max(0, m.reward); }
            return a;
        }
    }
}
