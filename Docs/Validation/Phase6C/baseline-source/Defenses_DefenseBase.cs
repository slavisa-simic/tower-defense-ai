using System;
using ShorelineDefense.Combat;
using ShorelineDefense.Core;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Defenses
{
    public sealed class DefenseBase : MonoBehaviour, IDamageable
    {
        private Health health;
        private bool removed;
        public ShorelineDefense.Building.BuildManager Owner { get; private set; }
        public int ConstructionCost { get; private set; }
        public int Level { get; private set; } = 1;
        public long TotalInvestment { get; private set; }
        public int MaxHealth => health?.MaxHealth ?? 0;
        public event Action Changed;
        public event Action Upgraded;
        public event Action<int> Damaged;
        private DefenseLevelData LevelStats => Definition.LevelData(Level);
        public int Damage => Mathf.RoundToInt(Definition.Damage * (LevelStats?.damage ?? 1));
        public float FireRate => Definition.FireRate * (LevelStats?.fireRate ?? 1);
        public float MovementMultiplier => Mathf.Max(.05f, Definition.MovementMultiplier * (LevelStats?.slow ?? 1));
        public float SplashRadiusInCells => Definition.HeavyWeapon.splashRadiusInCells * (LevelStats?.splash ?? 1);
        public float BurnDuration => Definition.HeavyWeapon.burnDuration * (LevelStats?.burnDuration ?? 1);
        public float BurnDps => Definition.HeavyWeapon.burnDps * (LevelStats?.burnDps ?? 1);
        internal void ApplyUpgrade(int cost)
        {
            Level++;
            TotalInvestment += cost;
            health.IncreaseMaximum(Mathf.RoundToInt(Definition.MaxHealth * (LevelStats?.health ?? 1)));
            Upgraded?.Invoke();
            Changed?.Invoke();
        }
        internal void Repair() => health.RestoreFull();
        internal void RestoreState(Persistence.StructureSaveData data)
        {
            Level = data.level; ConstructionCost = data.constructionCost; TotalInvestment = data.investment;
            health.IncreaseMaximum(Mathf.RoundToInt(Definition.MaxHealth * (LevelStats?.health ?? 1)));
            if (health.MaxHealth != data.maxHealth) throw new InvalidOperationException("Structure tuning differs from this save.");
            health.TakeDamage(health.MaxHealth - data.health);
        }
        private void HealthChanged() => Changed?.Invoke();
        public bool Dismantled { get; private set; }
        internal void SetConstructionOwner(ShorelineDefense.Building.BuildManager owner, int cost) { Owner = owner; ConstructionCost = cost; TotalInvestment = cost; }
        internal void RemoveByDismantle() {
            if (!IsAlive) return;
            Dismantled = true;
            removed = true;
            Changed?.Invoke();
            ReleaseCells();
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        private BattlefieldGrid grid;
        private Vector2Int footprint;
        public DefenseDefinition Definition { get; private set; }
        public GameStateController Game { get; private set; }
        public EnemyRegistry Enemies { get; private set; }
        public Vector2Int Anchor { get; private set; }
        public bool IsAlive => health != null && health.IsAlive && !Dismantled && !removed;
        public int CurrentHealth => health?.CurrentHealth ?? 0;
        public float Range => Definition.RangeInCells * (LevelStats?.range ?? 1) * grid.CellSize *
            (Definition.Id == "land-mine" || Definition.Id == "barbed-wire" ? 1 : Owner?.Events?.Modifiers.defenseRange ?? 1);
        public BattlefieldGrid Grid => grid;
        public event Action Died;

        public void Initialize(DefenseDefinition definition, Vector2Int anchor, BattlefieldGrid battlefield,
            GameStateController state, EnemyRegistry enemies)
        {
            if (health != null) throw new InvalidOperationException("A defense can only be initialized once.");
            Definition = definition;
            Anchor = anchor;
            grid = battlefield;
            footprint = definition.Footprint;
            Game = state;
            Enemies = enemies;
            health = new Health(definition.MaxHealth);
            health.Changed += HealthChanged;
            health.Died += OnDied;
            if (grid.Plane == GameplayPlane.XY && definition.BlocksMovement && TryGetComponent<BoxCollider2D>(out var collider))
                collider.size = new Vector2(footprint.x, footprint.y) * grid.CellSize;
        }

        public bool ActivateNavigation() => !Definition.BlocksMovement || grid.TrySetNavigationBlocker(Anchor, footprint, this);

        public Vector3 ClosestFootprintPoint(Vector3 position)
        {
            Vector3 center = grid.FootprintCenter(Anchor, footprint);
            var half = new Vector2(footprint.x, footprint.y) * (grid.CellSize * .5f);
            return grid.Plane == GameplayPlane.XY
                ? new Vector3(Mathf.Clamp(position.x, center.x - half.x, center.x + half.x), Mathf.Clamp(position.y, center.y - half.y, center.y + half.y), center.z)
                : new Vector3(Mathf.Clamp(position.x, center.x - half.x, center.x + half.x), center.y, Mathf.Clamp(position.z, center.z - half.y, center.z + half.y));
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            int before = CurrentHealth;
            health.TakeDamage(amount);
            if (IsAlive) Damaged?.Invoke(before - CurrentHealth);
        }
        private void OnDied()
        {
            if (removed) return;
            removed = true;
            ReleaseCells();
            Changed?.Invoke();
            Died?.Invoke();
            // Disable combat, targeting, smoke and selection presentation in this command, not at frame end.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        private void ReleaseCells()
        {
            grid?.ClearTraversalModifier(Anchor, footprint, this);
            grid?.ClearNavigationBlocker(Anchor, footprint, this);
            grid?.ClearOccupied(Anchor, footprint, this);
        }
        private void OnDestroy()
        {
            removed = true;
            ReleaseCells();
            if (health != null) { health.Died -= OnDied; health.Changed -= HealthChanged; }
            Changed?.Invoke();
        }
    }
}
