using UnityEngine;

namespace ShorelineDefense.Enemies
{
    public enum EnemyKind { Basic, Runner, Brute, IsisBreacher, IsisHeavy, PirateRunner, PirateRaider, UndeadOsama, PirateWarlord }
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
        public int BreachPenalty => Mathf.Max(0, breachPenalty);
        public int DifficultyCost => Mathf.Max(1, difficultyCost);
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
