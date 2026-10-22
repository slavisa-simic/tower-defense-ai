using UnityEngine;

namespace ShorelineDefense.Defenses
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Heavy Weapon Data")]
    public sealed class HeavyWeaponData : ScriptableObject
    {
        public float minimumRangeInCells;
        public float splashRadiusInCells = 2;
        public float travelTime = .65f;
        public float arcHeight = 2;
        [Range(1, 32)] public int clusterCandidates = 16;
        [Range(1, 180)] public float coneAngle = 55;
        public float damageTick = .2f;
        public float burnDuration = 2;
        public float burnDps = 15;
        public bool IsValid(float range) => minimumRangeInCells >= 0 && minimumRangeInCells < range &&
            splashRadiusInCells > 0 && splashRadiusInCells < float.PositiveInfinity &&
            travelTime > 0 && travelTime < float.PositiveInfinity && arcHeight >= 0 && arcHeight < float.PositiveInfinity &&
            clusterCandidates >= 1 && clusterCandidates <= 32 && coneAngle > 0 && coneAngle <= 180 &&
            damageTick >= .05f && damageTick <= 1 && burnDuration >= 0 && burnDuration <= 30 && burnDps >= 0 && burnDps <= 1000;
    }
}
