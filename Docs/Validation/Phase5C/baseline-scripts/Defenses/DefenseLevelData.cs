using System;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    /// <summary>Absolute multipliers relative to Level I, never compounded between upgrades.</summary>
    [Serializable]
    public sealed class DefenseLevelData
    {
        [Min(1)] public int cost = 100;
        [Min(1)] public float health = 1.25f;
        [Min(1)] public float damage = 1.25f;
        [Min(1)] public float fireRate = 1.1f;
        [Min(1)] public float range = 1f;
        [Min(1)] public float splash = 1f;
        [Range(.05f, 1f)] public float slow = 1f;
        [Min(1)] public float burnDuration = 1f;
        [Min(1)] public float burnDps = 1f;
        public Sprite overlay;
    }
}
