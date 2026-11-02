using System;
using ShorelineDefense.Defenses;
using UnityEngine;

namespace ShorelineDefense.Progression
{
    /// <summary>Effective values, not live node references. Future catalog edits cannot change Continue.</summary>
    [Serializable] public sealed class RunMetaSnapshot
    {
        public int version = 1, startingRsd;
        public float preparationRepair = 1, combatRepair = 1, upgradeCost = 1, refundBonus, waveReward = 1;
        public float blockingHealth = 1, wireHealth = 1, ballisticDamage = 1, sniperRange = 1, areaRadius = 1, burnDuration = 1, fireRate = 1;
        public bool countIntel, threatIntel, compositionIntel, eventForecast, deepWaterIntel;
        public RunMetaSnapshot Copy() => (RunMetaSnapshot)MemberwiseClone();
        public static bool Valid(RunMetaSnapshot s)
        {
            if (s == null || s.version != 1 || s.startingRsd < 0 || s.startingRsd > 1000) return false;
            foreach (float f in new[] { s.preparationRepair, s.combatRepair, s.upgradeCost }) if (!float.IsFinite(f) || f < .5f || f > 1) return false;
            foreach (float f in new[] { s.waveReward, s.blockingHealth, s.wireHealth, s.ballisticDamage, s.sniperRange, s.areaRadius, s.burnDuration, s.fireRate })
                if (!float.IsFinite(f) || f < 1 || f > 1.5f) return false;
            return float.IsFinite(s.refundBonus) && s.refundBonus >= 0 && s.refundBonus <= .15f;
        }
    }
    /// <summary>Immutable cached run queries. No disk/profile access in gameplay.</summary>
    public sealed class MetaModifierService
    {
        public static readonly MetaModifierService Neutral = new MetaModifierService(new RunMetaSnapshot());
        private readonly RunMetaSnapshot values;
        public MetaModifierService(RunMetaSnapshot snapshot)
        { if (!RunMetaSnapshot.Valid(snapshot)) throw new ArgumentException("Invalid research snapshot."); values = snapshot.Copy(); }
        public RunMetaSnapshot Capture() => values.Copy();
        public int StartingRsd => values.startingRsd;
        public double RepairMultiplier(bool combat) => Math.Round(combat ? values.combatRepair : values.preparationRepair, 6);
        public double UpgradeMultiplier => Math.Round(values.upgradeCost, 6);
        public double RefundBonus => Math.Round(values.refundBonus, 6);
        public double WaveRewardMultiplier => Math.Round(values.waveReward, 6);
        public bool CountIntel => values.countIntel;
        public bool ThreatIntel => values.threatIntel;
        public bool CompositionIntel => values.compositionIntel;
        public bool EventForecast => values.eventForecast;
        public bool DeepWaterIntel => values.deepWaterIntel;
        public float HealthMultiplier(DefenseDefinition d) => d.Id == "barbed-wire" ? values.wireHealth : d.BlocksMovement && d.Id != "land-mine" ? values.blockingHealth : 1;
        public int MaxHealth(DefenseDefinition d, int level) => Mathf.Max(1, Mathf.RoundToInt(d.MaxHealth * HealthMultiplier(d) * (d.LevelData(level)?.health ?? 1)));
        public float DamageMultiplier(DefenseDefinition d) => d.Id == "machine-gun-nest" || d.Id == "autocannon" ? values.ballisticDamage : 1;
        public float RangeMultiplier(DefenseDefinition d) => d.Id == "sniper-tower" ? values.sniperRange : 1;
        public float SplashMultiplier(DefenseDefinition d) => d.Id == "mortar" || d.Id == "rocket" ? values.areaRadius : 1;
        public float BurnMultiplier(DefenseDefinition d) => d.Id == "flamethrower" ? values.burnDuration : 1;
        public float FireRateMultiplier(DefenseDefinition d) => d.BlocksMovement && d.Id != "land-mine" && d.Id != "barbed-wire" ? values.fireRate : 1;
        public static RunMetaSnapshot FromProfile(PlayerProfileService profile, ResearchCatalog catalog)
        {
            var s = new RunMetaSnapshot();
            if (catalog == null) return s;
            foreach (var n in catalog.nodes)
            {
                if (n == null) continue;
                int rank = profile.Rank(n.id);
                foreach (var e in n.effects)
                {
                    float a = e.amount * rank;
                    switch (e.kind)
                    {
                        case ResearchEffectKind.StartingRsd: s.startingRsd += Mathf.RoundToInt(a); break;
                        case ResearchEffectKind.PreparationRepairDiscount: s.preparationRepair -= a; break;
                        case ResearchEffectKind.CombatRepairDiscount: s.combatRepair -= a; break;
                        case ResearchEffectKind.RefundBonus: s.refundBonus += a; break;
                        case ResearchEffectKind.WaveRewardBonus: s.waveReward += a; break;
                        case ResearchEffectKind.BlockingHealthBonus: s.blockingHealth += a; break;
                        case ResearchEffectKind.WireHealthBonus: s.wireHealth += a; break;
                        case ResearchEffectKind.UpgradeDiscount: s.upgradeCost -= a; break;
                        case ResearchEffectKind.BallisticDamageBonus: s.ballisticDamage += a; break;
                        case ResearchEffectKind.SniperRangeBonus: s.sniperRange += a; break;
                        case ResearchEffectKind.AreaRadiusBonus: s.areaRadius += a; break;
                        case ResearchEffectKind.BurnDurationBonus: s.burnDuration += a; break;
                        case ResearchEffectKind.FireRateBonus: s.fireRate += a; break;
                        case ResearchEffectKind.EnemyCountIntel: s.countIntel |= rank > 0; break;
                        case ResearchEffectKind.ThreatIntel: s.threatIntel |= rank > 0; break;
                        case ResearchEffectKind.CompositionIntel: s.compositionIntel |= rank > 0; break;
                        case ResearchEffectKind.EventForecast: s.eventForecast |= rank > 0; break;
                        case ResearchEffectKind.DeepWaterIntel: s.deepWaterIntel |= rank > 0; break;
                    }
                }
            }
            return s;
        }
    }
}
