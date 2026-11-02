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
        private SpitterAttack spitter;
        private BurrowerTraversal burrower;
        private BroodmotherSpawn brood;
        private float attackPose;
        public bool IsBurrowed => burrower != null && burrower.IsBurrowed;
        public bool IsAttacking => attackPose > 0 || spitter != null && spitter.IsAttacking;
        public bool IsCombatActive => initialized && !resolved && isActiveAndEnabled && health.IsAlive && game != null && game.IsPlaying;
        public event System.Action CreatureAttack;
        public event System.Action CreatureHit;
        public event System.Action CreatureDeath;
        public void NotifyAttack() { attackPose = .5f; Notify(CreatureAttack); }
        private void Notify(System.Action observers)
        { if (observers == null) return; foreach (System.Action observer in observers.GetInvocationList()) try { observer(); } catch (System.Exception e) { Debug.LogException(e, this); } }
        public void ConfigurePool(System.Action<EnemyController> release) => returnToPool = release;
        public EnemyDefinition Definition => definition;
        public EnemyMovement Movement => movement;
        public EnemyHealth Health => health;
        public bool IsTargetable => IsCombatActive && !IsBurrowed;

        public void Configure(EnemyDefinition data) => definition = data;

        private void Awake()
        {
            movement = GetComponent<EnemyMovement>();
            combat = GetComponent<EnemyCombat>();
            health = GetComponent<EnemyHealth>();
            health.Died += OnDied;
            health.Damaged += OnHit;
            spitter = GetComponent<SpitterAttack>();
            burrower = GetComponent<BurrowerTraversal>();
            brood = GetComponent<BroodmotherSpawn>();
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
            attackPose = 0;
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
            spitter?.Initialize(this, grid, state);
            burrower?.Initialize(this, grid.Grid.CellSize);
            brood?.Initialize(this, grid);
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
            attackPose = Mathf.Max(0, attackPose - deltaTime);
            bool attacking = spitter != null && spitter.Tick(deltaTime);
            if (!attacking) { if (burrower != null) burrower.Move(deltaTime); else movement.Tick(deltaTime); }
            brood?.Tick(deltaTime);
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
            Notify(CreatureDeath);
            registry?.NotifyKilled(this);
            // Health is already zero, so targeting/movement/attacks stop before deferred destruction.
            if (returnToPool != null) { gameObject.SetActive(false); returnToPool(this); }
            else Destroy(gameObject);
        }
        public bool TryResolveBreach() { if (!IsTargetable) return false; resolved = true; return true; }
        public void RemoveBreached() { if (!resolved) return; registry?.Unregister(this); gameObject.SetActive(false); if (returnToPool != null) returnToPool(this); else Destroy(gameObject); }
        private void OnEnable() { if (initialized && health.IsAlive) registry?.Register(this); }
        private void OnHit(int amount) { if (health.IsAlive) Notify(CreatureHit); }
        private void OnDisable() => registry?.Unregister(this);
        private void OnDestroy()
        {
            registry?.Unregister(this);
            if (health != null) health.Died -= OnDied;
            if (health != null) health.Damaged -= OnHit;
        }
    }
}
