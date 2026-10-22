using UnityEngine;

namespace ShorelineDefense.Defenses
{
    public enum TargetingMode { Closest, ClosestToBase, HighestHealth, BestCluster }

    [CreateAssetMenu(menuName = "Shoreline Defense/Defense Definition")]
    public sealed class DefenseDefinition : ScriptableObject
    {
        [SerializeField] private string id = "machine-gun-nest";
        [SerializeField] private string displayName = "MG Nest";
        [SerializeField, Min(0)] private int cost = 150;
        [SerializeField] private DefenseBase prefab;
        [SerializeField] private Vector2Int footprint = Vector2Int.one;
        [SerializeField, Min(1)] private int maxHealth = 300;
        [SerializeField, Min(0.1f)] private float rangeInCells = 5f;
        [SerializeField, Min(0)] private int damage = 20;
        [SerializeField, Min(0.1f)] private float fireRate = 4f;
        [SerializeField] private TargetingMode targetingMode;
        [SerializeField] private HeavyWeaponData heavyWeapon;
        public HeavyWeaponData HeavyWeapon => heavyWeapon;
        [Header("Upgrades: absolute multipliers from Level I")]
        [SerializeField] private DefenseLevelData levelII = new DefenseLevelData();
        [SerializeField] private DefenseLevelData levelIII = new DefenseLevelData { cost = 200, health = 1.5f, damage = 1.6f, fireRate = 1.25f, range = 1.1f };
        public DefenseLevelData LevelData(int level) => level == 2 ? levelII : level == 3 ? levelIII : null;
        [Header("Walkable area effects")]
        [SerializeField, Range(0.05f, 1f)] private float movementMultiplier = 0.5f;
        [SerializeField, Min(1)] private float traversalMultiplier = 2f;
        [Header("Proximity explosion")]
        [SerializeField, Min(0.1f)] private float triggerRadiusInCells = 0.55f;
        [SerializeField, Min(0)] private float armingDelay = 0.35f;
        public string Id => id;
        public string DisplayName => displayName;
        public int Cost => cost;
        public DefenseBase Prefab => prefab;
        public Vector2Int Footprint => footprint;
        public int MaxHealth => maxHealth;
        public float RangeInCells => rangeInCells;
        public int Damage => damage;
        public float FireRate => fireRate;
        public TargetingMode Targeting => targetingMode;
        public float MovementMultiplier => movementMultiplier;
        public float TraversalMultiplier => traversalMultiplier;
        public float TriggerRadiusInCells => triggerRadiusInCells;
        public float ArmingDelay => armingDelay;
        public bool IsValid => !string.IsNullOrWhiteSpace(id) && prefab != null && cost >= 0 &&
            footprint.x > 0 && footprint.y > 0 && maxHealth > 0 && damage >= 0 &&
            rangeInCells > 0 && !float.IsInfinity(rangeInCells) && fireRate > 0 && !float.IsInfinity(fireRate) &&
            movementMultiplier > 0 && movementMultiplier <= 1 && traversalMultiplier >= 1 && !float.IsInfinity(traversalMultiplier) &&
            triggerRadiusInCells > 0 && !float.IsInfinity(triggerRadiusInCells) && armingDelay >= 0 && !float.IsInfinity(armingDelay) &&
            (heavyWeapon == null || heavyWeapon.IsValid(rangeInCells));
    }
}
