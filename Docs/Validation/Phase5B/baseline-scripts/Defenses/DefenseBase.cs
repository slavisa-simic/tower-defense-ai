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
        private void HealthChanged() => Changed?.Invoke();
        public bool Dismantled { get; private set; }
        internal void SetConstructionOwner(ShorelineDefense.Building.BuildManager owner, int cost) { Owner = owner; ConstructionCost = cost; TotalInvestment = cost; }
        internal void RemoveByDismantle() {
            Dismantled = true;
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
        public float Range => Definition.RangeInCells * (LevelStats?.range ?? 1) * grid.CellSize;
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
        }

        public void TakeDamage(int amount) => health?.TakeDamage(amount);
        private void OnDied()
        {
            Died?.Invoke();
            ReleaseCells();
            Destroy(gameObject);
        }
        private void ReleaseCells() => grid?.ClearOccupied(Anchor, footprint, this);
        private void OnDestroy()
        {
            removed = true;
            ReleaseCells();
            if (health != null) { health.Died -= OnDied; health.Changed -= HealthChanged; }
            Changed?.Invoke();
        }
    }
}
