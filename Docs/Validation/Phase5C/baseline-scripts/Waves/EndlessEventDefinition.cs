using System;
using ShorelineDefense.Enemies;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    public enum EventDuration { NextWave, EntireWave, Timed, Preparation, MultiWave }
    public enum EventCategory { Challenge, Relief, Pattern }
    public enum LandingPattern { Normal, Distributed, Concentrated }
    public enum LandingSector { South, Center, North }

    [Serializable]
    public sealed class CompositionBias
    {
        public EnemyKind kind;
        [Min(0)] public float multiplier = 1;
    }

    [Serializable]
    public sealed class EventModifiers
    {
        [Min(.1f)] public float quantity = 1, pressure = 1, boatSpeed = 1, boatInterval = 1, boatConcurrency = 1;
        [Range(.5f, 1)] public float defenseRange = 1;
        [Min(.1f)] public float upgradeCost = 1, combatRepair = 1, preparationRepair = 1;
        [Range(0, 3)] public int floodedColumns;
        public bool fog;
        public GameObject boatVisual;
        public LandingPattern landingPattern;
        public CompositionBias[] composition = Array.Empty<CompositionBias>();
        public float Weight(EnemyKind kind)
        {
            foreach (var bias in composition) if (bias != null && bias.kind == kind) return Mathf.Max(0, bias.multiplier);
            return 1;
        }
    }

    [CreateAssetMenu(menuName = "Shoreline Defense/Endless Event")]
    public sealed class EndlessEventDefinition : ScriptableObject
    {
        public string id, displayName;
        [TextArea] public string description;
        public EventCategory category;
        [Min(1)] public int minimumWave = 5;
        [Min(0)] public int maximumWave;
        [Min(0)] public float weight = 1;
        public EventDuration durationType = EventDuration.EntireWave;
        [Min(1)] public float duration = 1;
        [Min(1)] public int cooldown = 8;
        public bool allowBossWave, preparationEffects;
        public string[] incompatibleEvents = Array.Empty<string>();
        public Sprite uiIcon;
        public EventModifiers modifiers = new EventModifiers();
        [Min(0)] public int supplyBase, supplyPerWave, supplyMaximum = 500;
        [Min(0)] public float preparationWarningSeconds;
        public bool Eligible(int wave, bool boss) => !string.IsNullOrWhiteSpace(id) && weight > 0 && wave >= minimumWave &&
            (maximumWave == 0 || wave <= maximumWave) && (!boss || allowBossWave);
        public bool Compatible(EndlessEventDefinition other) => other == null ||
            other != this && Array.IndexOf(incompatibleEvents, other.id) < 0 && Array.IndexOf(other.incompatibleEvents, id) < 0;
    }
}
