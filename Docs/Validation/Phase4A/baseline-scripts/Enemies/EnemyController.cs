using ShorelineDefense.Base;
using ShorelineDefense.Core;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Enemies
{
    [RequireComponent(typeof(EnemyMovement), typeof(EnemyCombat), typeof(EnemyHealth))]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        private EnemyMovement movement;
        private EnemyCombat combat;
        private EnemyHealth health;
        private GameStateController game;
        private BaseHealth target;
        private bool initialized;
        private EnemyRegistry registry;
        public EnemyDefinition Definition => definition;
        public EnemyMovement Movement => movement;
        public EnemyHealth Health => health;
        public bool IsTargetable => initialized && isActiveAndEnabled && health.IsAlive && game != null && game.IsPlaying;

        public void Configure(EnemyDefinition data) => definition = data;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            combat = GetComponent<EnemyCombat>();
            health = GetComponent<EnemyHealth>();
            health.Died += OnDied;
        }

        public void Initialize(GridManager grid, BaseHealth playerBase, GameStateController state, EnemyRegistry enemies = null)
        {
            if (definition == null || grid == null || grid.Grid == null || playerBase == null || state == null)
            {
                Debug.LogError("Crawler initialization is missing configuration or gameplay references.", this);
                enabled = false;
                return;
            }
            game = state;
            registry?.Unregister(this);
            registry = enemies;
            target = playerBase;
            if (!grid.Grid.TryWorldToCell(target.transform.position, out var goal))
            {
                Debug.LogError("Player base must be inside the battlefield grid.", target);
                enabled = false;
                return;
            }
            health.Initialize(definition.MaxHealth);
            movement.Initialize(grid.Grid, grid.Paths, goal, definition.MovementSpeed, definition.AttackRange);
            combat.Initialize(target, definition);
            initialized = true;
            if (isActiveAndEnabled) registry?.Register(this);
        }

        private void Update()
        {
            if (!initialized || !game.IsPlaying || !health.IsAlive || target == null || !target.IsAlive) return;
            bool wasInRange = movement.ReachedDestination;
            movement.Tick(Time.deltaTime);
            if (movement.ReachedDestination)
                movement.FaceTowards(target.transform.position);
            // Begin accumulating attack time on the first full frame spent at the base.
            combat.Tick(Time.deltaTime, wasInRange && movement.ReachedDestination);
        }

        private void OnDied()
        {
            registry?.NotifyKilled(this);
            // Health is already zero, so targeting/movement/attacks stop before deferred destruction.
            Destroy(gameObject);
        }
        private void OnEnable() { if (initialized && health.IsAlive) registry?.Register(this); }
        private void OnDisable() => registry?.Unregister(this);
        private void OnDestroy()
        {
            registry?.Unregister(this);
            if (health != null) health.Died -= OnDied;
        }
    }
}
