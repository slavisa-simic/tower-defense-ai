using UnityEngine;

namespace ShorelineDefense.Enemies
{
    [CreateAssetMenu(menuName = "Shoreline Defense/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField, Min(1)] private int transportCapacityUnits = 1;
        public int TransportCapacityUnits => transportCapacityUnits;
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
