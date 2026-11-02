using System;
using UnityEngine;

namespace ShorelineDefense.Progression
{
    public enum ResearchBranch { Logistics, Engineering, Weapons, Intelligence }
    public enum ResearchEffectKind
    {
        StartingRsd, PreparationRepairDiscount, RefundBonus, WaveRewardBonus, BlockingHealthBonus,
        WireHealthBonus, CombatRepairDiscount, UpgradeDiscount, BallisticDamageBonus, SniperRangeBonus,
        AreaRadiusBonus, BurnDurationBonus, FireRateBonus, EnemyCountIntel, ThreatIntel, CompositionIntel,
        EventForecast, DeepWaterIntel
    }
    [Serializable] public sealed class ResearchEffectDefinition
    {
        public ResearchEffectKind kind;
        public float amount;
    }
    [CreateAssetMenu(menuName = "Shoreline Defense/Research Node")]
    public sealed class ResearchNodeDefinition : ScriptableObject
    {
        public string id, displayName;
        [TextArea] public string description;
        public ResearchBranch branch;
        [Min(1)] public int costRP = 10, tier = 1, maxRank = 1;
        public ResearchNodeDefinition[] prerequisites = Array.Empty<ResearchNodeDefinition>();
        public Sprite icon;
        public ResearchEffectDefinition[] effects = Array.Empty<ResearchEffectDefinition>();
        public Vector2 boardPosition;
    }
}
