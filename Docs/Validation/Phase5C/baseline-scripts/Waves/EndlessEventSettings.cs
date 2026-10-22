using System;
using UnityEngine;

namespace ShorelineDefense.Waves
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Endless Event Settings")]
    public sealed class EndlessEventSettings : ScriptableObject
    {
        public EndlessEventDefinition[] events = Array.Empty<EndlessEventDefinition>();
        [Min(5)] public int firstEventWave = 5;
        [Min(2)] public int minimumGap = 2, maximumGap = 4;
        [Min(5)] public int laterFrequencyWave = 30;
        [Min(2)] public int laterMinimumGap = 2, laterMaximumGap = 3;
        [Min(0)] public int recentExclusion = 2;
        [Min(0)] public float challengeWeight = .7f, reliefWeight = .2f, patternWeight = .1f;
    }
}
