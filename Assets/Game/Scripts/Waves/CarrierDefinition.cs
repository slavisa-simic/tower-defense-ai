using UnityEngine;
using ShorelineDefense.Enemies;
namespace ShorelineDefense.Waves
{
    public enum ArrivalMethod { DirectEmergence, DriftPod, BroodCarrier, LeviathanCarrier }
    [CreateAssetMenu(menuName = "Shoreline Defense/Carrier Definition")]
    public sealed class CarrierDefinition : ScriptableObject
    {
        public ArrivalMethod method;
        public GameObject visualPrefab;
        [Min(1)] public int capacityHalfUnits = 24;
        public Vector2 halfExtents = new Vector2(1.1f, .65f);
        [Min(.1f)] public float speedMultiplier = 1;
        [Min(0)] public float ruptureSeconds = .35f, remainsSeconds = .7f;
        [Min(.02f)] public float releaseInterval = .07f;
        public bool Accepts(EnemyDefinition enemy) => enemy != null && (method != ArrivalMethod.DirectEmergence ||
            enemy.Kind == EnemyKind.Crawler || enemy.Kind == EnemyKind.Skitter || enemy.Kind == EnemyKind.Burrower || enemy.Kind == EnemyKind.Dredger);
    }
}
