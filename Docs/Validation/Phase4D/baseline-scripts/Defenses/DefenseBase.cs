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
        public ShorelineDefense.Building.BuildManager Owner { get; private set; }
        public int ConstructionCost { get; private set; }
        public bool Dismantled { get; private set; }
        internal void SetConstructionOwner(ShorelineDefense.Building.BuildManager owner, int cost) { Owner = owner; ConstructionCost = cost; }
        internal void RemoveByDismantle() {
            Dismantled = true;
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
        public bool IsAlive => health != null && health.IsAlive && !Dismantled;
        public int CurrentHealth => health?.CurrentHealth ?? 0;
        public float Range => Definition.RangeInCells * grid.CellSize;
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
            ReleaseCells();
            if (health != null) health.Died -= OnDied;
        }
    }
}
