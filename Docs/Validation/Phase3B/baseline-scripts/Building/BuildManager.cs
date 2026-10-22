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
