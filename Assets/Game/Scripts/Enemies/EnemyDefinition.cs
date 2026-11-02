using UnityEngine;

namespace ShorelineDefense.Enemies
{
    // Explicit slots preserve serialized references. Slot six is reserved for migrated totals.
    public enum EnemyKind { Crawler = 0, Skitter = 1, Bulwark = 2, Spitter = 3, Broodling = 4, Burrower = 5, LegacyUnclassified = 6, Dredger = 7, Broodmother = 8 }
    public enum CreatureFamily { Swarm, Armored, Aberrant }
    [CreateAssetMenu(menuName = "Shoreline Defense/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField, Range(0, .9f)] private float armor;
        public float Armor => Mathf.Clamp(armor, 0, .9f);
        [SerializeField] private EnemyKind kind;
        [SerializeField, Min(0)] private int breachPenalty = 25;
        [SerializeField, Min(1)] private int difficultyCost = 1;
        [SerializeField, Min(1)] private int unlockWave = 1;
        public EnemyKind Kind => kind;
        public CreatureFamily Family => kind == EnemyKind.Bulwark || kind == EnemyKind.Dredger ? CreatureFamily.Armored :
            kind == EnemyKind.Spitter || kind == EnemyKind.Burrower ? CreatureFamily.Aberrant : CreatureFamily.Swarm;
        // Half-unit capacity permits two tiny Broodlings per ordinary cargo unit.
        public int CarrierHalfUnits => kind == EnemyKind.Broodling ? 1 : TransportCapacityUnits * 2;
        public int BreachPenalty => Mathf.Max(0, breachPenalty);
        public int DifficultyCost => Mathf.Max(1, difficultyCost);
        public int WaveCostHalfUnits => kind == EnemyKind.Broodling ? 1 : DifficultyCost * 2;
        public int UnlockWave => Mathf.Max(1, unlockWave);
        [SerializeField, Min(1)] private int transportCapacityUnits = 1;
        public int TransportCapacityUnits => Mathf.Max(1, transportCapacityUnits);
        [SerializeField] private string displayName = "Crawler";
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0.01f)] private float movementSpeed = 2.5f;
        [SerializeField, Min(1)] private int attackDamage = 10;
        [SerializeField, Min(0.05f)] private float attackInterval = 1f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.6f;
        [SerializeField, Min(0)] private int killReward = 25;
        [Header("Attacks against blocking player structures")]
        [SerializeField, Min(1)] private int structureDamage = 12;
        [SerializeField, Min(.1f)] private float structureAttackInterval = 1;
        [SerializeField, Range(.5f, 6)] private float structureRangeCells = .6f;
        public int StructureDamage => Mathf.Max(1, structureDamage);
        public float StructureAttackInterval => Mathf.Max(.1f, structureAttackInterval);
        public float StructureRangeCells => Mathf.Max(.5f, structureRangeCells);

        public string DisplayName => displayName;
        public int MaxHealth => maxHealth;
        public float MovementSpeed => movementSpeed;
        public int AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;
        public int KillReward => killReward;

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            movementSpeed = Mathf.Max(0.01f, movementSpeed);
            attackDamage = Mathf.Max(1, attackDamage);
            attackInterval = Mathf.Max(0.05f, attackInterval);
            attackRange = Mathf.Max(0.1f, attackRange);
            killReward = Mathf.Max(0, killReward);
        }
    }
}
