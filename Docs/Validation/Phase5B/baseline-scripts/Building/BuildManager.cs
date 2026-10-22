using System;
using ShorelineDefense.Core;
using ShorelineDefense.Defenses;
using ShorelineDefense.Economy;
using ShorelineDefense.Enemies;
using ShorelineDefense.Grid;
using UnityEngine;

namespace ShorelineDefense.Building
{
    public class BuildManager : MonoBehaviour
    {
        [SerializeField] private GridManager grid;
        [SerializeField] private EconomyManager economy;
        [SerializeField] private GameStateController game;
        [SerializeField] private EnemyRegistry enemies;
        [SerializeField] private Transform defenseParent;
        private Transform staging;
        private bool placing;
        [SerializeField] private ShorelineDefense.Waves.WaveManager waves;
        [SerializeField, Min(.01f)] private float repairBasisFraction = .5f;
        [SerializeField, Min(.01f)] private float preparationRepairMultiplier = .75f;
        [SerializeField, Min(.01f)] private float waveRepairMultiplier = 1.25f;
        public void ConfigureWaves(ShorelineDefense.Waves.WaveManager manager) => waves = manager;
        public float RepairMultiplier => waves != null && (waves.State == ShorelineDefense.Waves.WaveState.Spawning || waves.State == ShorelineDefense.Waves.WaveState.Combat) ? waveRepairMultiplier : preparationRepairMultiplier;
        public int UpgradeCost(DefenseBase defense) => defense != null && defense.Level < 3 ? defense.Definition.LevelData(defense.Level + 1)?.cost ?? 0 : 0;
        public int RepairCost(DefenseBase defense) => defense != null && defense.IsAlive && defense.CurrentHealth < defense.MaxHealth ?
            Math.Max(1, (int)Math.Min(int.MaxValue, Math.Ceiling(defense.TotalInvestment * (double)repairBasisFraction * (defense.MaxHealth - defense.CurrentHealth) / defense.MaxHealth * RepairMultiplier))) : 0;
        public bool CanUpgrade(DefenseBase defense) => CanDismantle(defense) && defense.Level < 3 && UpgradeCost(defense) > 0 && economy.CanAfford(UpgradeCost(defense));
        public bool CanRepair(DefenseBase defense) => CanDismantle(defense) && RepairCost(defense) > 0 && economy.CanAfford(RepairCost(defense));
        public bool TryUpgrade(DefenseBase defense)
        {
            if (placing || !CanUpgrade(defense)) return false;
            placing = true;
            try { int cost = UpgradeCost(defense); if (!economy.TrySpend(cost)) return false; defense.ApplyUpgrade(cost); return true; }
            finally { placing = false; }
        }
        public bool TryRepair(DefenseBase defense)
        {
            if (placing || !CanRepair(defense)) return false;
            placing = true;
            try { if (!economy.TrySpend(RepairCost(defense))) return false; defense.Repair(); return true; }
            finally { placing = false; }
        }
        [SerializeField, Range(0, 1)] private float dismantleRefundFraction = .5f;
        public int RefundFor(DefenseBase defense) => defense != null ? (int)Math.Min(int.MaxValue, Math.Floor(defense.TotalInvestment * (double)(float.IsNaN(dismantleRefundFraction) ? .5f : Mathf.Clamp01(dismantleRefundFraction)))) : 0;
        public bool CanDismantle(DefenseBase defense) => game != null && game.IsPlaying && defense != null && defense.Owner == this && defense.IsAlive && !defense.Dismantled && defense.gameObject.activeInHierarchy && !(defense.GetComponent<LandMine>()?.Exploded ?? false);
        public bool TryDismantle(DefenseBase defense, out int refund) {
            refund = 0;
            if (placing || !CanDismantle(defense)) return false;
            placing = true;
            try {
                refund = RefundFor(defense);
                defense.RemoveByDismantle();
                economy.RefundConstruction(refund);
                return true;
            } finally { placing = false; }
        }
        public GridManager Battlefield => grid;
        public EconomyManager Economy => economy;
        public GameStateController Game => game;
        public void Configure(GridManager battlefield, EconomyManager money, GameStateController state, EnemyRegistry registry, Transform parent)
        { grid = battlefield; economy = money; game = state; enemies = registry; defenseParent = parent; }

        public PlacementResult Validate(DefenseDefinition definition, Vector2Int cell)
        {
            if (game == null || !game.IsPlaying) return PlacementResult.NotPlaying;
            if (definition == null || !definition.IsValid || economy == null || enemies == null) return PlacementResult.InvalidDefinition;
            return PlacementRules.Validate(grid?.Grid, cell, definition.Footprint, definition.Cost, economy.Money);
        }

        // Explicit gameplay command: no mouse, UI, targeting, or MG-specific logic.
        public PlacementResult TryPlaceDefense(DefenseDefinition definition, Vector2Int cell, out DefenseBase defense)
        {
            defense = null;
            if (placing) return PlacementResult.Busy;
            var result = Validate(definition, cell);
            if (result != PlacementResult.Valid) return result;
            placing = true;
            DefenseBase candidate = null;
            bool committed = false;
            try
            {
                if (staging == null)
                {
                    staging = new GameObject("Inactive Defense Staging").transform;
                    staging.SetParent(transform, false);
                    staging.gameObject.SetActive(false);
                }
                // Prepare under an inactive parent: behaviors cannot run before the command commits.
                candidate = CreateDefense(definition, grid.Grid.FootprintCenter(cell, definition.Footprint), staging);
                if (candidate == null) return PlacementResult.CreationFailed;
                candidate.gameObject.SetActive(false);
                candidate.Initialize(definition, cell, grid.Grid, game, enemies);
                result = Validate(definition, cell);
                if (result != PlacementResult.Valid) return result;
                if (!grid.Grid.TryOccupy(cell, definition.Footprint, candidate)) return PlacementResult.Occupied;
                // All fallible creation/initialization happens before payment. No refund path is needed.
                if (!economy.TrySpend(definition.Cost)) return game.IsPlaying ? PlacementResult.InsufficientMoney : PlacementResult.NotPlaying;
                committed = true;
                candidate.SetConstructionOwner(this, definition.Cost);
                candidate.transform.SetParent(defenseParent, true);
                candidate.gameObject.SetActive(true);
                defense = candidate;
                return PlacementResult.Valid;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                return PlacementResult.CreationFailed;
            }
            finally
            {
                if (!committed && candidate != null)
                {
                    grid.Grid.ClearOccupied(cell, definition.Footprint, candidate);
                    if (Application.isPlaying) Destroy(candidate.gameObject);
                    else DestroyImmediate(candidate.gameObject);
                }
                placing = false;
            }
        }

        protected virtual DefenseBase CreateDefense(DefenseDefinition definition, Vector3 position, Transform parent) =>
            Instantiate(definition.Prefab, position, Quaternion.identity, parent);
    }
}

