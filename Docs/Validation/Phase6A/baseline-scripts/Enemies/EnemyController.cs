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
        private BorderBreachService border;
        private bool resolved;
        private System.Action<EnemyController> returnToPool;
        public void ConfigurePool(System.Action<EnemyController> release) => returnToPool = release;
        public EnemyDefinition Definition => definition;
        public EnemyMovement Movement => movement;
        public EnemyHealth Health => health;
        public bool IsTargetable => initialized && !resolved && isActiveAndEnabled && health.IsAlive && game != null && game.IsPlaying;

        public void Configure(EnemyDefinition data) => definition = data;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            combat = GetComponent<EnemyCombat>();
            health = GetComponent<EnemyHealth>();
            health.Died += OnDied;
        }

        public void Initialize(GridManager grid, BaseHealth playerBase, GameStateController state, EnemyRegistry enemies = null,
            BorderBreachService breach = null, Vector2Int? breachGoal = null, float healthMultiplier = 1, float speedMultiplier = 1)
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
            border = breach; resolved = false;
            if (!grid.Grid.TryWorldToCell(target.transform.position, out var goal))
            {
                Debug.LogError("Player base must be inside the battlefield grid.", target);
                enabled = false;
                return;
            }
            goal = breachGoal ?? goal;
            health.ConfigureArmor(definition.Armor);
            health.Initialize((int)System.Math.Min(int.MaxValue, System.Math.Max(1, System.Math.Ceiling(definition.MaxHealth * (double)healthMultiplier))));
            movement.Initialize(grid.Grid, grid.Paths, goal, definition.MovementSpeed * speedMultiplier, border != null ? .05f : definition.AttackRange);
            combat.Initialize(target, definition);
            initialized = true;
            if (isActiveAndEnabled) registry?.Register(this);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }
        public void Tick(float deltaTime)
        {
            if (!initialized || resolved || !game.IsPlaying || !health.IsAlive || target == null || !target.IsAlive) return;
            bool wasInRange = movement.ReachedDestination;
            movement.Tick(deltaTime);
            if (border != null) { if (movement.ReachedDestination) border.TryBreach(this); return; }
            if (movement.ReachedDestination)
                movement.FaceTowards(target.transform.position);
            // Begin accumulating attack time on the first full frame spent at the base.
            combat.Tick(deltaTime, wasInRange && movement.ReachedDestination);
        }

        private void OnDied()
        {
            if (resolved) return;
            resolved = true;
            registry?.NotifyKilled(this);
            // Health is already zero, so targeting/movement/attacks stop before deferred destruction.
            if (returnToPool != null) { gameObject.SetActive(false); returnToPool(this); }
            else Destroy(gameObject);
        }
        public bool TryResolveBreach() { if (!IsTargetable) return false; resolved = true; return true; }
        public void RemoveBreached() { if (!resolved) return; registry?.Unregister(this); gameObject.SetActive(false); if (returnToPool != null) returnToPool(this); else Destroy(gameObject); }
        private void OnEnable() { if (initialized && health.IsAlive) registry?.Register(this); }
        private void OnDisable() => registry?.Unregister(this);
        private void OnDestroy()
        {
            registry?.Unregister(this);
            if (health != null) health.Died -= OnDied;
        }
    }
}
